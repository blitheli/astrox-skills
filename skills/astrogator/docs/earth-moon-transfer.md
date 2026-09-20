# 地月转移与环月轨道设计 (Earth-Moon Transfer, Impulsive)

本文档说明如何在 Astrogator MCS 中**自主完成**地月转移轨道设计、近月制动(LOI)与环月轨道递推,并从响应中提取所需数据。所有示例通过 `POST /Astrogator/RunMCS` 调用,**轨道机动仅考虑脉冲**(`ManeuverImpulsive`)。

上游算例来源:`raw/Astrogator/Target/E2M_*.json`、`raw/Astrogator/Propagate/E2M_*.json`、`raw/Astrogator/Propagate/HpopCislunar_*.json`(即 `astrox.aerospace.tests/Astrogator` 中的地月转移测试)。本文所有 fixture 均已对线上服务实测通过。

## 1. 任务分段与总体流程

```mermaid
flowchart LR
  A[近地出发状态<br/>Earth Inertial, Spherical] --> B[Propagate CisLunar<br/>至地心距 320000 km]
  B --> C[Propagate CisLunar<br/>至近月点 Periapsis Moon]
  C --> D[ManeuverImpulsive<br/>VNC Moon -X 制动]
  D --> E[Propagate Moon_Hpop_*<br/>环月轨道]
  subgraph T1 [TargetSequence 地月转移]
    A
    B
    C
  end
  subgraph T2 [TargetSequence 近月制动]
    D
  end
```

| 阶段 | 段类型 | 积分器 | 终止条件 | 设计变量 / 约束 |
| --- | --- | --- | --- | --- |
| 近地出发 | `InitialState` | — | — | 自变量:`InitialState.Spherical.Right_Asc / Decl / VMag` |
| 转移前半段 | `Propagate` | `CisLunar` | Scalar `PointElement.Magnitude = 320000 km` | 仅分段,无约束 |
| 近月段 | `Propagate` | `CisLunar` | `Periapsis(Moon)` + `Cartographic.Height(Moon)=0` + `Duration` 保护 | 约束:`BDotR/BDotT/Epoch` 或 `Height/Inclination/Epoch` |
| 近月制动 | `ManeuverImpulsive` | — | — | 自变量:`ImpulsiveMnvr.Cartesian.X`(VNC(Moon));约束:月心 `Eccentricity = 0` |
| 环月轨道 | `Propagate` | `Moon_Hpop_default_v10` / `Moon_Hpop_De430` / `Moon_Point_Mass` | `Duration` / `Apoapsis(Moon)` 等 | 读取 Moon Inertial 轨道根数 |

两种出发建模方式:

1. **近地点状态直接出发(推荐,上游算例采用)**:`InitialState.Element.$type = "Spherical"`,`HorizFPA = 0` 表示位于近地点,`RadiusMagnitude` 为近地点地心距(如 6678137 m = 300 km 高度),`VelocityMagnitude` 约 10.84~10.92 km/s。赤经/赤纬/速度大小作为微分修正自变量。
2. **停泊轨道 + TLI 脉冲**:`InitialState`(Cartesian) → `Propagate`(滑行) → `ManeuverImpulsive`(VNC-X) → `Propagate`。自变量可改为 `ImpulsiveMnvr.Cartesian.X` 与滑行段 `StopConditions.Duration`。fixture `mcs-propagate-e2m-tli-impulsive-moon-flyby.json` 给出仅递推版本。

## 2. 关键建模要素

### 2.1 积分器(缺省可直接引用,无需传 `Propagators`)

| `PropagatorName` | 中心天体 | 力模型 | 用途 |
| --- | --- | --- | --- |
| `CisLunar` | Earth | WGS84 8x8 + 月球/太阳第三体(DE430) | 地月转移全程(含近月段) |
| `Moon_Hpop_default_v10` | Moon | LP150Q 48x48 + 地球/太阳第三体 + 球形光压 | 环月轨道高精度递推 |
| `Moon_Hpop_De430` | Moon | GL0900D 48x48 + 地球/太阳第三体 + 光压 | 环月轨道高精度递推(与内部 DE430 历表一致) |
| `Moon_NonSphere` | Moon | GL0900D 48x48(仅非球形) | 环月轨道 |
| `Moon_Point_Mass` | Moon | 月球质点 | 环月轨道 Target 内快速迭代 |
| 自定义 `CisLunarMoonCentered` | Moon | GL0900D 8x8 + 地球/太阳第三体 | 需在顶层 `Propagators` 中定义,见 flyby fixture |

