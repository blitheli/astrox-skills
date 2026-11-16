---
name: celestial-transfer-dsm
description: 行星与小行星之间带一次脉冲深空机动(1-DSM)的转移计算。出发/到达天体可为行星(Earth,Mars,Ceres 等)或近地小行星(NEA 编号/名称)。用户需要在 Lambert 转移上增加一次深空机动、比较弹道解与 DSM 解、或按给定 Vinf/u/v/η 求值时使用。
---

# 天体间 1-DSM 转移技能 (Celestial Transfer DSM)

通过 Astrox WebAPI 的 `POST /celestial/TransferBetweenPlanetsDSM`,在指定出发/到达时间网格上求解两天体之间带**一次脉冲深空机动**(1-DSM)的转移: 出发天体 → 日心二体滑行 → 一次脉冲 → 单圈 Lambert 到达天体. 不改变 `POST /celestial/transfer` 的 Lambert 行为与结果.

决策变量为出发双曲超速大小 `Vinf`、方向参数 `u,v`(θ=2πu, φ=acos(2v-1)-π/2; `u,v` 都在单位区间 [0,1], 不是弧度) 与机动时刻比例 `η∈(EtaMin, EtaMax)`(默认 0.1/0.9). `t_dsm = t_dep + η·TOF`.

默认 `Mode=Optimize`: 每个出发/到达时间对在无机动 Lambert 初值附近寻优, **每个时间对至多保留目标最小的一条**. 若 DSM 相对该时间对的单圈 Lambert 改进不足 10 m/s, 回退弹道解(`SolutionType=Ballistic`, `ΔV_dsm=0`). `Mode=Evaluate` 按给定 `Vinf`、`U`、`V`、`Eta` 求值(调试), 不因差于 Lambert 而改回弹道解.

目标 `J`(`TotalDV`) 始终含 `|ΔV_dsm|`. 默认 `AddDepartureVinfToCost` 与 `AddArrivalVinfToCost` 都为 true, 即 `J = |ΔV1| + |ΔV_dsm| + |ΔV2|`. 飞掠/撞击可不把到达相对速度计入目标(`AddArrivalVinfToCost=false`). 两个开关独立, 都为 false 时 `J = |ΔV_dsm|`. `MaxDepartureDV` / `MaxArrivalDV` / `MaxDsmDV` 三道过滤门限不受这两个开关影响; `MaxDsmDV` 默认 3000 m/s.

两段弧分别给出 `Leg1MinRangeAu`/`Leg1MaxRangeAu`、`Leg2MinRangeAu`/`Leg2MaxRangeAu`; `MinRangeAu`/`MaxRangeAu` 为两段并集. 单位与 Lambert 接口相同: 速度 m/s, 位置 m, 时间 UTC, 距离 AU. 结果位置速度通常输出在 `SunFrameName` 所指定的日心参考系中.

字段以 ASTROX Web API(`http://astrox.cn:8765/scalar/v1`, OpenAPI `/openapi/v1.json`) 与上游 `PlanetTransferDsmInput`/`TransferDsmResultData` 为准, 不自行编造.

## 核心指令 (Core Instructions)

1. **输入解析**:识别出发天体 `DepartureCbName`、到达天体 `ArrivalCbName`、出发时间段 `DepartureInterval`、到达时间段 `ArrivalInterval`,以及可选的日心输出系、TOF/DV 门限、步长、小行星轨道根数,以及 1-DSM 专用字段(`EtaMin`/`EtaMax`、`MaxDsmDV`、目标开关、`Mode`,以及 Evaluate 所需的 `Vinf`/`U`/`V`/`Eta`).
2. **小行星轨道根数**:与 `POST /celestial/transfer` 相同. 当出发或到达天体为小行星时,可传 `DepartureElements` / `ArrivalElements`. **若对应 `*Elements` 不为 `null`,服务端直接使用该根数积分,不从本地 `NEA.txt` 查找.** 若为小行星且 `*Elements` 为 `null`,则由服务端从本地 `NEA.txt` 按 NEA 编号/名称查找.
3. **时间区间格式**: `DepartureInterval` 与 `ArrivalInterval` 均为 **Start/Stop** 两段 ISO8601 UTC 字符串,中间以 `/` 分隔,例如 `2020-07-20T00:00:00Z/2020-07-20T00:00:00Z`.
4. **API 调用逻辑**:向 `{BASE_URL}/celestial/TransferBetweenPlanetsDSM` 发送 `POST`,`Content-Type: application/json`. 不要误调用 `/celestial/transfer`.
5. **结果说明**:成功时 `TransferResults` 为每个可行时间对至多一条结果,含出发/到达/机动时刻、三段 ΔV(出发、DSM、到达)及模长、日心系 RV1/RV2、机动点位置与机动前后速度、`Vinf`/`U`/`V`/`Eta`、`TotalDV`、`SolutionType`,以及两段弧与并集的日心径向最近/最远距离.

