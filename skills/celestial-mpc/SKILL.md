---
name: celestial-mpc
description: 根据小行星名称或编号，从 MPC(Minor Planet Center)获取轨道根数，并计算该小行星的星历数据(日心黄道坐标系)。用户需要小行星位置/轨迹时使用。
---

# MPC 小行星星历计算技能 (Celestial MPC)

通过 Astrox WebAPI 的 `POST /celestial/mpc`，先从 MPC 获取小行星轨道根数，再按日心 MeanEclpJ2000 坐标系 + Heliocentric 积分器进行轨道递推，输出 CZML 格式星历数据。

## 核心指令 (Core Instructions)

1. **输入解析**:识别小行星名称或编号(`TargetName`)、观测者(`ObserverName`)、观测者坐标系(`ObserverFrame`)、分析结束时刻(`Stop`)。
2. **参数处理**:
   - `TargetName` 必须提供，可为名称(如 `Ceres`、`Apophis`)或编号(如 `99942`)。
   - `Start` 固定为小行星轨道根数的历元时刻，由服务端自动确定，**用户无需输入，调用时传空字符串**。
   - `Stop` 缺省可不输入(传空字符串)，服务端默认为历元时刻 + 1 年。
   - `ObserverName` 缺省为 `"Sun"`，`ObserverFrame` 缺省为 `"MEANECLPJ2000"`。
3. **API 调用逻辑**:向 `{BASE_URL}/celestial/mpc` 发送 `POST`，`Content-Type: application/json`。
4. **结果说明**:响应同时包含 `OrbitElements`(MPC 轨道根数解析结果)和 `Position`(CesiumPosition 格式星历)。

## API 规范 (Tool Definition)

### 接口地址

`POST /celestial/mpc`

### 输入参数结构 (JSON)

| 参数名             | 类型     | 必须  | 缺省值             | 说明                                                                                         |
| --------------- | ------ | --- | --------------- | ------------------------------------------------------------------------------------------ |
| `TargetName`    | string | 是   | `Ceres`         | 小行星名称或编号，例如 `Ceres`、`Apophis`、`99942`                                                      |
| `ObserverName`  | string | 否   | `Sun`           | 观测者名称，缺省为 `Sun`                                                                            |
| `ObserverFrame` | string | 否   | `MEANECLPJ2000` | 观测者坐标系，可选：`FIXED`、`INERTIAL`、`MEANECLPJ2000`、`J2000`                                        |
| `Start`         | string | 否   | (历元时刻)          | 分析开始时刻(UTCG)，格式 `yyyy-MM-ddTHH:mm:ssZ`；**暂固定为历元时刻，调用时传空字符串 `""`** |
| `Stop`          | string | 否   | 历元时刻 + 1 年      | 分析结束时刻(UTCG)，格式 `yyyy-MM-ddTHH:mm:ssZ`；缺省传空字符串 `""`                                        |

### 输出说明

响应为 JSON 对象，包含以下字段：

| 字段名             | 类型      | 说明                              |
| --------------- | ------- | ------------------------------- |
| `IsSuccess`     | boolean | 结果(True:成功；False:失败)            |
| `Message`       | string  | 结果信息(主要存储失败原因)                  |
| `OrbitElements` | object  | MPC 小行星轨道根数解析结果，详见下表            |
| `Position`      | object  | 小行星星历数据(CesiumPosition CZML 格式) |

#### OrbitElements 子字段

| 子字段名              | 类型     | 单位    | 说明              |
| ----------------- | ------ | ----- | --------------- |
| `ArgOfPeriapsis`  | number | deg   | 近日点幅角           |
| `Eccentricity`    | number | —     | 偏心率             |
| `EpochMjdTdt`     | number | MJD   | 轨道根数历元(MJD TDT) |
| `Inclination`     | number | deg   | 轨道倾角            |
| `MeanAnomaly`     | number | deg   | 平近点角            |
| `PeriTimeMjdTdt`  | number | MJD   | 近日点时刻(MJD TDT)  |
| `Q`               | number | AU    | 近日点距            |
| `Raan`            | number | deg   | 升交点赤经/黄经        |
| `SemimajorAxis`   | number | m     | 半长轴             |

## 注意事项

- 时间格式必须为 UTC ISO8601：`yyyy-MM-ddTHH:mm:ssZ`。
- `Start` 当前版本固定为轨道根数历元时刻，调用时始终传空字符串 `""`，不可自定义。
- `Stop` 传空字符串时服务端默认历元时刻 + 1 年。
- 坐标系枚举使用大写字符串，避免拼写偏差。
- 轨道根数数据来源为外部 MPC 服务（`https://data.minorplanetcenter.net/api/get-orb`），若 MPC 服务不可用则调用失败。
- 积分器采用 Heliocentric，坐标系为日心 MeanEclpJ2000。

## 标准执行流程

1. 参数预检
   - 检查 `TargetName` 必填，不可为空。
   - `Start` 传 `""`，`Stop` 可传 `""` 或用户指定的结束时刻（如指定须满足格式要求）。
2. 请求构造
   - 按接口字段名构造 JSON，未提供参数使用缺省值或传空字符串。
3. 结果判定
   - 先判 HTTP 状态是否为 200。
   - 再检查响应 JSON 中 `IsSuccess` 是否为 `true`。
4. 输出归一化
   - 返回请求摘要（目标名称、观测者、坐标系、时间区间）。
   - 输出 `OrbitElements` 中的轨道根数摘要。
   - 输出 `Position` 中的星历（CZML）数据。

## 调用示例

### 示例 1:仅指定目标名称(最小请求)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/mpc" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "TargetName": "Ceres"
  }'
```

### 示例 2:完整参数

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/mpc" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "TargetName": "Ceres",
    "ObserverName": "Sun",
    "ObserverFrame": "MEANECLPJ2000",
    "Start": "",
    "Stop": ""
  }'
```

### 示例 3:使用小行星编号并指定结束时刻

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/mpc" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "TargetName": "99942",
    "ObserverName": "Sun",
    "ObserverFrame": "MEANECLPJ2000",
    "Start": "",
    "Stop": "2026-12-31T00:00:00Z"
  }'
```

## Fixtures

- `skills/celestial-mpc/fixtures/mpc-min.json`：最小可运行请求（仅指定 TargetName）。
- `skills/celestial-mpc/fixtures/mpc-defaults.json`：包含所有默认值字段的请求模板。
