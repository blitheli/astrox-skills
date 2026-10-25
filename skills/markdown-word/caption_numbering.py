"""Keep template A图片 / A表 Word auto-numbering across pandoc and LibreOffice.

Caption paragraphs carry only the title. The visible 「图1」「表1」 come from
numbering.xml (lvlText ``图%1`` / ``表%1``) via the caption paragraph styles.
LibreOffice roundtrip remaps styleId (often A0/A → A3/A4) while leaving
abstractNum ``pStyle`` on the old id, so this repair must run again after LO.

Idempotent: safe to call before and after the roundtrip.
"""
from __future__ import annotations

import re
import shutil
import tempfile
import zipfile
from pathlib import Path

from lxml import etree

W_NS = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
CT_NS = "http://schemas.openxmlformats.org/package/2006/content-types"
REL_NS = "http://schemas.openxmlformats.org/package/2006/relationships"
NUM_REL_TYPE = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/numbering"
NUM_CONTENT_TYPE = (
    "application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml"
)
XML_SPACE = "{http://www.w3.org/XML/1998/namespace}space"
W = f"{{{W_NS}}}"
NS = {"w": W_NS}

FIG_STYLE = "A图片"
TAB_STYLE = "A表"
IMG_STYLE = "A图片2"
# styleIds in the bundled template, before LibreOffice renames them
LEGACY_STYLE_IDS = {"图": frozenset({"A0"}), "表": frozenset({"A"})}
EXACT_LVL_TEXT = {"图": "图%1", "表": "表%1"}

# Manual prefix only (auto-number must not be baked into w:t).
CAP_RE = re.compile(
    r"^[\s\u00a0\u3000]*(?:CAPMARK)?[\s\u00a0\u3000]*"
    r"(表|图)[\s\u00a0\u3000]*([0-9]+)[\s\u00a0\u3000]*[.\u3001]?[\s\u00a0\u3000]*(.*)$",
    re.DOTALL,
)
TITLE_ONLY_MAX = 120

PPR_ORDER = [
    "pStyle",
    "keepNext",
    "keepLines",
    "pageBreakBefore",
    "framePr",
    "widowControl",
    "numPr",
    "suppressLineNumbers",
    "pBdr",
    "shd",
    "tabs",
    "suppressAutoHyphens",
    "kinsoku",
    "wordWrap",
    "overflowPunct",
    "topLinePunct",
    "autoSpaceDE",
    "autoSpaceDN",
    "bidi",
    "adjustRightInd",
    "snapToGrid",
    "spacing",
    "ind",
    "contextualSpacing",
    "mirrorIndents",
    "suppressOverlap",
    "jc",
    "textDirection",
    "textAlignment",
    "textboxTightWrap",
    "outlineLvl",
    "divId",
    "cnfStyle",
    "rPr",
    "sectPr",
    "pPrChange",
]

_PANDDOC_CAPTION = {
    "Image Caption": "图",
    "Table Caption": "表",
    "Caption": None,
}


def q(tag: str) -> str:
    return f"{W}{tag}"


def ptext(p: etree._Element) -> str:
    return "".join(p.xpath(".//w:t/text()", namespaces=NS))


def sid_by_name(styles_root: etree._Element, name: str, typ: str = "paragraph") -> str | None:
    want = name.lower()
    for s in styles_root.findall(q("style")):
        if typ and s.get(q("type")) not in (None, typ):
            continue
        n = s.find(q("name"))
        got = (n.get(q("val")) if n is not None else "") or ""
        if got.lower() == want:
            return s.get(q("styleId"))
    return None


def style_name_map(styles_root: etree._Element) -> dict[str, str]:
    out: dict[str, str] = {}
    for s in styles_root.findall(q("style")):
        n = s.find(q("name"))
        out[s.get(q("styleId")) or ""] = (n.get(q("val")) if n is not None else "") or ""
    return out


def caption_style_ids(styles_root: etree._Element) -> set[str]:
    """Style ids demote must not turn into plain numbered body text."""
    ids: set[str] = set()
    for name in (FIG_STYLE, TAB_STYLE, IMG_STYLE):
        sid = sid_by_name(styles_root, name)
        if sid:
            ids.add(sid)
    return ids


def _lvl0(abstract_num: etree._Element) -> etree._Element | None:
    for lvl in abstract_num.findall(q("lvl")):
        if lvl.get(q("ilvl")) in (None, "0"):
            return lvl
    lvls = abstract_num.findall(q("lvl"))
    return lvls[0] if lvls else None


