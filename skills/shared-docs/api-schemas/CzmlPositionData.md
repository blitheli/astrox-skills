# CzmlPositionData

CZML 单段位置数据，用于 `CzmlPositions` 类型。

## 字段说明

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `CentralBody` | string | 中心天体（`Earth`、`Moon` 等） |
| `interpolationAlgorithm` | string | 插值算法（通常为 `LAGRANGE`） |
| `interpolationDegree` | integer | 插值阶数（通常为 7） |
| `referenceFrame` | string | 参考坐标系（通常为 `INERTIAL`） |
| `epoch` | string | 历元时刻 (UTCG)，格式：`yyyy-MM-ddTHH:mm:ssZ` |
| `interval` | string | 有效时间段，格式：`start/stop` |
| `cartesianVelocity` | number[] | 位置速度数据，平铺格式：<br>每个点 7 个值：[时间偏移(s), X(m), Y(m), Z(m), Vx(m/s), Vy(m/s), Vz(m/s)] |

## JSON 示例

```json
{
  "CentralBody": "Earth",
  "interpolationAlgorithm": "LAGRANGE",
  "interpolationDegree": 7,
  "referenceFrame": "INERTIAL",
  "epoch": "2025-06-06T04:00:00Z",
  "interval": "2025-06-06T04:00:00Z/2025-06-06T04:10:00Z",
  "cartesianVelocity": [
    0.0,     // 时间偏移 0s
    6678137.0,  // X (m)
    0.0,        // Y (m)
    0.0,        // Z (m)
    0.0,        // Vx (m/s)
    0.0,        // Vy (m/s)
    6789.5303,  // Vz (m/s)
    60.0,    // 时间偏移 60s
    6662055.5482,  // X (m)
    407044.7716,  // Y (m)
    221007.2787,  // Z (m)
    -535.8331,    // Vx (m/s)
    6773.1806,    // Vy (m/s)
    3677.5370,    // Vz (m/s)
    ...
  ]
}
```

## 数据格式说明

`cartesianVelocity` 数组中，每 7 个数字构成一个时间-位置-速度点：
1. 时间偏移（相对于 epoch 的秒数）
2-4. X, Y, Z 位置坐标 (m)
5-7. Vx, Vy, Vz 速度分量 (m/s)
