---
name: astrogator
description: 运行轨道机动序列(MCS),功能与 STK Astrogator 基本一致。支持初始状态、轨道递推、脉冲/有限推力机动、目标序列(微分修正)、跟随段等。含地月转移轨道设计(粗修正/B 平面/近月点高度倾角三级微分修正)、近月制动(LOI)与环月轨道递推/抬轨、地月平动点 L1/L2 Halo 轨道设计、DRO、霍曼转移。用户需要轨道设计、机动序列仿真、地月转移/环月轨道设计时使用。
---

# Astrogator 轨道机动序列技能 (Astrogator MCS)

通过 Astrox WebAPI 的 `POST /Astrogator/RunMCS`,按 **Mission Control Sequence (MCS, 轨道机动序列)** 依次执行各飞行段,完成轨道递推、机动与目标求解。概念与用法与 **STK Astrogator** 基本一致:以 `MainSequence` 定义段序列,在 `TargetSequence` 中配置 **Differential Corrector(微分修正)** 求解机动参数。

## 核心指令 (Core Instructions)

1. **输入解析**:识别中心天体 `CentralBody`、主序列 `MainSequence`,以及可选的 `Entities`、`Propagators`、`EngineModels`。
2. **段类型判定**:根据任务选择段类型——`InitialState`(初始状态)、`Propagate`(递推)、`ManeuverImpulsive`(脉冲机动)、`ManeuverFinite`(有限推力机动)、`TargetSequence`(目标序列/微分修正)、`Sequence`(子序列)、`Follow`(跟随其它实体)、`Stop`(终止)。
3. **初始状态**:首段通常为 `InitialState`,`InitialState.Element` 支持 `$type` 为 `Keplerian`、`Cartesian` 或 `Spherical`。
4. **积分器**:`PropagatorName` 引用内置缺省积分器(如 `Earth_Point_Mass`、`Earth_Hpop_default_v10`)或输入中的 `Propagators` 自定义积分器。
5. **终止条件**:`StopConditions` 常用 `$type` 包括 `Duration`(固定时长,s)、`Epoch`(历元)、`Periapsis`/`Apoapsis`(需 `CentralBodyName` 与 `Mu`,可指定 Moon)、`Scalar`(标量条件,`UserCalcObject` 为 CalcScalar,如高度、地心距、真近点角等)。同一段可并列多个终止条件,任一触发即停止。
6. **目标求解**:在 `TargetSequence.Profiles` 中配置 `$type: "DifferentialCorrector"`,通过 `ControlParameters`(自变量)与 `Results`(约束)迭代求解;约束名引用被约束段 `Results[]` 中声明的标量。
7. **API 调用逻辑**:向 `{BASE_URL}/Astrogator/RunMCS` 发送 `POST`,`Content-Type: application/json`。
8. **结果判定**:先判 HTTP 状态,再判 `IsSuccess`;成功时读取 `MainSequenceResults`(各段结果)与 `Positions`(Czml 位置序列,可选)。含 TargetSequence 时再检查 `OperatorResults[].Converged`。
9. **跨天体任务**:地月转移/环月任务见 [docs/earth-moon-transfer.md](docs/earth-moon-transfer.md);地月平动点 Halo/DRO 见 [docs/earth-moon-libration.md](docs/earth-moon-libration.md)。

## API 规范 (Tool Definition)

### 接口地址

`POST /Astrogator/RunMCS`

### 输入参数结构 (JSON)


| 参数名                    | 类型           | 必须  | 缺省         | 说明                                                          |
| ---------------------- | ------------ | --- | ---------- | ----------------------------------------------------------- |
| `CentralBody`          | string       | 否   | `Earth`    | 中心天体(Earth, Moon, Mars, Sun 等);决定输出 `MainSequenceResults[].InitialState/FinalState` 与 `Positions` 的参考系(该天体 Inertial) |
| `OutCzmlFrameName`     | string       | 否   | `INERTIAL` | 输出 CzmlPositions 参考系(INERTIAL, FIXED, MEANECLPJ2000, J2000) |
| `MainSequence`         | array        | 是   | —          | 飞行段序列,每段以 `$type` 区分类型                                      |
| `Entities`             | array | null | 否   | null       | 其它对象(用于相对运动、Follow 段)                                       |
| `Propagators`          | array | null | 否   | null       | 自定义积分器;使用缺省积分器时可省略                                          |
| `EngineModels`         | array | null | 否   | null       | 发动机模型;使用缺省时可省略                                              |
| `ComputeCzmlPositions` | boolean      | 否   | true       | 是否返回 `Positions`;频繁调用且只关注段结果时可设为 false                      |


