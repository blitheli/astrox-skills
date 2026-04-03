---
name: propagator-hpop
description: 通过 Astrox WebAPI 的 POST /Propagator/HPOP 计算高精度轨道递推(HPOP)轨迹，考虑多种摄动力，输出 CzmlPositionOut（CZML结构的位置序列）。当用户需要高精度轨道计算、考虑多种摄动因素、HPOP 时使用。
---

# 高精度轨道递推器技能 (HPOP Propagator)

## 核心指令 (Core Instructions)
1. **输入解析**：识别用户提供的轨道历元、开始/结束时间以及轨道根数（经典根数或笛卡尔坐标）。
2. **坐标系匹配**：默认使用 `Inertial` 系和 `Earth` 中心天体。
3. **HpopPropagator 配置**：识别并配置以下摄动模型：
   - 非球形引力位（GravityField）
   - 第三体引力（ThirdBodyForce）
   - 大气阻力（AtmosphericModel）
   - 太阳辐射压（SRPModel）
4. **API 调用逻辑**：向 `{BASE_URL}/Propagator/HPOP` 发送 `POST`，`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址
`POST /Propagator/HPOP`

### 输入参数结构 (JSON)

#### 基础参数

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `Description` | string | 否 | 说明信息 |
| `Start` | string | 是 | 分析开始时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ") |
| `Stop` | string | 是 | 分析结束时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ") |
| `OrbitEpoch` | string | 是 | 轨道历元(UTCG) ("yyyy-MM-ddTHH:mm:ssZ") |
| `CoordEpoch` | string | 是 | 坐标系历元 (UTCG) |
| `CoordSystem`| string | 否 | 缺省 "Inertial"(Inertial,J2000,ICRF...) |
| `CoordType` | string | 否 | "Classical" 或 "Cartesian" (缺省 Classical) |
| `OrbitalElements` | array | 是 | 轨道根数,具体根数由CoordType决定 |
| `GravitationalParameter`|number|否|引力常数(m^3/s^2), 默认 3.986004415E14 |
| `Mass` | number | 否 | 质量(kg)，默认 1000 |
| `CoefficientOfDrag`|number|否|阻力系数，默认 2.2 |
| `AreaMassRatioDrag`|number|否|阻力面质比(m^2/kg)，默认 0.02 |
| `CoefficientOfSRP`|number|否|太阳辐射压反射系数，默认 1.0 |
| `AreaMassRatioSRP`|number|否|太阳辐射压面质比(m^2/kg)，默认 0.02 |
| `HpopPropagator` | object | 是 | 轨道积分器配置对象 |

#### HpopPropagator 配置对象

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `Name` | string | 是 | HPOP 配置名称 |
| `Description` | string | 否 | 说明信息 |
| `CentralBodyName` | string | 否 | 中心天体名称，默认 "Earth" |
| `GravityModel` | object | 否 | 非球形引力位配置 |
| `ThirdBodyForce` | array | 否 | 第三体引力列表(ThirdBodyFunction类型数组) |
| `AtmosphericModel`| object| 否 | 大气模型配置 |
| `SRPModel` | object | 否 | 太阳辐射压模型配置 |
| `NumericalIntegrator` | object | 否 | 数值积分器配置(缺省为RKF7th8th) |

##### GravityModel（非球形引力位）

目前有两种

- GravityField

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `$type` | string | 是 | "GravityField" |
| `Name` | string | 否 | 引力场模型名称(如 EGM2008、MRO110C) |
| `Description` | string | 否 | 说明信息 |
| `UserComment` | string | 否 | 用户自定义信息。 |
| `GravityFileName` | string | 是 | 引力文件名（确保后台有此文件，如 EGM2008.grv、MRO110C.grv) |
| `Degree` | number | 是 | 引力场阶数 |
| `Order` | number | 是 | 引力场次数 |
| `EOPfilePath` | string | 否 | EOP 文件路径(如 EOP-v1.1.txt)|
| `UseSecularVariations`|boolean| 否 | 是否使用长期变化,缺省:false |
| `SolidTideType` | string | 否 | 固体潮类型(缺省:"Permanent tide only"),其余:"None" |

- TwoBody

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `$type` | string | 是 | "TwoBody" |
| `Name` | string | 否 | 二体引力名称(如 Earth,Moon) |
| `Description` | string | 否 | 说明信息 |
| `UserComment` | string | 否 | 用户自定义信息。 |
| `GravSource` | string | 否 | 中心天体引力常数来源,缺省: "CbFile" |
| `Mu` | number | 是 | 中心天体引力常数(m^3/s^2) |

##### ThirdBodyFunction（第三体引力）

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `Name` | string | 否 | 第三体引力星体名称，(Moon, Sun...) |
| `Description` | string | 否 | 说明信息 |
| `UserComment` | string | 否 | 用户自定义信息。 |
| `EphemSource` | string | 否 | 星历来源(DeFile),缺省:"DeFile"。内部使用 JplDe430，目前仅此一种。 |
| `GravSource` | string | 否 | 第三体引力常数来源(DeFile,CbFile)，缺省: "DeFile"。当为 DeFile 时可不用设置 `Mu`。 |
| `ModeType` | string | 否 | 第三体引力类型，缺省:"PointMass"。目前仅支持该类型。 |
| `Mu` | number(double) | 否 | 第三体引力常数(m^3/s^2)。 |
| `ThirdBodyName` | string | 是 | 第三体名称(Earth, Moon, Sun...) |

##### AtmosphericModel（大气模型）

目前仅有 JacchiaRoberts 模型可用

- JacchiaRoberts

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `$type` | string | 是 | "JacchiaRoberts" |
| `Name` | string | 否 | 大气密度模型名称,缺省为 "Jacchia-Roberts" |
| `Description` | string | 否 | 说明信息 |
| `UserComment` | string | 否 | 用户备注信息 |
| `DragModelType` | string | 否 | 阻力模型类型(缺省:"Spherical") |
| `AtmosDataSource` | string | 否 | 大气数据源(缺省: "Constant Values") |
| `F10p7` | number | 否 | 太阳辐射通量F10.7(缺省: 150) |
| `F10p7Avg` | number | 否 | 太阳辐射通量平均值F10.7(缺省: 150) |
| `Kp` | number | 否 | 地磁Kp 指数(缺省: 3.0) |

##### SRPModel（太阳辐射压模型）
目前仅有一种配置.

- SRPSpherical

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `$type` | string | 是 | "SRPSpherical" |
| `Name` | string | 否 | 光压模型名称,缺省为 "SRPSpherical" |
| `Description` | string | 否 | 说明信息 |
| `UserComment` | string | 否 | 用户备注信息 |
| `ShadowModel` | string | 否 | 阴影模型(缺省:"DualCone"),其它: "Cylindrical" |
| `SunPosition` | string | 否 | 太阳位置类型(缺省:"Apparent"),其它: "True" |
| `EclipsingBodies` | array | 否 | 掩食天体列表(缺省: ["Earth", "Moon"]) |

### 响应数据结构（CzmlPositionOut）

详见 shared-docs/api-schemas/CzmlPositionOut.md

## 注意事项

- 单位检查：半长径必须是米(m)，质量为千克，引力常数 m^3/s^2。
- 时间格式：首选为ISO8601 格式
- HPOP 是复杂的轨道传播器，建议使用预配置的 fixture 作为起点
- 不同中心天体有不同的默认引力场文件和参数

## 标准执行流程

1. 参数预检
   - 检查必填字段完整性
   - 检查 UTC 时间格式
   - 检查 `Start < Stop`
   - 检查 `OrbitalElements` 长度必须为 6
2. 模型判定
   - 未指定 `CoordType` 时默认 `Classical`
   - 验证 HpopPropagator 子配置的有效性
3. 请求构造
   - 按接口契约原样传参
   - 确保 `$type` 字段正确
4. 结果判定
   - 先判 HTTP 状态，再判 `IsSuccess`
   - `IsSuccess = false` 时优先返回 `Message`
5. 输出归一化
   - 给出关键输入摘要、执行状态、核心输出、限制说明

## 调用示例（最小可运行：地球默认 HPOP）

**场景**：使用 STK 默认 V10 配置的 Earth HPOP，传播 24 小时。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/HPOP" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@propagator-hpop/fixtures/hpop-earth-default-v10.json"
```

由于 HPOP 配置复杂，建议直接使用 fixture 文件。以下是 JSON 结构示例：

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

## 本地快速验证（可选）

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/HPOP" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@propagator-hpop/fixtures/hpop-earth-default-v10.json"
```

## 更多示例与测试数据（fixtures）

| 文件 | 用途简述 |
|------|----------|
| `propagator-hpop/fixtures/hpop-earth-default-v10.json` | 地球 STK Default V10 配置，24h 传播 |
| `propagator-hpop/fixtures/hpop-moon-default-v10.json` | 月球 STK Default V10 配置，24h 传播 |
| `propagator-hpop/fixtures/hpop-mars-250603.json` | 火星 MRO110C 引力场，24h 传播 |

说明：HPOP 配置较为复杂，推荐从 fixture 开始修改。

从响应中取**末时刻**位置速度：按 `shared-docs/api-schemas/CzmlPositionOut.md` 中 `cartesianVelocity` 平铺格式，取最后一组 `[t, X, Y, Z, Vx, Vy, Vz]`。
