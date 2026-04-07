---
name: lighting-times
description: 计算地面站、卫星等对象的光照时间，考虑 De430 视太阳位置。支持天体遮挡和地形遮罩。当用户需要计算光照时间、阴影(日食)时间、半影/本影时间时使用。
---

# 光照时间计算技能 (Lighting Times)

## 核心指令 (Core Instructions)

1. **输入解析**：识别用户提供的分析时间范围、位置对象（地面站或卫星）、可选地形遮罩和遮挡天体列表。
2. **位置对象匹配**：根据用户需求选择合适的位置类型：
  - 地面站：`SitePosition`（可带地形遮罩）
  - 卫星：`J2`、`TwoBody`、`CzmlPositions`、`SGP4` 等
3. **API 调用逻辑**：向 `{BASE_URL}/Lighting/LightingTimes` 发送 `POST`，`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址

`POST /Lighting/LightingTimes`

### 输入参数结构 (JSON)


| 参数名                 | 类型     | 必须  | 说明                                                                            |
| ------------------- | ------ | --- | ----------------------------------------------------------------------------- |
| `Description`       | string | 否   | 说明                                                                            |
| `Start`             | string | 是   | 分析开始时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ")                                        |
| `Stop`              | string | 是   | 分析结束时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ")                                        |
| `Position`          | object | 是   | 位置对象 (IEntityPosition)，详见 `skills/shared-docs/api-schemas/IEntityPosition.md` |
| `AzElMaskData`      | array  | 否   | 地形遮罩数据(仅地面站使用)，格式：(方位角1，高度角1，方位角2，高度角2...)(单位:rad)                            |
| `OccultationBodies` | array  | 否   | 遮挡天体列表，第1个为中心天体；默认：地月中心天体为地月，其他为自身中心天体                                        |


### 位置类型 (Position)

`Position` 采用 `IEntityPosition` 多态结构，不在本技能文档内展开字段定义。  
统一引用：`skills/shared-docs/api-schemas/IEntityPosition.md`。

### 响应数据结构（LightingTimesOut）

`LightingTimesOut` 为光照时间计算输出对象，字段如下：


| 字段名         | 类型              | 说明                           |
| ----------- | --------------- | ---------------------------- |
| `IsSuccess` | boolean         | 结果标识（`true`: 成功；`false`: 失败） |
| `Message`   | string          | 错误信息（失败时存储失败原因）              |
| `SunLight`  | `LightTimeData` | 全光照时间参数                      |
| `Penumbra`  | `LightTimeData` | 半影时间参数                       |
| `Umbra`     | `LightTimeData` | 本影（无光照）时间参数                  |


### LightTimeData 结构

`LightTimeData` 用于描述某一种光照状态（`SunLight`、`Penumbra`、`Umbra`）的统计结果：


| 字段名             | 类型                   | 说明            |
| --------------- | -------------------- | ------------- |
| `Intervals`     | `TimeIntervalData`[] | 所有时间段数组       |
| `MinDuration`   | `TimeIntervalData`   | 最小持续时间        |
| `MaxDuration`   | `TimeIntervalData`   | 最大持续时间        |
| `MeanDuration`  | number               | 平均时长，单位：秒 (s) |
| `TotalDuration` | number               | 总时长，单位：秒 (s)  |


### TimeIntervalData 结构

`TimeIntervalData` 为时间间隔对象：


| 字段名        | 类型     | 说明                                        |
| ---------- | ------ | ----------------------------------------- |
| `Start`    | string | 开始时刻 (UTCG)，格式：`yyyy-MM-ddTHH:mm:ss.fffZ` |
| `Stop`     | string | 结束时刻 (UTCG)，格式：`yyyy-MM-ddTHH:mm:ss.fffZ` |
| `Duration` | number | 持续时间，单位：秒 (s)                             |


## 注意事项

- 时间格式：使用 ISO8601 格式 (`yyyy-MM-ddTHH:mm:ssZ`)
- 坐标单位：经纬度为度(deg)，高度为米(m)
- 地形遮罩：`AzElMaskData` 仅用于地面站，单位为弧度(rad)
- 遮挡天体：默认根据中心天体自动选择（地/月球为地月，其他为自身中心天体）
- 光延迟：考虑光延迟，不考虑光行差，与 STK 结果一致

## 标准执行流程

1. 参数预检
  - 检查必填字段：`Start`、`Stop`、`Position`
  - 检查 UTC 时间格式
  - 检查 `Start < Stop`
2. 位置判定
  - 根据用户描述判断是地面站还是卫星
  - 若为地面站，检查 `cartographicDegrees` 格式
  - 参考具体的位置类型要求检查参数完整性
3. 请求构造
  - 按接口契约原样传参
4. 结果判定
  - 先判 HTTP 状态，再判 `IsSuccess`
  - `IsSuccess = false` 时优先返回 `Message`
5. 输出归一化
  - 给出关键输入摘要、执行状态、核心输出（日照/半影/本影时间）

## 调用示例

### 示例 1：地面站光照计算（无地形）

**场景**：地球地面站，圣海伦斯火山坑，计算 48 小时光照情况。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Lighting/LightingTimes" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Description": "St.Helens火山坑一点的光照",
    "Start": "2022-09-05T04:00:00Z",
    "Stop": "2022-09-07T04:00:00Z",
    "Position": {
      "$type": "SitePosition",
      "CentralBody": "Earth",
      "cartographicDegrees": [-122.18936, 46.19557, 0],
      "clampToGround": false
    }
  }'
```

