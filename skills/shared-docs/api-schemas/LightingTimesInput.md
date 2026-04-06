# LightingTimesInput

光照时间计算输入参数结构。

## 字段说明

| 字段名 | 类型 | 必须 | 默认值 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| `Description` | string | 否 | - | 说明 |
| `Start` | string | 是 | - | 分析开始时刻 (UTCG)，格式：`yyyy-MM-ddTHH:mm:ssZ` |
| `Stop` | string | 是 | - | 分析结束时刻 (UTCG)，格式：`yyyy-MM-ddTHH:mm:ssZ` |
| `Position` | [IEntityPosition2](IEntityPosition2.md) | 是 | - | 位置对象（地面站或卫星等） |
| `AzElMaskData` | number[] | 否 | null | 地形遮罩数据（仅地面站使用），格式：(方位角1，高度角1，方位角2，高度角2...)，单位：rad |
| `OccultationBodies` | string[] | 否 | null | 遮挡天体列表，第1个为中心天体；默认：地月中心天体为地月，其他为自身中心天体 |

## JSON 示例

```json
{
  "Description": "地面站光照计算",
  "Start": "2022-09-05T04:00:00Z",
  "Stop": "2022-09-07T04:00:00Z",
  "Position": {
    "$type": "SitePosition",
    "CentralBody": "Earth",
    "cartographicDegrees": [-122.18936, 46.19557, 0],
    "clampToGround": false
  }
}
```
