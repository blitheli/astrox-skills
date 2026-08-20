---
name: celestial-mpc
description: 根据小行星名称或编号从 MPC 获取轨道根数并计算日心星历;也可直接传入 MPC 轨道根数跳过网络查询。用户需要小行星位置/轨迹时使用。
---

# MPC 小行星星历计算技能 (Celestial MPC)

通过 Astrox WebAPI 的 `POST /celestial/mpc`,根据小行星名称(或编号)调用 `https://data.minorplanetcenter.net/api/get-orb` 获取轨道根数(MJD TDT 历元);也可直接提供 MPC 轨道根数。使用 **Heliocentric3day** 积分器递推(固定 3 天步长),输出星历为日心系,参考系由 `ObserverFrame` 指定(如 `MeanEclpJ2000`、`ICRF`)。

## 核心指令 (Core Instructions)

1. **输入解析**:识别小行星名称或编号(`TargetName`)、日心输出系(`ObserverFrame`)、分析起止时刻(`Start`/`Stop`),以及可选的 `TargetElements`(含根数坐标系 `ReferenceFrame`)。
2. **轨道根数来源**:
  - 若 `TargetElements` 不为 `null`,服务端直接使用该 MPC 根数积分,**不调用 MPC 网络查询**。此时 `TargetName` 可不输入。根数为日心平黄道系,须用 `ReferenceFrame` 标明来源:`MeanEclpJ2000`(JPL)或 `EclpJ2000ICRF`(MPC,缺省)。
  - 若 `TargetElements` 为 `null` 或省略,则必须提供 `TargetName`(名称如 `Ceres`、`Apophis`,或编号如 `99942`),由服务端查询 MPC。
3. **时间区间**:
  - `Start` 缺省不输入,默认为轨道历元时刻;**不能早于轨道历元时刻**。
  - `Stop` 缺省可不输入,表示 `Start + 1` 年。
4. **API 调用逻辑**:向 `{BASE_URL}/celestial/mpc` 发送 `POST`,`Content-Type: application/json`。
5. **结果说明**:响应同时包含 `OrbitElements`(MPC 轨道根数解析结果)和 `Position`(CesiumPosition 格式星历)。

## API 规范 (Tool Definition)

### 接口地址

`POST /celestial/mpc`

### 输入参数结构 (JSON) (`MpcInput`)


| 参数名              | 类型            | 必须  | 缺省值             | 说明                                                               |
| ---------------- | ------------- | --- | --------------- | ---------------------------------------------------------------- |
| `TargetName`     | string        | 条件  | `Ceres`         | 小行星名称或编号,例如 `Ceres`、`Apophis`、`99942`;已提供 `TargetElements` 时可不输入 |
| `ObserverFrame`  | string        | 否   | `MeanEclpJ2000` | 日心坐标系,见下表                                                        |
| `Start`          | string        | 否   | 轨道历元时刻          | 开始时刻(UTCG),格式 `yyyy-MM-ddTHH:mm:ssZ`;不能早于轨道历元时刻                  |
| `Stop`           | string        | 否   | `Start` + 1 年   | 结束时刻(UTCG),格式 `yyyy-MM-ddTHH:mm:ssZ`                             |
| `TargetElements` | object \| null | 否   | `null`          | 小行星 MPC 轨道根数;非 `null` 时不经网络查询 MPC。根数为日心平黄道系,缺省 `EclpJ2000ICRF`(MPC);JPL 根数用 `MeanEclpJ2000` |


#### ObserverFrame 可选值


| 取值               | 说明                      |
| ---------------- | ----------------------- |
| `FIXED`          | 中心天体固定系                 |
| `INERTIAL`       | 中心天体惯性系                 |
| `J2000`          | J2000 坐标系               |
| `ICRF`           | 国际天球参考系                 |
| `MeanEclpJ2000`  | J2000 平黄道坐标系(JPL)       |
| `EclpJ2000ICRF`  | J2000 平黄道坐标系(ICRF,MPC) |


#### TargetElements / OrbitElements 子字段 (`MpcOrbElements`)


