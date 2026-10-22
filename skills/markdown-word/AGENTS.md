# skills/markdown-word

遇到「按模板把 Markdown 转成 docx」,或用户提到 Word 模板与中文样式 A正文 / 一级标题 / A表 / A图片时,读本目录 [SKILL.md](SKILL.md),并调用:

```bash
python3 skills/markdown-word/md_to_docx_with_template.py <report.md> skills/markdown-word/template.docx <outdir>
```

这不是 ASTROX Web API 技能,不要发 HTTP 请求,也不要找 fixtures。

交付优先输出目录中的 `final-word.docx`(LibreOffice roundtrip)。没有 `soffice` 时交付 `final.docx`。交给用户时的文件名按具体任务而定,不要写死报告名。Word 打不开时先看 OMML 空 `m:sepChr`。`<outdir>` 不要用本目录,也不要把中间产物提交进仓。

排版已锁定,后处理在 roundtrip 前后都要保持:

1. 一级标题顶格,数字后恰好两个 ASCII 空格。
2. 正文样式 `A正文`(小四)。首行两个汉字(`firstLineChars=200`,`firstLine=480`)。不要在 run 上写显式 `w:sz`(例如五号 21)。
3. 编号不用 `numPr`,续行顶格。
4. 表格表头居中并铺 `D9D9D9`,表体居左,单元格 `vAlign=center`,行高 `atLeast` 480 twips,段前段后各 40 twips,`tblW` 为 `pct`/`5000`。
5. 图题 / 表题用模板样式 `A图片` / `A表`。
6. 优先交付 `final-word.docx`。文件名按任务定。

同一套材料在 `blitheli/STK-Scenario` 的 `demo/docx/` 有 demo 副本;在本仓库以本目录为准。
