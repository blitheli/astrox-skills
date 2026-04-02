---
name: propagator-simple-ascent
description: 通过 Astrox WebAPI 的 POST /Propagator/SimpleAscent 计算火箭主动上升段轨迹，输出 CzmlPositionOut（CZML结构的位置序列）。当用户需要主动上升段、火箭上升、发射轨迹计算时使用。
---

# 火箭主动上升段积分器技能 (Simple Ascent Propagator)

## 核心指令 (Core Instructions)
1. **输入解析**：识别用户提供的发射点坐标（经纬高）、关机点坐标（经纬高）、关机速度、时间窗口等参数。
2. **坐标系匹配**：默认使用 `Earth` 中心天体。
3. **参数验证**：
   - 确保 `Start < Stop`
   - 检查经度范围 [-180, 180]，纬度范围 [-90, 90]
   - 高度和速度必须为正值
4. **API 调用逻辑**：向 `{BASE_URL}/Propagator/SimpleAscent` 发送 `POST`，`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址
`POST /Propagator/SimpleAscent`

### 输入参数结构 (JSON)

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `Start` | string | 是 | 分析开始时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ") |
| `Stop` | string | 是 | 分析结束时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ") |
| `Step` | number | 否 | 积分步长(s)，默认 5 |
| `CentralBody` | string | 否 | 缺省 "Earth" |
| `LaunchLatitude` | number | 是 | 发射点纬度 (deg) |
| `LaunchLongitude` | number | 是 | 发射点经度 (deg) |
| `LaunchAltitude` | number | 是 | 发射点高度 (m) |
| `BurnoutVelocity` | number | 是 | 关机点速度 (m/s) |
| `BurnoutLatitude` | number | 是 | 关机点纬度 (deg) |
| `BurnoutLongitude` | number | 是 | 关机点经度 (deg) |
| `BurnoutAltitude` | number | 是 | 关机点高度 (m) |

### 响应数据结构（CzmlPositionOut）

详见 shared-docs/api-schemas/CzmlPositionOut.md

## 注意事项

- 单位检查：高度必须为米(m)，速度为 m/s，角度为度(deg)。
- 时间格式：首选为 ISO8601 格式
- 经度范围：-180 到 180 度
- 纬度范围：-90 到 90 度
- 支持的中心天体：Earth（默认）、Mars 等

## 标准执行流程

1. 参数预检
   - 检查必填字段完整性
   - 检查 UTC 时间格式
   - 检查 `Start < Stop`
   - 检查经纬度范围
2. 请求构造
   - 按接口契约原样传参，不做单位隐式转换
   - 明确记录 `CentralBody`
3. 结果判定
   - 先判 HTTP 状态，再判 `IsSuccess`
   - `IsSuccess = false` 时优先返回 `Message`
4. 输出归一化
   - 给出关键输入摘要、执行状态、核心输出、限制说明

## 调用示例（最小可运行：地球）

**场景**：从肯尼迪航天中心（约 28.6°N, 80.6°W）发射，关机高度 300km，关机速度 7725.84 m/s，飞行 10 分钟。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/SimpleAscent" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Start": "2022-05-23T04:00:00.000Z",
    "Stop": "2022-05-23T04:10:00.000Z",
    "Step": 5.0,
    "LaunchLatitude": 28.6084,
    "LaunchLongitude": -80.6042,
    "LaunchAltitude": 0.0,
    "BurnoutVelocity": 7725.84,
    "BurnoutLatitude": 25.051,
    "BurnoutLongitude": -51.326,
    "BurnoutAltitude": 300000
  }'
```

## 本地快速验证（可选）

用 fixture 可避免 PowerShell 下行内 JSON 转义问题：

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/SimpleAscent" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@propagator-simple-ascent/fixtures/simple-ascent-earth-min.json"
```

## 更多示例与测试数据（fixtures）

| 文件 | 用途简述 |
|------|----------|
| `propagator-simple-ascent/fixtures/simple-ascent-earth-min.json` | 地球主动上升段，10 分钟窗口 |
| `propagator-simple-ascent/fixtures/simple-ascent-mars.json` | 火星主动上升段 |

说明：部分 fixture 使用服务可接受的非 `Z` 结尾时间字符串；若接口校验只认 ISO8601，请改为 `yyyy-MM-ddTHH:mm:ssZ` 格式后再测。

从响应中取**末时刻**位置速度：按 `shared-docs/api-schemas/CzmlPositionOut.md` 中 `cartesianVelocity` 平铺格式，取最后一组 `[t, X, Y, Z, Vx, Vy, Vz]`。
