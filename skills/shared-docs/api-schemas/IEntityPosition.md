# IEntityPosition

对象位置基类，用于定义各类位置对象（地面站、卫星等）。通过 `$type` 字段区分具体类型。

## 支持的类型


| `$type` 值       | 类型名称       | 说明              | 参考文档                                                             |
| --------------- | ---------- | --------------- | ---------------------------------------------------------------- |
| `SitePosition`  | 地面站        | 固定点位置（如地面站、月面站） | [EntityPositionSite.md](EntityPositionSite.md)                   |
| `J2`            | J2摄动       | J2摄动轨道传播器       | [EntityPositionJ2.md](EntityPositionJ2.md)                       |
| `TwoBody`       | 二体问题       | 二体轨道传播器         | [EntityPositionTwoBody.md](EntityPositionTwoBody.md)             |
| `CzmlPositions` | CZML位置     | CZML格式的位置数据     | [EntityPositionCzmlPositions.md](EntityPositionCzmlPositions.md) |
| `SGP4`          | SGP4       | SGP4/TLE卫星      | [EntityPositionSGP4.md](EntityPositionSGP4.md)                   |
| `Ballistic`     | 弹道         | 弹道传播器           | [EntityPositionBallistic.md](EntityPositionBallistic.md)         |
| `CzmlPosition`  | CZML位置(单段) | 单段CZML位置数据      | [EntityPositionCzml.md](EntityPositionCzml.md)                   |
| `CentralBody`   | 中心天体       | 中心天体位置          | [EntityPositionCentralBody.md](EntityPositionCentralBody.md)     |


## 使用方式

通过 `$type` 字段指定具体类型，然后提供对应类型的参数：

```json
{
  "$type": "SitePosition",
  "CentralBody": "Earth",
  "cartographicDegrees": [-122.18936, 46.19557, 0],
  "clampToGround": false
}
```

或：

```json
{
  "$type": "J2",
  "OrbitEpoch": "2022-09-05T04:00:00Z",
  "CoordSystem": "Inertial",
  "CoordType": "Classical",
  "OrbitalElements": [7678140, 0, 58.5, 0, 0, 0]
}
```

