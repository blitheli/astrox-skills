---
name: lighting-solar-intensity
description: 计算地面站或飞行器相对视太阳的光照强度参数(太阳圆盘可见比例 Intensity、遮挡率 PercentShadow、日食状态 Sunlight/Umbra/Penumbra 等),基于 DE430。当用户需要 SolarIntensity、光照强度、遮挡率、本影/半影/全日照、太阳圆盘可见比,或口头提及 lighting-solarIntensity 时使用。
---

# 视太阳光照强度计算技能 (Solar Intensity)

别名:`lighting-solarIntensity`(口头/历史命名);目录与 frontmatter 统一为 `lighting-solar-intensity`。

## 核心指令 (Core Instructions)

1. **输入解析**:识别用户提供的分析时间范围、位置对象(地面站或飞行器)、计算步长、可选遮挡天体列表与地形遮罩。
2. **位置对象匹配**:根据用户需求选择合适的位置类型:
  - 地面站:`SitePosition`(可附 `AzElMaskData` 考虑地形)
  - 飞行器:`TwoBody`、`J2`、`SGP4`、`CzmlPositions` 等(仅天体遮挡,无地形)
3. **API 调用逻辑**:向 `{BASE_URL}/Lighting/SolarIntensity` 发送 `POST`,`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址

`POST /Lighting/SolarIntensity`

摘要:视太阳光照强度参数计算;DE430;取代原 `SolarIntensitySc` / `SolarIntensitySite`。

### 输入参数结构 (JSON) — SolarIntensityInput


| 参数名                 | 类型       | 必须  | 说明                                                                                         |
| ------------------- | -------- | --- | ------------------------------------------------------------------------------------------ |
| `Description`       | string   | 否   | 说明                                                                                         |
| `Start`             | string   | 是   | 分析开始时刻 (UTCG);常用 ISO8601 (`yyyy-MM-ddTHH:mm:ssZ`),WebApi 亦接受 STK 风格日期(见 `site-moon-pole.json`) |
| `Stop`              | string   | 是   | 分析结束时刻 (UTCG);格式同 `Start`                                                                  |
| `Position`          | object   | 是   | 位置对象 (IEntityPosition),详见 `skills/shared-docs/api-schemas/IEntityPosition.md`              |
| `AzElMaskData`      | number[] | 否   | 仅地面站;地形遮罩扁平数组 `[az,el,az,el,...]`,单位:弧度 (rad);可先调 `/Terrain/AzElMask` 再传入                |
| `TimeStepSec`       | number   | 否   | 计算步长,单位:秒 (s);缺省 3600                                                                      |
| `OccultationBodies` | string[] | 否   | 遮挡天体列表;第 1 个为中心天体。未提供时:日/地/月中心 → 地月遮挡;其它中心 → 自身中心天体;太阳中心 → 无                            |


### 位置类型 (Position)

`Position` 采用 `IEntityPosition` 多态结构,不在本技能文档内展开字段定义。  
统一引用:`skills/shared-docs/api-schemas/IEntityPosition.md`。

### 响应数据结构 (SolarIntensityOut)


| 字段名         | 类型                     | 说明                           |
| ----------- | ---------------------- | ---------------------------- |
| `IsSuccess` | boolean                | 结果标识(`true`: 成功;`false`: 失败) |
| `Message`   | string                 | 错误信息(失败时存储失败原因)              |
| `Datas`     | `SolarIntensityData`[] | 均匀离散时间点的光照参数(多态 `$type`)     |


### Datas 多态类型

按 `$type` 区分飞行器与地面站:

#### SolarIntensityScData(飞行器)


| 字段名                  | 类型     | 说明                                      |
| -------------------- | ------ | --------------------------------------- |
| `$type`              | string | 固定 `SolarIntensityScData`               |
| `CurrentCondition`   | string | 当前光照状态:`SunLight` / `Umbra` / `Penumbra`(OpenAPI 描述写作 Sunlight) |
| `Obstruction`        | string | 当前遮挡天体:`None` / `Earth` / `Moon` 等      |
| `Time`               | string | 时刻 (UTCG),格式:`yyyy-MM-ddTHH:mm:ss.fffZ` |
| `Intensity`          | number | 太阳圆盘可见性比例(全可见 1,不可见 0)                  |
| `PercentShadow`      | number | 太阳圆盘遮挡比例(全遮挡 1,未遮挡 0)                   |
| `ApparentSolarRange` | number | 视太阳距离,单位:千米 (km)                        |
| `SolarDiskHalfAngle` | number | 视太阳半径(半角),单位:度 (deg)                    |
| `SolarGrazingAngle`  | number | 中心天体边界与视太阳夹角,单位:度 (deg)                 |
| `RelativeAngle`      | number | 中心天体质心到视太阳中心的视角度                        |


#### SolarIntensitySiteData(地面站)


| 字段名                      | 类型     | 说明                                      |
| ------------------------ | ------ | --------------------------------------- |
| `$type`                  | string | 固定 `SolarIntensitySiteData`             |
| `ApparentSolarAzimuth`   | number | 视太阳方位角,单位:度 (deg)                       |
| `ApparentSolarElevation` | number | 视太阳高度角,单位:度 (deg)                       |
| `TerrainElevation`       | number | 地形高度角,单位:度 (deg)                        |
| `Time`                   | string | 时刻 (UTCG),格式:`yyyy-MM-ddTHH:mm:ss.fffZ` |
| `Intensity`              | number | 太阳圆盘可见性比例(全可见 1,不可见 0)                  |
| `PercentShadow`          | number | 太阳圆盘遮挡比例(全遮挡 1,未遮挡 0)                   |
| `ApparentSolarRange`     | number | 视太阳距离,单位:千米 (km)                        |
| `SolarDiskHalfAngle`     | number | 视太阳半径(半角),单位:度 (deg)                    |
| `SolarGrazingAngle`      | number | 中心天体边界与视太阳夹角,单位:度 (deg)                 |
| `RelativeAngle`          | number | 相对角度                                    |


## 注意事项

- 必填:`Start`、`Stop`、`Position`
- 时间格式:优先 ISO8601 UTC (`yyyy-MM-ddTHH:mm:ssZ`);正式 fixture `site-moon-pole.json` 使用 STK 风格日期(`26 Mar 2024 00:36:46.000`),WebApi 实测可解析
- 坐标单位:经纬度为度 (deg),高度为米 (m);`AzElMaskData` 为弧度 (rad)
- 星历:DE430;仅考虑光延迟,不考虑光行差,与 STK 一致
- 遮挡逻辑:提供 `OccultationBodies` 则用列表;否则按中心天体默认(见上表)。第 1 个遮挡天体为中心天体
- 飞行器仅天体遮挡、无地形;地面站可考虑地形(`AzElMaskData`)
- `Intensity` + `PercentShadow` 通常互补(可见比 + 遮挡比)
- 失败判定:先判 HTTP 状态,再判 `IsSuccess`;失败时优先返回 `Message`

## 标准执行流程

1. 参数预检
  - 检查必填字段:`Start`、`Stop`、`Position`
  - 检查时间可解析且 `Start < Stop`
2. 位置判定
  - 地面站 vs 飞行器;地面站可按需附带 `AzElMaskData`
  - 需要自定义遮挡时设置 `OccultationBodies`(第 1 个为中心天体)
3. 请求构造
  - 按接口契约原样传参(含正式 fixture 中的 STK 日期 / 完整 `AzElMaskData`)
4. 结果判定
  - 先判 HTTP 状态,再判 `IsSuccess`
5. 输出归一化
  - 按 `$type` 解读 `Datas`;摘要 Intensity / PercentShadow / CurrentCondition(飞行器) 或 ApparentSolarElevation / TerrainElevation(地面站)

## 调用示例

正式 fixtures 来自 `ASTROX.AeroSpace.Tests/Lighting/SolarIntensity_*.cs`(C# raw string JSON 原样落盘)。

### 示例 1:月球 TwoBody 卫星(进出本影)

**场景**:月球中心 TwoBody 卫星,约 30 s 窗口、步长 1 s,覆盖 SunLight → Umbra。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Lighting/SolarIntensity" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-solar-intensity/fixtures/satellite-moon-twobody.json"
```