| 子字段名             | 类型     | 单位  | 说明                     |
| ---------------- | ------ | --- | ---------------------- |
| `EpochMjdTdt`    | number | MJD | 轨道根数历元(TDT)            |
| `SemimajorAxis`  | number | AU  | 半长轴                    |
| `Eccentricity`   | number | —   | 偏心率                    |
| `Inclination`    | number | deg | 轨道倾角                   |
| `Raan`           | number | deg | 升交点赤经/黄经               |
| `ArgOfPeriapsis` | number | deg | 近日点幅角                  |
| `MeanAnomaly`    | number | deg | 平近点角                   |
| `PeriTimeMjdTdt` | number | MJD | 近日点时刻(TDT),作为输入参数时可不输入 |
| `Q`              | number | AU  | 近日点距,作为输入参数时可不输入       |
| `ReferenceFrame` | string | —   | 根数坐标系:`MeanEclpJ2000` 或 `EclpJ2000ICRF`;缺省 `EclpJ2000ICRF` |


### 输出说明

响应为 JSON 对象,包含以下字段:


| 字段名             | 类型      | 说明                              |
| --------------- | ------- | ------------------------------- |
| `IsSuccess`     | boolean | 结果(True:成功;False:失败)            |
| `Message`       | string  | 结果信息(主要存储失败原因)                  |
| `OrbitElements` | object  | MPC 小行星轨道根数解析结果(日心平黄道系:`MeanEclpJ2000` 为 JPL,`EclpJ2000ICRF` 为 MPC);结构同上方 `MpcOrbElements` |
| `Position`      | object  | 小行星星历数据(CesiumPosition CZML 格式) |


## 注意事项

- 时间格式必须为 UTC ISO8601:`yyyy-MM-ddTHH:mm:ssZ`。
- `Start` 缺省为轨道历元;若显式给出,不得早于该历元。
- `Stop` 缺省为 `Start + 1` 年。
- 积分器为 Heliocentric3day,步长固定 3 天,请求中无需(也不能)改步长。
- 输出星历为日心系,坐标系由 `ObserverFrame` 指定,缺省 `MeanEclpJ2000`。`ObserverFrame` 与根数坐标系 `ReferenceFrame` 不是同一字段。
- 日心平黄道有两种:`MeanEclpJ2000`(JPL)与 `EclpJ2000ICRF`(MPC)。传入或解读 `TargetElements`/`OrbitElements` 时按来源选择,缺省 `EclpJ2000ICRF`。
- 未提供 `TargetElements` 时依赖外部 MPC(`https://data.minorplanetcenter.net/api/get-orb`);MPC 不可用则调用失败。
- 判定成功:HTTP 200 且 `IsSuccess` 为 `true`。

## 标准执行流程

1. 参数预检
  - 无 `TargetElements` 时 `TargetName` 必填。
  - 若给出 `Start`/`Stop`,检查 UTC 格式,且 `Start` 不早于轨道历元、`Start < Stop`。
2. 请求构造
  - 按接口字段名构造 JSON;`null` 的 `TargetElements` 可省略。不要传已移除的 `ObserverName`。
3. 结果判定
  - 先判 HTTP 状态是否为 200。
  - 再检查响应 JSON 中 `IsSuccess` 是否为 `true`。
4. 输出归一化
  - 返回请求摘要(目标名称或根数来源、`ObserverFrame`、`ReferenceFrame`、时间区间)。
  - 输出 `OrbitElements` 中的轨道根数摘要(含 `ReferenceFrame`)。
  - 输出 `Position` 中的星历(CZML)数据。

## 调用示例

### 示例 1:仅指定目标名称(经 MPC 查询)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/mpc" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/celestial-mpc/fixtures/mpc-min.json
```

### 示例 2:指定坐标系与结束时刻

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/mpc" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/celestial-mpc/fixtures/mpc-defaults.json
```

### 示例 3:直接传入 MPC 轨道根数(不经网络查询)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/mpc" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/celestial-mpc/fixtures/mpc-with-elements.json
```

## Fixtures

- `skills/celestial-mpc/fixtures/mpc-min.json`:最小可运行请求(仅指定 `TargetName`,由服务端查 MPC)。
- `skills/celestial-mpc/fixtures/mpc-defaults.json`:指定 `ObserverFrame` 与 `Stop` 的请求模板。
- `skills/celestial-mpc/fixtures/mpc-with-elements.json`:含 `TargetElements`(根数坐标系 `EclpJ2000ICRF`),不经 MPC 网络查询。

