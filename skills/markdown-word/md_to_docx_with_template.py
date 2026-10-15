#!/usr/bin/env python3
"""Convert Markdown to docx using a Chinese Word template as pandoc reference-doc.

Usage:
  python3 skills/markdown-word/md_to_docx_with_template.py <md> <template.docx> <outdir>

Produces in outdir (do not use the skill directory itself as outdir):
  reference.docx   – template with pandoc styles + heading numPr stripped
  out.docx         – pandoc output (intermediate)
  final.docx       – styles remapped, OMML fixed, tables styled
  final-word.docx  – LibreOffice roundtrip; preferred delivery when soffice exists
  final.pdf        – PDF exported after the roundtrip when LibreOffice exists
"""
from __future__ import annotations

import argparse
import re
import shutil
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path

from lxml import etree

W_NS = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
R_NS = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
W = f"{{{W_NS}}}"
R = f"{{{R_NS}}}"
NSMAP = {"w": W_NS, "r": R_NS}

# Pandoc styleId → target template styleId
STYLE_MAP = {
    "BodyText": "A5",
    "FirstParagraph": "A5",
    "Compact": "A5",
    "BlockText": "A5",
    "SourceCode": "A5",
    "TableCaption": "A",
    "ImageCaption": "A0",
    "Caption": "A0",
    "CaptionedFigure": "A0",
    "Figure": "A0",
    "Date": "a1",
    "Author": "A5",
    "Abstract": "A5",
    "AbstractTitle": "af6",
    # Title already maps to af6 via reference name match; keep explicit
    "Title": "af6",
    # Pandoc may also emit these English ids if reference lacks heading 1
    "Heading1": "1",
    "Heading2": "2",
    "Heading3": "3",
}

# Styles that must not carry auto-numbering (MD text already numbered)
STRIP_NUMPR_STYLE_IDS = {"1", "2", "3", "a1", "A0", "A", "a2", "a3", "-1", "-2"}


def _qn(tag: str) -> str:
    return f"{W}{tag}"


def _make_style(
    style_id: str,
    name: str,
    *,
    based_on: str = "A5",
    next_style: str = "A5",
    custom: bool = True,
    pPr_extra: list[etree._Element] | None = None,
    rPr_extra: list[etree._Element] | None = None,
) -> etree._Element:
    attrs = {f"{W}type": "paragraph", f"{W}styleId": style_id}
    if custom:
        attrs[f"{W}customStyle"] = "1"
    el = etree.Element(_qn("style"), attrs)
    etree.SubElement(el, _qn("name"), {f"{W}val": name})
    etree.SubElement(el, _qn("basedOn"), {f"{W}val": based_on})
    etree.SubElement(el, _qn("next"), {f"{W}val": next_style})
    etree.SubElement(el, _qn("qFormat"))
    if pPr_extra:
        pPr = etree.SubElement(el, _qn("pPr"))
        for child in pPr_extra:
            pPr.append(child)
    if rPr_extra:
        rPr = etree.SubElement(el, _qn("rPr"))
        for child in rPr_extra:
            rPr.append(child)
    return el


def strip_numPr_from_styles(styles_root: etree._Element, style_ids: set[str]) -> int:
    removed = 0
    for s in styles_root.findall(_qn("style")):
        sid = s.get(f"{W}styleId")
        if sid not in style_ids:
            continue
        pPr = s.find(_qn("pPr"))
        if pPr is None:
            continue
        for numPr in list(pPr.findall(_qn("numPr"))):
            pPr.remove(numPr)
            removed += 1
    return removed


