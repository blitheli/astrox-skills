---
name: convert-czmlPosition
description: 将 CZML 格式位置序列转换到目标天体坐标系(Inertial/Fixed/J2000/ICRF 等)。采用 JPL DE430 历表;用户需要地月转移、行星系间坐标变换或 CzmlPosition 参考系转换时使用。
---

# CZML 位置坐标系转换技能 (Convert CzmlPosition)

通过 Astrox WebAPI 的 `POST /OrbitSystem/ConvertCzmlPosition`,将 `EntityPositionCzml` 格式的位置(可选速度)序列从输入参考系转换到指定中心天体与参考坐标系。典型场景:地心惯性系下的转移轨道星历转换到月球 Fixed/Inertial 系。

## 核心指令 (Core Instructions)

1. **输入解析**:识别 `Position`(CZML 位置对象)、`TargetCbName`(目标中心天体)、`TargetFrame`(目标参考系)。
2. **Position 格式**: `Position` 为 `EntityPositionCzml` 结构,含 `epoch`、`referenceFrame`、`cartesian` 或 `cartesianVelocity`;数组平铺格式见 `skills/shared-docs/api-schemas/CzmlPositionOut.md`。
3. **历表与坐标轴**:服务端采用 **JPL DE430** 历表;除 Earth、Moon 外其它行星坐标轴采用 IAU2015;Earth 的 Inertial 为 ICRF、Fixed 为 ITRF;Sun 为 ICRF;Moon 的 Fixed 为 DE430 Mean Earth 坐标轴。
4. **API 调用逻辑**:向 `{BASE_URL}/OrbitSystem/ConvertCzmlPosition` 发送 `POST`,`Content-Type: application/json`。
5. **结果说明**:成功时返回 `CzmlPositionOut`,其中 `Position` 的 `CentralBody` 与 `referenceFrame` 已更新为目标天体与坐标系,`cartesian`/`cartesianVelocity` 为转换后的序列。

## API 规范 (Tool Definition)

### 接口地址

`POST /OrbitSystem/ConvertCzmlPosition`

### 输入参数结构 (JSON)


| 参数名            | 类型     | 必须  | 缺省值       | 说明                                                         |
| -------------- | ------ | --- | --------- | ---------------------------------------------------------- |
| `Position`     | object | 是   | —         | 待转换的 CZML 位置对象(`EntityPositionCzml`),见下表子字段                 |
| `TargetCbName` | string | 是   | `Earth`   | 转换后的目标中心天体名称,如 `Earth`、`Moon`、`Mars`                      |
| `TargetFrame`  | string | 是   | `INERTIAL` | 转换后的参考坐标系:`INERTIAL`、`FIXED`、`J2000`、`ICRF` 等(大小写不敏感) |


#### Position 子字段 (EntityPositionCzml)


| 子字段名                   | 类型       | 必须  | 缺省值        | 说明                                              |
| ---------------------- | -------- | --- | ---------- | ----------------------------------------------- |
| `epoch`                | string   | 是   | —          | 历元时刻 UTC ISO8601,如 `2020-11-23T21:06:35.7609999999986Z` |
| `CentralBody`          | string   | 否   | `Earth`    | 输入数据所属中心天体(通常可省略,由 `referenceFrame` 与数据上下文推断)    |
| `referenceFrame`       | string   | 否   | `FIXED`    | 输入参考系:`INERTIAL`、`FIXED`、`J2000`、`ICRF` 等        |
| `interpolationAlgorithm` | string | 否   | `LAGRANGE` | 插值算法:`LINEAR`、`LAGRANGE`、`HERMITE`              |
| `interpolationDegree`  | integer  | 否   | `7`        | 插值阶数                                            |
| `cartesian`            | number[] | 否*  | `null`     | 位置序列 `[Time, X, Y, Z, ...]`,Time 为相对 epoch 的秒,m   |
| `cartesianVelocity`    | number[] | 否*  | `null`     | 位置+速度序列 `[Time, X, Y, Z, Vx, Vy, Vz, ...]`,m 与 m/s |
| `interval`             | string   | 否   | `null`     | 数据时间段,如 `2023-11-06T04:10:00Z/2023-11-06T04:25:00Z` |


\* `cartesian` 与 `cartesianVelocity` 至少提供其一。

### 响应数据结构 (CzmlPositionOut)

详见 `skills/shared-docs/api-schemas/CzmlPositionOut.md`。


