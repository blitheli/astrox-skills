---
name: query-ssc
description: 通过 Astrox WebAPI 的 GET /ssc 从数据库中获取所有符合查询条件的卫星。当用户需要查询卫星TLE、卫星轨道、卫星星历时使用。
---

# 卫星TLE查询技能 (Query TLE)

## 核心指令 (Core Instructions)
1. **输入解析**：识别用户提供的卫星名称、卫星编号、任务类型、所属国家、轨道参数等查询条件。
2. **参数处理**：支持按卫星名称、卫星编号、任务类型、所属国家、轨道高度、轨道倾角等进行查询。
3. **API 调用逻辑**：向 `{BASE_URL}/ssc` 发送 `GET` 请求，`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址
`GET /ssc`

### 查询参数

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `sscName` | string | 否 | 卫星名称(例如: FENGYUN) |
| `sscNumber` | string | 否 | SSC编号(例如: 21544) |
| `mission` | string | 否 | 任务类型(Astronomy/Comm/Navigation) |
| `owner` | string | 否 | 所属国家(PRC/UK/US...) |
| `active` | string | 否 | 是否有效(true or false) |
| `minimumPerigee` | number | 否 | 最小近地点高度(m)，查询结果中近地点高度大于此数值 |
| `maximumPerigee` | number | 否 | 最大近地点高度(m)，查询结果中近地点高度小于此数值 |
| `minmumApogee` | number | 否 | 最小远地点高度(m)，查询结果中远地点高度大于此数值 |
| `maximumApogee` | number | 否 | 最大远地点高度(m)，查询结果中远地点高度小于此数值 |
| `minimumInclination` | number | 否 | 最小轨道倾角(deg) |
| `maximumInclination` | number | 否 | 最大轨道倾角(deg) |

### 响应数据结构 (SatelliteDatabaseOutput)

```json
{
  "IsSuccess": true,
  "Message": "",
  "TotalCount": 1,
  "TLEs": [
    {
      "Satellite": {
        "SSC": "25544",
        "Name": "ISS (ZARYA)",
        "Mission": "General",
        "Owner": "INT"
      },
      "TLE": {
        "SatelliteNumber": "25544",
        "InternationalDesignator": "1998-067A",
        "Epoch": "2024-04-01T12:00:00Z",
        "MeanMotion": 15.49835367,
        "Eccentricity": 0.0006703,
        "Inclination": 51.6416,
        "RAAN": 248.4787,
        "ArgumentOfPerigee": 130.5360,
        "MeanAnomaly": 325.0288,
        "Bstar": 0.0001,
        "MeanMotionDot": 0.00001,
        "Line1": "1 25544U 98067A   24092.50000000  .00010000  00000-0  18000-3 0  9999",
        "Line2": "2 25544  51.6416 248.4787 0006703 130.5360 325.0288 15.49835367444444"
      },
      "Orbit": {
        "Perigee": 408.5,
        "Apogee": 412.3,
        "SemiMajorAxis": 6778.1,
        "Period": 92.68
      },
      "Active": true
    }
  ]
}
```

## 注意事项

- SSC编号：即 NORAD ID，为 5 位数字字符串
- 任务类型：Astronomy(天文学)、Comm(通信)、Navigation(导航)、General(通用)等
- 所属国家：PRC(中国)、UK(英国)、US(美国)、INT(国际)等
- 轨道高度单位：米 (m)
- 轨道倾角单位：度 (deg)
- 半长轴单位：米 (m)
- 轨道周期单位：分钟 (min)
- 平均运动：圈/天 (rev/day)
- 偏心率：无量纲
- 一般情况下只需要提供卫星名称即可查询

## 标准执行流程

1. 参数预检
   - 检查至少提供一个查询条件（sscName、sscNumber、mission、owner 等）
   - 检查轨道参数范围的有效性
2. 请求构造
   - 按接口契约构造查询参数
3. 结果判定
   - 先判 HTTP 状态，再判 `IsSuccess`
   - `IsSuccess = false` 时优先返回 `Message`
4. 输出归一化
   - 给出查询条件摘要、执行状态、结果统计、详细数据

## 调用示例

**场景 1**：按卫星名称查询（推荐）

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?sscName=FENGYUN" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 2**：按 SSC编号查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?sscNumber=25544" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 3**：按任务类型查询（通信卫星）

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?mission=Comm" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 4**：按所属国家查询（中国）

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?owner=PRC" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 5**：查询有效的卫星

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?active=true" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 6**：按近地点高度范围查询（低轨道卫星）

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?minimumPerigee=200000&maximumPerigee=800000" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 7**：按轨道倾角范围查询（近地轨道）

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?minimumInclination=0&maximumInclination=90" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 8**：组合查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?owner=PRC&active=true&minimumInclination=90&maximumInclination=110" \
  --request GET \
  --header 'Content-Type: application/json'
```
