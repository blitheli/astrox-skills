# skills/markdown-word

遇到「按模板把 Markdown 转成 docx」,或用户提到 Word 模板与中文样式 A正文 / 一级标题 / A表 / A图片时,读本目录 [SKILL.md](SKILL.md),并调用:

```bash
python3 skills/markdown-word/md_to_docx_with_template.py <report.md> skills/markdown-word/template.docx <outdir>
```

这不是 ASTROX Web API 技能,不要发 HTTP 请求,也不要找 fixtures。

交付优先输出目录中的 `final-word.docx`(LibreOffice roundtrip)。没有 `soffice` 时交付 `final.docx`。Word 打不开时先看 OMML 空 `m:sepChr`。`<outdir>` 不要用本目录,也不要把中间产物提交进仓。

同一套材料在 `blitheli/STK-Scenario` 的 `demo/docx/` 有 demo 副本;在本仓库以本目录为准。
