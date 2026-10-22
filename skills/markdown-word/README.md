# Markdown 转 Word (按模板)

用法速查。触发条件、样式对照与失败判定见 [SKILL.md](SKILL.md)。

## 给 Cursor Agent

遇到「按模板把 Markdown 转成 docx」时,先读 [SKILL.md](SKILL.md),再调用本目录脚本,不要另写一套转换。

- 脚本: `skills/markdown-word/md_to_docx_with_template.py`
- 随附模板: `skills/markdown-word/template.docx`(样式含 A正文、heading 1 / 一级标题、A表、A图片)
- 事先做好的 pandoc reference-doc: `skills/markdown-word/reference.docx`(日常转换仍由脚本按当前模板重新生成到输出目录)

`blitheli/STK-Scenario` 的 `demo/docx/` 有一份 demo 副本。在本仓库中使用上面的路径。

```bash
python3 skills/markdown-word/md_to_docx_with_template.py <report.md> skills/markdown-word/template.docx <outdir>
```

交付优先使用输出目录里的 `final-word.docx`(LibreOffice roundtrip)。没有 `soffice` 时至少有 `final.docx`。交给用户时的文件名按具体任务而定,不要写死报告名。Word 打不开时,先查公式里的空 `m:sepChr`。`<outdir>` 不要用本目录,也不要把其中的中间产物提交进仓。

## 已锁定排版

与脚本后处理一致,roundtrip 前后都要保持。细节见 [SKILL.md](SKILL.md)。

1. 一级标题顶格,数字后恰好两个 ASCII 空格,例如 `1  摘要`。
2. 正文样式 `A正文`(小四),首行两个汉字:`firstLineChars=200`,`firstLine=480`,`left=0`。不要在 run 上写显式 `w:sz`(例如五号 21)。
3. 编号不用 Word `numPr`。编号行按正文首行缩进,续行顶格(`left=0`)。
4. 表格表头居中并铺 `D9D9D9` 底纹,表体左对齐,单元格 `vAlign=center`,行高 `atLeast` 480 twips(24 pt),单元格段前段后各 40 twips。
5. 图题 / 表题用模板样式 `A图片` / `A表`。
6. 交给用户的文件优先是 `final-word.docx`。文件名按任务定。

## 依赖

- `pandoc`(不要传 `--toc=false`;pandoc 3.1 会因此在转换一开始失败)
- `python3`
- `lxml`
- LibreOffice: `soffice`,或 `office soffice`

## 输出

| 文件 | 说明 |
| :--- | :--- |
| `reference.docx` | 由本次模板生成的 pandoc reference-doc |
| `out.docx` | pandoc 原始输出 |
| `final.docx` | 样式映射、页眉页脚、OMML 修复,以及表格、编号、正文缩进、一级标题后处理 |
| `final-word.docx` | LibreOffice DOCX→ODT→DOCX,并再次套用锁定排版。给 Word 用户时优先交这个 |
| `final.pdf` | roundtrip 并再次套用锁定排版后导出的 PDF(有 LibreOffice 时) |

表格约定:表头第一行底纹 `D9D9D9` 且段落居中,表体段落左对齐,全部单元格 `vAlign=center`,行高 `atLeast` 480 twips,单元格段前段后各 40 twips,表宽 `tblW` 为 `pct` / `5000`,列宽按版心等分。单元格上的首行缩进会被清掉。