## API 规范 (Tool Definition)

### 接口地址

`POST /celestial/TransferBetweenPlanetsDSM`

### 输入参数结构 (`PlanetTransferDsmInput`, 继承 `PlanetTransferInput` 的全部字段)


| 参数名 | 类型 | 必须 | 缺省值示例 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| `DepartureCbName` | string | 是 | `Earth` | 出发天体名称(行星或近地小行星 NEA 名称/编号) |
| `ArrivalCbName` | string | 是 | `2015 XF261` | 到达天体名称(行星或近地小行星) |
| `DepartureInterval` | string | 是 | `2028-06-01T00:00:00Z/2028-10-01T00:00:00Z` | 出发时间搜索区间 `Start/Stop`,UTC ISO8601 |
| `ArrivalInterval` | string | 是 | `2029-04-10T00:00:00Z/2029-04-10T00:00:00Z` | 到达时间搜索区间 `Start/Stop`,UTC ISO8601 |
| `SunFrameName` | string | 否 | `EclpJ2000ICRF` | 日心结果输出参考系,例如 `MeanEclpJ2000` 或 `EclpJ2000ICRF` |
| `MinTofDays` | int32 | 否 | `10` | 最小转移天数 |
| `MaxTofDays` | int32 | 否 | `500` | 最大转移天数 |
| `MaxDepartureDV` | int32 | 否 | `10000` | 最大出发速度增量(m/s);通常用于衡量地球出发 Vinf,`C3 = Vinf^2`. 过滤门限,不影响目标函数 |
| `MaxArrivalDV` | int32 | 否 | `10000` | 最大到达速度增量(m/s);通常用于衡量到达 Vinf,例如撞击速度或制动速度. 过滤门限,不影响目标函数 |
| `DepartureStepDay` | number | 否 | `1` | 出发时间步长(天) |
| `ArrivalStepDay` | number | 否 | `1` | 到达时间步长(天) |
| `DepartureElements` | object \| null | 否 | `null` | 出发小行星 MPC 轨道根数;非 `null` 时直接用给定根数积分,不从本地 `NEA.txt` 查找 |
| `ArrivalElements` | object \| null | 否 | `null` | 到达小行星 MPC 轨道根数;非 `null` 时直接用给定根数积分,不从本地 `NEA.txt` 查找 |
| `EtaMin` | number | 否 | `0.1` | 深空机动时刻比例下限 η, 开区间 `(EtaMin, EtaMax)` |
| `EtaMax` | number | 否 | `0.9` | 深空机动时刻比例上限 η, 开区间 `(EtaMin, EtaMax)` |
| `MaxDsmDV` | int32 | 否 | `3000` | 最大深空机动速度增量(m/s). 只影响过滤, 不改变目标函数 |
| `AddDepartureVinfToCost` | boolean | 否 | `true` | 目标是否计入出发双曲超速 `|ΔV1|`. 与 `AddArrivalVinfToCost` 独立; 过滤门限不受此开关影响 |
| `AddArrivalVinfToCost` | boolean | 否 | `true` | 目标是否计入到达双曲超速 `|ΔV2|`. 飞掠/撞击可不计入到达 Vinf. 两边都为 false 时 `J=|ΔV_dsm|` |
| `Mode` | integer / string | 否 | `0` (Optimize) | 求解模式. 接口说明: JSON 枚举为 `optimize` / `evaluate`. OpenAPI schema 类型为 integer(`0`=Optimize, `1`=Evaluate). Evaluate 须同时给 `Vinf`、`U`、`V`、`Eta` |
| `Vinf` | number \| null | 否 | `null` | Evaluate 模式: 出发双曲超速大小(m/s) |
| `U` | number \| null | 否 | `null` | Evaluate 模式: 出发 Vinf 方向参数 `u∈[0,1]`, `θ=2πu` |
| `V` | number \| null | 否 | `null` | Evaluate 模式: 出发 Vinf 方向参数 `v∈[0,1]`, `φ=acos(2v-1)-π/2` |
| `Eta` | number \| null | 否 | `null` | Evaluate 模式: 深空机动时刻比例 η, 须在 `(EtaMin, EtaMax)` 内 |


