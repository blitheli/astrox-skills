# J2Position (IEntityPosition2)

J2 摄动轨道传播器位置对象，用于计算考虑一阶带谐项（J2）的卫星轨道。

## 继承关系
- 实现：`IEntityPosition2`
- `$type`: `J2`

## 字段说明

| 字段名 | 类型 | 必须 | 默认值 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| `$type` | string | 是 | `J2` | 类型标识符 |
| `CentralBody` | string | 否 | `Earth` | 中心天体名称 |
| `GravitationalParameter` | number | 否 | `0` | 引力常数 (m³/s²)<br>如果为 0，则根据 CentralBody 自动获取<br>地球 EGM2008: 3.9860044150E+14<br>火星 MRO110C: 0.4282837564100E+14 |
| `J2NormalizedValue` | number | 是 | - | J2项归一化数值（引力场文件中 C20 的负值）<br>地球 EGM2008: 0.000484165143790815 |
| `RefDistance` | number | 是 | - | 参考椭球体半长轴 (m)<br>地球 EGM2008: 6378136.3 |
| `OrbitEpoch` | string | 是 | - | 轨道历元 (UTCG)，格式：`yyyy-MM-ddTHH:mm:ssZ` |
| `CoordSystem` | string | 否 | `Inertial` | 轨道坐标系（默认：`Inertial`） |
| `CoordType` | string | 否 | `Classical` | 轨道类型 |
| `OrbitalElements` | number[] | 是 | - | 轨道根数，具体内容由 CoordType 决定 |

## CoordType 说明

| 值 | OrbitalElements 内容 | 单位 |
| :--- | :--- | :--- |
| `Classical` | [半长径, 偏心率, 轨道倾角, 近点角距, 升交点经度, 真近点角] | m, -, deg, deg, deg, deg |
| `Cartesian` | [X, Y, Z, Vx, Vy, Vz] | m, m/s |

## JSON 示例

### Classical 根数

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

### 使用默认 J2 参数

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

- 单位：半长径为米，角度为度
- `J2NormalizedValue` 是 C20 的负值
- `GravitationalParameter` 为 0 时自动从中心天体获取