def _lvl_text(lvl: etree._Element | None) -> str:
    if lvl is None:
        return ""
    lt = lvl.find(q("lvlText"))
    return lt.get(q("val")) if lt is not None else ""


def _num_fmt(lvl: etree._Element | None) -> str:
    if lvl is None:
        return ""
    el = lvl.find(q("numFmt"))
    return el.get(q("val")) if el is not None else ""


def _pstyle(lvl: etree._Element | None) -> str:
    if lvl is None:
        return ""
    el = lvl.find(q("pStyle"))
    return el.get(q("val")) if el is not None else ""


def _norm_lvl_text(txt: str) -> str:
    return re.sub(r"\s+", "", txt or "")


def abstract_lvl0_is_caption(abstract_num: etree._Element) -> bool:
    """True for 图/表 caption definitions (demote must not rewrite them)."""
    norm = _norm_lvl_text(_lvl_text(_lvl0(abstract_num)))
    return norm.startswith("图") or norm.startswith("表")


def caption_num_ids(num_root: etree._Element) -> set[str]:
    """numId values that belong to 图/表 caption abstracts."""
    cap_abs: set[str] = set()
    for an in num_root.findall(q("abstractNum")):
        if abstract_lvl0_is_caption(an):
            aid = an.get(q("abstractNumId"))
            if aid is not None:
                cap_abs.add(aid)
    ids: set[str] = set()
    for num in num_root.findall(q("num")):
        abs_el = num.find(q("abstractNumId"))
        if abs_el is None:
            continue
        if abs_el.get(q("val")) in cap_abs:
            nid = num.get(q("numId"))
            if nid:
                ids.add(nid)
    return ids


def _abstract_by_id(num_root: etree._Element, abs_id: str | None) -> etree._Element | None:
    if not abs_id:
        return None
    for an in num_root.findall(q("abstractNum")):
        if an.get(q("abstractNumId")) == abs_id:
            return an
    return None


def _abstract_id_for_num(num_root: etree._Element, num_id: str | None) -> str | None:
    if not num_id or num_id == "0":
        return None
    for num in num_root.findall(q("num")):
        if num.get(q("numId")) != num_id:
            continue
        abs_el = num.find(q("abstractNumId"))
        return abs_el.get(q("val")) if abs_el is not None else None
    return None


def _style_num_id(style_el: etree._Element | None) -> str | None:
    if style_el is None:
        return None
    nid = style_el.find(f"{q('pPr')}/{q('numPr')}/{q('numId')}")
    if nid is None:
        return None
    return nid.get(q("val"))


def _find_style(styles_root: etree._Element, name: str) -> etree._Element | None:
    sid = sid_by_name(styles_root, name)
    if not sid:
        return None
    for s in styles_root.findall(q("style")):
        if s.get(q("styleId")) == sid and s.get(q("type")) in (None, "paragraph"):
            return s
    return None


def _score_caption_abstract(
    abstract_num: etree._Element,
    kind: str,
    target_sid: str | None,
    preferred_abs: str | None,
) -> int | None:
    """Pick the decimal definition whose visible label is exactly 图%1 / 表%1.

    The bundled template also contains ``图 %1``, ``图%1.`` and a Chinese-counting
    ``表%1``. Those must not steal the caption numId.
    """
    lvl = _lvl0(abstract_num)
    raw = _lvl_text(lvl)
    if _norm_lvl_text(raw) != EXACT_LVL_TEXT[kind]:
        return None
    fmt = _num_fmt(lvl)
    if fmt not in ("", "decimal"):
        return None
    score = 10
    if raw == EXACT_LVL_TEXT[kind]:
        score += 20
    if fmt == "decimal":
        score += 10
    ps = _pstyle(lvl)
    if target_sid and ps == target_sid:
        score += 100
    if ps in LEGACY_STYLE_IDS[kind]:
        score += 40
    aid = abstract_num.get(q("abstractNumId"))
    if preferred_abs and aid == preferred_abs:
        score += 200
    return score