要点:上游算例的近月段仍用地心 `CisLunar` 递推到近月点,再切换月心积分器做环月递推;两种中心天体的积分器可在同一 MCS 中混用,段间状态自动转换。

### 2.2 坐标系与推力坐标轴

| 名称 | 说明 |
| --- | --- |
| `Earth Inertial` | 地心惯性系,初始状态与地心结果 |
| `Moon Inertial` | 月心惯性系,月心轨道根数(`CoordSystemName`)与月心积分器 |
| `VNC` / `VNC(Earth)` | 地心 VNC 推力轴(V 沿速度,N 沿轨道法向,C 补全) |
| `VNC(Moon)` | 月心 VNC 推力轴,近月制动使用;`X < 0` 为反速度方向制动 |
| `VVLH(Moon)` / `LVLH(Moon)` | 月心轨道坐标系推力轴(可选) |

**输出坐标系由顶层 `CentralBody` 决定**:所有段的 `InitialState/FinalState`(Cartesian/Keplerian/Spherical/Geodetic)与 `Positions` 都在 `CentralBody` 的 Inertial 系下。地月任务两种选择:

| `CentralBody` | 段状态输出 | 环月轨道根数获取方式 | fixture |
| --- | --- | --- | --- |
| `Earth`(缺省) | Earth Inertial;环月段 `FinalState.Keplerian` 为地心根数,**不可直接用作环月轨道根数** | 在段 `Results` 中声明 `ModifiedKeplerianElement`/`KeplerianElement`(`CoordSystemName: "Moon Inertial"`, `Mu: 4902800305555.4`) | `mcs-target-e2m-moon-brake-lunar-orbit.json` |
| `Moon` | Moon Inertial;初始段仍可在 `Earth Inertial` 下给定 | 直接读 `FinalState.Keplerian`、`Geodetic_Altitude`,且 `Positions.CzmlPositions[].CentralBody = "Moon"` | `mcs-e2m-moon-brake-lunar-orbit-nominal-mooncb.json` |

### 2.3 近月段终止条件(三者并用)

```json
"StopConditions": [
  { "$type": "Periapsis", "Name": "近月点", "CentralBodyName": "Moon", "Mu": 4902800305555.4, "Tolerance": 1e-6 },
  { "$type": "Scalar", "Name": "高度", "Trip": 0, "Tolerance": 0.001,
    "UserCalcObject": { "$type": "Cartographic", "CentralBodyName": "Moon", "ComponentName": "Height" } },
  { "$type": "Duration", "Name": "持续时间", "Trip": 345600 }
]
```

- `Periapsis(Moon)`:正常出口,到达月心近月点。
- `Cartographic.Height(Moon) = 0`:撞月保护,迭代初期轨道可能直接撞月。
- `Duration`:时长保护,轨道远离月球时避免长时间积分。
- 先递推到地心距 320000 km 再启用 `Periapsis(Moon)`:避免在近地段就误触发月心近点判定,同时给微分修正一个稳定的分段点。
- 判断实际出口:读取该段结果的 `StoppingConditionName`,正常收敛时应为 `Periapsis` 段名(如 `"近月点"`/`"Periapsis"`);若为高度或时长名称,说明当前迭代未真正到达近月点。

### 2.4 常用标量计算对象 (CalcScalar,`Results` 与 Scalar 终止条件共用)

| `$type` | `ComponentName` | 必填字段 | 单位 | 典型用途 |
| --- | --- | --- | --- | --- |
| `DeltaSpherical` | `Delta_Right_Asc`, `Delta_Declination`, `Delta_RMag` | `CentralBodyName: "Moon"`, `ParentCbName: "Earth"` | deg, deg, m | 粗修正:飞行器与月球相对地球的赤经/赤纬差 → 0 |
| `BPlane` | `BDotR`, `BDotT` | `CentralBodyName: "Moon"`, `Mu` | m | B 平面瞄准(线性好、易收敛) |
| `Cartographic` | `Height`, `Latitude`, `Longitude` | `CentralBodyName: "Moon"` | m, deg | 近月点高度约束、撞月保护 |
| `ModifiedKeplerianElement` | `Inclination`, `Eccentricity`, `SemimajorAxis`, `AltitudeOfPeriapsis`, `AltitudeOfApoapsis`, `RadiusOfApoapsis`, `Period`, `TrueAnomaly`, `RAAN`, `ArgumentOfPeriapsis` | `CoordSystemName: "Moon Inertial"`, `Mu` | deg, —, m, s | 月心轨道根数约束与读取 |
| `KeplerianElement` | 同上,另有 `ElementType`(`Osculating`/`KozaiIzsakMean`) | `CoordSystemName`, `Mu` | 同上 | 与 Modified 等价的另一写法 |
| `SphericalElement` | `VelocityMagnitude`, `VelocityAzimuth`, `RightAscension`, `Declination`, `RadiusMagnitude`, `HorizFPA` | `CoordSystemName` | m/s, deg, m | 读取近月点月心速度(估算制动量) |
| `PointElement` | `X/Y/Z/Vx/Vy/Vz/Magnitude` | `CoordSystemName` | m, m/s | 地心距分段终止(`Magnitude`) |
| `Epoch` | — | 仅 `Name` | UTC 字符串 | 到达时刻约束(`DesiredValue` 为 ISO8601 字符串,`Tolerance` 单位 s) |
| `Duration` | — | 仅 `Name` | s | 读取段飞行时长 |

