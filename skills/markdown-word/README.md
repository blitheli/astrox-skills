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

交付优先使用输出目录里的 `final-word.docx`(LibreOffice roundtrip)。没有 `soffice` 时至少有 `final.docx`。Word 打不开时,先查公式里的空 `m:sepChr`。`<outdir>` 不要用本目录,也不要把其中的中间产物提交进仓。

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
| `final.docx` | 样式映射、页眉页脚、OMML 修复、表格后处理 |
| `final-word.docx` | LibreOffice DOCX→ODT→DOCX。给 Word 用户时优先交这个 |
| `final.pdf` | 由 roundtrip 后再套一次表格样式导出的 PDF(有 LibreOffice 时) |

表格约定:表头第一行底纹 `D9D9D9`,表宽 `tblW` 为 `pct` / `5000`,单元格段落左对齐,列宽按版心等分。
