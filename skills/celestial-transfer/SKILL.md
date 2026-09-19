---
name: celestial-transfer
description: 行星与小行星之间的 Lambert 转移轨道计算。出发/到达天体可为行星(Earth,Mars,Ceres 等)或近地小行星(NEA 编号/名称);小行星可传入历元轨道根数以避免从本地 NEA.txt 查找。用户需要日心系转移窗口、Delta-V 与转移弧日心径向最近/最远距离时使用。
---

# 天体间 Lambert 转移技能 (Celestial Transfer)

通过 Astrox WebAPI 的 `POST /celestial/transfer`,在指定出发/到达时间网格上求解行星或小行星之间的 Lambert 转移。小行星轨道采用日心 Heliocentric 积分器递推;结果位置速度通常输出在 `SunFrameName` 所指定的日心参考系中(如 `MeanEclpJ2000`、`EclpJ2000ICRF`)。每条转移解还输出转移弧上相对太阳的日心径向最近/最远距离 `MinRangeAu`/`MaxRangeAu`(AU)。

## 核心指令 (Core Instructions)

1. **输入解析**:识别出发天体 `DepartureCbName`、到达天体 `ArrivalCbName`、出发时间段 `DepartureInterval`、到达时间段 `ArrivalInterval`、转移时间上下限 `MinTofDays`/`MaxTofDays`、日心输出系 `SunFrameName`、出发/到达速度增量上限 `MaxDepartureDV`/`MaxArrivalDV`,以及可选的时间步长与小行星轨道根数。
2. **小行星轨道根数**:
  - 当出发或到达天体为小行星时,可传 `DepartureElements` / `ArrivalElements`(MPC 型轨道根数)。**若对应 `*Elements` 不为 `null`,服务端直接使用该根数积分,不从本地 `NEA.txt` 查找。**
  - 若为小行星且 `*Elements` 为 `null`,则由服务端从本地 `NEA.txt` 按 NEA 编号/名称查找轨道根数。
3. **时间区间格式**: `DepartureInterval` 与 `ArrivalInterval` 均为 **Start/Stop** 两段 ISO8601 UTC 字符串,中间以 `/` 分隔,例如 `2028-06-01T00:00:00Z/2028-10-01T00:00:00Z`。
4. **API 调用逻辑**:向 `{BASE_URL}/celestial/transfer` 发送 `POST`,`Content-Type: application/json`。
5. **结果说明**:成功时 `TransferResults` 为每次可行转移的列表,含出发/到达时刻、飞行时间 `TimeOfFlightDays`、两段双曲超速(Delta-V)向量与模长、日心系 RV1/RV2、到达时刻太阳光照角 `ArrivalLightAngle`,以及转移弧上相对太阳的日心径向最近/最远距离 `MinRangeAu`/`MaxRangeAu`(AU)。

## API 规范 (Tool Definition)

### 接口地址

`POST /celestial/transfer`

### 输入参数结构 (JSON)


