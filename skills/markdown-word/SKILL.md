---
name: markdown-word
description: 用户给出 Word 模板(.docx 或 .dotx),要把 Markdown 报告转成套用该模板样式的 docx 时使用。典型触发包括「按这个模板转 docx」、中文技术报告样式 A正文 / 一级标题 / A表 / A图片,以及 pandoc 默认样式对不上模板、公式 OMML 导致 Microsoft Word 打不开。调用 skills/markdown-word/md_to_docx_with_template.py。本技能不是 ASTROX Web API 技能,没有 HTTP 端点,也没有 fixtures。
---

# Markdown 转 Word (按用户模板)

把 Markdown 报告转成 Word,并套用用户给出的 `.docx` / `.dotx` 样式(页边距、页眉页脚、正文与标题),而不是 pandoc 默认样式。面向中文技术报告:正文两端对齐、首行缩进,以及模板中的「A正文 / 一级标题 / A表 / A图片」。公式走 OMML,并尽量保留表格和本地插图。

本技能是本地转换工具。不要为它构造 HTTP 请求,也不要找 `fixtures/`。同一套脚本与模板在 `blitheli/STK-Scenario` 的 `demo/docx/` 另有一份 demo 副本;在本仓库中以 `skills/markdown-word/` 为入口,不要把 STK-Scenario 的路径写成唯一来源。

## 核心指令 (Core Instructions)

1. **输入解析**:必填 Markdown 路径与 Word 模板。未另给模板时使用本目录 `template.docx`。输出目录由调用方指定,且必须位于本技能目录之外。
2. **不要另写转换**:读完本文后调用 `skills/markdown-word/md_to_docx_with_template.py`,不要再手写一套 pandoc 命令作为主路径。
3. **交付优先级**:有 LibreOffice 时交付 `<outdir>/final-word.docx`。没有 `soffice` 时至少交付 `<outdir>/final.docx`,并说明未做 roundtrip。
4. **不要改 Markdown 正文**,除非只为修正相对图片路径。
5. **不要提交** `<outdir>`、`__pycache__` 或中间 docx。

## 已锁定的中文报告排版 (勿回退)

后处理在 LibreOffice roundtrip 前后都要再跑,必须保持下面五条。改脚本时以这五条为准。

1. **一级标题顶格,数字后两个空格**。`left` 与 `firstLine` 都是 0。数字和标题文字之间恰好 2 个 ASCII 空格,例如 `1  摘要`。pandoc 会把 Markdown 里的连续空格压成 1 个,由 `fix_heading1_number_spacing` 恢复。LibreOffice 可能把样式改成 `Heading1` 并重新缩进,所以 roundtrip 之后必须再执行一次。
2. **正文首行两个汉字**。`w:firstLineChars="200"` 且 `w:firstLine="480"`(twips,约 2×12pt),`left=0`。模板里 Normal 的 `firstLine` 只有 200,LibreOffice 又会丢掉 `firstLineChars`,看起来会变成只缩进一个字。`ensure_body_first_line_indent` 对 Normal / A正文(`A5`)样式和正文段落在 roundtrip 前后各补一次。
3. **编号不用 `numPr`,续行顶格**。`demote_numbered_lists_to_body` 去掉自动编号,把 `N. ` 写进普通正文段落,首行缩进与正文相同,`left=0`,换行回到左边界。Markdown 写成 `1\.` 时直接是普通段落,同样按正文首行缩进。roundtrip 后若 LibreOffice 写回 `numPr`,再降级一次。
4. **表格表头居中,表体居左,垂直居中**。`style_tables_for_report`:表头第一行底纹 `D9D9D9`(可带 `themeFill` / `themeFillShade`),表头段落 `jc=center`;表体段落 `jc=left`;每个单元格 `vAlign=center`。清掉单元格上从 A正文继承来的首行缩进。`tblW` 为 `pct` / `5000`,列宽按版心等分。OMML 修复之后和 roundtrip 之后各执行一次。
5. **优先交付 `final-word.docx`**。有 LibreOffice 时把 `<outdir>/final-word.docx` 交给用户。`final.docx` 保留模板样式并已做 OMML 修复;Word 打不开时仍以 roundtrip 后的 `final-word.docx` 为主文件。

## 依赖

本技能不调用 ASTROX Web API。