### MainSequence 段类型 ($type)


| $type               | 用途           | 关键字段                                                  |
| ------------------- | ------------ | ----------------------------------------------------- |
| `InitialState`      | 设置飞行器初始轨道与质量 | `InitialState`(AgVAState), `Results`                  |
| `Propagate`         | 轨道递推至终止条件    | `PropagatorName`, `StopConditions`, `Results`         |
| `ManeuverImpulsive` | 瞬时速度增量机动     | `AttitudeControl`, `UpdateMass`                       |
| `ManeuverFinite`    | 有限时长推力机动     | `AttitudeControl`, `PropagatorName`, `StopConditions` |
| `TargetSequence`    | 目标序列(含微分修正)  | `Segments`, `Profiles`                                |
| `Sequence`          | 嵌套子序列        | `Segments`                                            |
| `Follow`            | 跟随 Leader 实体 | `LeaderName`                                          |
| `Stop`              | 终止 MCS       | —                                                     |


### InitialState / AgVAState 主要字段


| 参数名               | 类型     | 单位  | 说明                                      |
| ----------------- | ------ | --- | --------------------------------------- |
| `Epoch`           | string | UTC | 历元,ISO8601 如 `2018-12-01T00:00:00.000Z` |
| `CoordSystemName` | string | —   | 坐标系,如 `Earth Inertial`, `Moon Inertial`, `Moon L1`, `Moon L2`, `Moon EMLibration`;可与顶层 `CentralBody` 不同(如 `CentralBody: "Moon"` 下用 `Earth Inertial` 给出近地出发状态) |
| `Element`         | object | —   | 轨道根数,见下表                                |
| `DryMass`         | number | kg  | 结构质量,默认 500                             |
| `FuelMass`        | number | kg  | 燃料质量,默认 500                             |
| `Cd`              | number | —   | 阻力系数,通常 2.2                             |
| `Cr`              | number | —   | 光压系数,1.0 不反射,2.0 完全反射                   |
| `DragArea`        | number | m^2 | 阻力面积                                    |
| `SRPArea`         | number | m^2 | 光压面积                                    |


#### Element 子类型


| $type       | 主要字段                                                                        | 单位                         |
| ----------- | --------------------------------------------------------------------------- | -------------------------- |
| `Keplerian` | SemiMajorAxis, Eccentricity, Inclination, RAAN, ArgOfPeriapsis, TrueAnomaly | m, —, deg, deg, deg, deg   |
| `Cartesian` | X, Y, Z, Vx, Vy, Vz                                                         | m, m/s                     |
| `Spherical` | Right_Asc, Decl, RMag, Horiz_FPA, Azimuth, VMag                             | deg, deg, m, deg, deg, m/s |


### 常用缺省积分器 (PropagatorName)


| 名称                       | 中心天体 | 说明                      |
| ------------------------ | ---- | ----------------------- |
| `Earth_Point_Mass`       | Earth | 地球质点引力                  |
| `Earth_Hpop_default_v10` | Earth | 地球 HPOP(含阻力/光压等,版本 v10) |
| `CisLunar`               | Earth | 地球 WGS84 8x8 + 月球/太阳第三体(DE430),用于地月转移全程、DRO、L1/L2 Halo 等 |
| `Moon_Point_Mass`        | Moon | 月球质点,环月 Target 内快速迭代 |
| `Moon_Hpop_default_v10`  | Moon | 月球 LP150Q 48x48 + 地球/太阳第三体 + 光压,环月高精度递推 |
| `Moon_Hpop_De430`        | Moon | 月球 GL0900D 48x48 + 地球/太阳第三体 + 光压(与内部 DE430 历表一致) |
| `Moon_NonSphere`         | Moon | 月球 GL0900D 48x48 非球形引力 |
| `Sun_Point_Mass`         | Sun | 日心质点                    |


自定义积分器通过顶层 `Propagators` 数组定义,段内 `PropagatorName` 引用其 `Name`(示例:`fixtures/earth-moon-transfer/mcs-propagate-e2m-tli-impulsive-moon-flyby.json` 中的 `CisLunarMoonCentered`)。地心与月心积分器可在同一 MCS 内混用,段间状态自动转换。

