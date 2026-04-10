# EntityPositionJ2 数据结构

J2 摄动轨道位置对象,含 `$type` 判别字段(值为 `J2`)。

## 使用场景

## 字段说明


| 字段名                      | 类型       | 必须  | 默认值                    | 说明                                                        |
| ------------------------ | -------- | --- | ---------------------- | --------------------------------------------------------- |
| `$type`                  | string   | 是   | `J2`                   | 类型标识符,必须为 `J2`                                            |
| `CentralBody`            | string   | 否   | `Earth`                | 中心天体名称                                                    |
| `GravitationalParameter` | number   | 否   | `3.9860044150E+14`     | 引力常数 (m³/s²);为 0 时按 `CentralBody` 自动取值                    |
| `J2NormalizedValue`      | number   | 否   | `0.000484165143790815` | J2 归一化值(引力场 C20 的负值);地球 EGM2008 示例:`0.000484165143790815` |
| `RefDistance`            | number   | 否   | `6378136.3`            | 参考椭球半长轴 (m);地球 EGM2008 示例:`6378136.3`                     |
| `OrbitEpoch`             | string   | 是   | -                      | 轨道历元 (UTCG),格式:`yyyy-MM-ddTHH:mm:ssZ`                     |
| `CoordSystem`            | string   | 否   | `Inertial`             | 轨道坐标系                                                     |
| `CoordType`              | string   | 否   | `Classical`            | 轨道类型:`Classical` 或 `Cartesian`                            |
| `OrbitalElements`        | number[] | 是   | -                      | 轨道根数,含义由 `CoordType` 决定                                   |


## CoordType 与 OrbitalElements


| `CoordType` | `OrbitalElements` 内容              | 单位                       |
| ----------- | --------------------------------- | ------------------------ |
| `Classical` | [半长径, 偏心率, 倾角, 近点角距, 升交点经度, 真近点角] | m, -, deg, deg, deg, deg |
| `Cartesian` | [X, Y, Z, Vx, Vy, Vz]             | m, m/s                   |


## JSON 示例

### 使用默认 J2 参数

```json
{
  "$type": "J2",
  "OrbitEpoch": "2022-09-05T04:00:00Z",  
  "OrbitalElements": [7678140, 0, 58.5, 0, 0, 0]
}
```

### 使用全部参数

```json
{
  "$type": "J2",
  "CentralBody": "Earth",
  "OrbitEpoch": "2022-09-05T04:00:00Z",
  "CoordSystem": "Inertial",
  "CoordType": "Classical",
  "OrbitalElements": [7678140, 0, 58.5, 0, 0, 0],
  "J2NormalizedValue": 0.000484165143790815,
  "RefDistance": 6378136.3
}
```

## 注意事项

- `$type` 必须为 `J2`,否则无法作为该多态分支正确反序列化。
- `J2NormalizedValue` 为 C20 的负值;`GravitationalParameter` 为 0 时内部由中心天体名称解析。

