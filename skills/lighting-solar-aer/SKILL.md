---
name: lighting-solar-aer
description: 计算地面站或飞行器相对视太阳的 AER(方位角、高度角、距离),基于 DE430 星历。当用户需要太阳方位角/高度角/距离、Solar AER、视太阳角度序列时使用。
---

# 视太阳 AER 计算技能 (Solar AER)

## 核心指令 (Core Instructions)

1. **输入解析**:识别用户提供的分析时间范围、位置对象(地面站或飞行器)、计算步长。
2. **位置对象匹配**:根据用户需求选择合适的位置类型:
  - 地面站:`SitePosition`
  - 飞行器:`TwoBody`、`J2`、`SGP4`、`CzmlPositions` 等
3. **API 调用逻辑**:向 `{BASE_URL}/Lighting/SolarAER` 发送 `POST`,`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址

`POST /Lighting/SolarAER`

### 输入参数结构 (JSON)


| 参数名           | 类型     | 必须  | 说明                                                                            |
| ------------- | ------ | --- | ----------------------------------------------------------------------------- |
| `Description` | string | 否   | 说明                                                                            |
| `Start`       | string | 是   | 分析开始时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ")                                        |
| `Stop`        | string | 是   | 分析结束时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ")                                        |
| `Position`    | object | 是   | 位置对象 (IEntityPosition),详见 `skills/shared-docs/api-schemas/IEntityPosition.md` |
| `TimeStepSec` | int    | 否   | 计算步长,单位:秒 (s);缺省 3600                                                        |


### 位置类型 (Position)

`Position` 采用 `IEntityPosition` 多态结构,不在本技能文档内展开字段定义。  
统一引用:`skills/shared-docs/api-schemas/IEntityPosition.md`。

### 响应数据结构 (SolarAEROut)


| 字段名         | 类型              | 说明                           |
| ----------- | --------------- | ---------------------------- |
| `IsSuccess` | boolean         | 结果标识(`true`: 成功;`false`: 失败) |
| `Message`   | string          | 错误信息(失败时存储失败原因)              |
| `Datas`     | `SolarAERData`[] | 视太阳 AER 时间序列                 |


### SolarAERData 结构


| 字段名         | 类型     | 说明                                        |
| ----------- | ------ | ----------------------------------------- |
| `Time`      | string | 时刻 (UTCG),格式:`yyyy-MM-ddTHH:mm:ss.fffZ`   |
| `Azimuth`   | number | 方位角,单位:度 (deg)                             |
| `Elevation` | number | 高度角,单位:度 (deg)                             |
| `Range`     | number | 距离,单位:千米 (km)                              |


### 角度定义

- **地面站**:
  - 方位角:当地水平面,正北为 0°,向东为正
  - 高度角:与当地水平面夹角,天顶为正
- **飞行器**:
  - 方位角:VVLH 坐标系(前-右-下),前为 0°,右为正
  - 高度角:与 xy 平面夹角,天顶为正

## 注意事项

- 若没有设置 `Start`、`Stop`,则默认 `Start` 为当前日期零点,`Stop = Start + 1 天`
- 时间格式:使用 ISO8601 格式 (`yyyy-MM-ddTHH:mm:ssZ`)
- 坐标单位:经纬度为度 (deg),高度为米 (m)
- 星历:使用 DE430 计算视太阳位置
- 光延迟:考虑光延迟,不考虑光行差

## 标准执行流程

1. 参数预检
  - 检查必填字段:`Start`、`Stop`、`Position`
  - 检查 UTC 时间格式
  - 检查 `Start < Stop`
2. 位置判定
  - 根据用户描述判断是地面站还是飞行器
  - 若为地面站,检查 `cartographicDegrees` 格式
  - 参考具体的位置类型要求检查参数完整性
3. 请求构造
  - 按接口契约原样传参
4. 结果判定
  - 先判 HTTP 状态,再判 `IsSuccess`
  - `IsSuccess = false` 时优先返回 `Message`
5. 输出归一化
  - 给出关键输入摘要、执行状态、AER 时间序列(或关键采样点)

## 调用示例

### 示例 1:月面站视太阳 AER

**场景**:月球南极坑顶部地面点,计算 1 年视太阳 AER,步长 1 小时。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Lighting/SolarAER" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-solar-aer/fixtures/site-moon-pole.json"
```

### 示例 2:月球 TwoBody 卫星视太阳 AER

**场景**:月球中心 TwoBody 轨道卫星,计算 1 小时视太阳 AER,步长 60 秒。

```bash
curl "${BASE_URL}/Lighting/SolarAER" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-solar-aer/fixtures/satellite-moon-twobody.json"
```

## 本地快速验证(可选)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Lighting/SolarAER" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@lighting-solar-aer/fixtures/satellite-moon-twobody.json"
```

## 更多示例与测试数据 (fixtures)


| 文件                                                     | 用途简述                          |
| ------------------------------------------------------ | ----------------------------- |
| `lighting-solar-aer/fixtures/site-moon-pole.json`      | 月球南极坑顶地面点,1 年 AER,步长 3600 s |
| `lighting-solar-aer/fixtures/satellite-moon-twobody.json` | 月球 TwoBody 卫星,1 小时 AER,步长 60 s |


## 响应示例

```json
{
  "IsSuccess": true,
  "Message": "Success",
  "Datas": [
    {
      "Time": "2023-02-01T00:00:00.000Z",
      "Azimuth": 224.72325919154076,
      "Elevation": 44.26876843225584,
      "Range": 147636319.0662109
    }
  ]
}
```