| 字段名         | 类型      | 说明                    |
| ----------- | ------- | --------------------- |
| `IsSuccess` | boolean | 结果(`true`:成功;`false`:失败) |
| `Message`   | string  | 结果信息(失败原因等)           |
| `Position`  | object  | 转换后的 CZML 位置对象        |
| `Period`    | number  | 轨道周期(s),近地轨道场景可能返回     |

## 注意事项

- 时间字符串优先使用 UTC ISO8601,例如 `2020-11-23T21:06:35.7609999999986Z`。
- 位置单位:m;速度单位:m/s;Time 偏移:相对 `epoch` 的历元秒(s)。
- 输入 `Position.referenceFrame` 描述**源**参考系;`TargetFrame` 描述**目标**参考系;二者可不同(如 Earth Inertial → Moon Fixed)。
- 判定成功:HTTP 200 且 `IsSuccess` 为 `true`。
- 完整地月转移(E2M)星历验证需使用全长 `cartesian` 序列(约 403200 s 步长);fixture 中提供截断版用于快速冒烟测试。

## 标准执行流程

1. 参数预检:确认 `Position.epoch` 存在;`cartesian` 或 `cartesianVelocity` 非空;数组长度为 4 或 7 的整数倍(加 Time);`TargetCbName`、`TargetFrame` 已指定。
2. 请求构造:按 OpenAPI 字段名 PascalCase 构造 JSON(`Position`、`TargetCbName`、`TargetFrame`);`Position` 内部字段保持 camelCase。
3. 结果判定:HTTP 200 + `IsSuccess === true`;失败时返回 `Message`。
4. 输出归一化:摘要源/目标天体与参考系、采样点数;必要时抽取首/末时刻位置用于与 STK 等工具对照。

## 调用示例

### 示例 1:地心惯性系星历 → 月球 Fixed 系(地月转移,截断星历)

对应上游测试 `GetScInCbSystemTest` 中 Moon Fixed 分支;首点期望(与 STK 一致):`[0, 3.9631923150127202e+08, 4.4511426953787886e+07, 4.7966196271462679e+07]` m。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitSystem/ConvertCzmlPosition" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/convert-czml-position/fixtures/convert-czml-position-moon-fixed.json
```

### 示例 2:地心惯性系星历 → 月球 Inertial 系

首点期望(与 STK 一致):`[0, -3.9727941178273672e+08, 3.8590773800480925e+07, 4.5064148976947613e+07]` m。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitSystem/ConvertCzmlPosition" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/convert-czml-position/fixtures/convert-czml-position-moon-inertial.json
```

### 示例 3:仅 Position  payload(内联最小片段)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitSystem/ConvertCzmlPosition" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Position": {
      "interpolationAlgorithm": "LAGRANGE",
      "interpolationDegree": 7,
      "referenceFrame": "INERTIAL",
      "epoch": "2020-11-23T21:06:35.7609999999986Z",
      "cartesian": [0, -1808774.52236705, -6210104.58567043, -2393122.32753525]
    },
    "TargetCbName": "Moon",
    "TargetFrame": "Fixed"
  }'
```

## Fixtures


| 文件                                                                      | 用途简述                                   |
| ----------------------------------------------------------------------- | -------------------------------------- |
| `skills/convert-czml-position/fixtures/e2m-position-min.json`           | 地月转移 Earth Inertial 位置片段(截断,约 18 个采样点) |
| `skills/convert-czml-position/fixtures/convert-czml-position-moon-fixed.json`    | 上述星历 → Moon Fixed,完整请求体                 |
| `skills/convert-czml-position/fixtures/convert-czml-position-moon-inertial.json` | 上述星历 → Moon Inertial,完整请求体              |

## 验证参考 (与 STK 对照)

上游单元测试 `GetScInCbSystemTest` 使用 `E2M_Position20220402.json` 全长星历,DE430 历表。截断 fixture 首点应与下表一致(容差约 1 m):


| 目标系            | Time (s) | X (m)              | Y (m)             | Z (m)             |
| -------------- | -------- | ------------------ | ----------------- | ----------------- |
| Moon Fixed     | 0        | 3.9631923150127202e8 | 4.4511426953787886e7 | 4.7966196271462679e7 |
| Moon Inertial  | 0        | -3.9727941178273672e8 | 3.8590773800480925e7 | 4.5064148976947613e7 |
