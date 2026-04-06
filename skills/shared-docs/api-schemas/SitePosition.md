# SitePosition (IEntityPosition2)

地面站位置对象，用于定义固定地理位置（如地面站、月面站）。

## 继承关系
- 实现：`IEntityPosition2`
- `$type`: `SitePosition`

## 字段说明

| 字段名 | 类型 | 必须 | 默认值 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| `$type` | string | 是 | `SitePosition` | 类型标识符 |
| `CentralBody` | string | 否 | `Earth` | 中心天体（`Earth`、`Moon` 等） |
| `cartographicDegrees` | number[] | 是 | - | 椭球体的大地坐标 [经度(deg), 纬度(deg), 高度(m)]<br>地球：WGS84椭球体<br>月球：1737.4km的圆球体 |
| `clampToGround` | boolean | 否 | `false` | 是否贴地表（有地形时贴地形表面；无地形时贴参考椭球体表面）<br>为 true 时，`cartographicDegrees` 中的 Height 强制为实际地形表面对应的高度 |
| `HeightAboveGround` | number | 否 | `0` | 相对地面高度 (m) |

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

## 注意事项

- 经度为地理经度（东度）
- 纬度为大地纬度
- 坐标单位为：度 (deg)、米 (m)
- 月球使用 1737.4km 半径的圆球体
