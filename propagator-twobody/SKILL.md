---
name: propagator-twobody
description: 通过 Astrox WebAPI 的 POST /Propagator/TwoBody 计算中心天体引力场下的二体轨道轨迹，输出 CzmlPositionOut（CZML结构的位置序列）。当用户需要二体轨道递推、TwoBody、轨道外推、经典根数或笛卡尔初值递推时使用。
---

# 二体轨道积分器技能 (TwoBody Propagator)

## 核心指令 (Core Instructions)
1. **输入解析**：识别用户提供的轨道历元、开始/结束时间以及轨道根数（经典根数或笛卡尔坐标）。
2. **坐标系匹配**：默认使用 `Inertial` 系和 `Earth` 中心天体，除非用户明确指定其他行星。
3. **参数转换(OrbitalElements)**：
   - 若 `CoordType` 为 `Classical`，确保输入 6 个元素：[半长径(m)，偏心率，轨道倾角(deg)，近点角距(deg)，升交点经度(deg)，真近点角(deg)]。
   - 若 `CoordType` 为 `Cartesian`，确保输入 6 个元素：[X(m),Y(m),Z(m),Vx(m/s),Vy(m/s),Vz(m/s)]。
4. **API 调用逻辑**：向 `{BASE_URL}/Propagator/TwoBody` 发送 `POST`，`Content-Type: application/json`。若无特别指定，可用 `curl`；亦可用与用户环境一致的 HTTP 客户端（如 `Invoke-RestMethod`、Python `requests`）。

## API 规范 (Tool Definition)

### 接口地址
`POST /Propagator/TwoBody`

### 输入参数结构 (JSON)

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `CentralBody` | string | 否 | 缺省 "Earth" |
| `CoordSystem`| string | 否 | 缺省 "Inertial" |
| `GravitationalParameter`|number|否|缺省:398600441500000|
| `CoordType` | string | 否 | "Classical" 或 "Cartesian" (缺省 Classical) |
| `OrbitalElements` | array | 是 | 轨道根数,具体根数由CoordType决定 |
| `OrbitEpoch` | string | 是 | 轨道历元(UTCG) ("yyyy-MM-ddTHH:mm:ssZ") |
| `Start` | string | 是 | 分析开始时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ") |
| `Stop` | string | 是 | 分析结束时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ") |
| `Step` | number | 否 | 积分步长(s)，默认 60 |

### 响应数据结构（CzmlPositionOut）

详见 docs/api-schemas/CzmlPositionOut.md

## 注意事项

- 单位检查：半长径必须是米(m)而非千米(km)。
- 时间格式：首选为ISO8601 格式，也可为其它有效时间格式
- `GravitationalParameter`：引力常数（m^3/s^2），默认 `398600441500000`；若为 `0` 则按中心天体自动获取

## 标准执行流程

1. 参数预检
   - 检查必填字段完整性
   - 检查 UTC 时间格式
   - 检查 `Start < Stop`
   - 检查 `OrbitalElements` 长度必须为 6
2. 模型判定
   - 未指定 `CoordType` 时默认 `Classical`
   - 若给出速度分量则优先使用 `Cartesian`
3. 请求构造
   - 按接口契约原样传参，不做单位隐式转换
   - 明确记录 `CoordType`、`CoordSystem`、`CentralBody`
4. 结果判定
   - 先判 HTTP 状态，再判 `IsSuccess`
   - `IsSuccess = false` 时优先返回 `Message`
5. 输出归一化
   - 给出关键输入摘要、执行状态、核心输出、限制说明

## 调用示例（最小可运行：Classical）

**场景**：近地点高度约 500 km、RAAN 200° 的圆轨道，在未来 2 小时内传播。

**说明**：将下面 `export` 中的地址换成实际服务根地址。PowerShell 可用：`$env:BASE_URL='http://...'`，再对 `curl.exe` 使用同一 URL 拼接。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/TwoBody" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "CentralBody": "Earth",
    "CoordSystem": "Inertial",
    "CoordType": "Classical",
    "OrbitalElements": [6878137, 0, 45, 0, 200, 0],
    "OrbitEpoch": "2024-05-01T12:00:00Z",
    "Start": "2024-05-01T12:00:00Z",
    "Stop": "2024-05-01T14:00:00Z",
    "Step": 60
  }'
```

## 本地快速验证（可选）

用 fixture 可避免 PowerShell 下行内 JSON 转义问题：

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/TwoBody" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@propagator-twobody/fixtures/twobody-classical-min.json"
```

（路径相对于仓库根目录；若在 `propagator-twobody` 目录下执行，请改为 `@fixtures/twobody-classical-min.json`。）

## 更多示例与测试数据（fixtures）

| 文件 | 用途简述 |
|------|----------|
| `propagator-twobody/fixtures/twobody-classical-min.json` | 地球 + Classical + 2 h 窗口（与上文内联示例一致） |
| `propagator-twobody/fixtures/moon-classical-4h.json` | 月球中心天体 + Classical + 4 h（显式 `GravitationalParameter`） |
| `propagator-twobody/fixtures/mars-classical-24h.json` | 火星中心天体 + Classical + 24 h（显式 `GravitationalParameter`） |

说明：部分 fixture 使用服务可接受的非 `Z` 结尾时间字符串；若接口校验只认 ISO8601，请改为 `yyyy-MM-ddTHH:mm:ssZ` 格式后再测。

从响应中取**末时刻**位置速度：按 `docs/api-schemas/CzmlPositionOut.md` 中 `cartesianVelocity` 平铺格式，取最后一组 `[t, X, Y, Z, Vx, Vy, Vz]`。