### 段 Results / Scalar 终止条件的标量对象 (CalcScalar)

`Propagate`/`ManeuverImpulsive` 等段的 `Results[]` 与 `Scalar` 终止条件的 `UserCalcObject` 共用同一组标量对象,以 `$type` 区分;`Results[].Name` 即输出 `Results{}` 字典的键,也是微分修正约束 `Results[].Name` 的引用名。


| $type                      | ComponentName 示例                                                                                                   | 必填字段                                        | 单位            |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------ | ------------------------------------------- | ------------- |
| `PointElement`             | `X, Y, Z, Vx, Vy, Vz, Magnitude`                                                                                   | `CoordSystemName`                           | m, m/s        |
| `ModifiedKeplerianElement` | `SemimajorAxis, Eccentricity, Inclination, RAAN, ArgumentOfPeriapsis, TrueAnomaly, RadiusOfApoapsis, AltitudeOfPeriapsis, AltitudeOfApoapsis, Period` | `CoordSystemName`(如 `Moon Inertial`), `Mu` | m, —, deg, s  |
| `KeplerianElement`         | 同上,另有 `ElementType`(`Osculating`/`KozaiIzsakMean`)                                                              | `CoordSystemName`, `Mu`                     | 同上            |
| `SphericalElement`         | `RightAscension, Declination, RadiusMagnitude, HorizFPA, VelocityAzimuth, VelocityMagnitude`                        | `CoordSystemName`                           | deg, m, m/s   |
| `Cartographic`             | `Latitude, Longitude, Height`                                                                                      | `CentralBodyName`                           | deg, m        |
| `DeltaSpherical`           | `Delta_Right_Asc, Delta_Declination, Delta_RMag`(飞行器与中心天体相对父天体的差值)                                                | `CentralBodyName`(Moon), `ParentCbName`(Earth) | deg, m        |
| `BPlane`                   | `BDotR, BDotT`                                                                                                     | `CentralBodyName`, `Mu`                     | m             |
| `Epoch`                    | —(历元时刻;约束 `DesiredValue` 为 ISO8601 字符串,`Tolerance` 单位 s)                                                            | `Name`                                      | UTC           |
| `Duration`                 | —(段飞行时长)                                                                                                            | `Name`                                      | s             |
| `Relative`                 | 相对其它实体的标量(需 `Entities`)                                                                                            | 视实现                                        | —             |


角度类标量作为终止条件时建议显式加 `"Dimension": "Angle"`。

### 脉冲机动姿态 (AttitudeControl)


| $type            | 说明         | 典型字段                                                                    |
| ---------------- | ---------- | ----------------------------------------------------------------------- |
| `VelocityVector` | 沿/反速度方向    | `DeltaVMagnitude`(m/s)                                                  |
| `ThrustVector`   | 指定推力坐标系下矢量 | `ThrustAxesName`(VNC, VNC(Moon), VVLH, VVLH(Moon), LVLH, J2000 等), `CoordType`, X/Y/Z 或 Spherical 分量 |
| `Attitude`       | 姿态角/四元数    | EulerAngles 或 Quaternion                                                |


微分修正自变量命名示例:`ImpulsiveMnvr.Cartesian.X`、`ImpulsiveMnvr.Spherical.Magnitude`;初始段自变量:`InitialState.Spherical.Right_Asc / Decl / VMag / Azimuth`、`InitialState.Cartesian.Vy`、`InitialState.Keplerian.sma` 等;递推段自变量:`StopConditions.<终止条件名>`(如 `StopConditions.Duration`)。环月机动使用 `VNC(Moon)`,`X < 0` 表示反速度方向制动。

### TargetSequence / DifferentialCorrector


| 字段                  | 说明                                                                   |
| ------------------- | -------------------------------------------------------------------- |
| `Segments`          | 目标序列内的段列表(机动、递推等)                                                    |
| `Action`            | `RunActiveOperators`(缺省行为,迭代运行全部 Active 的 Profile)/ `RunNominalSequence`(只跑一遍不迭代,`OperatorResults` 为空) |
| `Profiles[].$type`  | 通常为 `DifferentialCorrector`;多个 Profile 顺序执行,后一个在前一个收敛结果上继续(先粗后精) |
| `ControlParameters` | 自变量:Name, ParentName, InitialValue, Tolerance, MaxStep, Perturbation, Enable |
| `Results`           | 约束:Name, ParentName, DesiredValue(字符串), Tolerance, Enable                 |
| `MaximumIterations` / `OnlyStoreFinalResults` | 最大迭代数(缺省 50);是否只保留最终结果(设 false 可得到 `Values[]` 迭代历史) |


