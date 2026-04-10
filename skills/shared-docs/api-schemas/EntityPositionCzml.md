# EntityPositionCzml 数据结构

Cesium/CZML 风格的单段位置对象,含 `**$type` 判别字段(值为 `CzmlPosition`)**。

编写请求时以具体接口要求的 `$ref` 为准,字段按下表填写。

## 字段说明


| 字段名                      | 类型       | 必须   | 默认值            | 说明                                  |
| ------------------------ | -------- | ---- | -------------- | ----------------------------------- |
| `$type`                  | string   | 是    | `CzmlPosition` | 类型标识符,必须为 `CzmlPosition`            |
| `CentralBody`            | string   | 否    | `Earth`        | 中心天体                                |
| `interpolationAlgorithm` | string   | 否    | `LAGRANGE`     | `LINEAR`、`LAGRANGE`、`HERMITE`       |
| `interpolationDegree`    | integer  | 否    | `7`            | 插值阶数                                |
| `referenceFrame`         | string   | 否    | `FIXED`        | `FIXED`、`INERTIAL`、`J2000`、`ICRF` 等 |
| `epoch`                  | string   | 是    | -              | 历元 (UTCG): `yyyy-MM-ddTHH:mm:ssZ`   |
| `interval`               | string   | null | 否              | `null`                              |
| `cartesian`              | number[] | null | 否              | -                                   |
| `cartesianVelocity`      | number[] | null | 否              | -                                   |


## JSON 示例

```json
{
  "$type": "CzmlPosition",
  "CentralBody": "Earth",
  "interpolationAlgorithm": "LAGRANGE",
  "interpolationDegree": 7,
  "referenceFrame": "INERTIAL",
  "epoch": "2025-06-06T04:00:00Z",
  "interval": null,
  "cartesianVelocity": [
    0.0, 6678137.0, 0.0, 0.0, 0.0, 6789.5303,
    60.0, 6662055.5482, 407044.7716, 221007.2787, -535.8331, 6773.1806
  ]
}
```

## 注意事项

- `$type` 必须为 `CzmlPosition`,否则无法作为该多态分支正确反序列化。

