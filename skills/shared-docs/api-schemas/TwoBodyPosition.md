# TwoBodyPosition (IEntityPosition2)

二体轨道传播器位置对象，用于计算二体问题下的卫星轨道。

## 继承关系
- 实现：`IEntityPosition2`
- `$type`: `TwoBody`

## 字段说明

| 字段名 | 类型 | 必须 | 默认值 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| `$type` | string | 是 | `TwoBody` | 类型标识符 |
| `CentralBody` | string | 否 | `Earth` | 中心天体名称 |
| `GravitationalParameter` | number | 否 | `398600441500000` | 引力常数 (m³/s²)<br>如果为 0，则根据 CentralBody 自动获取 |
| `OrbitEpoch` | string | 是 | - | 轨道历元 (UTCG)，格式：`yyyy-MM-ddTHH:mm:ssZ` |
| `CoordSystem` | string | 否 | `Inertial` | 轨道坐标系（默认：`Inertial`） |
| `CoordType` | string | 否 | `Classical` | 轨道类型：`Classical` 或 `Cartesian` |
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

- 单位：位置为米，速度为米/秒，角度为度
- `GravitationalParameter` 默认值为地球标准值
