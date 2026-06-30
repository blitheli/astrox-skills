---
name: kepler2rv
description: 将开普勒轨道根数(KeplerElements)转换为地心惯性系位置速度向量。通过 POST /OrbitConvert/Kepler2RV 输出 [X,Y,Z,Vx,Vy,Vz](m,m/s)。用户需要 Kepler 根数转 RV、椭圆轨道状态向量、经典根数转笛卡尔状态时使用。
---

# Kepler 根数转位置速度 (Kepler2RV)

通过 Astrox WebAPI 的 `POST /OrbitConvert/Kepler2RV`,将开普勒轨道根数(`KeplerElements`)转换为地心惯性系(ECI)下的位置与速度向量。

## 核心指令 (Core Instructions)

1. **输入解析**:识别用户提供的 6 个经典根数及可选引力常数 `GravitationalParameter`。
2. **适用轨道**:椭圆轨道(偏心率 `Eccentricity` < 1);圆轨道可令 `Eccentricity = 0`。
3. **角度单位**: `Inclination`、`ArgumentOfPeriapsis`、`RightAscensionOfAscendingNode`、`TrueAnomaly` 均为 deg。
4. **长度/速度单位**:半长轴 `SemimajorAxis` 为 m;输出位置 m、速度 m/s。
5. **引力常数**:缺省为地球 `398600441800000 m^3/s^2`;其他中心天体需显式传入 `GravitationalParameter`。
6. **API 调用逻辑**:向 `{BASE_URL}/OrbitConvert/Kepler2RV` 发送 `POST`,`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址

`POST /OrbitConvert/Kepler2RV`

### 输入参数结构 (KeplerElements)


| 参数名                               | 类型     | 必须  | 缺省值              | 说明                    |
| --------------------------------- | ------ | --- | ---------------- | --------------------- |
| `SemimajorAxis`                   | number | 否   | `6878137`        | 轨道半长轴(m)              |
| `Eccentricity`                    | number | 否   | `0`              | 轨道偏心率                 |
| `Inclination`                     | number | 否   | `28.5`           | 轨道倾角(deg)              |
| `ArgumentOfPeriapsis`             | number | 否   | `0`              | 近地点幅角(deg)             |
| `RightAscensionOfAscendingNode`   | number | 否   | `0`              | 升交点赤经(deg)             |
| `TrueAnomaly`                     | number | 否   | `0`              | 真近点角(deg)              |
| `GravitationalParameter`          | number | 否   | `398600441800000` | 引力常数(m^3/s^2),默认地球引力常数 |


常用天体引力常数参考:

- Earth: `398600441800000` (API 缺省)
- Earth (STK 常用): `3.986004415E14`
- Moon: `4.9028002221408E12`
- Mars: `4.2828375641E13`
- Sun: `1.3271244004193938E20`

### 响应数据结构

成功时 HTTP 200,响应体为长度 6 的 `number[]`(无 `IsSuccess` 包装):


| 索引  | 含义        | 单位   |
| --- | --------- | ---- |
| 0   | X         | m    |
| 1   | Y         | m    |
| 2   | Z         | m    |
| 3   | Vx        | m/s  |
| 4   | Vy        | m/s  |
| 5   | Vz        | m/s  |


坐标系:地心惯性系(ECI),与 Astrox 经典根数/TwoBody 递推所用惯性系一致。

## 注意事项

- 本接口仅接受 `KeplerElements` 对象,不支持数组形式根数;若用户给出 `[a, e, i, ω, Ω, ν]` 数组,需映射为上述 PascalCase 字段名。
- 与 `POST /OrbitConvert/RV2Kepler` 互为逆变换(后者输入 `[X,Y,Z,Vx,Vy,Vz, μ]` 数组)。
- 判定成功:HTTP 200 且响应为 6 元数值数组;失败时可能返回 400 及 validation errors。
- 未指定字段时使用 OpenAPI 缺省值(约 400 km 高度圆轨道,倾角 28.5°)。

## 标准执行流程

1. 参数预检
  - 确认 `SemimajorAxis` > 0
  - 确认 `0 <= Eccentricity < 1`
  - 确认角度在合理范围(通常 0–360 deg,倾角 0–180 deg)
2. 请求构造
  - 按 PascalCase 字段名构造 JSON
  - 非地球中心天体时显式传入 `GravitationalParameter`
3. 结果判定
  - HTTP 200 且数组长度为 6
4. 输出归一化
  - 给出输入根数摘要、位置向量 `[X,Y,Z]`、速度向量 `[Vx,Vy,Vz]` 及速度大小 `|V|`

## 调用示例

### 示例 2:椭圆轨道(上游单元测试对照)

与 C# `KeplerElements.ToRV()` 单元测试一致:a=7000000 m,e=0.01,i=45°,ω=30°,Ω=60°,ν=90°,μ=3.986004418e14。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitConvert/Kepler2RV" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/kepler2rv/fixtures/kepler2rv-cs-test.json
```

期望响应(与 C# Assert 容差一致):

```json
[
  -5461764.3701692522,
  -887696.60977142153,
  4286178.3891655747,
  -1015.9743313020208,
  -7003.4269049101022,
  -2621.8538719545941
]
```

容差建议:位置分量 ±0.001 m,速度分量 ±1e-6 m/s。

### 示例 3:近地圆轨道(使用 API 缺省根数)

缺省半长轴 6878137 m(约 400 km 高度)、e=0、i=28.5°、ν=0°。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitConvert/Kepler2RV" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/kepler2rv/fixtures/kepler2rv-earth-min.json
```

期望响应:

```json
[6878137, 0, 0, 0, 6690.090334619479, 3632.4226782776636]
```

### 示例 4:内联 JSON(GEO 圆轨道)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitConvert/Kepler2RV" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "SemimajorAxis": 42164193,
    "Eccentricity": 0,
    "Inclination": 0,
    "ArgumentOfPeriapsis": 0,
    "RightAscensionOfAscendingNode": 0,
    "TrueAnomaly": 0
  }'
```

## Fixtures


| 文件                                                      | 用途简述                                      |
| ------------------------------------------------------- | ----------------------------------------- |
| `skills/kepler2rv/fixtures/kepler2rv-cs-test.json`      | 上游 C# 单元测试用例,含期望 RV 对照(推荐验证)              |
| `skills/kepler2rv/fixtures/kepler2rv-earth-min.json`    | 近地圆轨道,使用 API 缺省根数,最小可运行示例                |
| `skills/kepler2rv/fixtures/kepler2rv-ellipse-min.json`  | 椭圆轨道,e=0.001,Ω=120°,备用示例                 |
