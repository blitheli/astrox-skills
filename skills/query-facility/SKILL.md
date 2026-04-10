---
name: query-facility
description: 从数据库中获取所有符合查询条件的地面站。当用户需要查询测控站、地面站、设施信息时使用。
---

# 地面站信息查询技能 (Query Facility)

目前仅支持通过 Astrox WebAPI 的 GET /facility

## 核心指令 (Core Instructions)
1. **输入解析**:识别用户提供的地面站设施名称、所属网络等查询条件。
2. **参数处理**:支持按设施名称、所属网络进行查询。需翻译为拼音或英文,以便与 API 接口参数匹配。
3. **API 调用逻辑**:向 `{BASE_URL}/facility` 发送 `GET` 请求,`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址
`GET /facility`

### 查询参数

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `facilityName` | string | 否 | 地面站名称(英文或者汉语拼音) |
| `networkName` | string | 否 | 所属网络(NASA DSN/NRO...) |

### 响应数据结构 (FacilityDataBaseOutput)

#### 顶层字段

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `IsSuccess` | boolean | 结果(True:成功;False:失败) |
| `Message` | string | 结果信息(主要是存储失败的原因) |
| `Facilities` | array `FacilityDatabaseEntry[]` \| null | 所有符合的地面站列表 |

#### Facilities 子项字段 (FacilityDatabaseEntry)

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `FacilityName` | string \| null | 地面站名称 |
| `NetworkName` | string \| null | 地面站所属网络名称 |
| `Latitude` | number | 地面站纬度(单位:rad) |
| `Longitude` | number | 地面站经度(单位:rad) |
| `Altitude` | number | 地面站高度(单位:m) |
| `CentralBodyName` | string \| null | 中心天体名称(缺省为Earth) |

#### 响应示例

```json
{
  "IsSuccess": true,
  "Message": "Success",
  "Facilities": [
    {
      "FacilityName": "Beijing",
      "NetworkName": "China Satellite Network",
      "Latitude": 0.69646,
      "Longitude": 2.03169,
      "Altitude": 43.5,
      "CentralBodyName": "Earth"
    }
  ]
}
```

## 注意事项

- 坐标系统:使用大地坐标系;接口字段 `Latitude`、`Longitude` 单位为弧度(rad)
- 高度单位:米 (m)
- 地面站名称支持:英文或汉语拼音
- 常见网络:NASA DSN、NRO 等

## 标准执行流程

1. 参数预检
   - 检查至少提供一个查询条件(facilityName 或 networkName)
2. 请求构造
   - 按接口契约构造查询参数
3. 结果判定
   - 先判 HTTP 状态,然后判 `IsSuccess`
   - `IsSuccess = false` 时优先返回 `Message`
4. 输出归一化
   - 给出查询条件摘要、执行状态、结果统计、详细数据

## 调用示例

**场景 1**:按设施名称查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/facility?facilityName=Beijing" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 2**:按所属网络查询(NASA DSN)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/facility?networkName=NASA DSN" \
  --request GET \
  --header 'Content-Type: application/json'
```

**场景 3**:按设施名称+所属网络组合查询

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/facility?facilityName=GOLDSTONE&networkName=NASA DSN" \
  --request GET \
  --header 'Content-Type: application/json'
```

