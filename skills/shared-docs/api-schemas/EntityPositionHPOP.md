# EntityPositionHPOP 数据结构

高精度轨道递推 (HPOP) 位置对象。作为 `IEntityPosition` 多态分支时 `$type` 为 `HPOP`;作为 `/Propagator/HPOP` 请求体时无需 `$type` 字段。

## 基础参数

| 参数名 | 类型 | 必须 | 说明 |
| --- | --- | --- | --- |
| `Description` | string | 否 | 说明信息 |
| `Start` | string | 是 | 分析开始时刻 (UTCG) |
| `Stop` | string | 是 | 分析结束时刻 (UTCG) |
| `OrbitEpoch` | string | 是 | 轨道历元 (UTCG) |
| `CoordEpoch` | string | 否 | 坐标系历元 (UTCG), 缺省: `2000-01-01T11:58:55.816Z` |
| `CoordSystem` | string | 否 | 缺省 `Inertial` (Inertial, J2000, ICRF...) |
| `CoordType` | string | 否 | `Classical` 或 `Cartesian` (缺省 Classical) |
| `OrbitalElements` | array | 是 | 轨道根数,含义由 `CoordType` 决定 |
| `GravitationalParameter` | number | 否 | 引力常数 (m³/s²), 默认 3.986004415E14 |
| `Mass` | number | 否 | 质量 (kg), 默认 1000 |
| `CoefficientOfDrag` | number | 否 | 阻力系数, 默认 2.2 |
| `AreaMassRatioDrag` | number | 否 | 阻力面质比 (m²/kg), 默认 0.02 |
| `CoefficientOfSRP` | number | 否 | 太阳辐射压反射系数, 默认 1.0 |
| `AreaMassRatioSRP` | number | 否 | 太阳辐射压面质比 (m²/kg), 默认 0.02 |
| `HpopPropagator` | object | 是 | 轨道积分器配置对象 |

## CoordType 与 OrbitalElements

| `CoordType` | `OrbitalElements` 内容 | 单位 |
| --- | --- | --- |
| `Classical` | [半长径, 偏心率, 倾角, 近点角距, 升交点经度, 真近点角] | m, -, deg, deg, deg, deg |
| `Cartesian` | [X, Y, Z, Vx, Vy, Vz] | m, m/s |

## HpopPropagator 配置对象

| 参数名 | 类型 | 必须 | 说明 |
| --- | --- | --- | --- |
| `Name` | string | 是 | HPOP 配置名称 |
| `Description` | string | 否 | 说明信息 |
| `CentralBodyName` | string | 否 | 中心天体名称, 默认 `Earth` |
| `GravityModel` | object | 否 | 非球形引力位配置 |
| `ThirdBodyForce` | array | 否 | 第三体引力列表 |
| `AtmosphericModel` | object | 否 | 大气模型配置 |
| `SRPModel` | object | 否 | 太阳辐射压模型配置 |
| `NumericalIntegrator` | object | 否 | 数值积分器配置 (缺省 RKF7th8th) |

### GravityModel

目前支持 `GravityField` 与 `TwoBody` 两种 `$type`。

**GravityField**

| 参数名 | 类型 | 必须 | 说明 |
| --- | --- | --- | --- |
| `$type` | string | 是 | `"GravityField"` |
| `Name` | string | 否 | 引力场模型名称 (如 EGM2008, MRO110C) |
| `GravityFileName` | string | 是 | 引力文件名 (如 EGM2008.grv, MRO110C.grv) |
| `Degree` | number | 是 | 引力场阶数 |
| `Order` | number | 是 | 引力场次数 |
| `EOPfilePath` | string | 否 | EOP 文件路径 (如 EOP-v1.1.txt) |
| `UseSecularVariations` | boolean | 否 | 是否使用长期变化, 缺省 false |
| `SolidTideType` | string | 否 | 固体潮类型, 缺省 `"Permanent tide only"`; 其余: `"None"` |

**TwoBody**