#### DepartureElements / ArrivalElements 子字段(小行星历元根数)


| 子字段名 | 类型 | 单位 | 说明 |
| :--- | :--- | :--- | :--- |
| `EpochMjdTdt` | number | MJD | 轨道根数历元(TDT) |
| `SemimajorAxis` | number | AU | 半长轴 |
| `Eccentricity` | number | — | 偏心率 |
| `Inclination` | number | deg | 轨道倾角 |
| `Raan` | number | deg | 升交点赤经/黄经 |
| `ArgOfPeriapsis` | number | deg | 近日点幅角 |
| `MeanAnomaly` | number | deg | 平近点角 |
| `PeriTimeMjdTdt` | number | MJD | 近日点时刻(TDT),可选 |
| `Q` | number | AU | 近日点距,可选 |


### 输出说明 (`PlanetTransferDsmOutput`)


| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `IsSuccess` | boolean | 结果(True:成功;False:失败) |
| `Message` | string | 结果信息(主要是存储失败的原因,或成功时的条数) |
| `TransferResults` | array | 转移结果列表. 每个出发/到达时间对至多一条,元素类型见下表 |


#### TransferResults[] 元素 (`TransferDsmResultData`)


| 字段名 | 类型 | 单位 | 说明 |
| :--- | :--- | :--- | :--- |
| `DepartureTime` | string | — | 出发时间(UTC 字符串) |
| `ArrivalTime` | string | — | 到达时间(UTC 字符串) |
| `DsmTime` | string | — | 深空机动时刻(UTC 字符串). 弹道解取转移弧上 η 对应的时刻, 脉冲为零 |
| `TimeOfFlightDays` | number | 天 | 飞行时间 |
| `DeltaV1` | number[] | m/s | 出发速度增量,即出发双曲超速矢量 `ΔV1 = v_sc - v_planet` [x,y,z] |
| `DV1_Mag` | number | m/s | 出发速度增量大小 |
| `DeltaV_dsm` | number[] | m/s | 深空机动速度增量. 弹道解为零矢量 |
| `DV_dsm_Mag` | number | m/s | 深空机动速度增量大小. 弹道解为 0 |
| `DeltaV2` | number[] | m/s | 到达速度增量,即到达双曲超速矢量 [x,y,z] |
| `DV2_Mag` | number | m/s | 到达速度增量大小 |
| `RV1` | number[] | m, m/s | 出发时航天器日心位置速度,长度 6:位置 3 + 速度 3(位置为出发天体位置, 速度为 `v_sc`) |
| `RV2` | number[] | m, m/s | 到达时航天器日心位置速度,长度 6:位置 3 + 速度 3(位置为到达天体位置, 速度为 Lambert 到达速度) |
| `DsmPosition` | number[] | m | 深空机动点日心位置 |
| `V_dsm_minus` | number[] | m/s | 深空机动前速度(日心),第一段二体滑行末端 |
| `V_dsm_plus` | number[] | m/s | 深空机动后速度(日心). 弹道解与机动前速度相同 |
| `ArrivalLightAngle` | number | deg | 到达时刻太阳光照角;相对速度 `DeltaV2` 与 `R2` 的夹角,一般用于撞击小行星的末端分析 |
| `Vinf` | number | m/s | 出发双曲超速大小,与 `DV1_Mag` 一致,便于按 `u,v` 复现方向 |
| `U` | number | — | 出发 Vinf 方向参数 u, `θ=2πu` |
| `V` | number | — | 出发 Vinf 方向参数 v, `φ=acos(2v-1)-π/2` |
| `Eta` | number | — | 深空机动时刻比例, `t_dsm = t_dep + η·TOF` |
| `TotalDV` | number | m/s | 目标 ΔV. 始终含 `|ΔV_dsm|`; 默认再加 `|ΔV1|` 与 `|ΔV2|` |
| `Leg1MinRangeAu` | number | AU | 第一段弧(出发→深空机动)日心径向最近距离 |
| `Leg1MaxRangeAu` | number | AU | 第一段弧(出发→深空机动)日心径向最远距离 |
| `Leg2MinRangeAu` | number | AU | 第二段弧(深空机动→到达)日心径向最近距离 |
| `Leg2MaxRangeAu` | number | AU | 第二段弧(深空机动→到达)日心径向最远距离 |
| `MinRangeAu` | number | AU | 两段弧并集的日心径向最近距离,即 `min(Leg1Min, Leg2Min)` |
| `MaxRangeAu` | number | AU | 两段弧并集的日心径向最远距离,即 `max(Leg1Max, Leg2Max)` |
| `SolutionType` | string | — | `Ballistic`=无深空机动的单圈 Lambert(`ΔV_dsm=0`); `Dsm`=一次深空脉冲 |