| 依赖 | 必须 | 说明 |
| :--- | :--- | :--- |
| `pandoc` | 是 | Markdown 转 docx,并使用 `--reference-doc`。pandoc 3.1 不接受 `--toc=false`,脚本不传 `--toc`(默认无目录)。 |
| `python3` | 是 | 运行 `md_to_docx_with_template.py`。 |
| `lxml` | 是 | 读写 OOXML。可用 `python3-lxml` 或 `pip install lxml`。 |
| LibreOffice (`soffice`) | 否 | DOCX→ODT→DOCX roundtrip 与 PDF。脚本会先尝试 `office soffice`(例如 `/opt/sand/sand-host/officekit-ts/bin` 里的 `office`),失败再调用 `soffice --headless`。没有它时仍写出 `final.docx`,没有 `final-word.docx` 与 `final.pdf`。 |

## 目录

```text
skills/markdown-word/
  SKILL.md                         # 本说明
  README.md                        # 用法速查
  AGENTS.md                        # 给 Cursor Agent 的触发说明
  md_to_docx_with_template.py      # 转换脚本
  template.docx                    # 随技能附带的中文报告模板
  reference.docx                   # 事先做好的 pandoc reference-doc,供对照
```

日常转换仍由脚本按**当前传入的模板**在输出目录重新生成 `reference.docx`。仓库里的 `reference.docx` 只作对照,不要把技能目录当作 `<outdir>`,否则会覆盖它。

## 模板样式

随附 `template.docx` 里与报告相关的段落样式如下。Word 中文界面里的「一级标题」对应该模板的 `heading 1`(styleId `1`)。

| 称呼 | OOXML `w:name` | styleId | 用途 |
| :--- | :--- | :--- | :--- |
| A正文 | A正文 | `A5` | 正文 |
| 一级标题 | heading 1 | `1` | Markdown `#` |
| 二级标题 | heading 2 | `2` | Markdown `##` |
| 三级标题 | heading 3 | `3` | Markdown `###` |
| A表 | A表 | `A` | 表题 |
| A图片 | A图片 | `A0` | 图题 |
| Title | Title | `af6` | 文档标题 |

脚本中的 `STYLE_MAP` 把 pandoc 英文样式映射回上述 styleId:

| pandoc styleId | 模板 styleId |
| :--- | :--- |
| `BodyText`, `FirstParagraph`, `Compact`, `BlockText`, `SourceCode` | `A5` (A正文) |
| `TableCaption` | `A` (A表) |
| `ImageCaption`, `Caption`, `CaptionedFigure`, `Figure` | `A0` (A图片) |
| `Heading1`, `Heading2`, `Heading3` | `1`, `2`, `3` |
| `Title`, `AbstractTitle` | `af6` |

换用 styleId 不同的用户模板时,先查看该模板 `word/styles.xml`,再改脚本里的 `STYLE_MAP` 与 `basedOn`。生成 reference-doc 时会去掉标题、表题、图题上的 `numPr`,避免与 Markdown 里已经写好的编号重复。

## 调用

在仓库根目录:

```bash
python3 skills/markdown-word/md_to_docx_with_template.py <report.md> skills/markdown-word/template.docx <outdir>
```

参数顺序: Markdown、模板、输出目录。插图使用相对 Markdown 文件的路径;pandoc `--resource-path` 取该 Markdown 所在目录。用户另给模板时,把第二个参数换成用户的 `.docx`(`.dotx` 若同样是 OOXML zip 包,也可以作为模板路径传入)。

## 输出

脚本向 stdout 打印 JSON,包含路径以及 `omml_fix`、`lo_roundtrip`、`table_style_pre_lo`、`table_style_post_lo`、`demote_lists_pre_lo`、`body_indent_pre`、`body_indent_post_lo`、`h1_space_pre`、`h1_space_post_lo`。

| 文件 | 说明 |
| :--- | :--- |
| `<outdir>/reference.docx` | 由本次模板生成的 pandoc reference-doc(补了英文样式名,并去掉标题自动编号) |
| `<outdir>/out.docx` | pandoc 原始输出 |
| `<outdir>/final.docx` | 样式回映射、页眉页脚与 `sectPr`、OMML 修复,以及表格、编号、正文缩进、一级标题后处理 |
| `<outdir>/final-word.docx` | LibreOffice DOCX→ODT→DOCX,并再次套用锁定排版。**给 Word 用户时优先交这个** |
| `<outdir>/final.pdf` | roundtrip 并再次套用锁定排版后导出的 PDF(有 LibreOffice 时) |
| `<outdir>/_ref_unpacked/` | 解包后的模板工作目录,属于中间产物 |

## 脚本做了什么

1. 拷贝模板,在 `word/styles.xml` 补上 pandoc 需要的英文样式名(`Body Text`、`First Paragraph`、`Compact`、`Block Text`、`Source Code`、`Table Caption`、`Image Caption`、`Caption` 等),`basedOn` 指向模板正文或题注。
2. 调用:

