---
name: cesium-astrox
description: ASTROX 扩展版 Cesium.js (Cesium-Astrox) 可视化用法。当用户询问 ASTROX 定制/扩展的 Cesium、与原生 Cesium 行为差异、或基于 Cesium-Astrox 的场景展示时使用;不适用于仅讨论原版 Cesium 的一般问题。
---

# ASTROX 扩展 Cesium (Cesium-Astrox)

本技能覆盖 **ASTROX 对 Cesium.js 的扩展与定制**。其 API、约定与运行时行为可能与官方/原版 Cesium 不同,不可直接套用通用 Cesium 文档或社区示例。

## 核心指令 (Core Instructions)

1. **优先本技能资料**:回答或编写代码时,优先查阅并复用本目录下 `examples/` 中的示例与说明;不要默认采用原版 Cesium 的通用写法。
2. **区分扩展与原版**:仅当用户明确只讨论原版 Cesium、且与 ASTROX 扩展无关时,才可参考通用 Cesium 文档;涉及 ASTROX 场景、定制 API 或已有扩展模式时,以本技能为准。
3. **示例待补充**:当前为脚手架。收到用户提供的示例后,按专题拆分为独立子文档或放入 `examples/`(必要时再补充 fixtures 形态的最小可运行片段)。在此之前,不要臆造 ASTROX Cesium API 或示例代码。

## 目录结构 (可扩展)

```text
skills/cesium-astrox/
  SKILL.md          # 本说明
  examples/         # 示例片段、演示入口 (优先引用)
  notes/            # 补充说明、差异备注、专题笔记
```

- `examples/`:放置可复用的代码片段与演示说明 (见该目录 `README.md`)。
- `notes/`:放置与扩展行为、坐标系、与 Web API 输出对接等相关的笔记。

## 注意事项

- ASTROX Cesium 与 vanilla Cesium 在 API/行为上可能不一致;不确定时先查 `examples/` 与 `notes/`,再向用户确认。
- 使用英文标点 `:,()`。
- 示例待补充;收到用户示例后按专题拆分为独立子文档或 fixtures。

## 标准执行流程

1. 确认问题是否涉及 ASTROX 扩展 Cesium (而非纯原版 Cesium)。
2. 检索本技能 `examples/`、`notes/` 是否已有对口专题。
3. 有示例则按示例写法作答;无示例则说明资料尚未入库,避免编造 API。
4. 用户补充示例后,按专题归档到 `examples/` 或子文档并更新本技能说明。