def _choose_abstract(
    num_root: etree._Element,
    kind: str,
    target_sid: str | None,
    preferred_abs: str | None,
) -> etree._Element | None:
    best_score = -1
    best_aid = ""
    best: etree._Element | None = None
    for an in num_root.findall(q("abstractNum")):
        score = _score_caption_abstract(an, kind, target_sid, preferred_abs)
        if score is None:
            continue
        aid = an.get(q("abstractNumId")) or ""
        aid_key = f"{int(aid):08d}" if aid.isdigit() else aid
        # Higher score wins; then the lower abstractNumId (template 表%1 is 0).
        if best is None or score > best_score or (score == best_score and aid_key < best_aid):
            best_score = score
            best_aid = aid_key
            best = an
    return best


def _set_lvl_pstyle(lvl: etree._Element, style_id: str) -> None:
    ps = lvl.find(q("pStyle"))
    if ps is None:
        ps = etree.Element(q("pStyle"))
        nf = lvl.find(q("numFmt"))
        if nf is not None:
            nf.addnext(ps)
        else:
            lvl.insert(0, ps)
    ps.set(q("val"), style_id)


def _set_lvl_text(lvl: etree._Element, text: str) -> None:
    lt = lvl.find(q("lvlText"))
    if lt is None:
        lt = etree.Element(q("lvlText"))
        ps = lvl.find(q("pStyle"))
        if ps is not None:
            ps.addnext(lt)
        else:
            lvl.append(lt)
    lt.set(q("val"), text)


def _ensure_num_for_abstract(
    num_root: etree._Element, abs_id: str, preferred_num_id: str | None
) -> str:
    if preferred_num_id and preferred_num_id != "0":
        if _abstract_id_for_num(num_root, preferred_num_id) == abs_id:
            return preferred_num_id
    for num in num_root.findall(q("num")):
        abs_el = num.find(q("abstractNumId"))
        if abs_el is not None and abs_el.get(q("val")) == abs_id:
            nid = num.get(q("numId"))
            if nid and nid != "0":
                return nid
    used: set[int] = set()
    for num in num_root.findall(q("num")):
        nid = num.get(q("numId")) or ""
        if nid.isdigit():
            used.add(int(nid))
    new_id = 1
    while new_id in used:
        new_id += 1
    num = etree.SubElement(num_root, q("num"))
    num.set(q("numId"), str(new_id))
    etree.SubElement(num, q("abstractNumId")).set(q("val"), abs_id)
    return str(new_id)


def _bind_caption_numbering(
    num_root: etree._Element,
    fig_sid: str | None,
    tab_sid: str | None,
    fig_num: str | None,
    tab_num: str | None,
) -> dict[str, str]:
    """Point 图%1 / 表%1 abstracts at the current caption styleIds. Return numIds."""
    bound: dict[str, str] = {}
    targets = (("图", fig_sid, fig_num), ("表", tab_sid, tab_num))
    for kind, sid, preferred_num in targets:
        if not sid:
            continue
        preferred_abs = _abstract_id_for_num(num_root, preferred_num)
        # Ignore a preferred num that is not actually this caption format.
        if preferred_abs:
            an_pref = _abstract_by_id(num_root, preferred_abs)
            if an_pref is None or _score_caption_abstract(an_pref, kind, sid, preferred_abs) is None:
                preferred_abs = None
                preferred_num = None
        an = _choose_abstract(num_root, kind, sid, preferred_abs)
        if an is None:
            continue
        lvl = _lvl0(an)
        if lvl is None:
            continue
        _set_lvl_text(lvl, EXACT_LVL_TEXT[kind])
        fmt = lvl.find(q("numFmt"))
        if fmt is None:
            fmt = etree.Element(q("numFmt"))
            lvl.insert(0, fmt)
        if fmt.get(q("val")) != "decimal":
            fmt.set(q("val"), "decimal")
        _set_lvl_pstyle(lvl, sid)
        abs_id = an.get(q("abstractNumId")) or ""
        bound[kind] = _ensure_num_for_abstract(num_root, abs_id, preferred_num)
    return bound


def _numbering_has_captions(num_root: etree._Element, fig_sid: str | None, tab_sid: str | None) -> bool:
    return all(
        _choose_abstract(num_root, kind, sid, None) is not None
        for kind, sid in (("图", fig_sid), ("表", tab_sid))
    )


def _load_template_numbering(template: Path) -> bytes:
    with zipfile.ZipFile(template) as zf:
        return zf.read("word/numbering.xml")


