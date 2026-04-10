---

## name: access
description: 通过 Astrox WebAPI 的 POST /access/AccessComputeV2 计算两对象间可见性/访问弧段。当用户需要测站对星可见窗口、Access 弧段、AER 采样时使用。

# 访问计算技能 (Access V2)

## 核心指令

1. **单对对象 Access**：识别分析时间范围、`FromObjectPath`（转发端）、`ToObjectPath`（接收端），以及可选的 `OutStep`、`ComputeAER`、`UseLightTimeDelay`。
2. **位置类型**：`Position` 使用 `IEntityPosition` 多态（如 `SitePosition`、`SGP4`、`J2`、`TwoBody`、`CzmlPosition`、`CzmlPositions`、`CentralBody` 等），详见 `skills/shared-docs/api-schemas/IEntityPosition.md`。
3. **API**：向 `{BASE_URL}/access/AccessComputeV2` 发送 `POST`，`Content-Type: application/json`。

## API 说明

### Access 计算 V2

`POST /access/AccessComputeV2`

服务端说明摘要（见 `astrox-web-api.json`）：De430 精密历表，可光延迟；`FromObjectPath` 为转发端，`ToObjectPath` 为接收端。支持多种位置/姿态/传感器组合；输出中若 `From` 非地面物体，方位角等字段意义可能受限（以服务端实现为准）。

#### 请求体 `AccessInput2`


| 参数名                 | 类型      | 必须  | 说明                                                           |
| ------------------- | ------- | --- | ------------------------------------------------------------ |
| `Description`       | string  | 否   | 说明                                                           |
| `Start`             | string  | 是   | 分析开始 (UTCG)，`yyyy-MM-ddTHH:mm:ssZ`                           |
| `Stop`              | string  | 是   | 分析结束 (UTCG)，`yyyy-MM-ddTHH:mm:ssZ`                           |
| `OutStep`           | number  | 否   | 输出时间步长 (s)，缺省 60                                             |
| `FromObjectPath`    | object  | 是   | `EntityPath`：含 `Position`，可选 `Name`、`Orientation`、`Sensor` 等 |
| `ToObjectPath`      | object  | 是   | `EntityPath2`：同上，多态定义见 OpenAPI                               |
| `ComputeAER`        | boolean | 否   | 是否计算 AER，缺省 `false`                                          |
| `UseLightTimeDelay` | boolean | 否   | 是否使用光延迟，缺省 `false`                                           |


#### 响应 `AccessOutput`


| 字段          | 类型      | 说明                       |
| ----------- | ------- | ------------------------ |
| `IsSuccess` | boolean | 是否成功                     |
| `Message`   | string  | 失败原因等                    |
| `Passes`    | array   | 按时间排序的 `AccessData` 弧段列表 |


`AccessData` 含 `AccessStart`、`AccessStop`、`Duration`(s)，以及可选的 `MinElevationData`、`MaxElevationData`、`MinRangeData`、`MaxRangeData`、`AccessBeginData`、`AccessEndData`、`AllDatas`（弧段内按时间排序的采样点）。当 `ComputeAER` 为 `false` 时，部分 AER 相关字段可能为空或未填充（以实际响应为准）。

## 注意事项

- 时间使用 ISO8601 UTC；确保 `Start` < `Stop`，且与星历/TLE 历元覆盖区间一致（例如 SGP4 与 TLE 历元附近）。
- 单位：大地坐标为度 (deg)、高度为米 (m)；时长为秒 (s)；角度在 AER 输出中可能为弧度或度，以服务端字段注释为准。
- `FromObjectPath` / `ToObjectPath` 为 `EntityPath`/`EntityPath2` 结构，**根级一般不需要 `$type`**，但内部的 `Position` 等多态子对象需要 `$type`。
- 判定：HTTP 200 且 `IsSuccess === true`。
- 若仓库中存在 `raw/access` 下的上游测试 JSON，可与本目录 `fixtures` 对照使用；当前技能用例以 `astrox-web-api.json` 与 `fixtures` 为准。

## 标准流程

1. 预检必填字段与时间区间。
2. 为 `From`/`To` 选择合适 `Position` 类型并补全 `$type`。
3. 发送 POST，检查 HTTP 与 `IsSuccess`。
4. 解释 `Passes` 等结果。

## 调用示例

### 地面站对 SGP4 卫星（最小示例）

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/access/AccessComputeV2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/access/fixtures/access-compute-v2-site-sgp4-min.json
```

## 相关文档

- `skills/shared-docs/api-schemas/IEntityPosition.md`
- `skills/shared-docs/api-schemas/EntityPositionSite.md`
- `skills/shared-docs/api-schemas/EntityPositionSGP4.md`
- 完整字段以仓库根目录 `astrox-web-api.json` 为准。