| 参数名                 | 类型     | 必须  | 缺省值示例                                                                 | 说明                                                                 |
| ------------------- | ------ | --- | --------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `DepartureCbName`   | string | 是   | `Earth`                                                               | 出发天体名称(行星或近地小行星 NEA 名称/编号)                                            |
| `ArrivalCbName`     | string | 是   | `2015 XF261`                                                          | 到达天体名称(行星或近地小行星)                                                    |
| `DepartureInterval` | string | 是   | `2028-06-01T00:00:00Z/2028-10-01T00:00:00Z`                           | 出发时间搜索区间 `Start/Stop`,UTC ISO8601                                           |
| `ArrivalInterval`   | string | 是   | `2029-04-10T00:00:00Z/2029-04-10T00:00:00Z`                           | 到达时间搜索区间 `Start/Stop`,UTC ISO8601                                           |
| `SunFrameName`      | string | 否   | `EclpJ2000ICRF`                                                       | 日心结果输出参考系,例如 `MeanEclpJ2000` 或 `EclpJ2000ICRF`                                  |
| `MinTofDays`        | int32  | 否   | `10`                                                                  | 最小转移天数                                                             |
| `MaxTofDays`        | int32  | 否   | `500`                                                                 | 最大转移天数                                                             |
| `MaxDepartureDV`    | int32  | 否   | `10000`                                                               | 最大出发速度增量(m/s);通常用于衡量地球出发 Vinf,`C3 = Vinf^2`                         |
| `MaxArrivalDV`      | int32  | 否   | `10000`                                                               | 最大到达速度增量(m/s);通常用于衡量到达 Vinf,例如撞击速度或制动速度                           |
| `DepartureStepDay`  | number | 否   | `1`                                                                   | 出发时间步长(天)                                                         |
| `ArrivalStepDay`    | number | 否   | `1`                                                                   | 到达时间步长(天)                                                         |
| `DepartureElements` | object \| null | 否   | `null`                                                                | 出发小行星 MPC 轨道根数;非 `null` 时直接用给定根数积分,不从本地 `NEA.txt` 查找                                  |
| `ArrivalElements`   | object \| null | 否   | `null`                                                                | 到达小行星 MPC 轨道根数;非 `null` 时直接用给定根数积分,不从本地 `NEA.txt` 查找                                  |


#### DepartureElements / ArrivalElements 子字段(小行星历元根数)


| 子字段名             | 类型     | 单位  | 说明                                      |
| ---------------- | ------ | --- | --------------------------------------- |
| `EpochMjdTdt`    | number | MJD | 轨道根数历元(TDT)                             |
| `SemimajorAxis`  | number | AU  | 半长轴                                     |
| `Eccentricity`   | number | —   | 偏心率                                     |
| `Inclination`    | number | deg | 轨道倾角                                    |
| `Raan`           | number | deg | 升交点赤经/黄经                               |
| `ArgOfPeriapsis` | number | deg | 近日点幅角                                   |
| `MeanAnomaly`    | number | deg | 平近点角                                    |
| `PeriTimeMjdTdt` | number | MJD | 近日点时刻(TDT),可选                          |
| `Q`              | number | AU  | 近日点距,可选                                |


### 输出说明 (`PlanetTransferOutput`)


| 字段名               | 类型      | 说明                           |
| ----------------- | ------- | ---------------------------- |
| `IsSuccess`       | boolean | 结果(True:成功;False:失败)        |
| `Message`         | string  | 结果信息(主要是存储失败的原因)            |
| `TransferResults` | array   | 转移结果列表(包含每次转移的详细数据),元素类型见下表 |


#### TransferResults[] 元素 (`TransferResultData`)


| 字段名                 | 类型       | 单位     | 说明                                                                 |
| ------------------- | -------- | ------ | ------------------------------------------------------------------ |
| `DepartureTime`     | string   | —      | 出发时间(UTC 字符串)                                                     |
| `ArrivalTime`       | string   | —      | 到达时间(UTC 字符串)                                                     |
| `TimeOfFlightDays`  | number   | 天      | 飞行时间                                                               |
| `DeltaV1`           | number[] | m/s    | 天体出发速度增量(即出发双曲超速矢量) [x,y,z]                                       |
| `DV1_Mag`           | number   | m/s    | 天体出发速度增量大小(即出发双曲超速矢量大小)                                           |
| `DeltaV2`           | number[] | m/s    | 天体到达速度增量(即到达双曲超速矢量) [x,y,z]                                       |
| `DV2_Mag`           | number   | m/s    | 天体到达速度增量大小(即到达双曲超速矢量大小)                                           |
| `RV1`               | number[] | m, m/s | 出发时位置速度(日心系),长度 6:位置 3 + 速度 3                                    |
| `RV2`               | number[] | m, m/s | 到达时位置速度(日心系),长度 6:位置 3 + 速度 3                                    |
| `ArrivalLightAngle` | number   | deg    | 到达时刻太阳光照角;相对速度 `DeltaV2` 与 `R2` 的夹角,一般用于撞击小行星的末端分析               |
| `MinRangeAu`        | number   | AU     | 转移弧(出发→到达)上相对太阳的日心径向最近距离                                          |
| `MaxRangeAu`        | number   | AU     | 转移弧(出发→到达)上相对太阳的日心径向最远距离                                          |