def _ensure_content_type(ct_path: Path) -> None:
    tree = etree.parse(str(ct_path))
    root = tree.getroot()
    part = "/word/numbering.xml"
    found = False
    for ov in root.findall(f"{{{CT_NS}}}Override"):
        if ov.get("PartName") == part:
            ov.set("ContentType", NUM_CONTENT_TYPE)
            found = True
            break
    if not found:
        etree.SubElement(
            root,
            f"{{{CT_NS}}}Override",
            PartName=part,
            ContentType=NUM_CONTENT_TYPE,
        )
    tree.write(str(ct_path), xml_declaration=True, encoding="UTF-8", standalone=True)


def _ensure_numbering_rel(rels_path: Path) -> None:
    tree = etree.parse(str(rels_path))
    root = tree.getroot()
    for rel in root.findall(f"{{{REL_NS}}}Relationship"):
        if rel.get("Type") == NUM_REL_TYPE or rel.get("Target") == "numbering.xml":
            rel.set("Type", NUM_REL_TYPE)
            rel.set("Target", "numbering.xml")
            tree.write(str(rels_path), xml_declaration=True, encoding="UTF-8", standalone=True)
            return
    used: set[int] = set()
    for rel in root.findall(f"{{{REL_NS}}}Relationship"):
        m = re.match(r"rId(\d+)$", rel.get("Id") or "")
        if m:
            used.add(int(m.group(1)))
    nid = 1
    while nid in used:
        nid += 1
    etree.SubElement(
        root,
        f"{{{REL_NS}}}Relationship",
        Id=f"rId{nid}",
        Type=NUM_REL_TYPE,
        Target="numbering.xml",
    )
    tree.write(str(rels_path), xml_declaration=True, encoding="UTF-8", standalone=True)


def _reorder_ppr(pPr: etree._Element) -> None:
    kids = list(pPr)
    kids.sort(
        key=lambda e: PPR_ORDER.index(etree.QName(e).localname)
        if etree.QName(e).localname in PPR_ORDER
        else 99
    )
    for k in kids:
        pPr.remove(k)
    for k in kids:
        pPr.append(k)


def _ensure_ppr(el: etree._Element) -> etree._Element:
    pPr = el.find(q("pPr"))
    if pPr is None:
        pPr = etree.Element(q("pPr"))
        el.insert(0, pPr)
    return pPr


def _set_num_id(pPr: etree._Element, num_id: str) -> None:
    numPr = pPr.find(q("numPr"))
    if numPr is None:
        numPr = etree.Element(q("numPr"))
        pPr.insert(0, numPr)
    for child in list(numPr):
        numPr.remove(child)
    for attr in list(numPr.attrib):
        del numPr.attrib[attr]
    etree.SubElement(numPr, q("numId")).set(q("val"), num_id)
    _reorder_ppr(pPr)


def _strip_para_numpr(p: etree._Element) -> None:
    pPr = p.find(q("pPr"))
    if pPr is None:
        return
    for numPr in list(pPr.findall(q("numPr"))):
        pPr.remove(numPr)


def _zero_caption_ind(pPr: etree._Element) -> None:
    ind = pPr.find(q("ind"))
    if ind is None:
        ind = etree.SubElement(pPr, q("ind"))
    for attr in (q("hanging"), q("hangingChars")):
        if attr in ind.attrib:
            del ind.attrib[attr]
    ind.set(q("left"), "0")
    ind.set(q("leftChars"), "0")
    ind.set(q("firstLine"), "0")
    ind.set(q("firstLineChars"), "0")
    jc = pPr.find(q("jc"))
    if jc is None:
        jc = etree.SubElement(pPr, q("jc"))
    jc.set(q("val"), "center")
    _reorder_ppr(pPr)


def _set_pstyle(p: etree._Element, style_id: str) -> None:
    pPr = _ensure_ppr(p)
    ps = pPr.find(q("pStyle"))
    if ps is None:
        ps = etree.Element(q("pStyle"))
        pPr.insert(0, ps)
    ps.set(q("val"), style_id)
    _reorder_ppr(pPr)


def _set_text(p: etree._Element, text: str) -> None:
    runs = p.findall(q("r"))
    rpr = None
    if runs and runs[0].find(q("rPr")) is not None:
        rpr = etree.fromstring(etree.tostring(runs[0].find(q("rPr"))))
    for r in list(p.findall(q("r"))):
        p.remove(r)
    for h in list(p.findall(q("hyperlink"))):
        p.remove(h)
    r = etree.SubElement(p, q("r"))
    if rpr is not None:
        r.append(rpr)
    t = etree.SubElement(r, q("t"))
    if text[:1].isspace() or text[-1:].isspace() or "  " in text:
        t.set(XML_SPACE, "preserve")
    t.text = text