`Results` 中每个对象的 `Name` 即输出字典 `Results` 的键,也是微分修正 `Results[].Name` 引用的名称;支持中文键名。角度类标量作为**终止条件**时建议显式 `"Dimension": "Angle"`。

## 3. 微分修正策略(三级修正 + 制动圆化)

B 平面参数对出发赤经/赤纬/速度近似线性,而近月点高度/倾角在远离月球时高度非线性,因此上游采用**先粗后精**的多 Profile 顺序修正。同一 `TargetSequence.Profiles` 内多个 `DifferentialCorrector` 按顺序执行,后一个在前一个收敛结果上继续。

| 步骤 | 自变量(`ParentName` = 初始段) | 约束(`ParentName` = 近月段) | `MaxStep` | 迭代(实测) | fixture |
| --- | --- | --- | --- | --- | --- |
| 1 粗修正 | `Right_Asc`, `Decl` | `Delta_Declination = 0`, `Delta_Right_Asc = 0`(Tol 0.1 deg) | 30 deg | 9 | `mcs-target-e2m-coarse-delta-radec.json` |
| 2 B 平面 | `Right_Asc`, `Decl`, `VMag` | `BDotR = 5500 km`, `BDotT = 0`(Tol 100 m), `Epoch = 到达时刻`(Tol 1 s) | 5 deg / 10 m/s | 12 | `mcs-target-e2m-bplane-inc-alt.json` |
| 3 精修正 | 同上 | `Inclination = 90 deg`(Tol 0.1), `Altitude = 100 km`(Tol 1 m), `Epoch`(Tol 1 s) | 1 deg / 10 m/s | 5 | 同上(第 2 个 Profile) |
| 4 制动圆化 | `ImpulsiveMnvr.Cartesian.X`(VNC(Moon)) | 月心 `Eccentricity = 0`(Tol 0.001) | 100 m/s | 1~3 | `mcs-target-e2m-moon-brake-lunar-orbit.json` |

推荐参数:`Perturbation: 0.1`,`Tolerance: 0.0001`(自变量),`MaximumIterations: 50`,`OnlyStoreFinalResults: false`(保留迭代历史便于诊断),`Action: "RunActiveOperators"`(缺省即迭代;`RunNominalSequence` 只跑一遍不迭代,`OperatorResults` 为空)。

### 3.1 初值与目标值的物理估算

记月心引力常数 `mu_M = 4902.8 km^3/s^2`,目标近月点半径 `r_p = 1737.4 + h_p`(km),近月点月心速度 `v_p`(由近月段 `Results` 中 `SphericalElement.VelocityMagnitude`(Moon Inertial) 读取):

- 双曲线超速:`v_inf = sqrt(v_p^2 - 2*mu_M/r_p)`,地月转移典型 0.8~1.0 km/s。
- B 平面模量:`B = r_p * sqrt(1 + 2*mu_M/(r_p*v_inf^2))`。B 矢量方向决定月心轨道倾角:`BDotT = 0, BDotR = B` 得到近极月轨道(实测第 2 步收敛后倾角 90.00 deg);`BDotR ≈ 0, BDotT = B` 得到近赤道轨道(nominal-mooncb fixture:`BDotR = -135 km, BDotT = 5552 km`,倾角 2.34 deg)。实测:`h_p = 100.19 km`、`v_p = 2459.67 m/s` 时公式给出 `B = 5349.61 km`,与响应 `BDotR = 5349614 m` 一致;粗略目标 5500 km 对应 `h_p ≈ 190 km`,再由第 3 步精修至 100 km。
- 圆化制动量:`dV ≈ v_p - sqrt(mu_M/r_p)`,沿 `VNC(Moon)` 的 `X` 取负值。实测估算 826.2 m/s,微分修正结果 826.07 m/s。
- 环月霍曼抬轨:`dV = sqrt(mu_M*(2/r1 - 1/a_t)) - sqrt(mu_M/r1)`,`a_t = (r1 + r2)/2`。100 km 圆轨道抬远月点至 500 km 估算 78.3 m/s,微分修正结果 77.95 m/s。

