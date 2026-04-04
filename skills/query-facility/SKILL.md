---
name: query-facility
description: 通过 Astrox WebAPI 的 GET /facility 从数据库中获取所有符合查询条件的地面站。当用户需要查询测控站、地面站、设施信息时使用。
---

# 设施信息查询技能 (Query Facility)

## 核心指令 (Core Instructions)
1. **输入解析**：识别用户提供的设施名称、所属网络等查询条件。
2. **参数处理**：支持按设施名称、所属网络进行查询。
3. **API 调用逻辑**：向 `{BASE_URL}/facility` 发送 `GET` 请求，`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址
`GET /facility`

### 查询参数

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `facilityName` | string | 否 | 地面站名称(英文或者汉语拼音) |
| `networkName` | string | 否 | 所属网络(NASA DSN/NRO...) |

### 响应数据结构 (FacilityDataBaseOutput)

```json
{
  "IsSuccess": true,
  "Message": "",
  "Facilities": [
    {
      "Facility": {
        "Name": "北京测控站",
        "NameEn": "Beijing Ground Station",
        "Latitude": 39.9044,
        "Longitude": 116.4074,
        "Altitude": 43.5,
        "MinElevation": 5.0,
        "Range": 3000.0
      },
      "Network": {
        "Name": "China Satellite Network",
        "Code": "CSN"
      },
      "Location": {
        "City": "北京",
        "Province": "北京市",
        "Country": "中国"
      }
    }
  ]
}
```

## 注意事项

- 坐标系统：使用大地坐标系，纬度范围 [-90, 90]，经度范围 [-180, 180]
- 高度单位：米 (m)
- 仰角单位：度 (deg)
- 作用距离单位：千米 (km)
- 地面站名称支持：英文或汉语拼音
- 常见网络：NASA DSN、NRO 等

## 标准执行流程

1. 参数预检
   - 检查至少提供一个查询条件（facilityName 或 networkName）
2. 请求构造
   - 按接口契约构造查询参数
3. 结果判定
   - 先判 HTTP 状态，然后判 `IsSuccess`
   - `IsSuccess = false` 时优先返回 `Message`
4. 输出归一化
   - 给出查询条件摘要、执行状态、结果统计、详细数据

## 调用示例

**场景 1**：按设施名称查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/facility?facilityName=Beijing" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 2**：按所属网络查询（NASA DSN）

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/facility?networkName=NASA DSN" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 3**：组合查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/facility?facilityName=GOLDSTONE&networkName=NASA DSN" \
  --request GET \
  --header 'Content-Type: application/json'
```