def _has_drawing(p: etree._Element) -> bool:
    return bool(p.xpath(".//w:drawing|.//w:pict", namespaces=NS))


def _clear_drawing_caption_text(p: etree._Element) -> bool:
    raw = ptext(p).strip()
    if not raw:
        return False
    if CAP_RE.match(raw) or re.match(r"^(图|表)[\s\u00a0\u3000]*[0-9]*[\s\u00a0\u3000]*$", raw):
        for r in list(p.findall(q("r"))):
            for t in list(r.findall(q("t"))):
                r.remove(t)
            if all(etree.QName(c).localname == "rPr" for c in r):
                p.remove(r)
        return True
    return False


def _caption_title(text: str, kind: str) -> tuple[str, bool]:
    m = CAP_RE.match(text)
    if m and m.group(1) == kind:
        return m.group(3).strip(), True
    return text.strip(), False


def _para_style(p: etree._Element) -> str | None:
    ps = p.find(f"{q('pPr')}/{q('pStyle')}")
    if ps is None:
        return None
    return ps.get(q("val"))


def _is_heading_name(name: str) -> bool:
    low = name.lower()
    if low in {"heading 1", "heading 2", "heading 3", "heading 4", "title", "subtitle"}:
        return True
    return name in {"标题 1", "标题 2", "标题 3", "一级标题", "章的标题", "条的标题"}


def _looks_like_caption(p: etree._Element, kind: str, names: dict[str, str]) -> bool:
    if _has_drawing(p):
        return False
    sid = _para_style(p)
    name = names.get(sid or "", "")
    if _is_heading_name(name):
        return False
    text = ptext(p)
    if name == (FIG_STYLE if kind == "图" else TAB_STYLE):
        return True
    pandoc_kind = _PANDDOC_CAPTION.get(name)
    if pandoc_kind == kind:
        return True
    matched = CAP_RE.match(text)
    if matched and matched.group(1) == kind and len(text) <= TITLE_ONLY_MAX:
        return True
    return False


def _normalize_caption(p: etree._Element, kind: str, style_id: str) -> bool:
    """Style + title only. Returns True when a handwritten 图N/表N prefix was removed."""
    raw = ptext(p)
    title, stripped = _caption_title(raw, kind)
    _set_pstyle(p, style_id)
    _strip_para_numpr(p)
    _zero_caption_ind(_ensure_ppr(p))
    if kind == "表":
        pPr = _ensure_ppr(p)
        if pPr.find(q("keepNext")) is None:
            etree.SubElement(pPr, q("keepNext"))
        _reorder_ppr(pPr)
    if stripped or raw != title:
        _set_text(p, title)
    return stripped


def _ensure_caption_styles(
    styles_root: etree._Element, num_ids: dict[str, str], fig_sid: str | None, tab_sid: str | None
) -> list[str]:
    fixed: list[str] = []
    for kind, name, sid in (("图", FIG_STYLE, fig_sid), ("表", TAB_STYLE, tab_sid)):
        if not sid or kind not in num_ids:
            continue
        style_el = _find_style(styles_root, name)
        if style_el is None:
            continue
        pPr = style_el.find(q("pPr"))
        if pPr is None:
            pPr = etree.Element(q("pPr"))
            rPr = style_el.find(q("rPr"))
            if rPr is not None:
                style_el.insert(list(style_el).index(rPr), pPr)
            else:
                style_el.append(pPr)
        _set_num_id(pPr, num_ids[kind])
        # Direct ind/jc so a broken basedOn chain still centers the caption.
        _zero_caption_ind(pPr)
        fixed.append(name)
    return fixed


def suppress_image_style(styles_root: etree._Element) -> bool:
    """A图片2 is basedOn A图片 and would otherwise consume 图 numbers."""
    style_el = _find_style(styles_root, IMG_STYLE)
    if style_el is None:
        return False
    pPr = style_el.find(q("pPr"))
    if pPr is None:
        pPr = etree.Element(q("pPr"))
        rPr = style_el.find(q("rPr"))
        if rPr is not None:
            style_el.insert(list(style_el).index(rPr), pPr)
        else:
            style_el.append(pPr)
    _set_num_id(pPr, "0")
    return True