## 4. 从响应中提取数据

响应顶层:`IsSuccess`、`Message`、`MainSequenceResults[]`、`Positions`。各段结果以 `$type` 区分:

| `$type` | 关键字段 | 说明 |
| --- | --- | --- |
| `TargetSequenceResult` | `OperatorResults[]`, `SegmentResults[]` | `SegmentResults` 为**收敛后**最后一次标称运行的各段结果 |
| `PropagateResult` | `StoppingConditionName`, `StoppedOnMaximumDuration`, `DurationSec`, `Results{}`, `InitialState`, `FinalState` | `Results{}` 键为输入 `Results[].Name` |
| `ManeuverImpulsiveResult` | `ManeuverInformation`, `Results{}`, `FinalState` | `ManeuverInformation.DeltaV_Mag`(m/s)、`DeltaV_VNC[6]`、`DeltaV_Inertial[6]`(X,Y,Z,Az,El,Mag)、`EstimatedFuelUsed`(kg) |
| (InitialState) | `InitialState`/`FinalState` | 无 `$type`,`TypeName: "InitialState"` |

`OperatorResults[]`(`$type: "DifferentialCorrectorResults"`):

| 字段 | 说明 |
| --- | --- |
| `Converged`, `TotalIterations` | 收敛标志与迭代次数 |
| `ControlParameters[].Name/ParentName/InitialValue/FinalValue/Correction/Values[]` | 自变量收敛值(字符串),`Values` 为迭代历史 |
| `Results[].Name/DesiredValue/CurrentValue/Difference/Tolerance/Values[]` | 约束满足情况;`Epoch` 类型的 `Difference` 与 `Values` 为相对期望值的秒数 |

`SegmentState`(`InitialState`/`FinalState`):`Epoch`、`CoordSystemName`、`Cartesian{X,Y,Z,Vx,Vy,Vz}`、`Keplerian{SemiMajorAxis,Eccentricity,Inclination,RAAN,ArgOfPeriapsis,TrueAnomaly,Period}`、`Spherical{RightAscension,Declination,RadiusMagnitude,HorizFPA,VelocityAzimuth,VelocityMagnitude}`、`DryMass/FuelMass`、`Geodetic_*`。**坐标系为顶层 `CentralBody` 的 Inertial 系**(见 2.2)。

`Positions.CzmlPositions[]`:按中心天体/段分为多个区间,每个含 `interval`、`epoch`、`referenceFrame`、`cartesianVelocity`(扁平数组 `[t, x, y, z, vx, vy, vz, ...]`,t 为相对 `epoch` 的秒,单位 m、m/s)。详见 `shared-docs/api-schemas/CzmlPositionOut.md`。仅需标量结果时设 `ComputeCzmlPositions: false`。

### 4.1 jq 提取示例(以全流程 fixture 为例)

```bash
OUT=/tmp/e2m.json
curl -s "${BASE_URL}/Astrogator/RunMCS" -X POST -H 'Content-Type: application/json' \
  --data-binary @skills/astrogator/fixtures/earth-moon-transfer/mcs-target-e2m-moon-brake-lunar-orbit.json > "$OUT"

# 1) 成功与收敛
jq '{IsSuccess, ops: [.MainSequenceResults[] | .OperatorResults[]? | {Name, Converged, TotalIterations}]}' "$OUT"
# 2) 收敛后的出发状态(赤经/赤纬/速度) 与 地心根数
jq '.MainSequenceResults[0].SegmentResults[0].FinalState | {Epoch, Spherical, Keplerian}' "$OUT"
# 3) 近月点约束值(高度 m、月心倾角 deg、到达历元、B 平面 m、月心速度 m/s)
jq '.MainSequenceResults[0].SegmentResults[2] | {StoppingConditionName, DurationSec, Results}' "$OUT"
# 4) 近月制动 Delta-V
jq '.MainSequenceResults[1].SegmentResults[0].ManeuverInformation | {DeltaV_Mag, DeltaV_VNC, EstimatedFuelUsed}' "$OUT"
# 5) 环月轨道 1 天后的月心根数(来自 Results, Moon Inertial)
jq '.MainSequenceResults[2].Results' "$OUT"
# 6) 轨迹区间与点数
jq '.Positions.CzmlPositions[] | {interval, CentralBody, n: (.cartesianVelocity|length/7)}' "$OUT"
```