```bash
pandoc report.md -o out.docx \
  --reference-doc=<outdir>/reference.docx \
  --resource-path=<含插图的目录>
```

3. 后处理:按 `STYLE_MAP` 映射段落样式;必要时从模板拷回 `sectPr` 与页眉页脚。
4. **OMML(Word 兼容)**:`fix_omml_for_word` 会:
   - 把 `m:dPr` 子元素重排为 `begChr, sepChr, endChr, grow, …`(pandoc 常写成 `begChr, endChr, sepChr, grow`)
   - 删除空的 `m:sepChr`(无 `m:val` 或 `m:val=""`)。无分隔符时不应保留空 sepChr
   - 补上缺失的 `VerbatimChar` 字符样式
   - 去掉未引用的空 `comments.xml`
5. **表格**:`style_tables_for_report` 在 OMML 修复之后执行,roundtrip 后再执行一次。表头第一行底纹 `D9D9D9`(可带 `themeFill` / `themeFillShade`),表头段落 `jc=center`,表体段落 `jc=left`,全部单元格 `vAlign=center`,并清掉单元格首行缩进。`tblW` 为 `pct` / `5000`(拉满版心),列宽按版心等分。
6. **编号**:`demote_numbered_lists_to_body` 去掉段落 `numPr`,把编号写成正文里的 `N. `。首行缩进两个汉字,续行 `left=0`。roundtrip 后再执行一次。
7. **正文首行**:`ensure_body_first_line_indent` 在去掉自动编号之后执行。Normal / A正文(`A5`)与正文段落使用 `firstLineChars=200`、`firstLine=480`、`left=0`。roundtrip 后再补一次。
8. **一级标题**:`fix_heading1_number_spacing` 把一级标题缩进清零,并把数字后的空白收成恰好两个 ASCII 空格。roundtrip 之后必须再执行。
9. **LibreOffice roundtrip** (`libreoffice_word_safe_roundtrip`):DOCX→ODT→DOCX,写出 `final-word.docx`。上面的表格、编号、正文缩进和一级标题在 roundtrip 之后再跑一遍,然后导出 `final.pdf`。

## 注意事项

- LibreOffice 往往能打开 Microsoft Word 打不开的文件。能转成 PDF 不能当成 Word 已兼容。
- `final.docx` 保留模板 styleId(例如一级标题 `1`、图题 `A0`)。有 LibreOffice 时交给用户的仍是 `final-word.docx`。LibreOffice 可能改写 styleId,但样式显示名往往还在:试跑中 `A5` 仍是 A正文,图题由 styleId `A0` 变成 `A3` 且 `w:name` 仍为 A图片,标题 `1` / `af6` 变成 `Heading1` / `Title`。
- Word 打不开时先查公式里的空 `m:sepChr`,对照 stdout JSON 的 `omml_fix.empty_sepchr_removed`。
- 公式仍有问题时,可退回 pandoc `--mathml`,或把公式改成纯 Unicode / 图片后再转。
- `<outdir>` 不要用本技能目录,也不要把其中的中间产物提交进仓。

## 标准执行流程

1. 参数预检:确认 Markdown 与模板文件存在;插图路径相对 Markdown;输出目录不在 `skills/markdown-word/` 内。
2. 依赖预检: `pandoc`、`python3`、`lxml` 必须可用;探测 `soffice` 或 `office`。
3. 请求构造:按上面的三条参数调用脚本。不要追加 `--toc=false`。
4. 结果判定:进程退出码为 0,且 `<outdir>/final.docx` 存在。有 `soffice` 时还应存在 `final-word.docx`。可用 `unzip -t` 检查 docx 包。
5. 输出归一化:把 `final-word.docx` 交给用户。只有 `final.docx` 时一并说明缺少 LibreOffice roundtrip。

## 调用示例 (最小可运行)

```bash
python3 skills/markdown-word/md_to_docx_with_template.py \
  path/to/report.md \
  skills/markdown-word/template.docx \
  /tmp/report-docx
```

成功时 stdout JSON 中 `final` 指向 `/tmp/report-docx/final.docx`;若本机有 `soffice`,`final_word` 指向 `/tmp/report-docx/final-word.docx`。

## 相关副本

`blitheli/STK-Scenario` 仓库的 `demo/docx/` 保留同一套 demo(脚本、`template.docx`、`reference.docx`)。本技能目录是 ASTROX-Skills 中供 Cursor agent 发现的副本,两边互为对照,修改时不要假设只有其中一处存在。