def add_pandoc_styles(styles_root: etree._Element) -> list[str]:
    """Ensure pandoc English style names exist; format based on A5 / A / A0."""
    existing_names = set()
    existing_ids = set()
    for s in styles_root.findall(_qn("style")):
        existing_ids.add(s.get(f"{W}styleId"))
        name_el = s.find(_qn("name"))
        if name_el is not None:
            existing_names.add(name_el.get(f"{W}val"))

    added: list[str] = []

    def maybe_add(sid: str, name: str, **kwargs):
        if name in existing_names or sid in existing_ids:
            return
        styles_root.append(_make_style(sid, name, **kwargs))
        existing_names.add(name)
        existing_ids.add(sid)
        added.append(f"{sid}/{name}")

    # Body-like
    for sid, name in [
        ("BodyText", "Body Text"),
        ("FirstParagraph", "First Paragraph"),
        ("Compact", "Compact"),
        ("BlockText", "Block Text"),
        ("SourceCode", "Source Code"),
    ]:
        maybe_add(sid, name, based_on="A5", next_style="A5")

    # Captions → based on A0 (image) / A (table); no numPr
    center = etree.Element(_qn("jc"), {f"{W}val": "center"})
    no_indent = etree.Element(
        _qn("ind"),
        {f"{W}left": "0", f"{W}firstLineChars": "0", f"{W}firstLine": "0"},
    )
    maybe_add(
        "ImageCaption",
        "Image Caption",
        based_on="A0",
        next_style="A5",
        pPr_extra=[no_indent, center],
    )
    maybe_add(
        "TableCaption",
        "Table Caption",
        based_on="A",
        next_style="A5",
        pPr_extra=[etree.Element(_qn("jc"), {f"{W}val": "center"})],
    )
    maybe_add(
        "Caption",
        "Caption",
        based_on="A0",
        next_style="A5",
        pPr_extra=[
            etree.Element(
                _qn("ind"),
                {f"{W}left": "0", f"{W}firstLineChars": "0", f"{W}firstLine": "0"},
            ),
            etree.Element(_qn("jc"), {f"{W}val": "center"}),
        ],
    )
    maybe_add(
        "CaptionedFigure",
        "Captioned Figure",
        based_on="A0",
        next_style="A5",
        pPr_extra=[
            etree.Element(
                _qn("ind"),
                {f"{W}left": "0", f"{W}firstLineChars": "0", f"{W}firstLine": "0"},
            ),
            etree.Element(_qn("jc"), {f"{W}val": "center"}),
        ],
    )
    maybe_add(
        "Figure",
        "Figure",
        based_on="A0",
        next_style="A5",
        pPr_extra=[
            etree.Element(
                _qn("ind"),
                {f"{W}left": "0", f"{W}firstLineChars": "0", f"{W}firstLine": "0"},
            ),
            etree.Element(_qn("jc"), {f"{W}val": "center"}),
        ],
    )

    # Date / meta — based on Subtitle look but without outline/numPr
    maybe_add(
        "Date",
        "Date",
        based_on="a1",
        next_style="A5",
        pPr_extra=[etree.Element(_qn("jc"), {f"{W}val": "center"})],
    )
    maybe_add("Author", "Author", based_on="A5", next_style="A5")

    return added


def build_reference_docx(template: Path, reference_out: Path) -> dict:
    """Copy template → reference.docx with pandoc styles + numPr stripped."""
    work = reference_out.parent / "_ref_unpacked"
    if work.exists():
        shutil.rmtree(work)
    work.mkdir(parents=True)

    with zipfile.ZipFile(template, "r") as zf:
        zf.extractall(work)

    styles_path = work / "word" / "styles.xml"
    tree = etree.parse(str(styles_path))
    root = tree.getroot()

    removed = strip_numPr_from_styles(root, STRIP_NUMPR_STYLE_IDS)
    added = add_pandoc_styles(root)
    # Also strip numPr from any newly-added styles that inherited via basedOn
    # (basedOn alone doesn't copy numPr into the style element; A0/A still have theirs)
    # Re-strip A0/A so captions based on them don't pick up numbering when applied as A0/A
    strip_numPr_from_styles(root, {"A0", "A", "a1"})

    tree.write(
        str(styles_path),
        xml_declaration=True,
        encoding="UTF-8",
        standalone=True,
    )

    # Pack as docx
    if reference_out.exists():
        reference_out.unlink()
    with zipfile.ZipFile(reference_out, "w", zipfile.ZIP_DEFLATED) as zf:
        for path in sorted(work.rglob("*")):
            if path.is_file():
                zf.write(path, path.relative_to(work).as_posix())

    return {"removed_numPr": removed, "added_styles": added, "work": str(work)}


def run_pandoc(md: Path, reference: Path, out_docx: Path, resource_path: Path) -> None:
    cmd = [
        "pandoc",
        str(md),
        "-o",
        str(out_docx),
        f"--reference-doc={reference}",
        f"--resource-path={resource_path}",
    ]
    subprocess.run(cmd, check=True)


def remap_paragraph_styles(doc_root: etree._Element) -> dict[str, int]:
    counts: dict[str, int] = {}
    for pPr in doc_root.findall(".//" + _qn("pPr")):
        ps = pPr.find(_qn("pStyle"))
        if ps is None:
            continue
        old = ps.get(f"{W}val")
        if old in STYLE_MAP:
            new = STYLE_MAP[old]
            ps.set(f"{W}val", new)
            counts[f"{old}→{new}"] = counts.get(f"{old}→{new}", 0) + 1
    return counts


def strip_paragraph_numPr(doc_root: etree._Element, style_ids: set[str]) -> int:
    """Remove direct numPr on paragraphs using given styles (belt-and-suspenders)."""
    n = 0
    for p in doc_root.findall(".//" + _qn("p")):
        pPr = p.find(_qn("pPr"))
        if pPr is None:
            continue
        ps = pPr.find(_qn("pStyle"))
        if ps is None:
            continue
        if ps.get(f"{W}val") not in style_ids:
            continue
        for numPr in list(pPr.findall(_qn("numPr"))):
            pPr.remove(numPr)
            n += 1
    return n


