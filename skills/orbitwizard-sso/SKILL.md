---
name: orbitwizard-sso
description: 生成太阳同步轨道（SSO）参数。用户需要根据轨道高度与降交点地方时快速生成 SSO 初始轨道时使用。
---

# OrbitWizard SSO 技能

## 接口

- `POST /OrbitWizard/SSO`

## 输入参数 (SSO_Input)

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `Description` | string | 否 | 说明 |
| `OrbitEpoch` | string | 是 | 轨道历元 UTC (`yyyy-MM-ddTHH:mm:ss.fffZ`) |
| `Altitude` | number | 是 | 轨道高度 (km) |
| `LocalTimeOfDescendingNode` | number | 是 | 降交点地方时（例如 14:30 -> 14.5） |

## 输出参数 (SSO_Output)

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `IsSuccess` | boolean | 结果(True:成功；False:失败) |
| `Message` | string | 失败原因或提示信息 |
| `Elements_TOD` | object | TOD 系开普勒根数 |
| `Elements_Inertial` | object | 惯性系开普勒根数 |

开普勒根数的字段如下。
| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `SemimajorAxis` | number (double) | 轨道半长轴 (m) |
| `Eccentricity` | number (double) | 轨道偏心率 |
| `Inclination` | number (double) | 轨道倾角 (deg) |
| `ArgumentOfPeriapsis` | number (double) | 近地点幅角 (deg) |
| `RightAscensionOfAscendingNode` | number (double) | 升交点赤经 (deg) |
| `TrueAnomaly` | number (double) | 真近点角 (deg) |
| `GravitationalParameter` | number (double) | 引力常数 (m³/s²) |

## 标准执行流程

1. 参数预检
   - 检查 `OrbitEpoch`、`Altitude`、`LocalTimeOfDescendingNode` 是否存在
   - 时间采用 ISO8601 UTC
2. 请求构造
   - 向 `{BASE_URL}/OrbitWizard/SSO` 发送 JSON
3. 结果判定
   - 先看 HTTP 状态，再看 `IsSuccess`
4. 输出归一化
   - 返回关键输入摘要与轨道根数

## 调用示例

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitWizard/SSO" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/orbitwizard-sso/fixtures/orbitwizard-sso-min.json"
```
