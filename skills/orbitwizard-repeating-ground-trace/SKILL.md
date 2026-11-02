---
name: orbitwizard-repeating-ground-trace
description: 生成回归地面轨迹轨道(Repeating Ground Trace)参数。用户需要按倾角、回归圈数与首次升交点地固经度生成回归轨道时使用。
---

# OrbitWizard Repeating Ground Trace 技能

## 接口

- `POST /OrbitWizard/RepeatingGroundTrace`

契约见仓库根目录 `astrox-web-api.json` 的 `RepeatingGroundTrace_Input` / `RepeatingGroundTrace_Output`。OpenAPI 未写明的行为标为预留,不补字段。

## 输入参数 (RepeatingGroundTrace_Input)

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `Description` | string | 否 | 说明。schema 默认值:`回归轨道，倾角 98.2°，约 700 km，233 圈` |
| `OrbitEpoch` | string | 是 | 轨道历元 (UTCG),格式 `yyyy-MM-ddTHH:mm:ss.fffZ`(ISO8601,带 `Z`)。schema 类型可为 null,但仍在 `required` 中。默认 `2023-08-11T04:00:00Z` |
| `ApproxAltitude` | number (double) | 否 | 近似轨道高度 (km)。schema 写明与 `ApproxRevsPerDay` 二选一。属性只有 `format: double`,未标 `type` |
| `ApproxRevsPerDay` | number (double) | 否 | 近似每天圈数。schema 写明与 `ApproxAltitude` 二选一。属性只有 `format: double`,未标 `type` |
| `Inclination` | number (double) | 是 | 轨道倾角 (deg)。默认 `98.2` |
| `RevsToRepeat` | integer (int32) | 是 | 回归圈数 `RevsToRepeat`。默认 `233` |
| `LongitudeFirstAN` | number (double) | 是 | 首次升交点地固经度 (deg)。默认 `0` |
| `UseAnomalisticMeanMotion` | boolean | 否 | schema:`true` 时用近点平均角速度(仅对照)。默认 `false` |

二者同时给出或都不给时服务端如何取舍,schema 未定义,预留。

## 输出参数 (RepeatingGroundTrace_Output)

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `IsSuccess` | boolean | 结果(True:成功;False:失败) |
| `Message` | string | 结果信息(主要是存储失败的原因) |
| `OrbitEpoch` | string | 实际采用的轨道历元 (UTC)。schema 类型可为 null |
| `Elements_TOD` | object | TOD 坐标系下的开普勒轨道根数,可为 null |
| `Elements_Inertial` | object | 惯性坐标系下的开普勒轨道根数,可为 null |
| `RevsToRepeat` | integer (int32) | 回归圈数 N |
| `DaysToRepeat` | integer (int32) | 回归天数 D(节点日) |
| `DesiredRevsPerDay` | number (double) | 选取回归天数时用的近似每天圈数 |
| `NodePeriod` | number (double) | 节点周期 T_Ω (s) |
| `Altitude` | number (double) | 轨道高度 a−Re (km) |
| `ClosureErrorDeg` | number (double) | 升交点地理经度闭合误差 ε (deg) |
| `Inclination` | number (double) | 设计倾角 (deg),TOD |
| `RightAscensionOfAscendingNode` | number (double) | 设计升交点赤经 (deg),TOD |
| `LongitudeFirstAN` | number (double) | 首次升交点地固经度 (deg) |

`Elements_TOD` / `Elements_Inertial` 子字段 (`KeplerElements`):

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `SemimajorAxis` | number (double) | 轨道半长轴 (m) |
| `Eccentricity` | number (double) | 轨道偏心率 |
| `Inclination` | number (double) | 轨道倾角 (deg) |
| `ArgumentOfPeriapsis` | number (double) | 近地点幅角 (deg) |
| `RightAscensionOfAscendingNode` | number (double) | 升交点赤经 (deg) |
| `TrueAnomaly` | number (double) | 真近点角 (deg) |
| `GravitationalParameter` | number (double) | 引力常数 (m^3/s^2),默认值为地球引力常数 |

## 标准执行流程

1. 参数预检
   - 检查 `OrbitEpoch`、`Inclination`、`RevsToRepeat`、`LongitudeFirstAN` 是否存在
   - `OrbitEpoch` 使用 ISO8601,带 `Z`
   - `ApproxAltitude` 单位为 km,角度单位为 deg,`RevsToRepeat` 为整数
   - `ApproxAltitude` 与 `ApproxRevsPerDay` 按 schema 二选一
2. 请求构造
   - 向 `{BASE_URL}/OrbitWizard/RepeatingGroundTrace` 发送 JSON,`Content-Type: application/json`
3. 结果判定
   - 先看 HTTP 状态,再看 `IsSuccess` 与 `Message`
4. 输出归一化
   - 返回回归圈数、回归天数、节点周期 (s)、高度 (km) 与两套开普勒根数

## 注意事项

- 输入高度 `ApproxAltitude` 与输出高度 `Altitude` 的单位都是 km。开普勒半长轴 `SemimajorAxis` 的单位是 m。
- `NodePeriod` 单位是 s,角度单位是 deg。
- 无枚举字段。

## 调用示例

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitWizard/RepeatingGroundTrace" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/orbitwizard-repeating-ground-trace/fixtures/orbitwizard-repeating-ground-trace-min.json"
```