### 4.2 结果判定与排错

- 成功:HTTP 200,`IsSuccess: true`,每个 `OperatorResults[].Converged: true`,且各约束 `|Difference| <= Tolerance`。
- 近月段 `StoppingConditionName` 应为 `Periapsis` 类型的段名;若为 `Duration`/高度名称,说明该次运行没有到达近月点(初值太差或保护条件先触发)。
- 不收敛时:先跑第 1 步粗修正得到赤经/赤纬,再进入 B 平面;降低 `MaxStep`;适当放宽 `Tolerance`(B 平面 100~500 m、高度 1~200 m);检查 `Epoch` 期望值是否与初始历元相差合理的飞行时间(4~5 天)。
- 赤经收敛值可能以等价负角度输出(如 -151.49 deg 即 208.51 deg)。
- 使用 `CentralBody: "Earth"` 时不要把环月段 `FinalState.Keplerian` 当作月心根数;应在 `Results` 中声明 Moon Inertial 的 Keplerian 标量,或改用 `CentralBody: "Moon"`。

## 5. 实测参考值(线上服务)

| fixture | 关键结果 |
| --- | --- |
| `mcs-propagate-e2m-cislunar-rmag-320000km.json` | 飞行 206061.57 s 到达 320000 km;上游与 STK 12.8 对比位置 ~1e-2 m |
| `mcs-propagate-e2m-tli-impulsive-moon-flyby.json` | TLI 4000 m/s 后地心 e = 1.305(双曲线);近月点距月面 48175 km(飞越);段切换正常 |
| `mcs-target-e2m-coarse-delta-radec.json` | 9 次迭代;RA 208.51 deg(输出 -151.49),Dec 1.683 deg;末段出口为撞月高度 0(粗修正阶段正常) |
| `mcs-target-e2m-bplane-inc-alt.json` | DC-BPlane 12 次、DC-IncAlt 5 次;RA 227.3556, Dec -15.9863, VMag 10838.588 m/s;高度 100000.4 m,倾角 90.000 deg,到达 2022-06-24T20:00:00Z |
| `mcs-target-e2m-moon-brake-lunar-orbit.json` | 转移 DC 各 3 次;近月点高度 100189 m,月心速度 2459.67 m/s;制动 826.07 m/s(VNC(Moon)-X),月心 e = 2.2e-4;环月 1 天后 a = 1838.19 km, e = 0.0016, i = 89.93 deg, 周期 7072 s |
| `mcs-e2m-moon-brake-lunar-orbit-nominal-mooncb.json` | 无 Target;近月点高度 100004 m,飞行 403200 s;制动 814.47 m/s;段状态与 Positions 均为 Moon Inertial |
| `mcs-target-lunar-orbit-raise-apoapsis.json` | 100 km 圆极轨道 HPOP 1 天后 a = 1836.86 km;抬高远月点至 500 km 需 77.95 m/s(4 次迭代),解析霍曼估算 78.3 m/s |

## 6. Fixtures 与上游对照

| fixture(`fixtures/earth-moon-transfer/`) | 类型 | 上游 raw 文件 | 说明 |
| --- | --- | --- | --- |
| `mcs-propagate-e2m-cislunar-rmag-320000km.json` | Propagate | `Propagate/HpopCislunar_RMag_230522.json` | CisLunar 递推至 320000 km,附赤经/赤纬差 Results |
| `mcs-propagate-e2m-tli-impulsive-moon-flyby.json` | Propagate + Impulsive | `Propagate/E2M_RMag_TrueAnomaly_230619.json` | 停泊 + TLI 脉冲 + 地心/月心积分器切换 |
| `mcs-target-e2m-coarse-delta-radec.json` | Target | `Target/E2M_DeltaDecRA_250328.json` | 第 1 步粗修正 |
| `mcs-target-e2m-bplane-inc-alt.json` | Target(2 Profiles) | `Target/E2M_BPlane_250612.json` | 第 2/3 步 B 平面 + 倾角高度 |
| `mcs-target-e2m-moon-brake-lunar-orbit.json` | Target x2 + Propagate | `Target/E2M_MoonBrake_250616.json` | 全流程:转移 + 制动 + 环月 |
| `mcs-e2m-moon-brake-lunar-orbit-nominal-mooncb.json` | Propagate + Impulsive | `Target/E2M_MoonBrake_250616-2.json` | 标称运行,`CentralBody: "Moon"` 输出 |
| `mcs-target-lunar-orbit-raise-apoapsis.json` | Propagate + Target | `Propagate/Moon_Hpop_De430_250306.cs`, `ManeuverImpulsive/ThrustVectorVNC(Moon)_230608.json` | 环月轨道 HPOP 递推 + 抬远月点脉冲 |

