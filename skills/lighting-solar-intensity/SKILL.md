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
| `Start`             | string   | 是   | 分析开始时刻 (UTCG) (`yyyy-MM-ddTHH:mm:ssZ`)                                                     |
| `Stop`              | string   | 是   | 分析结束时刻 (UTCG) (`yyyy-MM-ddTHH:mm:ssZ`)                                                     |
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
- 时间格式:ISO8601 UTC (`yyyy-MM-ddTHH:mm:ssZ`)
- 坐标单位:经纬度为度 (deg),高度为米 (m);`AzElMaskData` 为弧度 (rad)
- 星历:DE430;仅考虑光延迟,不考虑光行差,与 STK 一致
- 遮挡逻辑:提供 `OccultationBodies` 则用列表;否则按中心天体默认(见上表)。第 1 个遮挡天体为中心天体
- 飞行器仅天体遮挡、无地形;地面站可考虑地形(`AzElMaskData`)
- `Intensity` + `PercentShadow` 通常互补(可见比 + 遮挡比)
- 失败判定:先判 HTTP 状态,再判 `IsSuccess`;失败时优先返回 `Message`

## 标准执行流程

1. 参数预检
  - 检查必填字段:`Start`、`Stop`、`Position`
  - 检查 UTC 时间格式与 `Start < Stop`
2. 位置判定
  - 地面站 vs 飞行器;地面站可按需附带 `AzElMaskData`
  - 需要自定义遮挡时设置 `OccultationBodies`(第 1 个为中心天体)
3. 请求构造
  - 按接口契约原样传参
4. 结果判定
  - 先判 HTTP 状态,再判 `IsSuccess`
5. 输出归一化
  - 按 `$type` 解读 `Datas`;摘要 Intensity / PercentShadow / CurrentCondition(飞行器) 或 ApparentSolarElevation(地面站)

## 调用示例

### 示例 1:月面站视太阳光照强度

**场景**:月球南极坑顶部地面点,计算 1 日视太阳光照强度,步长 1 小时。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Lighting/SolarIntensity" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-solar-intensity/fixtures/site-moon-pole.json"
```

### 示例 2:月球 TwoBody 卫星视太阳光照强度

**场景**:月球中心 TwoBody 轨道卫星,计算 1 小时视太阳光照强度,步长 60 秒。

```bash
curl "${BASE_URL}/Lighting/SolarIntensity" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-solar-intensity/fixtures/satellite-moon-twobody.json"
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


| 文件                                                              | 用途简述                                      |
| --------------------------------------------------------------- | ----------------------------------------- |
| `lighting-solar-intensity/fixtures/site-moon-pole.json`         | 月球南极坑顶地面点,1 日强度序列,步长 3600 s               |
| `lighting-solar-intensity/fixtures/satellite-moon-twobody.json` | 月球 TwoBody 卫星,1 小时强度序列,步长 60 s            |

后续可补充带 `AzElMaskData` / `OccultationBodies` 的示例。

## 响应示例

以下为对仓库 fixtures 实测精简样例(各取 `Datas[0]`)。

### 地面站 (SolarIntensitySiteData)

```json
{
  "IsSuccess": true,
  "Message": "Success",
  "Datas": [
    {
      "$type": "SolarIntensitySiteData",
      "ApparentSolarAzimuth": -139.361687386768,
      "ApparentSolarElevation": 1.1620686980242079,
      "TerrainElevation": 0,
      "Time": "2024-01-01T00:00:00.000Z",
      "Intensity": 1,
      "PercentShadow": 0,
      "ApparentSolarRange": 147328829.79611424,
      "SolarDiskHalfAngle": 0.2705568353481385,
      "SolarGrazingAngle": 2.8032563416695377,
      "RelativeAngle": 91.16206869802421
    }
  ]
}
```

### 飞行器 (SolarIntensityScData)

```json
{
  "IsSuccess": true,
  "Message": "Success",
  "Datas": [
    {
      "$type": "SolarIntensityScData",
      "CurrentCondition": "SunLight",
      "Obstruction": "None",
      "Time": "2023-02-01T00:00:00.000Z",
      "Intensity": 1,
      "PercentShadow": 0,
      "ApparentSolarRange": 147636784.44319654,
      "SolarDiskHalfAngle": 0.26999247835254836,
      "SolarGrazingAngle": 70.53810474391656,
      "RelativeAngle": 134.2746514441337
    }
  ]
}
```