段内 `Results` 定义可读取的标量(如 `ModifiedKeplerianElement` 的 RadiusOfApoapsis、Eccentricity),供 Profile 约束引用。

### 响应数据结构 (MCSOutput)


| 字段名                   | 类型      | 说明                                           |
| --------------------- | ------- | -------------------------------------------- |
| `IsSuccess`           | boolean | 是否成功                                         |
| `Message`             | string  | 失败原因或提示                                      |
| `MainSequenceResults` | array   | 各段执行结果(递推时长、机动 Delta-V、目标序列收敛信息等)            |
| `Positions`           | object  | Czml 格式位置序列(`ComputeCzmlPositions=true` 时返回) |


`Positions` 结构与 Czml 位置输出类似;详见 shared-docs/api-schemas/CzmlPositionOut.md。`Positions.CzmlPositions[]` 按段/中心天体切分为多个区间,每个区间的 `cartesianVelocity` 为扁平数组 `[t, x, y, z, vx, vy, vz, ...]`(t 为相对 `epoch` 的秒)。

#### MainSequenceResults 各段结果结构


| `$type` / `TypeName`                    | 关键字段                                                                                                                     |
| --------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| (无 `$type`) `TypeName: "InitialState"`  | `InitialState`, `FinalState`(同一状态)                                                                                        |
| `PropagateResult`                       | `StoppingConditionName`(实际触发的终止条件名), `StoppedOnMaximumDuration`, `DurationSec`, `Results{}`, `InitialState`, `FinalState` |
| `ManeuverImpulsiveResult`               | `ManeuverInformation{DeltaV_Mag, DeltaV_VNC[6], DeltaV_Inertial[6], EstimatedFuelUsed, FuelUsed, UpdateMass}`, `Results{}`, `FinalState` |
| `TargetSequenceResult`                  | `OperatorResults[]`(每个 DC 的 `Converged`, `TotalIterations`, `ControlParameters[].FinalValue`, `Results[].CurrentValue/Difference`), `SegmentResults[]`(收敛后各段结果,结构同上) |


- `FinalState`/`InitialState`(`SegmentState`):`Epoch`, `CoordSystemName`, `Cartesian`, `Keplerian`, `Spherical`, `DryMass/FuelMass`, `Geodetic_*`,**坐标系固定为顶层 `CentralBody` 的 Inertial 系**。例如 `CentralBody: "Earth"` 时环月段的 `FinalState.Keplerian` 是地心根数;需要月心根数应在该段 `Results[]` 中声明 `CoordSystemName: "Moon Inertial"` 的 Keplerian 标量,或把顶层 `CentralBody` 设为 `Moon`(初始段仍可用 `Earth Inertial` 给定)。
- `Results{}` 键名与输入 `Results[].Name` 一致(支持中文),值为 double;`Epoch` 类型为 ISO8601 字符串。
- DC 自变量/约束的 `InitialValue/FinalValue/DesiredValue/CurrentValue` 均为字符串,需转换为数值;`Epoch` 类约束的 `Difference`、`Values[]` 为秒。

## 注意事项

- 距离单位均为 **米(m)**,速度为 **m/s**,角度为 **deg**,时长为 **s**,质量为 **kg**。
- 每段必须设置唯一 `Name`;微分修正的 `ParentName` 须与段名一致。
- `MainSequence` 第一段通常为 `InitialState`;`Follow` 段可省略 InitialState,直接跟随 `Entities` 中的 Leader。
- 使用缺省积分器时无需传 `Propagators`;地月/日心等多体任务可能需自定义 `Propagators` 与 `Entities`。
- 仅关心段标量结果、不需轨迹点时,设 `ComputeCzmlPositions: false` 可减小响应体积。
- 顶层 `CentralBody` 决定所有段状态与 `Positions` 的输出坐标系;跨天体任务(地月转移)按需选择 `Earth` 或 `Moon`,并用段 `Results` 补充另一天体系下的标量。
- TargetSequence 收敛判定:每个 `OperatorResults[].Converged` 为 true,且各约束 `|Difference| <= Tolerance`;同时核对近月/近地递推段 `StoppingConditionName` 是否为预期的终止条件(而非保护性 `Duration`/高度条件)。
- 更多复杂示例见仓库 `raw/Astrogator/`(Propagate, Maneuver, ManeuverImpulsive, Target, Follow 等子目录)。