| 参数名 | 类型 | 必须 | 说明 |
| --- | --- | --- | --- |
| `$type` | string | 是 | `"TwoBody"` |
| `Name` | string | 否 | 二体引力名称 |
| `Mu` | number | 是 | 中心天体引力常数 (m³/s²) |
| `GravSource` | string | 否 | 引力常数来源, 缺省 `"CbFile"` |

### ThirdBodyFunction (ThirdBodyForce 数组元素)

| 参数名 | 类型 | 必须 | 说明 |
| --- | --- | --- | --- |
| `ThirdBodyName` | string | 是 | 第三体名称 (Moon, Sun...) |
| `EphemSource` | string | 否 | 星历来源, 缺省 `"DeFile"` (JplDe430) |
| `GravSource` | string | 否 | 引力常数来源, 缺省 `"DeFile"` |
| `ModeType` | string | 否 | 第三体引力类型, 缺省 `"PointMass"` |
| `Mu` | number | 否 | 第三体引力常数 (m³/s²) |

### AtmosphericModel (JacchiaRoberts)

| 参数名 | 类型 | 必须 | 说明 |
| --- | --- | --- | --- |
| `$type` | string | 是 | `"JacchiaRoberts"` |
| `Name` | string | 否 | 大气密度模型名称 |
| `DragModelType` | string | 否 | 阻力模型类型, 缺省 `"Spherical"` |
| `AtmosDataSource` | string | 否 | 大气数据源, 缺省 `"Constant Values"` |
| `F10p7` | number | 否 | 太阳辐射通量 F10.7, 缺省 150 |
| `F10p7Avg` | number | 否 | F10.7 平均值, 缺省 150 |
| `Kp` | number | 否 | 地磁 Kp 指数, 缺省 3.0 |

### SRPModel (SRPSpherical)

| 参数名 | 类型 | 必须 | 说明 |
| --- | --- | --- | --- |
| `$type` | string | 是 | `"SRPSpherical"` |
| `Name` | string | 否 | 光压模型名称 |
| `ShadowModel` | string | 否 | 阴影模型, 缺省 `"DualCone"`; 其余: `"Cylindrical"` |
| `SunPosition` | string | 否 | 太阳位置类型, 缺省 `"Apparent"`; 其余: `"True"` |
| `EclipsingBodies` | array | 否 | 掩食天体列表, 缺省 `["Earth", "Moon"]` |

## JSON 示例 (地球 STK Default V10)

```json
{
  "Description": "STK缺省的Propagator:Earth Default V10",
  "Start": "2018-12-01T00:00:00.000Z",
  "Stop": "2018-12-02T00:00:00.000Z",
  "OrbitEpoch": "2018-12-01T00:00:00.000Z",
  "CoordEpoch": "2000-01-01T11:58:55.81616Z",
  "CoordSystem": "Inertial",
  "CoordType": "Classical",
  "OrbitalElements": [6678137, 0, 28.5, 0, 0, 0],
  "GravitationalParameter": 3.986004415E14,
  "HpopPropagator": {
    "Name": "Earth_Hpop_default_v10",
    "CentralBodyName": "Earth",
    "GravityModel": {
      "$type": "GravityField",
      "GravityFileName": "EGM2008.grv",
      "Degree": 21,
      "Order": 21,
      "EOPfilePath": "EOP-v1.1.txt",
      "SolidTideType": "Permanent tide only"
    },
    "ThirdBodyForce": [
      { "ThirdBodyName": "Moon" },
      { "ThirdBodyName": "Sun" }
    ],
    "SRPModel": {
      "$type": "SRPSpherical",
      "ShadowModel": "DualCone",
      "SunPosition": "Apparent",
      "EclipsingBodies": ["Earth", "Moon"]
    }
  }
}
```

## 注意事项

- HPOP 配置较为复杂,推荐从 `propagator/fixtures/hpop/` 下 fixture 开始修改
- 不同中心天体有不同的默认引力场文件和参数
- 确保各子对象的 `$type` 字段正确
