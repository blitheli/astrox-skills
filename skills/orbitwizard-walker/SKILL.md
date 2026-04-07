---

## name: orbitwizard-walker
description: 生成 Walker 星座参数。用户需要根据种子轨道与星座构型参数快速生成 Walker 星座时使用。

# OrbitWizard Walker 技能

## 接口

- `POST /OrbitWizard/Walker`

## 输入参数 (Walker_Input)


| 参数名                              | 类型      | 必须  | 说明                                               |
| -------------------------------- | ------- | --- | ------------------------------------------------ |
| `SeedKepler`                     | object  | 是   | 种子轨道根数（半长轴m、偏心率、倾角deg、近地点幅角deg、升交点赤经deg、真近点角deg） |
| `WalkerType`                     | string  | 是   | `Delta` / `Star` / `Custom`                      |
| `NumPlanes`                      | integer | 是   | 轨道面数量                                            |
| `NumSatsPerPlane`                | integer | 是   | 每轨道面卫星数                                          |
| `InterPlanePhaseIncrement`       | integer | 否   | 相位因子（`Delta`/`Star` 有效）                          |
| `InterPlaneTrueAnomalyIncrement` | number  | 否   | 相邻轨道面真近点角增量 deg（`Custom` 有效）                     |
| `RAANIncrement`                  | number  | 否   | 相邻轨道面 RAAN 增量 deg（`Custom` 有效）                   |


## 输出参数 (Walker_Output)


| 字段名                | 类型      | 说明                                                  |
| ------------------ | ------- | --------------------------------------------------- |
| `IsSuccess`        | boolean | 结果(True:成功；False:失败)                                |
| `Message`          | string  | 失败原因或提示信息                                           |
| `WalkerSatellites` | array   | 二维数组形式的星座开普勒根数,[0][]为第1个轨道面所有卫星,[1][]为第2个轨道面所有卫星... |


## 输入和输出中的开普勒根数


| 字段名                             | 类型              | 说明           |
| ------------------------------- | --------------- | ------------ |
| `SemimajorAxis`                 | number (double) | 轨道半长轴 (m)    |
| `Eccentricity`                  | number (double) | 轨道偏心率        |
| `Inclination`                   | number (double) | 轨道倾角 (deg)   |
| `ArgumentOfPeriapsis`           | number (double) | 近地点幅角 (deg)  |
| `RightAscensionOfAscendingNode` | number (double) | 升交点赤经 (deg)  |
| `TrueAnomaly`                   | number (double) | 真近点角 (deg)   |
| `GravitationalParameter`        | number (double) | 引力常数 (m³/s²) |


## 调用示例

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/OrbitWizard/Walker" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/orbitwizard-walker/fixtures/orbitwizard-walker-min.json"
```