## 标准执行流程

1. 参数预检
  - 检查 `MainSequence` 非空且首段合理(InitialState 或 Follow)
  - 各段 `$type`、`Name` 完整
  - Propagate/ManeuverFinite 段含 `StopConditions`
  - TargetSequence 的 ControlParameters 与 Results 的 ParentName 可对应到段名
2. 模型判定
  - 近地简单任务优先 `Earth_Point_Mass`
  - 需大气/光压/高阶引力时用 `Earth_Hpop_default_v10` 或自定义 Propagator
  - 地月转移全程用 `CisLunar`;环月段用 `Moon_Hpop_default_v10`/`Moon_Hpop_De430`(Target 内迭代可用 `Moon_Point_Mass`)
  - 需要月心根数输出时在段 `Results` 声明 `Moon Inertial` 标量,或将顶层 `CentralBody` 设为 `Moon`
3. 请求构造
  - 按 OpenAPI 契约原样传参;`$type`  discriminator 不可省略
4. 结果判定
  - HTTP 200 且 `IsSuccess = true`
  - TargetSequence 失败时查看 Profile 迭代信息与 `Message`
5. 输出归一化
  - 摘要:中心天体、段数、是否含目标求解
  - 核心输出:末段轨道参数、机动 Delta-V(`ManeuverInformation.DeltaV_Mag`)、约束满足情况(`OperatorResults[].Results[].CurrentValue/Difference`)、自变量收敛值(`ControlParameters[].FinalValue`)
  - 可选:从 `Positions` 提取轨迹

## 调用示例(最小可运行:递推 1 天)

**场景**:地球圆轨道,质点引力,固定时长递推 86400 s。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Astrogator/RunMCS" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@astrogator/fixtures/mcs-propagate-duration-min.json"
```

## 脉冲机动 + 递推

```bash
curl "${BASE_URL}/Astrogator/RunMCS" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@astrogator/fixtures/mcs-maneuver-impulsive-along-velocity-min.json"
```

## 霍曼转移(微分修正)

两脉冲霍曼变轨:自变量为两次脉冲 VNC-X 分量,约束远地点半径与末轨道偏心率。

```bash
curl "${BASE_URL}/Astrogator/RunMCS" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@astrogator/fixtures/mcs-hohmann-target-min.json"
```

## 地月转移轨道设计 + 近月制动 + 环月轨道(脉冲)

**场景**:从近地点出发状态(`Earth Inertial`, `Spherical`:赤经/赤纬/地心距/速度方位角/速度大小)出发,用 `CisLunar` 递推至地心距 320000 km,再递推至 **月心近月点**(`Periapsis(Moon)` + 撞月高度保护 + 时长保护),通过微分修正求解出发赤经/赤纬/速度大小,使近月点满足高度、月心倾角与到达历元;然后在近月点施加 `VNC(Moon)` 反速度脉冲(自变量 `ImpulsiveMnvr.Cartesian.X`,约束月心 `Eccentricity = 0`)进入环月圆轨道,最后用 `Moon_Hpop_default_v10` 递推环月段。

详细原理、三级微分修正策略、B 平面/制动量估算公式、结果提取(jq 示例)与实测参考值见 [docs/earth-moon-transfer.md](docs/earth-moon-transfer.md)。

设计流程(先粗后精,均已实测收敛):

| 步骤 | 自变量 | 约束 | fixture |
| --- | --- | --- | --- |
| 1 粗修正 | `InitialState.Spherical.Right_Asc / Decl` | `DeltaSpherical` 的 `Delta_Declination = 0`, `Delta_Right_Asc = 0` | `earth-moon-transfer/mcs-target-e2m-coarse-delta-radec.json` |
| 2 B 平面 | `Right_Asc / Decl / VMag` | `BPlane` 的 `BDotR = 5500 km`, `BDotT = 0`, `Epoch` = 到达时刻 | `earth-moon-transfer/mcs-target-e2m-bplane-inc-alt.json`(Profile 1) |
| 3 精修正 | 同上(`MaxStep` 减小) | 月心 `Inclination = 90 deg`, `Cartographic.Height = 100 km`, `Epoch` | 同上(Profile 2) |
| 4 近月制动 | `ImpulsiveMnvr.Cartesian.X`(`VNC(Moon)`) | 月心 `Eccentricity = 0` | `earth-moon-transfer/mcs-target-e2m-moon-brake-lunar-orbit.json` |
| 5 环月递推 | — | `Duration`;`Results` 读取 Moon Inertial 根数 | 同上(末段) |

要点:

- 近月段终止条件三者并用:`Periapsis`(Moon, 正常出口)、`Scalar Cartographic.Height(Moon) = 0`(撞月保护)、`Duration`(时长保护);正常收敛时 `StoppingConditionName` 为 Periapsis 条件名。
- 月心根数必须通过 `Results`(`CoordSystemName: "Moon Inertial"`, `Mu: 4902800305555.4`)读取,或把顶层 `CentralBody` 设为 `Moon`(fixture `mcs-e2m-moon-brake-lunar-orbit-nominal-mooncb.json`)。
- 制动量初值可由近月点月心速度估算:`dV ≈ v_p - sqrt(mu_M / r_p)`(实测 826 m/s,100 km 极月轨道)。
- 缺省积分器 `CisLunar`、`Moon_Hpop_default_v10`、`Moon_Hpop_De430`、`Moon_Point_Mass` 与发动机 `Constant_Thrust_Isp` 均内置,无需传 `Propagators`/`EngineModels`。

全流程(转移 + 制动 + 环月 1 天):

```bash
curl "${BASE_URL}/Astrogator/RunMCS" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@astrogator/fixtures/earth-moon-transfer/mcs-target-e2m-moon-brake-lunar-orbit.json" \
  | jq '{ops: [.MainSequenceResults[] | .OperatorResults[]? | {Name, Converged, TotalIterations}], peri: .MainSequenceResults[0].SegmentResults[2].Results, dv: .MainSequenceResults[1].SegmentResults[0].ManeuverInformation.DeltaV_Mag, lunar: .MainSequenceResults[2].Results}'
