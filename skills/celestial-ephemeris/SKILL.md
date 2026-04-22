---
name: celestial-ephemeris
description: 根据目标天体、观测者和时间步长计算天体星历,并输出 CZML。用户需要行星/月球相对位置时使用。
---

# 天体星历计算技能 (Celestial Ephemeris)

通过 Astrox WebAPI 的 `POST /celestial/ephemeris` 计算目标天体在指定时间段内的星历数据,输出 CZML 格式。

## 核心指令 (Core Instructions)

1. **输入解析**:识别目标天体、观测者、观测者坐标系、分析开始/结束时刻和积分步长。
2. **参数处理**:若 `ObserverName`、`ObserverFrame`、`Step` 未提供,使用缺省值;`Start`/`Stop` 可空。
3. **API 调用逻辑**:向 `{BASE_URL}/celestial/ephemeris` 发送 `POST`,`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址

`POST /celestial/ephemeris`

### 输入参数结构 (JSON)


| 参数名             | 类型     | 必须  | 缺省值             | 说明                                                                           |
| --------------- | ------ | --- | --------------- | ---------------------------------------------------------------------------- |
| `TargetName`    | string | 是   | `Earth`         | 目标名称,如 `Moon`、`Mars`、`Venus`、`Mercury`、`Jupiter`、`Saturn`、`Uranus`、`Neptune` |
| `ObserverName`  | string | 否   | `Sun`           | 观测者名称                                                                        |
| `ObserverFrame` | string | 否   | `MEANECLPJ2000` | 观测者坐标系,可选:`FIXED`、`INERTIAL`、`MEANECLPJ2000`、`J2000`                         |
| `Start`         | string | 否   | 当年 1 月 1 日      | 分析开始时刻(UTCG),格式:`yyyy-MM-ddTHH:mm:ssZ`                                       |
| `Stop`          | string | 否   | 当年 12 月 31 日    | 分析结束时刻(UTCG),格式:`yyyy-MM-ddTHH:mm:ssZ`                                       |
| `Step`          | number | 否   | `86400`         | 积分步长(s)                                                                      |


### 输出说明

- 服务返回目标天体在时间区间内的星历数据,格式为 CZML(JSON)。
- 判定成功建议同时检查 HTTP 状态码和业务字段(若返回体包含 `IsSuccess`)。

## 注意事项

- 时间格式必须为 UTC ISO8601:`yyyy-MM-ddTHH:mm:ssZ`。
- 当 `Start` 和 `Stop` 为空字符串时,由服务端按缺省规则取值(当年全年)。
- `Step` 单位为秒,建议为正数且与分析区间长度匹配。
- 坐标系枚举建议严格使用大写字符串,避免拼写偏差。

## 标准执行流程

1. 参数预检
  - 检查 `TargetName` 必填。
  - 若 `Start`、`Stop` 非空,检查时间格式且 `Start < Stop`。
  - 检查 `Step > 0`。
2. 请求构造
  - 按接口字段名构造 JSON,未提供参数可省略或传默认值。
3. 结果判定
  - 先判 HTTP 状态是否为 200。
  - 再检查响应 JSON 结构是否为合法 CZML(数组/对象)。
4. 输出归一化
  - 返回请求摘要(目标、观测者、坐标系、时间区间、步长)与 CZML 结果。

## 调用示例

### 示例 1:默认参数(仅指定目标)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/ephemeris"   --request POST   --header 'Content-Type: application/json'   --data '{
  "TargetName": "Earth"
}'
```

### 示例 2:完整参数(用户提供示例)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/ephemeris"   --request POST   --header 'Content-Type: application/json'   --data '{
  "TargetName": "Earth",
  "ObserverName": "Sun",
  "ObserverFrame": "MEANECLPJ2000",
  "Start": "",
  "Stop": "",
  "Step": 86400
}'
```

## Fixtures

- `skills/celestial-ephemeris/fixtures/celestial-ephemeris-min.json`:最小可运行请求。
- `skills/celestial-ephemeris/fixtures/celestial-ephemeris-defaults.json`:包含默认值字段的请求模板。

