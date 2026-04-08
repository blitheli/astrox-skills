---
name: access
description: 计算两个对象之间的可见弧段（Access），支持站点与卫星等多态位置对象。输出按时间排序的可见窗口及可选 AER 数据。
---

# Access 计算技能 (Access Compute V2)

## 核心指令 (Core Instructions)

1. **输入解析**：识别分析时间窗、发送端 `FromObjectPath` 与接收端 `ToObjectPath`。
2. **对象建模**：`FromObjectPath` / `ToObjectPath` 使用 `EntityPath` 结构，`Position` 采用 `IEntityPosition` 多态（如 `SitePosition`、`TwoBody`、`J2`、`SGP4`）。
3. **API 调用逻辑**：向 `{BASE_URL}/access/AccessComputeV2` 发送 `POST`，`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址

`POST /access/AccessComputeV2`

### 输入参数结构 (JSON)

| 参数名 | 类型 | 必须 | 说明 |
| --- | --- | --- | --- |
| `Description` | string | 否 | 说明 |
| `Start` | string | 是 | 分析开始时刻(UTCG)，格式 `yyyy-MM-ddTHH:mm:ssZ` |
| `Stop` | string | 是 | 分析结束时刻(UTCG)，格式 `yyyy-MM-ddTHH:mm:ssZ` |
| `OutStep` | number | 否 | 输出时间步长(s)，缺省 60 |
| `FromObjectPath` | object | 是 | 发送端对象路径（`EntityPath`） |
| `ToObjectPath` | object | 是 | 接收端对象路径（`EntityPath2`） |
| `ComputeAER` | boolean | 否 | 是否计算 AER 参数，缺省 `false` |
| `UseLightTimeDelay` | boolean | 否 | 是否使用光延迟，缺省 `false` |

### 对象位置类型

`FromObjectPath.Position` 和 `ToObjectPath.Position` 支持 `IEntityPosition` 多态：

- `SitePosition`
- `TwoBody`
- `J2`
- `SGP4`
- `CzmlPosition` / `CzmlPositions`
- `CentralBody`
- `Ballistic`

通用结构参考：`skills/shared-docs/api-schemas/IEntityPosition.md`

### 响应数据结构（AccessOutput）

| 字段名 | 类型 | 说明 |
| --- | --- | --- |
| `IsSuccess` | boolean | 结果（`true`: 成功；`false`: 失败） |
| `Message` | string | 结果信息（失败原因） |
| `Passes` | array/null | 按时间顺序的 Access 弧段数据 |

## 注意事项

- 时间建议使用 ISO8601 UTC（`yyyy-MM-ddTHH:mm:ssZ`）。
- 必填字段：`Start`、`Stop`、`FromObjectPath`、`ToObjectPath`。
- 默认 `ComputeAER=false`，若需要方位/俯仰/距离参数需显式打开。
- 判定成功建议同时检查 HTTP 状态码与 `IsSuccess`。

## 标准执行流程

1. 参数预检
  - 检查必填字段完整性
  - 检查时间格式与 `Start < Stop`
2. 对象建模
  - 根据场景为两端对象设置 `Name` 与 `Position.$type`
  - 检查位置对象关键字段（如站点经纬高、轨道根数）
3. 请求构造
  - 按接口契约原样传参，不做隐式单位转换
4. 结果判定
  - 先判 HTTP 状态，再判 `IsSuccess`
  - 失败时优先返回 `Message`
5. 输出归一化
  - 输出窗口数量、首末窗口时刻、总可见时长（如可得）

## 调用示例（最小可运行）

**场景**：地球地面站到地球 TwoBody 卫星的可见性计算，分析 4 小时。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/access/AccessComputeV2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Description": "site-to-sat access min case",
    "Start": "2024-05-01T12:00:00Z",
    "Stop": "2024-05-01T16:00:00Z",
    "OutStep": 60,
    "ComputeAER": true,
    "UseLightTimeDelay": false,
    "FromObjectPath": {
      "Name": "Facility/Beijing",
      "Position": {
        "$type": "SitePosition",
        "CentralBody": "Earth",
        "cartographicDegrees": [116.4, 39.9, 50],
        "clampToGround": false
      }
    },
    "ToObjectPath": {
      "Name": "Satellite/Sat1",
      "Position": {
        "$type": "TwoBody",
        "CentralBody": "Earth",
        "CoordSystem": "Inertial",
        "CoordType": "Classical",
        "OrbitalElements": [6878137, 0.001, 53, 0, 0, 0],
        "OrbitEpoch": "2024-05-01T12:00:00Z"
      }
    }
  }'
```

## 本地快速验证（可选）

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/access/AccessComputeV2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/access/fixtures/access-min.json"
```

## 更多示例与测试数据（fixtures）

| 文件 | 用途简述 |
| --- | --- |
| `skills/access/fixtures/access-min.json` | 地面站到 TwoBody 卫星的最小 Access 示例 |