```

环月轨道抬高远月点(100 km 圆极轨道 → 远月点 500 km,`Moon_Hpop_De430` 递推 + `VNC(Moon)` 脉冲 + `Apoapsis(Moon)` 终止):

```bash
curl "${BASE_URL}/Astrogator/RunMCS" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@astrogator/fixtures/earth-moon-transfer/mcs-target-lunar-orbit-raise-apoapsis.json"
```

## DRO 轨道设计(地月旋转系 / Moon Libration)

**场景**:在月心 **Moon Libration**(地月旋转系)下,于 X 轴负向(-150000 km)给定初始 `Vy`,用 `CisLunar` 积分器递推至再次穿越 Z-X 平面(`Y=0`),通过微分修正使该时刻 `Vx=0`,得到典型 **DRO(Distant Retrograde Orbit)** 初值。

要点:

- 初始状态坐标系:`CoordSystemName: "Moon Libration"`
- 积分器:`PropagatorName: "CisLunar"`(地月多体)
- 终止条件:标量 `PointElement` 的 `Y` 分量穿越 0(`Criterion: ThresholdIncreasing`)
- 自变量:`InitialState.Cartesian.Vy`(初猜 850 m/s)
- 约束:递推末态 `EM_Vx = 0`(垂直穿越 Z-X 平面)

```bash
curl "${BASE_URL}/Astrogator/RunMCS" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@astrogator/fixtures/mcs-target-dro-moon-libration-min.json"
```

## 地月平动点 L1/L2 Halo 轨道

**场景**:在 **Moon L1** 或 **Moon L2** 局部坐标系下,给定 Z-X 平面上的初猜状态(`Y=0`, `Vx=Vz=0`),用 `CisLunar` 积分器递推至再次穿越 Z-X 平面,通过微分修正使该时刻 **`Vx=0`**,得到 **Halo 轨道**初值。已知收敛初值后,可仅用 Propagate 段外推轨迹。

详细原理、坐标系说明与示例对照见 [docs/earth-moon-libration.md](docs/earth-moon-libration.md)。

要点:

- L1 坐标系:`CoordSystemName: "Moon L1"`,初值 `X=-5000 km, Z=30000 km, Vy=204 m/s`
- L2 坐标系:`CoordSystemName: "Moon L2"`,初值 `X=+5000 km, Z=30000 km, Vy=-141 m/s`
- 积分器:`PropagatorName: "CisLunar"`
- 终止条件:标量 `PointElement` 的 `Y` 分量穿越 0(L1 用 `ThresholdIncreasing`, L2 用 `ThresholdDecreasing`)
- 自变量:`InitialState.Cartesian.Vy`
- 约束:递推末态 `L1_Vx=0` 或 `L2_Vx=0`

L1 Halo 微分修正:

```bash
curl "${BASE_URL}/Astrogator/RunMCS" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@astrogator/fixtures/earth-moon-libration/mcs-target-eml-l1-halo-min.json"
```

L2 Halo 微分修正:

```bash
curl "${BASE_URL}/Astrogator/RunMCS" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@astrogator/fixtures/earth-moon-libration/mcs-target-eml-l2-halo-min.json"
```

L1/L2 Halo 仅递推(已知初值):

```bash
curl "${BASE_URL}/Astrogator/RunMCS" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@astrogator/fixtures/earth-moon-libration/mcs-propagate-eml-l1-halo-min.json"