### 示例 2:月球南极站(STK 日期,无地形)

**场景**:南极坑顶部地面点;Start/Stop 为 STK 风格日期字符串,与测试一致。

```bash
curl "${BASE_URL}/Lighting/SolarIntensity" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-solar-intensity/fixtures/site-moon-pole.json"
```

### 示例 3:地球 St.Helens(含地形遮罩)

```bash
curl "${BASE_URL}/Lighting/SolarIntensity" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-solar-intensity/fixtures/site-earth-sthelens-terrain-mask.json"
```

### 示例 4:月球 Bruno 坑(含地形遮罩)

```bash
curl "${BASE_URL}/Lighting/SolarIntensity" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-solar-intensity/fixtures/site-moon-bruno-terrain-mask.json"
```

### 示例 5:月球南极 CE7(含地形遮罩)

```bash
curl "${BASE_URL}/Lighting/SolarIntensity" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-solar-intensity/fixtures/site-moon-ce7-terrain-mask.json"
```

## 本地快速验证(可选)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Lighting/SolarIntensity" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-solar-intensity/fixtures/satellite-moon-twobody.json"
```

## 更多示例与测试数据 (fixtures)

来源目录:`ASTROX.AeroSpace.Tests/Lighting/`(上游 MSTest)。当前正式 fixtures 未附 `OccultationBodies`(走默认遮挡);带地形的示例均保留完整 `AzElMaskData`。


| 文件                                                                      | 来源测试                                              | 用途简述                                              |
| ----------------------------------------------------------------------- | ------------------------------------------------- | ------------------------------------------------- |
| `lighting-solar-intensity/fixtures/satellite-moon-twobody.json`         | `SolarIntensity_MoonTwoBody_250519.cs`            | 月球 TwoBody,30 s / 1 s,进出本影                        |
| `lighting-solar-intensity/fixtures/site-moon-pole.json`                 | `SolarIntensity_MoonPoleSite_250530.cs`           | 月球南极站,STK 日期,无地形,10 s / 1 s                       |
| `lighting-solar-intensity/fixtures/site-earth-sthelens-terrain-mask.json` | `SolarIntensity_EarthStHelens_TerrainMask_250529.cs` | 地球 St.Helens,含 AzElMaskData,1 s / 0.1 s           |
| `lighting-solar-intensity/fixtures/site-moon-bruno-terrain-mask.json`   | `SolarIntensity_MoonBruno_TerrainMask_250519.cs`  | 月球 Bruno 坑,含 AzElMaskData,1 s / 0.1 s             |
| `lighting-solar-intensity/fixtures/site-moon-ce7-terrain-mask.json`     | `SolarIntensity_MoonSpCe7_TerrainMask_250529.cs`  | 月球南极 CE7,含 AzElMaskData,1 h / 600 s               |


## 响应示例

以下为对正式 fixtures 实测精简样例(HTTP 200,`IsSuccess: true`)。

### 飞行器 (SolarIntensityScData) — `satellite-moon-twobody.json`

首点 SunLight、末点 Umbra:

```json
{
  "IsSuccess": true,
  "Message": "Success",
  "Datas": [
    {
      "$type": "SolarIntensityScData",
      "CurrentCondition": "SunLight",
      "Obstruction": "None",
      "Time": "2022-09-05T05:17:50.000Z",
      "Intensity": 1,
      "PercentShadow": 0,
      "ApparentSolarRange": 150965936.69045085,
      "SolarDiskHalfAngle": 0.264038469650829,
      "SolarGrazingAngle": 0.4544763358404801,
      "RelativeAngle": 58.96690764506119
    },
    {
      "$type": "SolarIntensityScData",
      "CurrentCondition": "Umbra",
      "Obstruction": "Moon",
      "Time": "2022-09-05T05:18:20.000Z",
      "Intensity": 0,
      "PercentShadow": 1,
      "ApparentSolarRange": 150965975.72998947,
      "SolarDiskHalfAngle": 0.26403840137045687,
      "SolarGrazingAngle": -0.35322160315655227,
      "RelativeAngle": 58.15920970606418
    }
  ]
}
```

### 地面站无地形 (SolarIntensitySiteData) — `site-moon-pole.json`

STK 日期输入,响应时间为 ISO UTC:

```json
{
  "IsSuccess": true,
  "Message": "Success",
  "Datas": [
    {
      "$type": "SolarIntensitySiteData",
      "ApparentSolarAzimuth": -94.00806767739705,
      "ApparentSolarElevation": 0.26640335913966,
      "TerrainElevation": 0,
      "Time": "2024-03-26T00:36:46.000Z",
      "Intensity": 1,
      "PercentShadow": 0,
      "ApparentSolarRange": 149626258.27850586,
      "SolarDiskHalfAngle": 0.2664025545350163,
      "SolarGrazingAngle": 0.2664033591396694,
      "RelativeAngle": 90.26640335913967
    }
  ]
}
```

### 地面站含地形 — `site-earth-sthelens-terrain-mask.json`

```json
{
  "IsSuccess": true,
  "Message": "Success",
  "Datas": [
    {
      "$type": "SolarIntensitySiteData",
      "ApparentSolarAzimuth": 120.08658520860196,
      "ApparentSolarElevation": 33.965875000718434,
      "TerrainElevation": 33.703433141880744,
      "Time": "2022-09-05T17:02:24.000Z",
      "Intensity": 0.9996491519915867,
      "PercentShadow": 0.00035084800841331856,
      "ApparentSolarRange": 150821034.09767714,
      "SolarDiskHalfAngle": 0.26429214865724276,
      "SolarGrazingAngle": 0.26244185883769205,
      "RelativeAngle": 123.96587500071843
    }
  ]
}
```