其它相关上游算例:`Propagate/HpopCislunar_Epoch_230522.json`(到达历元终止)、`Propagate/HpopCislunar_Anomaly2_230606.json`(地心真近点角终止,需 `Dimension: Angle`)、`Propagate/E2M_Duration_230613.json`(全时长终止,精度对比)、`Propagate/Moon_NonSphere_250306.cs`。

## 7. curl 快速调用

```bash
export BASE_URL=http://astrox.cn:8765
FX=skills/astrogator/fixtures/earth-moon-transfer

# 第 1 步:粗修正
curl "${BASE_URL}/Astrogator/RunMCS" -X POST -H 'Content-Type: application/json' \
  --data-binary "@${FX}/mcs-target-e2m-coarse-delta-radec.json" | jq '.MainSequenceResults[0].OperatorResults[] | {Name, Converged, TotalIterations, ctrl: [.ControlParameters[] | {Name, FinalValue}]}'

# 第 2/3 步:B 平面 + 倾角高度
curl "${BASE_URL}/Astrogator/RunMCS" -X POST -H 'Content-Type: application/json' \
  --data-binary "@${FX}/mcs-target-e2m-bplane-inc-alt.json" | jq '.MainSequenceResults[0].SegmentResults[2].Results'

# 全流程:转移 + 近月制动 + 环月 1 天
curl "${BASE_URL}/Astrogator/RunMCS" -X POST -H 'Content-Type: application/json' \
  --data-binary "@${FX}/mcs-target-e2m-moon-brake-lunar-orbit.json" | jq '{dv: .MainSequenceResults[1].SegmentResults[0].ManeuverInformation.DeltaV_Mag, lunar: .MainSequenceResults[2].Results}'

# 环月轨道:抬高远月点
curl "${BASE_URL}/Astrogator/RunMCS" -X POST -H 'Content-Type: application/json' \
  --data-binary "@${FX}/mcs-target-lunar-orbit-raise-apoapsis.json" | jq '.MainSequenceResults[2] | {ops: .OperatorResults, res: .SegmentResults[1].Results}'
```

## 8. 自主设计新任务的步骤(供 Agent 执行)

1. 确定出发条件:历元、近地点高度(`RadiusMagnitude`)、速度方位角(`VelocityAzimuth`,决定地心轨道倾角)、初猜 `VelocityMagnitude`(10.85~10.92 km/s)。
2. 确定到达条件:近月点高度 `h_p`、月心倾角、到达历元(出发后 4~5 天)。
3. 复制 `mcs-target-e2m-coarse-delta-radec.json`,替换历元与出发参数,运行得到赤经/赤纬。
4. 复制 `mcs-target-e2m-bplane-inc-alt.json`,把第 3 步结果填入初始段;按 3.1 估算 `BDotR`(或直接用 5500 km),设置 `Epoch` 期望值;运行,读取收敛的 RA/Dec/VMag 与近月点 `Results`。
5. 复制 `mcs-target-e2m-moon-brake-lunar-orbit.json`,填入第 4 步收敛初值;按 3.1 用近月点 `月心速度` 估算制动量作为 `AttitudeControl.X` 初值;设置环月段积分器与时长;运行。
6. 按第 4 节提取:出发状态、到达时刻、近月点参数、制动 ΔV、环月轨道根数、轨迹(`Positions`)。
7. 如需环月轨道调整(抬轨/降轨/圆化),参照 `mcs-target-lunar-orbit-raise-apoapsis.json` 在月心 `Apoapsis`/`Periapsis` 终止处施加 `VNC(Moon)` 脉冲并约束 `AltitudeOfApoapsis`/`AltitudeOfPeriapsis`/`Eccentricity`。
