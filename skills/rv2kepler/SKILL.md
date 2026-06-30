---
name: rv2kepler
description: 将地心惯性系位置速度向量转换为开普勒轨道根数(KeplerElements)。通过 POST /OrbitConvert/RV2Kepler 输入 [X,Y,Z,Vx,Vy,Vz,μ] 数组。用户需要 RV 转 Kepler、状态向量转经典根数、笛卡尔状态转轨道六根数时使用。
---

# 位置速度转 Kepler 根数 (RV2Kepler)

通过 Astrox WebAPI 的 `POST /OrbitConvert/RV2Kepler`,将地心惯性系(ECI)下的位置与速度向量转换为开普勒轨道根数(`KeplerElements`)。

## 核心指令 (Core Instructions)

1. **输入解析**:识别长度 6 或 7 的数值数组:前 3 元为位置(m),中 3 元为速度(m/s),第 7 元(可选)为引力常数 μ(m^3/s^2)。
2. **单位一致**:位置、速度、引力常数须同一单位制;缺省为 m、m/s、m^3/s^2。
3. **引力常数**:数组仅 6 元时,μ 缺省为地球 `398600441800000`;非地球中心天体时须传入第 7 元。
4. **输出角度**: `Inclination`、`ArgumentOfPeriapsis`、`RightAscensionOfAscendingNode`、`TrueAnomaly` 均为 deg。
5. **API 调用逻辑**:向 `{BASE_URL}/OrbitConvert/RV2Kepler` 发送 `POST`,`Content-Type: application/json`,请求体为 **JSON 数组**(非对象)。

## API 规范 (Tool Definition)

### 接口地址

`POST /OrbitConvert/RV2Kepler`

### 输入参数结构 (number[])


| 索引  | 含义                     | 必须  | 单位     | 说明              |
| --- | ---------------------- | --- | ------ | --------------- |
| 0   | X                      | 是   | m      | 位置 X 分量         |
| 1   | Y                      | 是   | m      | 位置 Y 分量         |
| 2   | Z                      | 是   | m      | 位置 Z 分量         |
| 3   | Vx                     | 是   | m/s    | 速度 X 分量         |
| 4   | Vy                     | 是   | m/s    | 速度 Y 分量         |
| 5   | Vz                     | 是   | m/s    | 速度 Z 分量         |
| 6   | GravitationalParameter | 否   | m^3/s^2 | 引力常数,缺省为地球引力常数 |


常用天体引力常数参考:

- Earth: `398600441800000` 或 `3.986004418e14` (API 缺省)
- Earth (STK 常用): `3.986004415E14`
- Moon: `4.9028002221408E12`
- Mars: `4.2828375641E13`
- Sun: `1.3271244004193938E20`

OpenAPI 说明:单位须统一,亦可用 km、km/s、km^3/s^2 等同制组合。

### 响应数据结构 (KeplerElements)

成功时 HTTP 200,响应体为 `KeplerElements` 对象(无 `IsSuccess` 包装):


| 字段名                             | 类型     | 单位     | 说明        |
| ------------------------------- | ------ | ------ | --------- |
| `SemimajorAxis`                 | number | m      | 轨道半长轴     |
| `Eccentricity`                  | number | —      | 轨道偏心率     |
| `Inclination`                   | number | deg    | 轨道倾角      |
| `ArgumentOfPeriapsis`           | number | deg    | 近地点幅角     |
| `RightAscensionOfAscendingNode` | number | deg    | 升交点赤经     |
| `TrueAnomaly`                   | number | deg    | 真近点角      |
| `GravitationalParameter`        | number | m^3/s^2 | 使用的引力常数   |

坐标系:输入 RV 须为地心惯性系(ECI),与 `Kepler2RV` 输出及 TwoBody 递推所用惯性系一致。

## 注意事项

- 请求体必须是 **JSON 数组**,不能包装为 `{ "RV": [...] }` 等对象。
- 与 `POST /OrbitConvert/Kepler2RV` 互为逆变换;可与 `skills/kepler2rv/fixtures/kepler2rv-cs-test.json` 做往返验证。
- 判定成功:HTTP 200 且响应含 `SemimajorAxis` 等 Kepler 字段;失败时可能返回 400。
- 双曲线轨道(e ≥ 1)可能无法得到有效椭圆根数,需结合物理意义判断。

## 标准执行流程

1. 参数预检
  - 确认数组长度 ≥ 6
  - 确认位置、速度非全零
  - 非地球场景确认第 7 元 μ 已传入
2. 请求构造
  - 按 `[X, Y, Z, Vx, Vy, Vz]` 或 `[X, Y, Z, Vx, Vy, Vz, μ]` 构造 JSON 数组
3. 结果判定
  - HTTP 200 且响应为 KeplerElements 对象
4. 输出归一化
  - 给出输入 RV 摘要及六根数 a、e、i、ω、Ω、ν

## 调用示例

### 示例 1:椭圆轨道(上游单元测试对照)

与 C# `new KeplerElements(rv0, 3.986004418e14)` 单元测试一致;RV 来自 `kepler2rv` 同算例。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitConvert/RV2Kepler" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/rv2kepler/fixtures/rv2kepler-cs-test.json
```

期望响应(与 C# Assert 容差一致):

```json
{
  "SemimajorAxis": 7000000,
  "Eccentricity": 0.01,
  "Inclination": 45,
  "ArgumentOfPeriapsis": 30,
  "RightAscensionOfAscendingNode": 60,
  "TrueAnomaly": 90,
  "GravitationalParameter": 398600441800000
}
```

容差建议:`SemimajorAxis` ±0.001 m;其余标量 ±1e-6。

### 示例 2:近地圆轨道(6 元数组,缺省 μ)

由 `kepler2rv` 缺省根数对应的 RV;仅传 6 元,使用地球缺省引力常数。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitConvert/RV2Kepler" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/rv2kepler/fixtures/rv2kepler-earth-min.json
```

期望响应(近似):

```json
{
  "SemimajorAxis": 6878137,
  "Eccentricity": 0,
  "Inclination": 28.5,
  "ArgumentOfPeriapsis": 0,
  "RightAscensionOfAscendingNode": 0,
  "TrueAnomaly": 0
}
```

### 示例 3:内联 JSON 数组

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitConvert/RV2Kepler" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '[
    -5461764.3701692522,
    -887696.60977142153,
    4286178.3891655747,
    -1015.9743313020208,
    -7003.4269049101022,
    -2621.8538719545941,
    3.986004418e14
  ]'
```

## Fixtures


| 文件                                                   | 用途简述                                   |
| ---------------------------------------------------- | -------------------------------------- |
| `skills/rv2kepler/fixtures/rv2kepler-cs-test.json`   | 上游 C# 单元测试用例,与 kepler2rv 往返对照(推荐验证)    |
| `skills/rv2kepler/fixtures/rv2kepler-earth-min.json` | 近地圆轨道 RV,6 元数组(缺省 μ),最小可运行示例           |

## 往返验证 (Kepler2RV ↔ RV2Kepler)

```bash
export BASE_URL=http://astrox.cn:8765
# Kepler → RV
curl "${BASE_URL}/OrbitConvert/Kepler2RV" \
  --request POST -H 'Content-Type: application/json' \
  --data-binary @skills/kepler2rv/fixtures/kepler2rv-cs-test.json
# RV → Kepler
curl "${BASE_URL}/OrbitConvert/RV2Kepler" \
  --request POST -H 'Content-Type: application/json' \
  --data-binary @skills/rv2kepler/fixtures/rv2kepler-cs-test.json
```