def ensure_body_uses_A5(doc_root: etree._Element) -> int:
    """Paragraphs with no pStyle (Normal) → A5 for body text consistency."""
    n = 0
    body = doc_root.find(_qn("body"))
    if body is None:
        return 0
    for p in body.findall(_qn("p")):
        pPr = p.find(_qn("pPr"))
        if pPr is None:
            pPr = etree.Element(_qn("pPr"))
            p.insert(0, pPr)
        ps = pPr.find(_qn("pStyle"))
        if ps is None:
            # Skip empty paragraphs that are only section breaks
            if p.find(_qn("r")) is None and p.find(_qn("sectPr")) is not None:
                continue
            etree.SubElement(pPr, _qn("pStyle"), {f"{W}val": "A5"})
            n += 1
    return n


def copy_headers_footers_and_sectPr(
    template_dir: Path, out_dir: Path, doc_root: etree._Element
) -> bool:
    """If pandoc dropped header/footer refs, restore from template unpack."""
    sect = doc_root.find(".//" + _qn("sectPr"))
    if sect is None:
        return False

    has_header = sect.find(_qn("headerReference")) is not None
    if has_header:
        return False  # already present

    tpl_doc = etree.parse(str(template_dir / "word" / "document.xml"))
    tpl_sect = tpl_doc.find(".//" + _qn("sectPr"))
    if tpl_sect is None:
        return False

    # Copy header/footer part files
    for name in [
        "header1.xml",
        "header2.xml",
        "header3.xml",
        "footer1.xml",
        "footer2.xml",
        "footer3.xml",
    ]:
        src = template_dir / "word" / name
        if src.exists():
            shutil.copy2(src, out_dir / "word" / name)

    # Merge relationships for headers/footers from template
    tpl_rels = etree.parse(str(template_dir / "word" / "_rels" / "document.xml.rels"))
    out_rels_path = out_dir / "word" / "_rels" / "document.xml.rels"
    out_rels = etree.parse(str(out_rels_path))
    out_root = out_rels.getroot()
    existing_targets = {r.get("Target") for r in out_root}
    existing_ids = {r.get("Id") for r in out_root}
    id_map = {}
    for rel in tpl_rels.getroot():
        target = rel.get("Target")
        if not target:
            continue
        if not (
            target.startswith("header")
            or target.startswith("footer")
        ):
            continue
        old_id = rel.get("Id")
        if target in existing_targets:
            # find existing id for this target
            for r in out_root:
                if r.get("Target") == target:
                    id_map[old_id] = r.get("Id")
                    break
            continue
        # allocate new rId
        n = 1
        while f"rId{n}" in existing_ids:
            n += 1
        new_id = f"rId{n}"
        existing_ids.add(new_id)
        id_map[old_id] = new_id
        new_rel = etree.SubElement(
            out_root,
            "{http://schemas.openxmlformats.org/package/2006/relationships}Relationship",
        )
        new_rel.set("Id", new_id)
        new_rel.set("Type", rel.get("Type"))
        new_rel.set("Target", target)

    out_rels.write(
        str(out_rels_path), xml_declaration=True, encoding="UTF-8", standalone=True
    )

    # Replace sectPr content with template's, remapping r:ids
    parent = sect.getparent()
    new_sect = etree.fromstring(etree.tostring(tpl_sect))
    for href in new_sect.findall(_qn("headerReference")) + new_sect.findall(
        _qn("footerReference")
    ):
        old = href.get(f"{R}id")
        if old in id_map:
            href.set(f"{R}id", id_map[old])
    parent.replace(sect, new_sect)

    # Ensure Content_Types has overrides for headers/footers
    ct_path = out_dir / "[Content_Types].xml"
    ct = etree.parse(str(ct_path))
    ct_root = ct.getroot()
    ns = {"ct": "http://schemas.openxmlformats.org/package/2006/content-types"}
    existing_p = {
        o.get("PartName")
        for o in ct_root.findall("{http://schemas.openxmlformats.org/package/2006/content-types}Override")
    }
    for part, ctype in [
        ("/word/header1.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml"),
        ("/word/header2.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml"),
        ("/word/header3.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml"),
        ("/word/footer1.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml"),
        ("/word/footer2.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml"),
        ("/word/footer3.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml"),
    ]:
        if part not in existing_p:
            etree.SubElement(
                ct_root,
                "{http://schemas.openxmlformats.org/package/2006/content-types}Override",
                PartName=part,
                ContentType=ctype,
            )
    ct.write(str(ct_path), xml_declaration=True, encoding="UTF-8", standalone=True)
    return True