## 注意事项

- 时间字符串使用 UTC ISO8601,例如 `2028-06-01T00:00:00Z`;区间必须为 `Start/Stop` 用 `/` 连接。
- `MinTofDays`、`MaxTofDays` 为整数(天);须满足 `MinTofDays` <= `MaxTofDays`。步长 `DepartureStepDay`、`ArrivalStepDay` 为天。
- `MaxDepartureDV`、`MaxArrivalDV` 为整数(m/s),用于过滤超出上限的转移解。出发端通常对应地球出发 Vinf(`C3 = Vinf^2`);到达端通常对应到达 Vinf(撞击速度或制动速度)。
- `DeltaV1`/`DeltaV2` 为双曲超速矢量;`TimeOfFlightDays` 为飞行时间(天)。
- `ArrivalLightAngle` 单位为度,是到达相对速度 `DeltaV2` 与到达位置 `R2`(`RV2` 前 3 个分量)的夹角,常用于撞击小行星的末端光照/几何分析。
- `MinRangeAu`/`MaxRangeAu` 单位为 AU,分别表示转移弧(出发→到达)上相对太阳的日心径向最近/最远距离;摘要结果时应一并报告。
- 行星名称与天体星历技能中约定一致(如 `Earth`、`Mars`);近地小行星可用 NEA 名称或编号样式字符串。
- 小行星不传 `*Elements` 时由服务端从本地 `NEA.txt` 查找根数;传了 `*Elements` 则直接积分、不查 `NEA.txt`。若名称不在本地库中可能失败。
- 判定成功:HTTP 200 且 `IsSuccess` 为 `true`。

## 标准执行流程

1. 参数预检:确认四个名称/区间字段非空;区间格式正确;若给出 TOF/DV 上限则检查单位与 `MinTofDays` <= `MaxTofDays`;若需显式给定小行星根数则填全 `*Elements`(否则依赖本地 `NEA.txt`)。
2. 请求构造:按字段名构造 JSON,`null` 的 `*Elements` 可省略或显式传 `null`。
3. 结果判定:HTTP 200 + `IsSuccess === true`。
4. 输出归一化:摘要出发/到达天体、时间网格与参考系;列出 `TransferResults` 中各方案的 `DepartureTime`、`ArrivalTime`、`TimeOfFlightDays`、`DV1_Mag`、`DV2_Mag`、`ArrivalLightAngle`、`MinRangeAu`、`MaxRangeAu`。

## 调用示例

### 示例 1:行星到行星(不传 MPC 根数)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/transfer" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/celestial-transfer/fixtures/transfer-min.json
```

### 示例 2:含小行星历元根数(与接口文档一致的完整字段)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/transfer" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/celestial-transfer/fixtures/transfer-with-elements.json
```

### 示例 3:地球到小行星 Apophis(从本地 NEA.txt 查找轨道根数并积分)

对应上游测试 `Transfer_EarthToApophis_Mpc_260424`:验证「行星 → 非内置行星类天体(近地小行星)」全链路;**不传** `ArrivalElements`,服务端从本地 `NEA.txt` 按名称查找 Apophis 根数。若本地库无该天体则请求会失败。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/transfer" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/celestial-transfer/fixtures/transfer-earth2mpc.json
```

## Fixtures

- `skills/celestial-transfer/fixtures/transfer-min.json`:最小可运行请求(地球到火星,不传 `*Elements`)。
- `skills/celestial-transfer/fixtures/transfer-with-elements.json`:含 `DepartureElements`/`ArrivalElements` 的完整模板(与公开 API 示例一致)。
- `skills/celestial-transfer/fixtures/transfer-earth2mpc.json`:地球 → Apophis,不传 `*Elements`,依赖本地 `NEA.txt` 查找;时间窗口为 2029 年逼近地球前后的短区间,步长 10 天(与单元测试 `Transfer_EarthToApophis_Mpc_260424` 一致)。
