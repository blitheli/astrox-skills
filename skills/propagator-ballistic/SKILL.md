---
name: propagator-ballistic
description: 通过 Astrox WebAPI 的 POST /Propagator/Ballistic 计算导弹弹道轨迹，输出 CzmlPositionOut（CZML结构的位置序列）。当用户需要弹道计算、导弹轨迹、 ballistic missile 计算时使用。
---

# 导弹弹道积分器技能 (Ballistic Propagator)

## 核心指令 (Core Instructions)
1. **输入解析**：识别用户提供的发射点坐标（经纬高）、落点坐标（经纬高）、弹道类型及对应参数。
2. **弹道类型识别**：
   - `ApogeeAlt`：固定远地点高度
   - `DeltaV`：固定速度增量
   - `DeltaV_MinEcc`：固定速度增量（最小偏心率）
   - `TimeOfFlight`：固定飞行时间
3. **参数验证**：
   - 确保 `LaunchLatitude/Longitude` 和 `ImpactLatitude/Longitude` 有效
   - 根据弹道类型验证 `BallisticTypeValue` 范围
4. **API 调用逻辑**：向 `{BASE_URL}/Propagator/Ballistic` 发送 `POST`，`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址
`POST /Propagator/Ballistic`

### 输入参数结构 (JSON)

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `Start` | string | 是 | 发射时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ") |
| `Step` | number | 否 | 积分步长(s)，默认 5 |
| `LaunchLatitude` | number | 是 | 发射点纬度 (deg) |
| `LaunchLongitude` | number | 是 | 发射点经度 (deg) |
| `LaunchAltitude` | number | 是 | 发射点高度 (m)，默认 0 |
| `BallisticType` | string | 是 | 弹道类型：`ApogeeAlt`/`DeltaV`/`DeltaV_MinEcc`/`TimeOfFlight` |
| `BallisticTypeValue` | number | 是 | 弹道类型对应的数值（高度 m / 速度 m/s / 飞行时间 s） |
| `ImpactLatitude` | number | 是 | 落点纬度 (deg) |
| `ImpactLongitude` | number | 是 | 落点经度 (deg) |
| `ImpactAltitude` | number | 是 | 落点高度 (m)，默认 0 |

### 响应数据结构（CzmlPositionOut）

详见 shared-docs/api-schemas/CzmlPositionOut.md

## 注意事项

- 单位检查：高度必须为米(m)，速度为 m/s，时间为秒(s)，角度为度(deg)。
- 时间格式：首选为 ISO8601 格式
- 经度范围：-180 到 180 度
- 纬度范围：-90 到 90 度
- 弹道类型必须为有效的四种类型之一

## 弹道类型说明

| 弹道类型 | BallisticTypeValue 含义 | 适用场景 |
| :--- | :--- | :--- |
| `ApogeeAlt` | 远地点高度 (m) | 指定弹道最高点 |
| `DeltaV` | 速度增量 (m/s) | 指定发动机提供的速度增量 |
| `DeltaV_MinEcc` | 速度增量 (m/s) | 指定速度增量（能量最优） |
| `TimeOfFlight` | 飞行时间 (s) | 指定飞行到达时间 |

## 标准执行流程

1. 参数预检
   - 检查必填字段完整性
   - 检查 UTC 时间格式
   - 检查经纬度范围
   - 验证弹道类型有效性
2. 请求构造
   - 按接口契约原样传参，不做单位处隐式转换
3. 结果判定
   - 先判 HTTP 状态，再判 `IsSuccess`
   - `IsSuccess = false` 时优先返回 `Message`
4. 输出归一化
   - 给出关键输入摘要、执行状态、核心输出、限制说明

## 调用示例（最小可运行：ApogeeAlt）

**场景**：从赤道 (0°, 0°) 发射，落点在 (20°N, 20°E)，远地点高度 500km。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/Ballistic" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Start": "2022-06-27T04:00:00.000Z",
    "Step": 5,
    "LaunchLatitude": 0,
    "LaunchLongitude": 0,
    "LaunchAltitude": 0.0,
    "BallisticType": "ApogeeAlt",
    "BallisticTypeValue": 500000,
    "ImpactLatitude": 20,
    "ImpactLongitude": 20,
    "ImpactAltitude": 0
  }'
```

## 其他弹道类型示例

### TimeOfFlight（固定飞行时间）

```bash
curl "${BASE_URL}/Propagator/Ballistic" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Start": "2022-06-27T04:00:00.000Z",
    "Step": 5,
    "LaunchLatitude": 0,
    "LaunchLongitude": 0,
    "LaunchAltitude": 0.0,
    "BallisticType": "TimeOfFlight",
    "BallisticTypeValue": 3000,
    "ImpactLatitude": 20,
    "ImpactLongitude": 20,
    "ImpactAltitude": 0
  }'
```

## 本地快速验证（可选）

用 fixture 可避免 PowerShell 下行内 JSON 转义问题：

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/Ballistic" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@propagator-ballistic/fixtures/ballistic-apogeealt-min.json"
```

## 更多示例与测试数据（fixtures）

| 文件 | 用途简述 |
|------|----------|
| `propagator-ballistic/fixtures/ballistic-apogeealt-min.json` | 固定远地点高度（500km） |
| `propagator-ballistic/fixtures/ballistic-deltav-min.json` | 固定速度增量 |
| `propagator-ballistic/fixtures/ballistic-deltav-minecc.json` | 固定速度增量（最小偏心率） |
| `propagator-ballistic/fixtures/ballistic-timeofflight.json` | 固定飞行时间（3000s） |

从响应中取**末时刻**位置速度：按 `shared-docs/api-schemas/CzmlPositionOut.md` 中 `cartesianVelocity` 平铺格式，取最后一组 `[t, X, Y, Z, Vx, Vy, Vz]`。
