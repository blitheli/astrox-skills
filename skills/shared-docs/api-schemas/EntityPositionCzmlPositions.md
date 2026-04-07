# EntityPositionCzmlPositions

由 `CzmlPositions` 数组给出的全程星历对象(多段Czml Position字段)，含 `$type` 判别字段（值为 `CzmlPositions`）。

## 使用场景

- 需要多段 CZML 位置数据、且请求体引用本 schema 的接口。
- 顶层多态位置对象使用 `IEntityPosition2` 时，写法与本文档一致。

## 字段说明


| 字段名             | 类型                     | 必须  | 默认值             | 说明                               |
| --------------- | ---------------------- | --- | --------------- | -------------------------------- |
| `$type`         | string                 | 是   | `CzmlPositions` | 类型标识符，必须为 `CzmlPositions`        |
| `CentralBody`   | string                 | 否   | `Earth`         | 中心天体                             |
| `CzmlPositions` | [EntityPositionCzml][] | 是   | -               | CZML 位置数据段数组, 注意, 不包含 `$type` 字段 |


## JSON 示例

```json
{
  "$type": "CzmlPositions",
  "CentralBody": "Earth",
  "CzmlPositions": [
    {
      "CentralBody": "Earth",
      "interpolationAlgorithm": "LAGRANGE",
      "interpolationDegree": 7,
      "referenceFrame": "INERTIAL",
      "epoch": "2025-06-06T04:00:00Z",
      "interval": "2025-06-06T04:00:00Z/2025-06-06T04:10:00Z",
      "cartesianVelocity": [0, 6678137, 0, 0, 0, 6789.53...]
    },
    {
      "CentralBody": "Earth",
      "interpolationAlgorithm": "LAGRANGE",
      "interpolationDegree": 7,
      "referenceFrame": "INERTIAL",
      "epoch": "2025-06-06T04:10:00Z",
      "interval": "2025-06-06T04:10:00Z/2025-06-06T04:20:00Z",
      "cartesianVelocity": [0, 6678137, 0, 0, 0, 6789.53...]
    }
  ]
}
```

## 注意事项

- `$type` 必须为 `CzmlPositions`，否则无法作为该多态分支正确反序列化。
- EntityPositionCzml数组不包含 `$type` 字段

