---
name: lambert
description: 求解 Lambert 问题(始末位置速度已知,单圈转移),输出起点和终点的速度增量(DV1、DV2)。当用户需要根据始末状态向量和飞行时间计算轨道转移速度增量时使用。支持多个Lambert转移算例同时计算。
---

# Lambert 方程求解技能 (Lambert Solver)

## 核心指令 (Core Instructions)

1. **输入解析**:识别用户提供的始末位置速度向量(RV1、RV2)和飞行时间(TOF)。
2. RV1、RV2 均为长度6的倍数数组,支持多个算例输入,每个算例前三元为位置(m),后三元为速度(m/s)
3. **单位确认**:位置分量单位为 m,速度分量单位为 m/s,飞行时间单位为 s。
4. **中心天体**:默认地球引力常数 `Gm = 3.986004415E14 m^3/s^2`,其他天体需显式传入。
5. **API 调用逻辑**:向 `{BASE_URL}/orbit/lambert` 发送 `POST`,`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址

`POST /orbit/lambert`

### 输入参数结构 (JSON)


| 参数名   | 类型       | 必须  | 说明                                                |
| ----- | -------- | --- | ------------------------------------------------- |
| `RV1` | number[] | 是   | 初始位置速度:前 3 元为位置(m),后 3 元为速度(m/s),数组长度6的倍数,支持多个算例  |
| `RV2` | number[] | 是   | 末态位置速度:前 3 元为位置(m),后 3 元为速度(m/s),数组长度 6的倍数,支持多个算例 |
| `TOF` | number[] | 是   | 飞行时间数组(s)                                         |
| `Gm`  | number   | 否   | 中心天体引力常数(m^3/s^2),缺省为 `3.986004415E14`(地球)        |


常用天体引力常数参考:

- Earth: `3.986004415E14`
- Moon: `4.9028002221408E12`
- Mars: `4.2828375641E13`
- Sun: `1.3271244004193938E20`

### 响应数据结构


| 字段          | 类型       | 说明                               |
| ----------- | -------- | -------------------------------- |
| `DV1`       | number[] | 起点速度增量 [DVx, DVy, DVz(m/s)](m/s) |
| `DV2`       | number[] | 终点速度增量 [DVx, DVy, DVz(m/s)](m/s) |
| `IsSuccess` | boolean  | 是否求解成功                           |
| `Message`   | string   | 失败时的说明信息                         |


## 注意事项

- `RV1` 和 `RV2` 均为长度至少 6 的数组:前 3 元为位置(m),后 3 元为速度(m/s)。
- `TOF` 必须为正数数组(s)。
- 若求解失败(`IsSuccess = false`),优先查看 `Message` 字段获取错误原因。
- 单圈转移假设:本接口仅支持单圈 Lambert 转移。

## 标准执行流程

1. 参数预检
  - 检查 `RV1`、`RV2` 长度均 >= 6
  - 检查 `TOF` > 0
  - 检查位置单位为 m(非 km)
2. 请求构造
  - 按接口契约原样传参,不做单位隐式转换
  - 未指定 `Gm` 时使用地球默认值
3. 结果判定
  - 先判 HTTP 状态,再判 `IsSuccess`
  - `IsSuccess = false` 时优先返回 `Message`
4. 输出归一化
  - 给出关键输入摘要、执行状态、DV1 和 DV2 向量及其大小(m/s)

## 调用示例

### 单个算例

**场景**:地球轨道单圈 Lambert 转移,飞行时间 817.4257 s。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/orbit/lambert" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "RV1": [1112487.4, 6184933, 487640.8, -8088.605, 1334.5963, 1525.882],
    "RV2": [-4963330.5, 4154175.2, 1301603, -5569.688, -5716.8755, 323.9083],
    "Gm": 398600441500000,
    "TOF": [817.4257]
  }'
```

### 多个算例(同时计算多个Lambert转移)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/orbit/lambert" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "RV1": [1112487.4, 6184933, 487640.8, -8088.605, 1334.5963, 1525.882, 1112487.4, 6184933, 487640.8, -8088.605, 1334.5963, 1525.882],
    "RV2": [-4963330.5, 4154175.2, 1301603, -5569.688, -5716.8755, 323.9083, -4963330.5, 4154175.2, 1301603, -5569.688, -5716.8755, 323.9083],
    "Gm": 398600441500000,
    "TOF": [817.4257, 840]
  }'
```

## 本地快速验证(可选)

用 fixture 可避免 PowerShell 下行内 JSON 转义问题:

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/orbit/lambert" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/lambert/fixtures/lambert-earth-min.json"
```

## 更多示例与测试数据(fixtures)


| 文件                                               | 用途简述                    |
| ------------------------------------------------ | ----------------------- |
| `skills/lambert/fixtures/lambert-earth-min.json` | 地球单圈 Lambert 转移,最小可运行示例 |