curl "${BASE_URL}/Astrogator/RunMCS" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@astrogator/fixtures/earth-moon-libration/mcs-propagate-eml-l2-halo-min.json"
```

## 本地快速验证(可选)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Astrogator/RunMCS" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/astrogator/fixtures/mcs-propagate-duration-min.json" | jq '{IsSuccess, Message, segmentCount: (.MainSequenceResults | length)}'
```

## 更多示例与测试数据 (fixtures)

### Propagate(轨道递推)


| 文件                                     | 用途简述                                     |
| -------------------------------------- | ---------------------------------------- |
| `mcs-propagate-duration-min.json`      | 开普勒初值 + 质点递推固定时长(86400 s)                |
| `mcs-propagate-hpop-duration.json`     | HPOP 积分器(`Earth_Hpop_default_v10`)递推 1 天 |
| `mcs-propagate-periapsis.json`         | 递推至近地点(Periapsis, RepeatCount=2)         |
| `mcs-propagate-apoapsis.json`          | 递推至远地点(Apoapsis)                         |
| `mcs-propagate-scalar-cross-lvlh.json` | 标量终止条件:穿越参考卫星 LVLH 的 XY 平面(含 `Entities`) |


### ManeuverImpulsive(脉冲机动)


| 文件                                                    | 用途简述                                     |
| ----------------------------------------------------- | ---------------------------------------- |
| `mcs-maneuver-impulsive-along-velocity-min.json`      | 沿速度方向脉冲(`VelocityVector`)                |
| `mcs-maneuver-impulsive-thrust-vector-vnc.json`       | VNC 坐标系笛卡尔推力矢量                           |
| `mcs-maneuver-impulsive-thrust-vector-spherical.json` | VNC 球坐标推力矢量(Azimuth/Elevation/Magnitude) |


### ManeuverFinite(有限推力机动)


| 文件                                        | 用途简述                                  |
| ----------------------------------------- | ------------------------------------- |
| `mcs-maneuver-finite-along-velocity.json` | 沿速度有限推力(自定义 Propagator + EngineModel) |
| `mcs-maneuver-finite-thrust-vector.json`  | VNC 推力方向有限推力(缺省质点积分器)                 |


### TargetSequence(目标序列/微分修正)


| 文件                                         | 用途简述                           |
| ------------------------------------------ | ------------------------------ |
| `mcs-hohmann-target-min.json`              | 霍曼转移:两脉冲 VNC-X 自变量,约束远地点半径与偏心率 |
| `mcs-target-along-velocity-sma.json`       | 沿速度脉冲:自变量 Delta-V 模,约束半长轴      |
| `mcs-target-propagate-duration-epoch.json` | 递推段:自变量 Duration,约束末历元 Epoch   |
| `mcs-target-dro-moon-libration-min.json`   | DRO 设计:Moon Libration 初值 + CisLunar,约束 Vx=0 |


### Earth-Moon Libration(地月平动点 / Halo)


