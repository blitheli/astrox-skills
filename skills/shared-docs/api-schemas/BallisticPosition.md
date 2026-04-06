# BallisticPosition (IEntityPosition2)

弹道传播器位置对象，用于计算再入/弹道轨迹。

## 继承关系
- 实现：`IEntityPosition2`
- `$type`: `Ballistic`

## 字段说明

| 字段名 | 类型 | 必须 | 默认值 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| `$type` | string | 是 | `Ballistic` | 类型标识符 |
| `Start` | string | 是 | - | 发射时刻 (UTCG)，格式：`yyyy-MM-ddTHH:mm:ssZ` |
| `CentralBody` | string | 否 | `Earth` | 中心天体名称 |
| `GravitationalParameter` | number | 否 | `398600441500000` | 中心天体引力常数 (m³/s²) |
| `LaunchLatitude` | number | 否 | `0` | 发射点大地纬度 (deg) |
| `LaunchLongitude` | number | 否 | `0` | 发射点地理经度 (deg) |
| `LaunchAltitude` | number | 否 | `0` | 发射点高度 (m) |
| `BallisticType` | string | 否 | `DeltaV` | 弹道类型 |
| `BallisticTypeValue` | number | 否 | `6901.943` | 弹道类型对应的数值 |
| `ImpactLatitude` | number | 否 | `20` | 关机点大地纬度 (deg) |
| `ImpactLongitude` | number | 否 | `20` | 关机点地理经度 (deg) |
| `ImpactAltitude` | number | 否 | `0` | 关机点高度 (m) |

## BallisticType 说明

| 值 | BallisticTypeValue 含义 | 单位 |
| :--- | :--- | :--- |
| `DeltaV` | 关机增量 | m/s |
| `DeltaV_MinEcc` | 偏心率最小化的关机增量 | m/s |
| `ApogeeAlt` | 远地点高度 | m |
| `TimeOfFlight` | 飞行时间 | s |

## JSON 示例

```json
{
  "$type": "Ballistic",
  "Start": "2022-06-27T04:00:00Z",
  "CentralBody": "Earth",
  "LaunchLatitude": 0,
  "LaunchLongitude": 0,
  "LaunchAltitude": 0,
  "BallisticType": "DeltaV",
  "BallisticTypeValue": 6901.943,
  "ImpactLatitude": 20,
  "ImpactLongitude": 20,
  "ImpactAltitude": 0
}
```

## 注意事项

- 适用于再入弹道、导弹弹道等
- 需要指定发射点和关机点
- 支持多种弹道条件约束
