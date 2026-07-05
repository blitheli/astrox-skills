---
name: propagator
description: 通过 Astrox WebAPI 的 Propagator 端点进行轨道递推/星历外推,输出 CzmlPositionOut。支持 TwoBody(二体)、J2(J2摄动)、HPOP(高精度多摄动)、SGP4(TLE)。当用户需要轨道递推、星历外推、轨道积分、TLE/SGP4、J2摄动、HPOP 高精度计算时使用。
---

# 轨道递推技能 (Propagator)

## 积分器选型

根据用户输入选择端点。**有 TLE 两行时只能走 SGP4**,禁止用根数类端点。

| 用户输入 | 端点 | 说明 |
| --- | --- | --- |
| 有 TLE 两行 | `POST /Propagator/sgp4` | 无需 `OrbitalElements` |
| 需高精度多摄动 | `POST /Propagator/HPOP` | 需 `HpopPropagator` 配置 |
| 需 J2 扁率影响 | `POST /Propagator/J2` | 解析 J2,配置简单 |
| 纯二体/快速估算 | `POST /Propagator/TwoBody` | 最快,无摄动 |

**注意**: SGP4 路径为 `/Propagator/sgp4`(`sgp4` 小写),与 `/Propagator/TwoBody` 命名风格不同。

## 核心指令 (Core Instructions)

1. **输入解析**:识别时间窗、初值类型(TLE 或轨道根数)及可选的积分器偏好。
2. **端点路由**:按上表选择端点;根数类默认 `Inertial` 系和 `Earth` 中心天体,除非用户指定其他。
3. **参数转换(根数类)**:
   - `CoordType` 为 `Classical`:6 元素 [半长径(m), 偏心率, 倾角(deg), 近点角距(deg), 升交点经度(deg), 真近点角(deg)]
   - `CoordType` 为 `Cartesian`:6 元素 [X(m), Y(m), Z(m), Vx(m/s), Vy(m/s), Vz(m/s)]
4. **API 调用**:向 `{BASE_URL}/Propagator/{Endpoint}` 发送 `POST`,`Content-Type: application/json`。

公共约定、单位、执行流程详见 [PropagatorCommon.md](../shared-docs/api-schemas/PropagatorCommon.md)。

## 端点索引

### TwoBody — 二体积分器

- **端点**: `POST /Propagator/TwoBody`
- **Schema**: [EntityPositionTwoBody.md](../shared-docs/api-schemas/EntityPositionTwoBody.md)
- **特点**: 纯中心天体引力,无摄动;可选 `CentralBody`、`GravitationalParameter`、`Step`

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

### J2 — J2 摄动积分器

- **端点**: `POST /Propagator/J2`
- **Schema**: [EntityPositionJ2.md](../shared-docs/api-schemas/EntityPositionJ2.md)
- **特点**: J2 解析摄动;额外可选 `J2NormalizedValue`、`RefDistance`

```bash
curl "${BASE_URL}/Propagator/J2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Start": "2022-04-18T04:00:00.000Z",
    "Stop": "2022-04-18T16:00:00.000Z",
    "CentralBody": "Earth",
    "OrbitEpoch": "2022-04-18T04:00:00.000Z",
    "CoordType": "Classical",
    "OrbitalElements": [6678140, 0, 28.5, 0, 0, 0]
  }'
```

### HPOP — 高精度轨道递推

- **端点**: `POST /Propagator/HPOP`
- **Schema**: [EntityPositionHPOP.md](../shared-docs/api-schemas/EntityPositionHPOP.md)
- **特点**: 数值积分 + 非球形引力、第三体、大气阻力、太阳辐射压;配置复杂,建议从 fixture 起步

```bash
curl "${BASE_URL}/Propagator/HPOP" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@propagator/fixtures/hpop/hpop-earth-default-v10.json"
```

### SGP4 — TLE 递推

- **端点**: `POST /Propagator/sgp4`
- **Schema**: [EntityPositionSGP4.md](../shared-docs/api-schemas/EntityPositionSGP4.md)
- **特点**: 输入为 `TLEs` 数组(恰好 2 个字符串),不用 `OrbitalElements`

```bash
curl "${BASE_URL}/Propagator/sgp4" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Start": "2021-05-01T00:00:00Z",
    "Stop": "2021-05-01T06:00:00Z",
    "Step": 60,
    "SatelliteNumber": "25730",
    "TLEs": [
      "1 25730U 99025A   21120.62396556  .00000659  00000-0  35583-3 0  9997",
      "2 25730  99.0559 142.6068 0014039 175.9692 333.4962 14.16181681132327"
    ]
  }'
```

## 响应数据结构

所有端点均返回 `CzmlPositionOut`,详见 [CzmlPositionOut.md](../shared-docs/api-schemas/CzmlPositionOut.md)。

## 本地快速验证 (fixtures)

路径相对于仓库根目录:

```bash
export BASE_URL=http://astrox.cn:8765

# TwoBody
curl "${BASE_URL}/Propagator/TwoBody" -X POST -H 'Content-Type: application/json' \
  --data-binary "@propagator/fixtures/twobody/twobody-classical-min.json"

# J2
curl "${BASE_URL}/Propagator/J2" -X POST -H 'Content-Type: application/json' \
  --data-binary "@propagator/fixtures/j2/j2-earth-classical-min.json"

# HPOP
curl "${BASE_URL}/Propagator/HPOP" -X POST -H 'Content-Type: application/json' \
  --data-binary "@propagator/fixtures/hpop/hpop-earth-default-v10.json"

# SGP4
curl "${BASE_URL}/Propagator/sgp4" -X POST -H 'Content-Type: application/json' \
  --data-binary "@propagator/fixtures/sgp4/sgp4-min.json"
```

## Fixtures 索引

| 文件 | 端点 | 用途简述 |
| --- | --- | --- |
| `propagator/fixtures/twobody/twobody-classical-min.json` | TwoBody | 地球 + Classical + 2 h |
| `propagator/fixtures/twobody/moon-classical-4h.json` | TwoBody | 月球 + Classical + 4 h |
| `propagator/fixtures/twobody/mars-classical-24h.json` | TwoBody | 火星 + Classical + 24 h |
| `propagator/fixtures/j2/j2-earth-classical-min.json` | J2 | 地球默认 J2, 12 h |
| `propagator/fixtures/j2/j2-mars-classical.json` | J2 | 火星 J2, 24 h |
| `propagator/fixtures/hpop/hpop-earth-default-v10.json` | HPOP | 地球 STK Default V10, 24 h |
| `propagator/fixtures/hpop/hpop-moon-default-v10.json` | HPOP | 月球 STK Default V10, 24 h |
| `propagator/fixtures/hpop/hpop-mars-250603.json` | HPOP | 火星 MRO110C 引力场, 24 h |
| `propagator/fixtures/sgp4/sgp4-min.json` | SGP4 | 默认 TLE, 6 h + Step 60 |

## 相关技能

- 导弹弹道: [propagator-ballistic](../propagator-ballistic/SKILL.md)
- 火箭上升段: [propagator-simple-ascent](../propagator-simple-ascent/SKILL.md)
- 可见性/access 中引用 propagator 位置: [access](../access/SKILL.md)

## 可选扩展 (未文档化)

OpenAPI 中还有批量递推端点,后续可补充:

- `POST /Propagator/MultiTwoBody`
- `POST /Propagator/MultiJ2`
- `POST /Propagator/MultiSgp4`
