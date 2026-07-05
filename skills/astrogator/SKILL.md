---
name: astrogator
description: 运行轨道机动序列(MCS),功能与 STK Astrogator 基本一致。支持初始状态、轨道递推、脉冲/有限推力机动、目标序列(微分修正)、跟随段等。含地月平动点 L1/L2 Halo 轨道设计(递推与微分修正)、DRO、霍曼转移、地月转移。用户需要轨道设计、机动序列仿真时使用。
---

# Astrogator 轨道机动序列技能 (Astrogator MCS)

通过 Astrox WebAPI 的 `POST /Astrogator/RunMCS`,按 **Mission Control Sequence (MCS, 轨道机动序列)** 依次执行各飞行段,完成轨道递推、机动与目标求解。概念与用法与 **STK Astrogator** 基本一致:以 `MainSequence` 定义段序列,在 `TargetSequence` 中配置 **Differential Corrector(微分修正)** 求解机动参数。

## 核心指令 (Core Instructions)

1. **输入解析**:识别中心天体 `CentralBody`、主序列 `MainSequence`,以及可选的 `Entities`、`Propagators`、`EngineModels`。
2. **段类型判定**:根据任务选择段类型——`InitialState`(初始状态)、`Propagate`(递推)、`ManeuverImpulsive`(脉冲机动)、`ManeuverFinite`(有限推力机动)、`TargetSequence`(目标序列/微分修正)、`Sequence`(子序列)、`Follow`(跟随其它实体)、`Stop`(终止)。
3. **初始状态**:首段通常为 `InitialState`,`InitialState.Element` 支持 `$type` 为 `Keplerian`、`Cartesian` 或 `Spherical`。
4. **积分器**:`PropagatorName` 引用内置缺省积分器(如 `Earth_Point_Mass`、`Earth_Hpop_default_v10`)或输入中的 `Propagators` 自定义积分器。
5. **终止条件**:`StopConditions` 常用 `$type` 包括 `Duration`(固定时长,s)、`Epoch`(历元)、`Periapsis`、`Apoapsis`、`Scalar`(标量条件,如高度、速度等)。
6. **目标求解**:在 `TargetSequence.Profiles` 中配置 `$type: "DifferentialCorrector"`,通过 `ControlParameters`(自变量)与 `Results`(约束)迭代求解。
7. **API 调用逻辑**:向 `{BASE_URL}/Astrogator/RunMCS` 发送 `POST`,`Content-Type: application/json`。
8. **结果判定**:先判 HTTP 状态,再判 `IsSuccess`;成功时读取 `MainSequenceResults`(各段结果)与 `Positions`(Czml 位置序列,可选)。

## API 规范 (Tool Definition)

### 接口地址

`POST /Astrogator/RunMCS`

### 输入参数结构 (JSON)


| 参数名                    | 类型           | 必须  | 缺省         | 说明                                                          |
| ---------------------- | ------------ | --- | ---------- | ----------------------------------------------------------- |
| `CentralBody`          | string       | 否   | `Earth`    | 中心天体(Earth, Moon, Mars, Sun 等)                              |
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
| `CoordSystemName` | string | —   | 坐标系,如 `Earth Inertial`, `Moon L1`, `Moon L2`, `Moon Libration` |
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


| 名称                       | 说明                      |
| ------------------------ | ----------------------- |
| `Earth_Point_Mass`       | 地球质点引力                  |
| `Earth_Hpop_default_v10` | 地球 HPOP(含阻力/光压等,版本 v10) |
| `Moon_Point_Mass`        | 月球质点                    |
| `CisLunar`               | 地月多体(CR3BP 等),用于地月转移、DRO、L1/L2 Halo 等 |
| `Sun_Point_Mass`         | 日心质点                    |


自定义积分器通过顶层 `Propagators` 数组定义,段内 `PropagatorName` 引用其 `Name`。

### 脉冲机动姿态 (AttitudeControl)


