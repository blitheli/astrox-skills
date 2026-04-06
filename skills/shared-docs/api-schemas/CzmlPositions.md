# CzmlPositions (IEntityPosition2)

CZML 格式的位置数据对象，用于提供离散的时间-位置序列。

## 继承关系
- 实现：`IEntityPosition2`
- `$type`: `CzmlPositions`

## 字段说明

| 字段名 | 类型 | 必须 | 默认值 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| `$type` | string | 是 | `CzmlPositions` | 类型标识符 |
| `CentralBody` | string | 否 | `Earth` | 中心天体（`Earth`、`Moon` 等） |
| `CzmlPositions` | [CzmlPositionData](CzmlPositionData.md)[] | 是 | - | CZML 格式的位置数据数组 |

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
      "cartesianVelocity": [
        0.0, 6678137.0, 0.0, 0.0, 0.0, 6789.5303,
        60.0, 6662055.5482, 407044.7716, 221007.2787, -535.8331, 6773.1806,
        ...
      ]
    }
  ]
}
```

## 注意事项

- 适用于已有位置数据（如轨道计算结果）
- 支持 Lagrange 插值算法
- 坐标系通常为惯性系 (INERTIAL)
