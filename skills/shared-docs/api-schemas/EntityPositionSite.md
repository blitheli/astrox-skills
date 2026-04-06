# EntityPositionSite 数据结构

地面站/固定点位置对象，含 `$type` 判别字段（值为 `SitePosition`）。

## 字段说明


| 字段名                   | 类型       | 必须  | 默认值            | 说明                                                             |
| --------------------- | -------- | --- | -------------- | -------------------------------------------------------------- |
| `$type`               | string   | 是   | `SitePosition` | 类型标识符，必须为 `SitePosition`                                       |
| `CentralBody`         | string   | 否   | `Earth`        | 中心天体（`Earth`、`Moon` 等）                                         |
| `cartographicDegrees` | number[] | 是   | `[100, 30, 0]` | 椭球大地坐标 [经度(deg), 纬度(deg), 高度(m)]；地球为 WGS84，月球为半径 1737.4 km 的圆球 |
| `clampToGround`       | boolean  | 否   | `false`        | 是否贴地表；为 `true` 时，`cartographicDegrees` 中的高度按实际地形/椭球面强制         |
| `HeightAboveGround`   | number   | 否   | `0`            | 相对地面高度 (m)                                                     |


## JSON 示例

### 地球地面站

```json
{
  "$type": "SitePosition",
  "CentralBody": "Earth",
  "cartographicDegrees": [-122.18936, 46.19557, 0],
  "clampToGround": false
}
```

### 月面站

```json
{
  "$type": "SitePosition",
  "CentralBody": "Moon",
  "cartographicDegrees": [102.91745, 35.911758, -2345.2],
  "clampToGround": true
}
```

## 相关文档

- 与本文档等价叙述（`IEntityPosition2` 视角）：[SitePosition.md](SitePosition.md)

## 注意事项

- `$type` 必须为 `SitePosition`，否则无法作为该多态分支正确反序列化。
- 经度为地理经度（东经为正），纬度为大地纬度。
- 坐标单位为：度 (deg)、米 (m)
- 月球使用 1737.4km 半径的圆球体