| $type            | 说明         | 典型字段                                                                    |
| ---------------- | ---------- | ----------------------------------------------------------------------- |
| `VelocityVector` | 沿/反速度方向    | `DeltaVMagnitude`(m/s)                                                  |
| `ThrustVector`   | 指定推力坐标系下矢量 | `ThrustAxesName`(VNC, VVLH, J2000 等), `CoordType`, X/Y/Z 或 Spherical 分量 |
| `Attitude`       | 姿态角/四元数    | EulerAngles 或 Quaternion                                                |


微分修正自变量命名示例:`ImpulsiveMnvr.Cartesian.X`、`ImpulsiveMnvr.Spherical.Magnitude`。

### TargetSequence / DifferentialCorrector


| 字段                  | 说明                                                                   |
| ------------------- | -------------------------------------------------------------------- |
| `Segments`          | 目标序列内的段列表(机动、递推等)                                                    |
| `Profiles[].$type`  | 通常为 `DifferentialCorrector`                                          |
| `ControlParameters` | 自变量:Name, ParentName, InitialValue, Tolerance, MaxStep, Perturbation |
| `Results`           | 约束:Name, ParentName, DesiredValue, Tolerance                         |


段内 `Results` 定义可读取的标量(如 `ModifiedKeplerianElement` 的 RadiusOfApoapsis、Eccentricity),供 Profile 约束引用。

### 响应数据结构 (MCSOutput)


| 字段名                   | 类型      | 说明                                           |
| --------------------- | ------- | -------------------------------------------- |
| `IsSuccess`           | boolean | 是否成功                                         |
| `Message`             | string  | 失败原因或提示                                      |
| `MainSequenceResults` | array   | 各段执行结果(递推时长、机动 Delta-V、目标序列收敛信息等)            |
| `Positions`           | object  | Czml 格式位置序列(`ComputeCzmlPositions=true` 时返回) |


`Positions` 结构与 Czml 位置输出类似;详见 shared-docs/api-schemas/CzmlPositionOut.md。

## 注意事项

- 距离单位均为 **米(m)**,速度为 **m/s**,角度为 **deg**,时长为 **s**,质量为 **kg**。
- 每段必须设置唯一 `Name`;微分修正的 `ParentName` 须与段名一致。
- `MainSequence` 第一段通常为 `InitialState`;`Follow` 段可省略 InitialState,直接跟随 `Entities` 中的 Leader。
- 使用缺省积分器时无需传 `Propagators`;地月/日心等多体任务可能需自定义 `Propagators` 与 `Entities`。
- 仅关心段标量结果、不需轨迹点时,设 `ComputeCzmlPositions: false` 可减小响应体积。
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
3. 请求构造
  - 按 OpenAPI 契约原样传参;`$type`  discriminator 不可省略
4. 结果判定
  - HTTP 200 且 `IsSuccess = true`
  - TargetSequence 失败时查看 Profile 迭代信息与 `Message`
5. 输出归一化
  - 摘要:中心天体、段数、是否含目标求解
  - 核心输出:末段轨道参数、机动 Delta-V、约束满足情况
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


### Follow(跟随段)


| 文件                        | 用途简述                              |
| ------------------------- | --------------------------------- |
| `mcs-follow-twobody.json` | 跟随 TwoBody Leader + 脉冲机动 + 递推至远地点 |


## 上游参考示例 (raw/Astrogator)


| 子目录                  | 内容                                                   |
| -------------------- | ---------------------------------------------------- |
| `Propagate/`         | 各中心天体/积分器/终止条件;含 `EarthMoonL1_250704`, `EarthMoonL2_250704`, `EarthMoonLibration_250702` |
| `ManeuverImpulsive/` | 脉冲机动姿态(ThrustVector VNC/VVLH/J2000、VelocityVector 等) |
| `Maneuver/`          | 有限推力机动                                               |
| `Target/`            | 微分修正(霍曼、地月 E2M、L1/L2 Halo、DRO 等);含 `EarthMoonL1_250704`, `EarthMoonL2_250704`, `EarthMoonLibration_250702` |
| `Follow/`            | 跟随 Leader 实体(含 TwoBody 定义)                           |


编写新 MCS 时,优先在 `raw/Astrogator/` 中查找相近场景 JSON,再按需裁剪为 fixture。