def _neighbor(el: etree._Element, direction: int) -> etree._Element | None:
    e = el.getnext() if direction > 0 else el.getprevious()
    while e is not None and etree.QName(e).localname not in ("p", "tbl"):
        e = e.getnext() if direction > 0 else e.getprevious()
    return e


def fix_caption_numbering(docx_path: Path, template_path: Path) -> dict:
    """Restore A图片/A表 numPr and align numbering pStyle to current styleIds.

    Call on the pre-LO docx and again on the LibreOffice roundtrip result.
    """
    template_path = Path(template_path)
    docx_path = Path(docx_path)
    if not template_path.is_file():
        raise FileNotFoundError(f"template not found: {template_path}")
    if not docx_path.is_file():
        raise FileNotFoundError(f"docx not found: {docx_path}")

    work = Path(tempfile.mkdtemp(prefix="capnum-"))
    try:
        with zipfile.ZipFile(docx_path) as zf:
            zf.extractall(work)

        styles_path = work / "word" / "styles.xml"
        doc_path = work / "word" / "document.xml"
        num_path = work / "word" / "numbering.xml"
        styles_tree = etree.parse(str(styles_path))
        styles_root = styles_tree.getroot()
        doc_tree = etree.parse(str(doc_path))
        doc_root = doc_tree.getroot()

        fig_sid = sid_by_name(styles_root, FIG_STYLE)
        tab_sid = sid_by_name(styles_root, TAB_STYLE)
        img_sid = sid_by_name(styles_root, IMG_STYLE)
        fig_style = _find_style(styles_root, FIG_STYLE)
        tab_style = _find_style(styles_root, TAB_STYLE)
        fig_num = _style_num_id(fig_style)
        tab_num = _style_num_id(tab_style)

        replaced = False
        if not num_path.exists():
            num_path.write_bytes(_load_template_numbering(template_path))
            replaced = True
        num_tree = etree.parse(str(num_path))
        num_root = num_tree.getroot()
        if not _numbering_has_captions(num_root, fig_sid, tab_sid):
            num_path.write_bytes(_load_template_numbering(template_path))
            num_tree = etree.parse(str(num_path))
            num_root = num_tree.getroot()
            replaced = True
            fig_num = None
            tab_num = None

        num_ids = _bind_caption_numbering(num_root, fig_sid, tab_sid, fig_num, tab_num)
        fixed_styles = _ensure_caption_styles(styles_root, num_ids, fig_sid, tab_sid)
        img_suppressed = suppress_image_style(styles_root)

        ct_path = work / "[Content_Types].xml"
        rels_path = work / "word" / "_rels" / "document.xml.rels"
        if ct_path.exists():
            _ensure_content_type(ct_path)
        if rels_path.exists():
            _ensure_numbering_rel(rels_path)

        names = style_name_map(styles_root)
        # styles were patched; refresh name map (ids unchanged)
        body = doc_root.find(q("body"))
        drawings = 0
        captions = 0
        prefixes = 0
        if body is not None:
            for el in list(body):
                if etree.QName(el).localname != "p" or not _has_drawing(el):
                    continue
                drawings += 1
                if img_sid:
                    _set_pstyle(el, img_sid)
                    _strip_para_numpr(el)
                elif fig_sid and _para_style(el) == fig_sid:
                    _set_num_id(_ensure_ppr(el), "0")
                if _clear_drawing_caption_text(el):
                    prefixes += 1
                nxt = _neighbor(el, 1)
                if nxt is not None and etree.QName(nxt).localname == "p" and fig_sid:
                    if _looks_like_caption(nxt, "图", names):
                        if _normalize_caption(nxt, "图", fig_sid):
                            prefixes += 1
                        captions += 1

            for el in list(body):
                if etree.QName(el).localname != "tbl" or not tab_sid:
                    continue
                prv = _neighbor(el, -1)
                if prv is None or etree.QName(prv).localname != "p":
                    continue
                if _looks_like_caption(prv, "表", names):
                    if _normalize_caption(prv, "表", tab_sid):
                        prefixes += 1
                    captions += 1

            # Any remaining caption-styled paragraphs (already counted or not).
            seen = set()
            for el in list(body):
                if etree.QName(el).localname != "p" or _has_drawing(el):
                    continue
                sid = _para_style(el)
                name = names.get(sid or "", "")
                if name == FIG_STYLE and fig_sid:
                    kind, target = "图", fig_sid
                elif name == TAB_STYLE and tab_sid:
                    kind, target = "表", tab_sid
                else:
                    continue
                if id(el) in seen:
                    continue
                seen.add(id(el))
                if _normalize_caption(el, kind, target):
                    prefixes += 1

        num_tree.write(str(num_path), xml_declaration=True, encoding="UTF-8", standalone=True)
        styles_tree.write(str(styles_path), xml_declaration=True, encoding="UTF-8", standalone=True)
        doc_tree.write(str(doc_path), xml_declaration=True, encoding="UTF-8", standalone=True)

        tmp = docx_path.with_suffix(".capnum.docx")
        with zipfile.ZipFile(tmp, "w", compression=zipfile.ZIP_DEFLATED) as zf:
            for f in sorted(work.rglob("*")):
                if f.is_file():
                    zf.write(f, f.relative_to(work).as_posix())
        tmp.replace(docx_path)

        return {
            "fig_style_id": fig_sid,
            "tab_style_id": tab_sid,
            "img2_style_id": img_sid,
            "caption_num_ids": num_ids,
            "numbering_replaced": replaced,
            "caption_styles_numPr": fixed_styles,
            "img2_numId0": img_suppressed,
            "drawings_restyled": drawings,
            "captions_normalized": captions,
            "manual_prefixes_stripped": prefixes,
        }
    finally:
        shutil.rmtree(work, ignore_errors=True)


