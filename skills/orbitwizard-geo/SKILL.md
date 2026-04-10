---
name: orbitwizard-geo
description: 生成地球同步轨道(GEO)参数。用户需要根据定点经度和倾角快速生成 GEO 初始轨道时使用。
---

# OrbitWizard GEO 技能

## 接口

- `POST /OrbitWizard/GEO`

## 输入参数 (GEO_Input)

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `Description` | string | 否 | 说明 |
| `OrbitEpoch` | string | 是 | 轨道历元 UTC (`yyyy-MM-ddTHH:mm:ss.fffZ`) |
| `Inclination` | number | 是 | 轨道倾角 (deg) |
| `SubSatellitePoint` | number | 是 | 定点地理经度 (deg) |

## 输出参数 (GEO_Output)

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `IsSuccess` | boolean | 结果(True:成功;False:失败) |
| `Message` | string | 失败原因或提示信息 |
| `Elements_TOD` | object | TOD 系开普勒根数 |
| `Elements_Inertial` | object | 惯性系开普勒根数 |

`Elements_TOD` / `Elements_Inertial` 子字段(KeplerElements):

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `SemimajorAxis` | number (double) | 轨道半长轴 (m) |
| `Eccentricity` | number (double) | 轨道偏心率 |
| `Inclination` | number (double) | 轨道倾角 (deg) |
| `ArgumentOfPeriapsis` | number (double) | 近地点幅角 (deg) |
| `RightAscensionOfAscendingNode` | number (double) | 升交点赤经 (deg) |
| `TrueAnomaly` | number (double) | 真近点角 (deg) |
| `GravitationalParameter` | number (double) | 引力常数 (m³/s²) |

## 调用示例

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitWizard/GEO" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/orbitwizard-geo/fixtures/orbitwizard-geo-min.json"
```
