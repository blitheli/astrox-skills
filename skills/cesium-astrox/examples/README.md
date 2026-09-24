# examples

ASTROX 扩展 Cesium (Cesium-Astrox) 的可复用写法以 `../SKILL.md` 为准,优先于通用 Cesium 文档。

已入库:

- 多天体场景: `Cesium.SolarSystem`、`addViewer`、`addPlanetary`、`CzmlDataSource` / VGT、分辨率、回退 Viewer
- 地球或月球影像与地形: `UrlTemplateImageryProvider`、`configAddImageLayer`、`CesiumTerrainProvider`、`configAddTerrainProvider`
- 轨迹 Entity: `Cesium.CzmlPosition`、`createEntityFromCzmlPosition`、`centralBody`、`CzmlDataSource.processPositionProperty` / `processPositionPacketData`、`path.groundTracks`、`entity.orbits` / `OrbitsGraphics`、`TwoBodyPropagator.toCzmlPosition(...).getCzmlPostions()`
- 多段 path 按时间上色: `examples/path-segment-colors.js`(`TimeIntervalCollectionProperty`、`ColorMaterialProperty`、`getMultipleSegmentColors`;只改 `entity.path`)

其他专题仍待按文件拆到本目录。没有写进 `SKILL.md` 的 API 不要臆造。