def apply_table_borders(doc_root: etree._Element) -> int:
    """Ensure tables have visible grid borders if missing."""
    n = 0
    borders_xml = """
    <w:tblBorders xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
      <w:top w:val="single" w:sz="4" w:space="0" w:color="000000"/>
      <w:left w:val="single" w:sz="4" w:space="0" w:color="000000"/>
      <w:bottom w:val="single" w:sz="4" w:space="0" w:color="000000"/>
      <w:right w:val="single" w:sz="4" w:space="0" w:color="000000"/>
      <w:insideH w:val="single" w:sz="4" w:space="0" w:color="000000"/>
      <w:insideV w:val="single" w:sz="4" w:space="0" w:color="000000"/>
    </w:tblBorders>
    """
    for tbl in doc_root.findall(".//" + _qn("tbl")):
        tblPr = tbl.find(_qn("tblPr"))
        if tblPr is None:
            tblPr = etree.Element(_qn("tblPr"))
            tbl.insert(0, tblPr)
        # Prefer Table Grid style if present
        ts = tblPr.find(_qn("tblStyle"))
        if ts is None:
            etree.SubElement(tblPr, _qn("tblStyle"), {f"{W}val": "a8"})  # may not exist
        borders = tblPr.find(_qn("tblBorders"))
        if borders is None:
            tblPr.append(etree.fromstring(borders_xml))
            n += 1
    return n


def find_table_grid_style_id(styles_root: etree._Element) -> str | None:
    for s in styles_root.findall(_qn("style")):
        if s.get(f"{W}type") != "table":
            continue
        name = s.find(_qn("name"))
        if name is not None and name.get(f"{W}val") in ("Table Grid", "表格网格", "TableGrid"):
            return s.get(f"{W}styleId")
    # fallback: any style with Grid in name
    for s in styles_root.findall(_qn("style")):
        if s.get(f"{W}type") != "table":
            continue
        name = s.find(_qn("name"))
        if name is not None and "Grid" in (name.get(f"{W}val") or ""):
            return s.get(f"{W}styleId")
    return None


def postprocess(
    pandoc_docx: Path,
    template_unpacked: Path,
    final_out: Path,
    styles_from_reference: Path | None = None,
) -> dict:
    work = final_out.parent / "_final_unpacked"
    if work.exists():
        shutil.rmtree(work)
    work.mkdir(parents=True)

    with zipfile.ZipFile(pandoc_docx, "r") as zf:
        zf.extractall(work)

    doc_path = work / "word" / "document.xml"
    styles_path = work / "word" / "styles.xml"
    doc_tree = etree.parse(str(doc_path))
    doc_root = doc_tree.getroot()
    styles_tree = etree.parse(str(styles_path))
    styles_root = styles_tree.getroot()

    # Ensure numPr stripped in final styles too
    removed_styles = strip_numPr_from_styles(styles_root, STRIP_NUMPR_STYLE_IDS)

    remap_counts = remap_paragraph_styles(doc_root)
    stripped_p = strip_paragraph_numPr(doc_root, {"1", "2", "3", "a1", "A0", "A", "af6"})
    # Don't force Normal→A5 for every empty para; only leave as-is.
    # CaptionedFigure already remapped.

    # Date paragraphs: center + clear outline leftovers
    for p in doc_root.findall(".//" + _qn("p")):
        pPr = p.find(_qn("pPr"))
        if pPr is None:
            continue
        ps = pPr.find(_qn("pStyle"))
        if ps is not None and ps.get(f"{W}val") == "a1":
            # remove outlineLvl if present (Subtitle was heading-like)
            for ol in list(pPr.findall(_qn("outlineLvl"))):
                pPr.remove(ol)
            if pPr.find(_qn("jc")) is None:
                etree.SubElement(pPr, _qn("jc"), {f"{W}val": "center"})

    grid_id = find_table_grid_style_id(styles_root)
    border_count = 0
    TBLPR_ORDER = [
        "tblStyle", "tblpPr", "tblOverlap", "bidiVisual", "tblStyleRowBandSize",
        "tblStyleColBandSize", "tblW", "jc", "tblCellSpacing", "tblInd", "tblBorders",
        "shd", "tblLayout", "tblCellMar", "tblLook", "tblCaption", "tblDescription",
        "tblPrChange",
    ]
    order_idx = {n: i for i, n in enumerate(TBLPR_ORDER)}
    borders_xml = (
        '<w:tblBorders xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">'
        '<w:top w:val="single" w:sz="4" w:space="0" w:color="000000"/>'
        '<w:left w:val="single" w:sz="4" w:space="0" w:color="000000"/>'
        '<w:bottom w:val="single" w:sz="4" w:space="0" w:color="000000"/>'
        '<w:right w:val="single" w:sz="4" w:space="0" w:color="000000"/>'
        '<w:insideH w:val="single" w:sz="4" w:space="0" w:color="000000"/>'
        '<w:insideV w:val="single" w:sz="4" w:space="0" w:color="000000"/>'
        "</w:tblBorders>"
    )
    for tbl in doc_root.findall(".//" + _qn("tbl")):
        tblPr = tbl.find(_qn("tblPr"))
        if tblPr is None:
            tblPr = etree.Element(_qn("tblPr"))
            tbl.insert(0, tblPr)
        if grid_id:
            ts = tblPr.find(_qn("tblStyle"))
            if ts is None:
                etree.SubElement(tblPr, _qn("tblStyle"), {f"{W}val": grid_id})
            else:
                ts.set(f"{W}val", grid_id)
        if tblPr.find(_qn("tblBorders")) is None:
            tblPr.append(etree.fromstring(borders_xml))
            border_count += 1
        # Keep OOXML child order valid
        children = list(tblPr)
        children_sorted = sorted(
            children, key=lambda el: order_idx.get(etree.QName(el).localname, 50)
        )
        if [etree.QName(c).localname for c in children] != [
            etree.QName(c).localname for c in children_sorted
        ]:
            for c in list(tblPr):
                tblPr.remove(c)
            for c in children_sorted:
                tblPr.append(c)

    restored_hf = copy_headers_footers_and_sectPr(template_unpacked, work, doc_root)

    # Verify images present
    media = list((work / "word" / "media").glob("*")) if (work / "word" / "media").exists() else []

    doc_tree.write(str(doc_path), xml_declaration=True, encoding="UTF-8", standalone=True)
    styles_tree.write(str(styles_path), xml_declaration=True, encoding="UTF-8", standalone=True)

    if final_out.exists():
        final_out.unlink()
    with zipfile.ZipFile(final_out, "w", zipfile.ZIP_DEFLATED) as zf:
        for path in sorted(work.rglob("*")):
            if path.is_file():
                zf.write(path, path.relative_to(work).as_posix())

    blips = doc_root.findall(
        ".//{http://schemas.openxmlformats.org/drawingml/2006/main}blip"
    )
    omml = doc_root.findall(
        ".//{http://schemas.openxmlformats.org/officeDocument/2006/math}oMath"
    )

    return {
        "remap": remap_counts,
        "removed_style_numPr": removed_styles,
        "stripped_para_numPr": stripped_p,
        "table_borders": border_count,
        "restored_headers_footers": restored_hf,
        "media_files": [m.name for m in media],
        "images": len(blips),
        "equations": len(omml),
        "table_grid_style": grid_id,
    }



