---
name: orbitwizard-walker
description: 生成 Walker 星座参数。用户需要根据种子轨道与星座构型参数快速生成 Walker 星座时使用。
---

# OrbitWizard Walker 技能

## 接口

- `POST /OrbitWizard/Walker`

## 输入参数 (Walker_Input)

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `SeedKepler` | object | 是 | 种子轨道开普勒根数(见下表) |
| `WalkerType` | string | 是 | 星座构型类型: `Delta` / `Star` / `Custom` |
| `NumPlanes` | integer | 是 | 轨道面数量 |
| `NumSatsPerPlane` | integer | 是 | 每轨道面卫星数 |
| `InterPlanePhaseIncrement` | integer | 否 | 相位因子 F (`Delta`/`Star` 有效, 0 ≤ F < NumPlanes) |
| `InterPlaneTrueAnomalyIncrement` | number | 否 | 相邻轨道面真近点角增量 deg (`Custom` 有效) |
| `RAANIncrement` | number | 否 | 相邻轨道面 RAAN 增量 deg (`Custom` 有效) |

### WalkerType 说明

| 类型 | RAAN 分布 | 相位关系 | 适用场景 |
| :--- | :--- | :--- | :--- |
| `Delta` | 均匀分布在 0°~360° | 由 F 因子决定相邻面相位差 | 全球覆盖星座(如 GPS、Starlink) |
| `Star` | 均匀分布在 0°~180° | 由 F 因子决定相邻面相位差 | 极轨覆盖星座(如铱星) |
| `Custom` | 由 `RAANIncrement` 自定义 | 由 `InterPlaneTrueAnomalyIncrement` 自定义 | 非均匀构型 |

### 种子轨道开普勒根数 (SeedKepler)

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `SemimajorAxis` | number (double) | 轨道半长轴 (m) |
| `Eccentricity` | number (double) | 轨道偏心率 |
| `Inclination` | number (double) | 轨道倾角 (deg) |
| `ArgumentOfPeriapsis` | number (double) | 近地点幅角 (deg) |
| `RightAscensionOfAscendingNode` | number (double) | 升交点赤经 (deg) |
| `TrueAnomaly` | number (double) | 真近点角 (deg) |
| `GravitationalParameter` | number (double) | 引力常数 (m³/s²), 可省略使用默认地球值 |

## 输出参数 (Walker_Output)

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `IsSuccess` | boolean | 结果(True:成功;False:失败) |
| `Message` | string | 失败原因或提示信息 |
| `WalkerSatellites` | array | 二维数组, `[i][j]` 为第 i 个轨道面第 j 颗卫星的开普勒根数 |

## 标准执行流程

1. 参数预检
   - 检查 `SeedKepler` 六根数是否完整(半长轴单位 m, 角度单位 deg)
   - 检查 `WalkerType` 是否为 `Delta`/`Star`/`Custom` 之一
   - `Delta`/`Star` 模式需要 `InterPlanePhaseIncrement`(默认 0)
   - `Custom` 模式需要 `InterPlaneTrueAnomalyIncrement` 和 `RAANIncrement`
2. 请求构造
   - 向 `{BASE_URL}/OrbitWizard/Walker` 发送 JSON
3. 结果判定
   - 先看 HTTP 状态, 再看 `IsSuccess`
4. 输出归一化
   - 返回星座总卫星数 (NumPlanes × NumSatsPerPlane)
   - 返回各轨道面的 RAAN 分布和卫星相位分布摘要

## 调用示例

### Delta 模式 (4面×6星, F=1)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitWizard/Walker" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/orbitwizard-walker/fixtures/orbitwizard-walker-min.json"
```

### Star 模式 (6面×11星, F=0)

```bash
curl "${BASE_URL}/OrbitWizard/Walker" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
  "SeedKepler": {
    "SemimajorAxis": 7158137,
    "Eccentricity": 0.001,
    "Inclination": 86.4,
    "ArgumentOfPeriapsis": 0,
    "RightAscensionOfAscendingNode": 0,
    "TrueAnomaly": 0
  },
  "WalkerType": "Star",
  "NumPlanes": 6,
  "NumSatsPerPlane": 11,
  "InterPlanePhaseIncrement": 0
}'
```

### Custom 模式 (3面×4星, 自定义RAAN和相位增量)

```bash
curl "${BASE_URL}/OrbitWizard/Walker" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
  "SeedKepler": {
    "SemimajorAxis": 6878136.3,
    "Eccentricity": 0,
    "Inclination": 53,
    "ArgumentOfPeriapsis": 0,
    "RightAscensionOfAscendingNode": 0,
    "TrueAnomaly": 0
  },
  "WalkerType": "Custom",
  "NumPlanes": 3,
  "NumSatsPerPlane": 4,
  "InterPlaneTrueAnomalyIncrement": 30.0,
  "RAANIncrement": 120.0
}'
```