| 文件 | 用途简述 |
| --- | --- |
| `earth-moon-libration/mcs-target-eml-l1-halo-min.json` | L1 Halo 微分修正:Moon L1 系,约束 L1_Vx=0 |
| `earth-moon-libration/mcs-target-eml-l2-halo-min.json` | L2 Halo 微分修正:Moon L2 系,约束 L2_Vx=0 |
| `earth-moon-libration/mcs-propagate-eml-l1-halo-min.json` | L1 Halo 仅递推:Duration 1051200 s |
| `earth-moon-libration/mcs-propagate-eml-l2-halo-min.json` | L2 Halo 仅递推:Duration 1227600 s |

专题文档:[docs/earth-moon-libration.md](docs/earth-moon-libration.md)。DRO 示例见上表 `mcs-target-dro-moon-libration-min.json`。

### Earth-Moon Transfer(地月转移 / 近月制动 / 环月轨道,脉冲)


| 文件 | 用途简述 |
| --- | --- |
| `earth-moon-transfer/mcs-propagate-e2m-cislunar-rmag-320000km.json` | CisLunar 递推至地心距 320000 km,Results 附月球赤经/赤纬差 |
| `earth-moon-transfer/mcs-propagate-e2m-tli-impulsive-moon-flyby.json` | 停泊轨道 + TLI 脉冲(VNC-X 4000 m/s)+ 地心/月心积分器切换至近月点(飞越,含自定义 `CisLunarMoonCentered`) |
| `earth-moon-transfer/mcs-target-e2m-coarse-delta-radec.json` | 第 1 步粗修正:自变量赤经/赤纬,约束 Delta_Right_Asc/Delta_Declination = 0 |
| `earth-moon-transfer/mcs-target-e2m-bplane-inc-alt.json` | 第 2/3 步:DC-BPlane(BDotR/BDotT/Epoch)→ DC-IncAlt(倾角 90 deg / 高度 100 km / Epoch) |
| `earth-moon-transfer/mcs-target-e2m-moon-brake-lunar-orbit.json` | 全流程:地月转移 Target + 近月制动 Target(VNC(Moon)-X,约束 e=0)+ 环月 1 天(Moon Inertial 根数 Results) |
| `earth-moon-transfer/mcs-e2m-moon-brake-lunar-orbit-nominal-mooncb.json` | 标称运行(无 Target),`CentralBody: "Moon"`,段状态与 Positions 直接为月心系 |
| `earth-moon-transfer/mcs-target-lunar-orbit-raise-apoapsis.json` | 环月轨道:月心 Keplerian 初值 + `Moon_Hpop_De430` 递推 1 天 + 抬远月点脉冲(约束 AltitudeOfApoapsis = 500 km) |

专题文档:[docs/earth-moon-transfer.md](docs/earth-moon-transfer.md)。


### Follow(跟随段)


| 文件                        | 用途简述                              |
| ------------------------- | --------------------------------- |
| `mcs-follow-twobody.json` | 跟随 TwoBody Leader + 脉冲机动 + 递推至远地点 |


## 上游参考示例 (raw/Astrogator)


| 子目录                  | 内容                                                   |
| -------------------- | ---------------------------------------------------- |
| `Propagate/`         | 各中心天体/积分器/终止条件;含地月转移 `E2M_Duration_230613`, `E2M_RMag_TrueAnomaly_230619`, `HpopCislunar_RMag/Epoch/Anomaly2_*`,环月 `Moon_Hpop_De430_250306`, `Moon_NonSphere_250306`,平动点 `EarthMoonL1_250704`, `EarthMoonL2_250704`, `EarthMoonLibration_250702` |
| `ManeuverImpulsive/` | 脉冲机动姿态(ThrustVector VNC/VVLH/J2000/VNC(Moon)/VVLH(Moon)/LVLH(Moon)、VelocityVector 等) |
| `Maneuver/`          | 有限推力机动                                               |
| `Target/`            | 微分修正(霍曼、地月 E2M、L1/L2 Halo、DRO 等);含 `E2M_DeltaDecRA_250328`, `E2M_BPlane_250612`, `E2M_MoonBrake_250616(-2)`, `EarthMoonL1_250704`, `EarthMoonL2_250704`, `EarthMoonLibration_250702` |
| `Follow/`            | 跟随 Leader 实体(含 TwoBody 定义)                           |


编写新 MCS 时,优先在 `raw/Astrogator/` 中查找相近场景 JSON,再按需裁剪为 fixture。