def _content_width_dxa(doc_root: etree._Element) -> int:
    """Page content width in twips from sectPr (pgSz.w - left - right). Default A4~9071."""
    sect = doc_root.find(".//" + _qn("sectPr"))
    if sect is None:
        return 9071
    pgSz = sect.find(_qn("pgSz"))
    pgMar = sect.find(_qn("pgMar"))
    if pgSz is None or pgMar is None:
        return 9071
    try:
        page_w = int(pgSz.get(f"{W}w") or 11907)
        left = int(pgMar.get(f"{W}left") or 1440)
        right = int(pgMar.get(f"{W}right") or 1440)
        return max(page_w - left - right, 1)
    except (TypeError, ValueError):
        return 9071


def _ensure_tblPr_child(tblPr: etree._Element, tag: str, attrs: dict) -> etree._Element:
    el = tblPr.find(_qn(tag))
    if el is None:
        el = etree.SubElement(tblPr, _qn(tag), attrs)
    else:
        for k, v in attrs.items():
            el.set(k, v)
    return el


def _reorder_tblPr(tblPr: etree._Element) -> None:
    TBLPR_ORDER = [
        "tblStyle", "tblpPr", "tblOverlap", "bidiVisual", "tblStyleRowBandSize",
        "tblStyleColBandSize", "tblW", "jc", "tblCellSpacing", "tblInd", "tblBorders",
        "shd", "tblLayout", "tblCellMar", "tblLook", "tblCaption", "tblDescription",
        "tblPrChange",
    ]
    order_idx = {n: i for i, n in enumerate(TBLPR_ORDER)}
    children = list(tblPr)
    children_sorted = sorted(
        children, key=lambda el: order_idx.get(etree.QName(el).localname, 50)
    )
    if [etree.QName(c).localname for c in children] != [
        etree.QName(c).localname for c in children_sorted
    ]:
        for c in list(tblPr):
            tblPr.remove(c)
        for c in children_sorted:
            tblPr.append(c)


