---
name: celestial-transfer
description: 行星与小行星之间的 Lambert 转移轨道计算。出发/到达天体可为行星(Earth,Mars,Ceres 等)或小行星(MPC 编号/名称);小行星可传入历元轨道根数以避免 MPC 网络查询。用户需要日心系转移窗口与 Delta-V 时使用。
---

# 天体间 Lambert 转移技能 (Celestial Transfer)

通过 Astrox WebAPI 的 `POST /celestial/transfer`,在指定出发/到达时间网格上求解行星或小行星之间的 Lambert 转移。小行星轨道采用日心 Heliocentric 积分器递推;结果位置速度通常输出在 `SunFrameName` 所指定的日心参考系中(如 `MeanEclpJ2000`、`ICRF`)。

## 核心指令 (Core Instructions)

1. **输入解析**:识别出发天体 `DepartureCbName`、到达天体 `ArrivalCbName`、出发时间段 `DepartureInterval`、到达时间段 `ArrivalInterval`、最小转移时间 `MinTofDays`、日心输出系 `SunFrameName`,以及可选的时间步长与小行星轨道根数。
2. **小行星轨道根数**:
  - 当出发或到达天体为小行星时,可传 `DepartureElements` / `ArrivalElements`(MPC 型轨道根数)。**若对应 `*Elements` 不为 `null`,服务端直接使用该根数积分,不调用 MPC 网络查询。**
  - 若为小行星且 `*Elements` 为 `null`,则由服务端通过 MPC 等途径获取轨道根数(依赖网络)。
3. **时间区间格式**: `DepartureInterval` 与 `ArrivalInterval` 均为 **Start/Stop** 两段 ISO8601 UTC 字符串,中间以 `/` 分隔,例如 `2028-06-01T00:00:00Z/2028-10-01T00:00:00Z`。
4. **API 调用逻辑**:向 `{BASE_URL}/celestial/transfer` 发送 `POST`,`Content-Type: application/json`。
5. **结果说明**:成功时 `TransferResults` 为每次可行转移的列表,含出发/到达时刻、两段 Delta-V 向量与模长、日心系 RV1/RV2。

## API 规范 (Tool Definition)

### 接口地址

`POST /celestial/transfer`

### 输入参数结构 (JSON)


| 参数名                 | 类型     | 必须  | 缺省值示例                                                                 | 说明                                                                 |
| ------------------- | ------ | --- | --------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `DepartureCbName`   | string | 是   | `Earth`                                                               | 出发天体名称(行星或小行星 MPC 名称/编号)                                            |
| `ArrivalCbName`     | string | 是   | `2015 XF261`                                                          | 到达天体名称(行星或小行星)                                                    |
| `DepartureInterval` | string | 是   | `2028-06-01T00:00:00Z/2028-10-01T00:00:00Z`                           | 出发时间搜索区间 `Start/Stop`,UTC ISO8601                                           |
| `ArrivalInterval`   | string | 是   | `2029-04-10T00:00:00Z/2029-04-10T00:00:00Z`                           | 到达时间搜索区间 `Start/Stop`,UTC ISO8601                                           |
| `SunFrameName`      | string | 否   | `MeanEclpJ2000`                                                       | 日心结果输出参考系,例如 `MeanEclpJ2000` 或 `ICRF`                                  |
| `MinTofDays`        | int32  | 否   | `10`                                                                  | 最小转移时间(天)                                                         |
| `DepartureStepDay`  | number | 否   | `1`                                                                   | 出发时间步长(天)                                                         |
| `ArrivalStepDay`    | number | 否   | `1`                                                                   | 到达时间步长(天)                                                         |
| `DepartureElements` | object \| null | 否   | `null`                                                                | 出发小行星 MPC 轨道根数;非 `null` 时不经网络查询 MPC                                  |
| `ArrivalElements`   | object \| null | 否   | `null`                                                                | 到达小行星 MPC 轨道根数;非 `null` 时不经网络查询 MPC                                  |


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


### 输出说明


| 字段名               | 类型       | 说明                           |
| ----------------- | -------- | ---------------------------- |
| `IsSuccess`       | boolean  | 结果(True:成功;False:失败)        |
| `Message`         | string   | 结果信息(失败原因等)                 |
| `TransferResults` | array    | 转移结果列表,元素类型见下表             |


#### TransferResults[] 元素 (TransferResultData)


| 字段名             | 类型     | 单位  | 说明                    |
| --------------- | ------ | --- | --------------------- |
| `DepartureTime` | string | —   | 出发时刻(UTC ISO8601 字符串) |
| `ArrivalTime`   | string | —   | 到达时刻(UTC ISO8601 字符串) |
| `DeltaV1`       | number[] | m/s | 出发端速度增量向量 [x,y,z]    |
| `DeltaV2`       | number[] | m/s | 到达端速度增量向量 [x,y,z]    |
| `DV1_Mag`       | number | m/s | 出发端速度增量模               |
| `DV2_Mag`       | number | m/s | 到达端速度增量模               |
| `RV1`           | number[] | m, m/s | 出发时日心系位置速度(长度 6:位置 3 + 速度 3) |
| `RV2`           | number[] | m, m/s | 到达时日心系位置速度(长度 6)         |


## 注意事项

- 时间字符串使用 UTC ISO8601,例如 `2028-06-01T00:00:00Z`;区间必须为 `Start/Stop` 用 `/` 连接。
- `MinTofDays` 为整数(天);步长 `DepartureStepDay`、`ArrivalStepDay` 为天。
- 行星名称与天体星历技能中约定一致(如 `Earth`、`Mars`);小行星可用名称或 MPC 编号样式字符串。
- 小行星不传 `*Elements` 时依赖 MPC 等外部数据,可能因网络或服务不可用而失败。
- 判定成功:HTTP 200 且 `IsSuccess` 为 `true`。

## 标准执行流程

1. 参数预检:确认四个名称/区间字段非空;区间格式正确;若需离线小行星根数则填全 `*Elements`。
2. 请求构造:按字段名构造 JSON,`null` 的 `*Elements` 可省略或显式传 `null`。
3. 结果判定:HTTP 200 + `IsSuccess === true`。
4. 输出归一化:摘要出发/到达天体、时间网格与参考系;列出 `TransferResults` 中各方案的 `DepartureTime`、`ArrivalTime`、`DV1_Mag`、`DV2_Mag`。

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

## Fixtures

- `skills/celestial-transfer/fixtures/transfer-min.json`:最小可运行请求(地球到火星,不传 `*Elements`)。
- `skills/celestial-transfer/fixtures/transfer-with-elements.json`:含 `DepartureElements`/`ArrivalElements` 的完整模板(与公开 API 示例一致)。