def verify_caption_numbering(docx_path: Path) -> list[str]:
    """Return human-readable problems. Empty list means caption auto-numbering is intact."""
    errors: list[str] = []
    docx_path = Path(docx_path)
    with zipfile.ZipFile(docx_path) as zf:
        names = set(zf.namelist())
        if "word/numbering.xml" not in names:
            return ["missing word/numbering.xml"]
        styles = etree.fromstring(zf.read("word/styles.xml"))
        num_root = etree.fromstring(zf.read("word/numbering.xml"))
        doc = etree.fromstring(zf.read("word/document.xml"))
        ct = zf.read("[Content_Types].xml")
        rels = zf.read("word/_rels/document.xml.rels")

    if b"numbering" not in ct:
        errors.append("Content_Types missing numbering")
    if b"numbering" not in rels:
        errors.append("document.xml.rels missing numbering")

    smap = style_name_map(styles)
    fig_sid = sid_by_name(styles, FIG_STYLE)
    tab_sid = sid_by_name(styles, TAB_STYLE)
    img_sid = sid_by_name(styles, IMG_STYLE)
    if not fig_sid:
        errors.append("style A图片 missing")
    if not tab_sid:
        errors.append("style A表 missing")

    def _check_style(style_name: str, sid: str | None, kind: str) -> None:
        if not sid:
            return
        style_el = _find_style(styles, style_name)
        nid = _style_num_id(style_el)
        if not nid or nid == "0":
            errors.append(f"{style_name} sid={sid} missing numPr")
            return
        abs_id = _abstract_id_for_num(num_root, nid)
        an = _abstract_by_id(num_root, abs_id)
        lvl = _lvl0(an) if an is not None else None
        txt = _lvl_text(lvl)
        ps = _pstyle(lvl)
        if txt != EXACT_LVL_TEXT[kind]:
            errors.append(f"{style_name} lvlText={txt!r} want {EXACT_LVL_TEXT[kind]!r}")
        if ps != sid:
            errors.append(f"{style_name} numbering pStyle={ps!r} != styleId {sid!r}")

    _check_style(FIG_STYLE, fig_sid, "图")
    _check_style(TAB_STYLE, tab_sid, "表")

    if img_sid:
        img_el = _find_style(styles, IMG_STYLE)
        img_num = _style_num_id(img_el)
        if img_num != "0":
            errors.append(f"A图片2 numId should be 0, got {img_num!r}")
    else:
        errors.append("style A图片2 missing")

    for p in doc.findall(f"{q('body')}/{q('p')}"):
        if _has_drawing(p):
            continue
        sid = _para_style(p)
        name = smap.get(sid or "", "")
        if name not in (FIG_STYLE, TAB_STYLE):
            continue
        text = ptext(p)
        if CAP_RE.match(text):
            errors.append(f"manual caption prefix still in text: {text!r}")
    return errors