def style_tables_for_report(docx_path: Path) -> dict:
    """Post-process every table: full-width, header gray D9D9D9, cell text left-aligned.

    Matches the sample table in the Chinese Word template (header fill D9D9D9 with
    optional themeFill background1 / themeFillShade D9). Runs in-place on the docx.
    """
    work = Path(tempfile.mkdtemp(prefix="tblstyle-"))
    stats = {
        "tables": 0,
        "header_cells_shaded": 0,
        "jc_forced_left": 0,
        "widths_set": 0,
        "content_width_dxa": None,
    }
    try:
        with zipfile.ZipFile(docx_path, "r") as zf:
            zf.extractall(work)
        doc_path = work / "word" / "document.xml"
        tree = etree.parse(str(doc_path))
        root = tree.getroot()
        content_w = _content_width_dxa(root)
        stats["content_width_dxa"] = content_w

        HEADER_SHD = {
            f"{W}val": "clear",
            f"{W}color": "auto",
            f"{W}fill": "D9D9D9",
            f"{W}themeFill": "background1",
            f"{W}themeFillShade": "D9",
        }

        for tbl in root.findall(".//" + _qn("tbl")):
            stats["tables"] += 1
            tblPr = tbl.find(_qn("tblPr"))
            if tblPr is None:
                tblPr = etree.Element(_qn("tblPr"))
                tbl.insert(0, tblPr)

            # Full content width (100% of text area)
            _ensure_tblPr_child(tblPr, "tblW", {f"{W}w": "5000", f"{W}type": "pct"})
            # Left align table (not centered)
            _ensure_tblPr_child(tblPr, "jc", {f"{W}val": "left"})
            # Drop floating/indent that can leave right gap
            for tag in ("tblInd", "tblpPr"):
                el = tblPr.find(_qn(tag))
                if el is not None:
                    tblPr.remove(el)
            _ensure_tblPr_child(tblPr, "tblLayout", {f"{W}type": "fixed"})
            _reorder_tblPr(tblPr)
            stats["widths_set"] += 1

            # Equal column widths summing to content width
            grid = tbl.find(_qn("tblGrid"))
            if grid is None:
                grid = etree.Element(_qn("tblGrid"))
                # insert after tblPr
                tbl.insert(1, grid)
            cols = grid.findall(_qn("gridCol"))
            n_cols = len(cols)
            if n_cols == 0:
                # infer from first row
                first_tr = tbl.find(_qn("tr"))
                if first_tr is not None:
                    n_cols = len(first_tr.findall(_qn("tc")))
                    for _ in range(n_cols):
                        etree.SubElement(grid, _qn("gridCol"))
                    cols = grid.findall(_qn("gridCol"))
            if n_cols > 0:
                base = content_w // n_cols
                rem = content_w - base * n_cols
                widths = [base + (1 if i < rem else 0) for i in range(n_cols)]
                for col, w in zip(cols, widths):
                    col.set(f"{W}w", str(w))

            rows = tbl.findall(_qn("tr"))
            for ri, tr in enumerate(rows):
                cells = tr.findall(_qn("tc"))
                for ci, tc in enumerate(cells):
                    tcPr = tc.find(_qn("tcPr"))
                    if tcPr is None:
                        tcPr = etree.Element(_qn("tcPr"))
                        tc.insert(0, tcPr)

                    # Per-cell width: use gridSpan if present
                    span_el = tcPr.find(_qn("gridSpan"))
                    span = int(span_el.get(f"{W}val")) if span_el is not None else 1
                    # Approximate start col by counting previous cells' spans
                    start = 0
                    for prev in cells[:ci]:
                        prev_pr = prev.find(_qn("tcPr"))
                        ps = prev_pr.find(_qn("gridSpan")) if prev_pr is not None else None
                        start += int(ps.get(f"{W}val")) if ps is not None else 1
                    if n_cols > 0 and start < n_cols:
                        cell_w = sum(widths[start : start + span])
                        tw = tcPr.find(_qn("tcW"))
                        if tw is None:
                            etree.SubElement(
                                tcPr, _qn("tcW"), {f"{W}w": str(cell_w), f"{W}type": "dxa"}
                            )
                        else:
                            tw.set(f"{W}w", str(cell_w))
                            tw.set(f"{W}type", "dxa")

                    # Header row shading
                    if ri == 0:
                        shd = tcPr.find(_qn("shd"))
                        if shd is None:
                            etree.SubElement(tcPr, _qn("shd"), HEADER_SHD)
                        else:
                            for k, v in HEADER_SHD.items():
                                shd.set(k, v)
                        stats["header_cells_shaded"] += 1

                    # Force left alignment on every paragraph in the cell
                    for p in tc.findall(".//" + _qn("p")):
                        pPr = p.find(_qn("pPr"))
                        if pPr is None:
                            pPr = etree.Element(_qn("pPr"))
                            p.insert(0, pPr)
                        jc = pPr.find(_qn("jc"))
                        if jc is None:
                            etree.SubElement(pPr, _qn("jc"), {f"{W}val": "left"})
                            stats["jc_forced_left"] += 1
                        elif jc.get(f"{W}val") != "left":
                            jc.set(f"{W}val", "left")
                            stats["jc_forced_left"] += 1

        tree.write(
            str(doc_path), xml_declaration=True, encoding="UTF-8", standalone=True
        )
        tmp = docx_path.with_suffix(".tblstyle.docx")
        with zipfile.ZipFile(tmp, "w", compression=zipfile.ZIP_DEFLATED) as zf:
            for f in sorted(work.rglob("*")):
                if f.is_file():
                    zf.write(f, f.relative_to(work).as_posix())
        tmp.replace(docx_path)
        return stats
    finally:
        shutil.rmtree(work, ignore_errors=True)