### 示例 2：月面站光照计算

**场景**：月面 Bruno 坑，计算 1 年光照情况。

```bash
curl "${BASE_URL}/Lighting/LightingTimes" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Description": "Moon Bruno坑一点的光照",
    "Start": "2022-09-05T04:00:00Z",
    "Stop": "2023-09-04T04:00:00Z",
    "Position": {
      "$type": "SitePosition",
      "CentralBody": "Moon",
      "cartographicDegrees": [102.91745, 35.911758, -2345.2],
      "clampToGround": true
    }
  }'
```

### 示例 3：J2 卫星光照计算

**场景**：地球 J2 摄动卫星，计算 24 小时光照情况。

```bash
curl "${BASE_URL}/Lighting/LightingTimes" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Description": "地球J2卫星光照计算",
    "Start": "2022-09-05T04:00:00Z",
    "Stop": "2022-09-06T04:00:00Z",
    "Position": {
      "$type": "J2",
      "OrbitEpoch": "2022-09-05T04:00:00Z",
      "CoordSystem": "Inertial",
      "CoordType": "Classical",
      "OrbitalElements": [7678140, 0, 58.5, 0, 0, 0]
    }
  }'
```

## 本地快速验证（可选）

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Lighting/LightingTimes" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-times/fixtures/site-earth-min.json"
```

## 更多示例与测试数据（fixtures）


| 文件                                                               | 用途简述                     |
| ---------------------------------------------------------------- | ------------------------ |
| `lighting-times/fixtures/site-earth-min.json`                    | 地球地面站，48 小时计算            |
| `lighting-times/fixtures/site-moon-burno.json`                   | 月面站，1 年计算                |
| `lighting-times/fixtures/satellite-j2.json`                      | J2 卫星，24 小时计算            |
| `lighting-times/fixtures/site-earth-terrain-mask.json`           | 地球地面站 + 地形遮罩样例           |
| `lighting-times/fixtures/site-moon-bruno-terrain-mask.json`      | 月面 Bruno 点 + 地形遮罩样例      |
| `lighting-times/fixtures/site-moon-pole.json`                    | 月球南极附近地面点，长时段计算          |
| `lighting-times/fixtures/site-moon-bode.json`                    | 月面博得月溪点（含月食场景）           |
| `lighting-times/fixtures/site-mars-utopia.json`                  | 火星乌托邦平原地面点               |
| `lighting-times/fixtures/satellite-moon-twobody.json`            | 月球中心 TwoBody 轨道卫星        |
| `lighting-times/fixtures/satellite-mars-twobody.json`            | 火星中心 TwoBody 轨道卫星        |
| `lighting-times/fixtures/satellite-earth-czmlpositions-min.json` | 地球 CzmlPositions 星历（节选点） |


## 响应示例

```json
{
  "IsSuccess": true,
  "SunLight": {
    "Intervals": [
      {
        "Start": "2022-09-05T13:40:58.370Z",
        "Stop": "2022-09-06T02:32:56.676Z",
        "Duration": 46318.3
      }
    ],
    "MinDuration": { "Start": "...", "Stop": "...", "Duration": 46128.403 },
    "MaxDuration": { "Start": "...", "Stop": "...", "Duration": 46318.302 },
    "MeanDuration": 46223.352,
    "TotalDuration": 92446.705
  },
  "Penumbra": {
    "Intervals": [
      {
        "Start": "2022-09-05T13:37:52.308Z",
        "Stop": "2022-09-05T13:40:58.370Z",
        "Duration": 186.066
      }
    ],
    "MeanDuration": 185.676,
    "TotalDuration": 742.706
  },
  "Umbra": {
    "Intervals": [
      {
        "Start": "2022-09-05T04:00:00.000Z",
        "Stop": "2022-09-05T13:37:52.308Z",
        "Duration": 34672.308
      }
    ],
    "MeanDuration": 19906.863,
    "TotalDuration": 79610.589
  }
}
```

