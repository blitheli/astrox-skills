---
name: Markdown 转 Word（模板）
description: >-
  Use when converting Markdown to .docx with a user-provided Word template
  (reference-doc), especially Chinese styles like A正文 / 一级标题 / A表 / A图片.
  A图片 / A表 keep Word auto-numbering (图1 / 表1). Use when pandoc alone does not
  match the template.
---
# Markdown → Word（按用户 docx 模板）

把 Markdown 报告转成 Word，并套用用户给的 `.docx` / `.dotx` 样式（页边距、页眉页脚、正文与标题样式），而不是默认 pandoc 样式。

## 何时用

- 用户给了 Word 模板，要求「按这个模板转 docx」
- 中文技术报告：正文两端对齐、首行缩进、自定义「A正文 / A表 / A图片」等样式
- 需要保留公式（OMML）、表格、本地插图

## 前置

- `pandoc`、`python3`、`lxml`；可选 `office` CLI（`/opt/sand/sand-host/officekit-ts/bin`）做 validate/render
- LibreOffice（`soffice` / `office soffice`）用于 Word 安全 roundtrip 与 PDF
- 可复用脚本：技能目录 `md_to_docx_with_template.py`（参数：md、template.docx、outdir）

## 步骤

1. **拷贝模板**到工作目录，查看 `word/styles.xml`：记下正文、Heading 1/2/3、表题、图题、Title 的 `styleId`。
2. **做 pandoc reference-doc**：补充 `Body Text`、`First Paragraph`、`Compact`、`Block Text`、`Source Code`、`Table Caption`、`Image Caption`、`Caption` 等，`basedOn` 指到模板正文。若模板标题带 `numPr` 而 Markdown 已自带编号，只删掉标题 1/2/3 上的 `numPr`。技能实现必须保留「A图片」「A表」的 Word 自动编号（`lvlText` 为 `图%1` / `表%1`）。这两个样式在模板里的 styleId 通常是 `A0` / `A`，LibreOffice 之后会变成别的 id。禁止把 `A0`、`A` 放进 `STRIP_NUMPR_STYLE_IDS`；`build_reference_docx` 和 `postprocess` 都不得剥掉它们的 `numPr`。
3. **转换**：
   ```bash
   pandoc report.md -o out.docx \
     --reference-doc=reference.docx \
     --resource-path=<含插图的目录>
   ```
4. **后处理**：映射 `BodyText`/`FirstParagraph`/`Compact`→正文；`TableCaption`→表题；`ImageCaption`→图题。必要时从模板拷回 `sectPr` 与页眉页脚。
4b. **表格后处理**（`style_tables_for_report`）：每个表表头第一行底纹 `D9D9D9`（可带 themeFill）、段落 `jc=center`、单元格 `vAlign=center`；表体行段落 `jc=left`；**行高 `atLeast` 480 twips（24 pt）**；单元格段落段前段后各 **40 twips（2 pt）**；清除单元格首行缩进（避免继承 A正文）；去掉单元格 `w:t` 首尾空白/NBSP；`tblW` 为 `pct`/`5000`；等分列宽。OMML 修复后与 LibreOffice roundtrip 后再各执行一次。
4c. **正文首行缩进**（`ensure_body_first_line_indent`）：正文两字缩进 — `w:firstLineChars="200"` 且 `w:firstLine="480"`（twips，≈2×12pt；模板原 `firstLine=200` 且 LO 会丢掉 `firstLineChars`，看起来像一字）。对 Normal/A5 样式与正文段落在 roundtrip 前后各补一次。
4d. **正文样式**：段落用模板 `A正文`（小四）。**不要**在 run 上再写显式 `w:sz`（例如五号 21），否则会盖过样式。
5. **必修（Word 兼容 — OMML）**：pandoc OMML 常导致 **Microsoft Word 直接打不开**（LibreOffice 仍可能能开）。脚本 `fix_omml_for_word` 会：
   - 把 `m:dPr` 子元素重排为 `begChr, sepChr, endChr, grow, …`（pandoc 常写成 `begChr, endChr, sepChr, grow`）
   - **删除空的** `m:sepChr m:val=""`（无分隔符时不应保留空 sepChr；这是 dPr 顺序修好后仍打不开的常见原因）
   - 补上缺失的 `VerbatimChar` 字符样式；去掉未引用的空 `comments.xml`
   - 然后用 `office validate` 确认 schema OK
6. **必修（Word 安全交付）**：在 OMML 修复后做 **LibreOffice roundtrip**：
   ```bash
   office soffice convert fixed.docx odt --outdir lo/
   office soffice convert lo/fixed.odt docx --outdir lo/
   office soffice convert lo/fixed.docx pdf --outdir lo/
   ```
   或调用脚本里的 `libreoffice_word_safe_roundtrip`。`convert()` 会写出 `final.docx`（模板样式 + OMML 修复）、`final-word.docx`（LO roundtrip，优先给 Word 用户）、`final.pdf`。