def convert(md: Path, template: Path, outdir: Path) -> dict:
    outdir.mkdir(parents=True, exist_ok=True)
    reference = outdir / "reference.docx"
    pandoc_out = outdir / "out.docx"
    final_out = outdir / "final.docx"

    ref_info = build_reference_docx(template, reference)
    resource_path = md.parent
    run_pandoc(md, reference, pandoc_out, resource_path)

    template_unpacked = Path(ref_info["work"])
    # Prefer original unpacked-tpl if present alongside template
    sibling = template.parent / "unpacked-tpl"
    if sibling.exists():
        template_unpacked = sibling

    post_info = postprocess(pandoc_out, template_unpacked, final_out)
    omml_info = fix_omml_for_word(final_out)
    # Table styling after OMML fix, before LO roundtrip
    table_style_pre = style_tables_for_report(final_out)

    word_safe = outdir / "final-word.docx"
    pdf_out = outdir / "final.pdf"
    lo_info = {}
    table_style_post = None
    try:
        lo_info = libreoffice_word_safe_roundtrip(final_out, word_safe, pdf_out)
        # LO may strip header shade / jc / widths — re-apply, then rebuild PDF
        if word_safe.exists():
            table_style_post = style_tables_for_report(word_safe)
            # Re-export PDF from restyled Word-safe doc so shading is visible
            if pdf_out is not None:
                try:
                    work_pdf = Path(tempfile.mkdtemp(prefix="lo-pdf-"))
                    src = work_pdf / "styled.docx"
                    shutil.copy2(word_safe, src)
                    try:
                        subprocess.run(
                            ["office", "soffice", "convert", str(src), "pdf",
                             "--outdir", str(work_pdf)],
                            check=True, capture_output=True, text=True,
                        )
                    except (FileNotFoundError, subprocess.CalledProcessError):
                        subprocess.run(
                            ["soffice", "--headless", "--convert-to", "pdf",
                             "--outdir", str(work_pdf), str(src)],
                            check=True, capture_output=True, text=True,
                        )
                    produced = list(work_pdf.glob("*.pdf"))
                    if produced:
                        shutil.copy2(produced[0], pdf_out)
                        lo_info["pdf_restyled"] = True
                    shutil.rmtree(work_pdf, ignore_errors=True)
                except Exception as pe:
                    lo_info["pdf_restyle_error"] = str(pe)
    except Exception as e:
        lo_info = {"error": str(e)}

    return {
        "reference": str(reference),
        "pandoc_out": str(pandoc_out),
        "final": str(final_out),
        "final_word": str(word_safe) if word_safe.exists() else None,
        "final_pdf": str(pdf_out) if pdf_out.exists() else None,
        "omml_fix": omml_info,
        "lo_roundtrip": lo_info,
        "ref_info": ref_info,
        "post_info": post_info,
        "table_style_pre_lo": table_style_pre,
        "table_style_post_lo": table_style_post,
    }


def fix_omml_for_word(docx_path: Path) -> dict:
    """Make pandoc OMML Word-openable.

    Fixes observed to cause Microsoft Word "cannot open":
    1) m:dPr child order must be begChr, sepChr, endChr, grow, … (not endChr before sepChr)
    2) empty m:sepChr m:val="" — remove when val is missing/empty (no separator)
    3) ensure VerbatimChar character style exists
    4) drop unused empty word/comments.xml part
    """
    import zipfile, tempfile, shutil
    from lxml import etree
    M = "{http://schemas.openxmlformats.org/officeDocument/2006/math}"
    W = "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}"
    DPR_ORDER = ["begChr", "sepChr", "endChr", "grow", "shp", "objDist", "rSpRule", "rSp", "ctrlPr"]
    work = Path(tempfile.mkdtemp(prefix="ommlfix-"))
    stats = {"dpr_reordered": 0, "empty_sepchr_removed": 0, "verbatim_added": False, "comments_stripped": False}
    try:
        with zipfile.ZipFile(docx_path) as z:
            z.extractall(work)
        doc_path = work / "word" / "document.xml"
        tree = etree.parse(str(doc_path))
        root = tree.getroot()
        for dPr in root.findall(f".//{M}dPr"):
            # Drop empty separators (Word often rejects m:sepChr m:val="")
            for sep in list(dPr.findall(f"{M}sepChr")):
                val = sep.get(f"{M}val")
                if val is None or val == "":
                    dPr.remove(sep)
                    stats["empty_sepchr_removed"] += 1
            kids = list(dPr)
            by, extras = {}, []
            for c in kids:
                name = etree.QName(c).localname
                if name in DPR_ORDER and name not in by:
                    by[name] = c
                else:
                    extras.append(c)
            new_order = [by[n] for n in DPR_ORDER if n in by] + extras
            if [etree.QName(c).localname for c in kids] != [etree.QName(c).localname for c in new_order]:
                for c in kids:
                    dPr.remove(c)
                for c in new_order:
                    dPr.append(c)
                stats["dpr_reordered"] += 1
            # Normalize grow default
            for grow in dPr.findall(f"{M}grow"):
                if grow.get(f"{M}val") is None:
                    grow.set(f"{M}val", "on")
        styles_path = work / "word" / "styles.xml"
        stroot = etree.parse(str(styles_path))
        defined = {s.get(W + "styleId") for s in stroot.getroot().findall(f"{W}style")}
        if "VerbatimChar" not in defined:
            el = etree.Element(f"{W}style", {f"{W}type": "character", f"{W}customStyle": "1", f"{W}styleId": "VerbatimChar"})
            etree.SubElement(el, f"{W}name", {f"{W}val": "Verbatim Char"})
            etree.SubElement(el, f"{W}basedOn", {f"{W}val": "a6" if "a6" in defined else "DefaultParagraphFont"})
            rPr = etree.SubElement(el, f"{W}rPr")
            etree.SubElement(rPr, f"{W}rFonts", {f"{W}ascii": "Consolas", f"{W}hAnsi": "Consolas", f"{W}cs": "Consolas"})
            stroot.getroot().append(el)
            styles_path.write_bytes(etree.tostring(stroot, xml_declaration=True, encoding="UTF-8", standalone=True))
            stats["verbatim_added"] = True
        # Strip unused empty comments part (template leftover)
        comments = work / "word" / "comments.xml"
        has_cref = bool(root.findall(f".//{W}commentReference") or root.findall(f".//{W}commentRangeStart"))
        if comments.exists() and not has_cref:
            comments.unlink()
            ct_path = work / "[Content_Types].xml"
            ct = etree.parse(str(ct_path))
            for ov in ct.getroot().xpath('//*[@PartName="/word/comments.xml"]'):
                ov.getparent().remove(ov)
            ct_path.write_bytes(etree.tostring(ct, xml_declaration=True, encoding="UTF-8", standalone=True))
            rels_path = work / "word" / "_rels" / "document.xml.rels"
            if rels_path.exists():
                rels = etree.parse(str(rels_path))
                for rel in list(rels.getroot()):
                    if rel.get("Target") == "comments.xml":
                        rels.getroot().remove(rel)
                rels_path.write_bytes(etree.tostring(rels, xml_declaration=True, encoding="UTF-8", standalone=True))
            stats["comments_stripped"] = True
        doc_path.write_bytes(etree.tostring(root, xml_declaration=True, encoding="UTF-8", standalone=True))
        tmp = docx_path.with_suffix(".ommlfix.docx")
        with zipfile.ZipFile(tmp, "w", compression=zipfile.ZIP_DEFLATED) as z:
            for f in work.rglob("*"):
                if f.is_file():
                    z.write(f, f.relative_to(work).as_posix())
        tmp.replace(docx_path)
        return stats
    finally:
        shutil.rmtree(work, ignore_errors=True)