## 注意事项

- 时间字符串使用 UTC ISO8601,例如 `2020-07-20T00:00:00Z`;区间必须为 `Start/Stop` 用 `/` 连接.
- `MinTofDays`、`MaxTofDays` 为整数(天);须满足 `MinTofDays` <= `MaxTofDays`. 步长 `DepartureStepDay`、`ArrivalStepDay` 为天. 密集 porkchop 网格会明显变慢;验证时用单点或稀疏时间对.
- `MaxDepartureDV`、`MaxArrivalDV`、`MaxDsmDV` 为整数(m/s),只用于过滤超出上限的解,不改变目标函数 `J`.
- `u,v` 都在 [0,1] 上,不是弧度. `u` 环绕; `η` 落在开区间 `(EtaMin, EtaMax)`.
- 默认 Optimize 只在 Lambert 初值附近做局部寻优,不是全局最优. 离弹道解较远的深空机动可能搜不到,但 Optimize 结果按同一 `J` 定义不会差于纯 Lambert(改进不足 10 m/s 时回退弹道解).
- Evaluate 不搜索,也不因差于 Lambert 而改回弹道解;须给出 `Vinf`、`U`、`V`、`Eta`.
- 摘要结果时应报告 `SolutionType`、`DsmTime`、`Eta`、`DV1_Mag`、`DV_dsm_Mag`、`DV2_Mag`、`TotalDV`,以及 `Leg1`/`Leg2` 与并集的 `MinRangeAu`/`MaxRangeAu`.
- 行星名称与天体星历技能中约定一致(如 `Earth`、`Mars`);近地小行星可用 NEA 名称或编号样式字符串.
- 小行星不传 `*Elements` 时由服务端从本地 `NEA.txt` 查找根数;传了 `*Elements` 则直接积分、不查 `NEA.txt`. 若名称不在本地库中可能失败.
- 判定成功:HTTP 200 且 `IsSuccess` 为 `true`.

## 标准执行流程

1. 参数预检:确认四个名称/区间字段非空;区间格式正确;若给出 TOF/DV 上限则检查单位与 `MinTofDays` <= `MaxTofDays`;若 `Mode` 为 Evaluate 则 `Vinf`/`U`/`V`/`Eta` 须齐全,且 `Eta` 在 `(EtaMin, EtaMax)` 内;若需显式给定小行星根数则填全 `*Elements`(否则依赖本地 `NEA.txt`).
2. 请求构造:按字段名构造 JSON,`null` 的 `*Elements` 可省略或显式传 `null`. Optimize 时可省略 `Vinf`/`U`/`V`/`Eta`.
3. 结果判定:HTTP 200 + `IsSuccess === true`.
4. 输出归一化:摘要出发/到达天体、时间网格、`Mode` 与参考系;列出 `TransferResults` 中各方案的 `DepartureTime`、`ArrivalTime`、`DsmTime`、`TimeOfFlightDays`、`SolutionType`、`Eta`、`DV1_Mag`、`DV_dsm_Mag`、`DV2_Mag`、`TotalDV`、`ArrivalLightAngle`、`Leg1MinRangeAu`/`Leg1MaxRangeAu`、`Leg2MinRangeAu`/`Leg2MaxRangeAu`、`MinRangeAu`、`MaxRangeAu`.

## 调用示例

### 示例 1:地球到火星,单时间对 Optimize(最小可运行)

对应上游测试 `Dsm_NotWorseThanLambert_SameEpochPair`:默认 Optimize,与同一时间对的 `POST /celestial/transfer` 比较,`TotalDV` 应不差于 Lambert.

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/TransferBetweenPlanetsDSM" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/celestial-transfer-dsm/fixtures/dsm-min.json
```

### 示例 2:短窗口稀疏步长(两个出发时刻)

对应上游测试 `Dsm_EarthToMars_ShortWindow_Closes`.

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/TransferBetweenPlanetsDSM" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/celestial-transfer-dsm/fixtures/dsm-earth-mars-short.json
```

## Fixtures

- `skills/celestial-transfer-dsm/fixtures/dsm-min.json`:最小可运行请求(地球到火星,单出发/到达时刻,默认 Optimize).
- `skills/celestial-transfer-dsm/fixtures/dsm-earth-mars-short.json`:地球到火星短窗口,出发步长 10 天,与单元测试 `Dsm_EarthToMars_ShortWindow_Closes` 一致.