6b. **题注自动编号（LO 之后再修一次）**：`convert()` 在 roundtrip 前调用 `fix_caption_numbering`，roundtrip 以及表格 / 列表 demote / 正文缩进 / 一级标题修复之后再调用一次（与报告构建里 LO 后补跑题注后处理一致）。这一步要：
   - 保证「A图片」「A表」样式带 `numPr`，对应 `lvlText` 为 `图%1` / `表%1`
   - 把 `numbering.xml` 里该 abstractNum 的 `pStyle` 改成**当前**「A图片」「A表」的 styleId（LO 常把 A0/A 改成 A3/A4，旧 `pStyle` 会让表号失效）
   - 题注段落使用这两个样式；正文只有标题。Markdown 图用 `![说明](路径)`，表用 `Table: 标题`。不要把手写的「图1」「表1」写进 `w:t`
   - 「A图片2」（图所在段落）设 `numId=0`，禁止继承图编号
   - 编号列表 demote 必须跳过 A图片 / A表 / A图片2 段落，也不得改写 `图%1` / `表%1` 的 abstractNum
7. **检查**：`unzip -t`、`office validate`、`office render` 或 soffice→PDF 抽查。不要用「能转 PDF」当成 Word 已兼容。交付用的 `final.docx` 和 `final-word.docx` 都要核对题注自动编号（见下文）。


## 已锁定的中文报告排版（勿回退）

后处理（LO roundtrip 前后都要再跑）必须保持：

1. **一级标题**：顶格（left/firstLine=0）；数字与标题之间恰好 **2 个 ASCII 空格**（如 `1  摘要`）。pandoc 会压成 1 个空格，靠 `fix_heading1_number_spacing` 恢复。
2. **正文**：样式 `A正文`，小四；首行 `firstLineChars=200`、`firstLine=480`（约两个汉字），`left=0`。禁止 run 级显式字号覆盖样式。
3. **编号列表**：列表不要用 Word `numPr`（会叠缩进）。MD 可用 `1\.` 写成普通段落后，按正文同样首行缩进；**续行顶格**（left=0）。这只针对列表。A图片 / A表 的题注编号要保留，demote 不得把它们收成静态「图N」「表N」。
4. **表格**（现行中文弹道报告交付一致）：
   - 表头：文字水平居中 + 灰底 D9D9D9
   - 表体：左对齐
   - **全部单元格垂直居中** `vAlign=center`
   - **行高至少 24 pt**（`trHeight` `atLeast` 480 twips）
   - 单元格段落段前段后各 **2 pt**（40 twips），使垂直居中可见且行高不过大
   - `tblW` pct 5000；去掉单元格误继承的首行缩进与首尾空白
5. **图题 / 表题**：用模板样式 `A图片` / `A表` 的 Word 自动编号（`图%1` / `表%1`），文案只写标题，不写「图1」「表1」。图所在段落用 `A图片2`，且 `numId=0`。图题字号可比正文小一号。轴标签用中文。LO roundtrip 之后必须再把 numbering 的 `pStyle` 对齐当前 styleId。
6. **交付**：优先 `final-word.docx`（或 LO roundtrip 后的主 docx）。**交付文件名按具体任务而定**，不要在本技能里写死报告名。

## 验证题注自动编号

用一张表加一张图做冒烟（不要在 Markdown 里写死「图1」「表1」）：

```markdown
Table: 发射参数

| 项目 | 数值 |
| --- | --- |
| 高度 | 200 km |

![地面轨迹](dot.png)
```

```bash
python3 skills/markdown-word/md_to_docx_with_template.py sample.md skills/markdown-word/template.docx /tmp/mdw-out
python3 - << 'PY'
import sys
sys.path.insert(0, "skills/markdown-word")
from caption_numbering import verify_caption_numbering
for name in ("final.docx", "final-word.docx"):
    errs = verify_caption_numbering(f"/tmp/mdw-out/{name}")
    print(name, "PASS" if not errs else errs)
    if errs:
        sys.exit(1)
PY
```

`verify_caption_numbering` 为空才算通过。它检查：A图片 / A表 带 `numPr`，lvlText 为 `图%1` / `表%1`，abstractNum 的 `pStyle` 等于这两个样式当前的 styleId，题注 `w:t` 没有手写「图N」「表N」，A图片2 的 `numId` 为 `0`。也可以 `unzip` 后直接看 `word/styles.xml` 与 `word/numbering.xml`。`final-word.docx` 上 styleId 可以不再是 A0/A，但必须和 `pStyle` 一致。

## 注意

- LibreOffice 往往能打开「Word 打不开」的文件；**以 LO roundtrip 后的 `*-word.docx` 作为给 Word 用户的主交付**。
- 模板样式优先保留在 `final.docx`；若 Word 仍拒收，交付 LO roundtrip（样式可能被 LO 轻微改写，但可打开）。
- 不要改 Markdown 正文，除非只为修正相对图片路径。
- 若公式仍有问题：可退回 pandoc `--mathml`，或把公式改成纯 Unicode/图片后再转 docx（最后手段）。
