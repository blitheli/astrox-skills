# TerrainMaskConfig 数据结构

地形遮罩计算参数(`AzimuthElevationMaskInput.TerrainMaskPara`)。若为 `null`/省略,服务端使用 `appsettings.json` 缺省配置。

## 字段说明


| 字段名                | 类型     | 必须  | 缺省值  | 说明                                                                                          |
| ------------------ | ------ | --- | ---- | ------------------------------------------------------------------------------------------- |
| `Text`             | string | 否   | —    | 地形遮罩说明                                                                                      |
| `TerrainServerUrl` | string | 否   | —    | 地形服务完整路径(stkTerrainServer,到 `layer.json` 之前)                                               |
| `FlagPole`         | int    | 否   | `0`  | 地形投影类型(`0`:4326,`1`:南极,`-1`:北极);暂时无效                                                       |
| `PolarDemFileName` | string | 否   | —    | 两极地形文件名;非空时优先于 `TerrainServerUrl`(实际引用 `appsettings.json` 中 `DemFiles`);典型:`Moon_LDEM_80s_20m` |
| `TerrainZoomLevel` | int    | 否   | `-1` | 地形最大级别(`-1` 表示自动);月球两极 tif 数据时此参数无效                                                          |
| `StepSize`         | number | 否   | `30` | 某方向计算步长(m);内部按中心天体半径折算为地心弧度                                                                 |
| `MaxSearchRange`   | number | 否   | `15` | 某方向最大搜索距离(km);内部按中心天体半径折算为地心弧度                                                               |


## JSON 示例

库内/文档示意(仅 URL 与步长):

```json
{
  "TerrainServerUrl": "http://astrox.cn:8765/AstroxTerrain/v1/tilesets/Moon_V14_new/tiles/",
  "StepSize": 30,
  "MaxSearchRange": 15
}
```

经当前 Web API 提交时,若包含本对象,需同时给出非 null 的 `TerrainServerUrl` 与 `PolarDemFileName`(模型非空字符串校验)。`PolarDemFileName` 非空则走极区 DEM 并忽略 URL;空字符串可能运行期失败。中低纬场景常直接省略整个 `TerrainMaskPara`。

## 注意事项

- 目前地形数据支持:地球、月球、火星、月球南极。
- 月球南极场景:`/Terrain/AzElMask` 使用 `.tif`;`/Terrain/AzElMaskSimple` 使用 `.lbl`(以实现为准)。
- 全球一般连接 StkTerrainServer 类服务;月球南北极一般使用极区 DEM(`PolarDemFileName`)。