def fix_omml_dpr_order(docx_path: Path) -> int:
    """Backward-compatible wrapper; prefer fix_omml_for_word."""
    return int(fix_omml_for_word(docx_path).get("dpr_reordered", 0))


def libreoffice_word_safe_roundtrip(docx_path: Path, out_docx: Path | None = None, out_pdf: Path | None = None) -> dict:
    """DOCX → ODT → DOCX via LibreOffice; optional PDF. Most reliable Word-open path."""
    import subprocess, tempfile, shutil
    out_docx = out_docx or docx_path.with_name(docx_path.stem + "-word.docx")
    work = Path(tempfile.mkdtemp(prefix="lo-roundtrip-"))
    result = {"docx": None, "pdf": None, "odt": None}
    try:
        src = work / "in.docx"
        shutil.copy2(docx_path, src)
        # office soffice convert when available; else raw soffice
        def _convert(src_file: Path, fmt: str) -> Path:
            outdir = src_file.parent
            try:
                subprocess.run(
                    ["office", "soffice", "convert", str(src_file), fmt, "--outdir", str(outdir)],
                    check=True, capture_output=True, text=True,
                )
            except (FileNotFoundError, subprocess.CalledProcessError):
                subprocess.run(
                    ["soffice", "--headless", "--convert-to", fmt, "--outdir", str(outdir), str(src_file)],
                    check=True, capture_output=True, text=True,
                )
            produced = list(outdir.glob(src_file.stem + ".*"))
            for p in produced:
                if p.suffix.lower() == ("." + fmt.split(":")[0].lower()):
                    return p
            raise FileNotFoundError(f"LO did not produce {fmt} from {src_file}")

        odt = _convert(src, "odt")
        result["odt"] = str(odt)
        docx2 = _convert(odt, "docx")
        out_docx.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(docx2, out_docx)
        result["docx"] = str(out_docx)
        if out_pdf is not None:
            # Prefer converting the Word-safe docx for pagination fidelity
            pdf_src = work / "forpdf.docx"
            shutil.copy2(out_docx, pdf_src)
            pdf = _convert(pdf_src, "pdf")
            out_pdf.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(pdf, out_pdf)
            result["pdf"] = str(out_pdf)
        return result
    finally:
        shutil.rmtree(work, ignore_errors=True)


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("md", type=Path)
    ap.add_argument("template", type=Path)
    ap.add_argument("outdir", type=Path)
    args = ap.parse_args()
    result = convert(args.md, args.template, args.outdir)
    import json

    print(json.dumps(result, ensure_ascii=False, indent=2, default=str))


if __name__ == "__main__":
    main()
