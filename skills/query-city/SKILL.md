---
name: query-city
description: 通过 Astrox WebAPI 的 GET /city 从数据库中获取所有符合查询条件的城市。当用户需要查询城市信息、城市坐标、城市名称时使用。
---

# 城市信息查询技能 (Query City)

## 核心指令 (Core Instructions)
1. **输入解析**：识别用户提供的城市名称、省份名称、国家名称或城市类型等查询条件。
2. **参数处理**：支持按城市名称、省份、国家或城市类型进行查询。
3. **API 调用逻辑**：向 `{BASE_URL}/city` 发送 `GET` 请求，`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址
`GET /city`

### 查询参数

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `cityName` | string | 否 | 城市名称(英文或者汉语拼音) |
| `provinceName` | string | 否 | 省份名称 |
| `countryName` | string | 否 | 国家名称 |
| `typeOfCity` | string | 否 | 城市类型(PopulatedPlace/AdministrationCenter/NationalCapital/TerritorialCapital) |

### 响应数据结构 (CityDataBaseOutput)

```json
{
  "IsSuccess": true,
  "Message": "",
  "Cities": [
    {
      "City": {
        "Name": "北京",
        "NameEn": "Beijing",
        "Latitude": 39.9042,
        "Longitude": 116.4077,
        "Altitude": 43.5
      },
      "Province": {
        "Name": "北京市",
        "NameEn": "Beijing"
      },
      "Country": {
        "Name": "中国",
        "NameEn": "China",
        "Code": "CN"
      },
      "TypeOfCity": "NationalCapital"
    }
  ]
}
```

## 注意事项

- 坐标系统：使用大地坐标系，纬度范围 [-90, 90]，经度范围 [-180, 180]
- 高度单位：米 (m)
- 城市名称支持：英文或汉语拼音
- 城市类型：PopulatedPlace(人口聚集区)、AdministrationCenter(行政中心)、NationalCapital(国家首都)、TerritorialCapital(地区首府)

## 标准执行流程

1. 参数预检
   - 检查至少提供一个查询条件（cityName、provinceName、countryName 或 typeOfCity）
2. 请求构造
   - 按接口契约构造查询参数
3. 结果判定
   - 先判 HTTP 状态，再判 `IsSuccess`
   - `IsSuccess = false` 时优先返回 `Message`
4. 输出归一化
   - 给出查询条件摘要、执行状态、结果统计、详细数据

## 调用示例

**场景 1**：按城市名称查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/city?cityName=Beijing" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 2**：按省份名称查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/city?provinceName=北京市" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 3**：按国家名称查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/city?countryName=China" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 4**：按城市类型查询（首都）

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/city?typeOfCity=NationalCapital" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 5**：组合查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/city?countryName=China&typeOfCity=NationalCapital" \
  --request GET \
  --header 'Content-Type: application/json'
```
