---
name: query-tle
description: 从数据库中获取所有符合查询条件的卫星。当用户需要查询卫星TLE、卫星轨道、卫星星历时使用。
---

# 卫星TLE查询技能 (Query TLE)

目前仅支持通过 Astrox WebAPI 的 GET /ssc

## 核心指令 (Core Instructions)
1. **输入解析**:识别用户提供的卫星名称、卫星编号、任务类型、所属国家、轨道参数等查询条件。
2. **参数处理**:支持按卫星名称、卫星编号、任务类型、所属国家、轨道高度、轨道倾角等进行查询。
3. **API 调用逻辑**:向 `{BASE_URL}/ssc` 发送 `GET` 请求,`Content-Type: application/json`。

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
| `minimumPerigee` | number | 否 | 最小近地点高度(m),查询结果中近地点高度大于此数值 |
| `maximumPerigee` | number | 否 | 最大近地点高度(m),查询结果中近地点高度小于此数值 |
| `minmumApogee` | number | 否 | 最小远地点高度(m),查询结果中远地点高度大于此数值 |
| `maximumApogee` | number | 否 | 最大远地点高度(m),查询结果中远地点高度小于此数值 |
| `minimumInclination` | number | 否 | 最小轨道倾角(deg) |
| `maximumInclination` | number | 否 | 最大轨道倾角(deg) |

### 响应数据结构 (SatelliteDatabaseOutput)

#### 顶层字段

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `IsSuccess` | boolean | 结果(True:成功;False:失败) |
| `Message` | string | 结果信息(主要是存储失败的原因) |
| `TotalCount` | integer | 查询到的卫星总数 |
| `TLEs` | array `SatelliteDatabaseEntry[]` \| null | 所有卫星集合(数据库查询结果) |

#### TLEs 子项字段 (SatelliteDatabaseEntry)

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `Active` | boolean | 是否有效 |
| `CommonName` | string \| null | 常用名称 |
| `OfficialName` | string \| null | 官方名称 |
| `SatelliteNumber` | string \| null | SSC编号 |
| `TleEpoch` | string \| null | TLE历元时刻(yyyy-MM-ddTHH:mm:ssZ) |
| `RevolutionNumber` | integer | 历元时刻对应的运行圈数 |
| `TLE_Line1` | string \| null | 两行根数TLE-Line1 |
| `TLE_Line2` | string \| null | 两行根数TLE-Line2 |
| `InternationalDesignator` | string \| null | 国际标识 |
| `Owner` | string \| null | 卫星所属国 |
| `Mission` | string \| null | 卫星任务类型 |
| `LaunchSite` | string \| null | 发射点 |
| `LaunchDateString` | string \| null | 发射日期(YYYYMMDDHHMM in UTC) |
| `OrbitDescription` | string \| null | 轨道描述(若已坠入大气,则包含"Decayed"以及日期) |
| `Mass` | number | 卫星质量(kg) |
| `Apogee` | number | 远地点高度(m) |
| `Perigee` | number | 近地点高度(m) |
| `Period` | number | 轨道周期(s) |
| `Inclination` | number | 轨道倾角(rad) |
| `LastDatabaseUpdate` | string | 数据库更新时间 |
| `WriteUp` | string \| null | 卫星描述信息 |

#### 响应示例

```json
{
  "IsSuccess": true,
  "Message": "Success",
  "TotalCount": 2,
  "TLEs": [
    {
      "Active": true,
      "CommonName": "ISS",
      "OfficialName": "ISS",
      "SatelliteNumber": "25544",
      "TleEpoch": "2026-04-03T21:00:58.615Z",
      "RevolutionNumber": 56023,
      "TLE_Line1": "1 25544U 98067A   26093.87567842  .00012431  00000-0  23534-3 0  9998",
      "TLE_Line2": "2 25544  51.6327 307.8415 0006253 268.9169  91.1103 15.48754008560234",
      "InternationalDesignator": "1998-067A",
      "Owner": "ISS",
      "Mission": "Engineer",
      "LaunchSite": "TYM",
      "LaunchDateString": "199811200640",
      "OrbitDescription": "",
      "Mass": 31100,
      "Apogee": 425000,
      "Perigee": 416000,
      "Period": 5580,
      "Inclination": 0.9005898940290741,
      "LastDatabaseUpdate": "2026-04-04T00:00:00",
      "WriteUp": "https://celestrak.org/satcat/1998/1998-067.php"
    }
  ]
}
```

## 注意事项

- SSC编号:即 NORAD ID,为 5 位数字字符串
- 任务类型:Astronomy(天文学)、Comm(通信)、Navigation(导航)、General(通用)等
- 所属国家:PRC(中国)、UK(英国)、US(美国)、INT(国际)等
- 轨道高度单位:米 (m)
- 轨道倾角单位:弧度 (rad)
- 半长轴单位:米 (m)
- 轨道周期单位:秒 (s)
- 平均运动:圈/天 (rev/day)
- 偏心率:无量纲
- 一般情况下只需要提供卫星名称即可查询

## 标准执行流程

1. 参数预检
   - 检查至少提供一个查询条件(sscName、sscNumber、mission、owner 等)
   - 检查轨道参数范围的有效性
2. 请求构造
   - 按接口契约构造查询参数
3. 结果判定
   - 先判 HTTP 状态,再判 `IsSuccess`
   - `IsSuccess = false` 时优先返回 `Message`
4. 输出归一化
   - 给出查询条件摘要、执行状态、结果统计、详细数据

## 调用示例

**场景 1**:按卫星名称查询(推荐)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?sscName=FENGYUN" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 2**:按 SSC编号查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?sscNumber=25544" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 3**:按任务类型查询(通信卫星)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?mission=Comm" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 4**:按所属国家查询(中国)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?owner=PRC" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 5**:查询有效的卫星

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?active=true" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 6**:按近地点高度范围查询(低轨道卫星)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?minimumPerigee=200000&maximumPerigee=800000" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 7**:按轨道倾角范围查询(近地轨道)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?minimumInclination=0&maximumInclination=90" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 8**:组合查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/ssc?owner=PRC&active=true&minimumInclination=90&maximumInclination=110" \
  --request GET \
  --header 'Content-Type: application/json'
```
