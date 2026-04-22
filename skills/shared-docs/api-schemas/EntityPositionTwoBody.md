# EntityPositionTwoBody

二体轨道位置对象,含 `$type` 判别字段(值为 `TwoBody`)。

## 字段说明


| 字段名                      | 类型       | 必须  | 默认值               | 说明                                 |
| ------------------------ | -------- | --- | ----------------- | ---------------------------------- |
| `$type`                  | string   | 是   | `TwoBody`         | 类型标识符,必须为 `TwoBody`                |
| `CentralBody`            | string   | 否   | `Earth`           | 中心天体名称                             |
| GravitationalParameter  | number | 否    | `398600441500000`    | 中心天体引力常数 (m³/s²)；为 0 时内部根据 CentralBody 自动取值（地球典型值：3.986004415E14） |
| `OrbitEpoch`             | string   | 是   | -                 | 轨道历元 (UTCG):`yyyy-MM-ddTHH:mm:ssZ` |
| `CoordSystem`            | string   | 否   | `Inertial`        | 轨道坐标系                              |
| `CoordType`              | string   | 否   | `Classical`       | `Classical` 或 `Cartesian`          |
| `OrbitalElements`        | number[] | 是   | -                 | 根数含义由 `CoordType` 决定               |


## CoordType 与 OrbitalElements


| `CoordType` | 内容                  | 单位                       |
| ----------- | ------------------- | ------------------------ |
| `Classical` | 半长径, e, i, ω, Ω, ν  | m, -, deg, deg, deg, deg |
| `Cartesian` | X, Y, Z, Vx, Vy, Vz | m, m/s                   |


## JSON 示例

### Classical 根数

```json
{
  "$type": "TwoBody",
  "CentralBody": "Earth",
  "OrbitEpoch": "2022-09-05T04:00:00Z",
  "CoordSystem": "Inertial",
  "CoordType": "Classical",
  "OrbitalElements": [7678140, 0, 58.5, 0, 0, 0]
}
```

### Cartesian 坐标

```json
{
  "$type": "TwoBody",
  "CentralBody": "Earth",
  "OrbitEpoch": "2022-09-05T04:00:00Z",
  "CoordSystem": "Inertial",
  "CoordType": "Cartesian",
  "OrbitalElements": [7678140, 0, 0, 0, 6000, 4000]
}
```

## 注意事项

- 单位:位置为米,速度为米/秒,角度为度
- `GravitationalParameter` 默认值为地球标准值
- Kepler轨道根数定义: (半长径(m),偏心率,轨道倾角(deg),近点角距(deg),升交点经度(deg),真近点角(deg))

