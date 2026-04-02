# CzmlPositionOut 数据结构

定义了(轨道积分器返回的)数据结构。

- `IsSuccess`：布尔，是否成功
- `Message`：结果信息（失败原因）
- `Period`：轨道周期（秒），一般用于低轨轨道
- `Position`：CZML 位置对象数据结构，基于时间序列的位置、速度数据及其在中心天体参考系下的插值算法。

## Position 参数 结构说明

| 参数名 | 类型 | 必填 | 默认值 | 说明 |
|---|---|---|---|---|
| epoch | string | 是 | - | 基础历元时刻 (UTCG)，格式："yyyy-MM-ddTHH:mm:ssZ"。 |
| CentralBody | string | 否 | "Earth" | 中心天体名称（如 "Earth", "Moon"）。 |
| referenceFrame | string | 否 | "FIXED" | 坐标参考系："FIXED", "INERTIAL", "J2000", "ICRF"。 |
| interpolationAlgorithm | string | 否 | "LAGRANGE" | 插值算法："LINEAR", "LAGRANGE", "HERMITE"。 |
| interpolationDegree | integer | 否 | 7 | 插值阶数。 |
| interval | string | 否 | null | 数据时间段，格式："开始时间/结束时间"。 |
| cartesian | number[] | 否 | null | 包含时间戳的位置数组，详见下方 数据排列格式。 |
| cartesianVelocity | number[] | 否 | null | 包含时间戳的位置+速度数组，详见下方 数据排列格式。 |

------------------------------
### 数据排列格式 (Data Layout)
数组采用“平铺”方式记录多个时间点的数据，Time 偏移量通常为相对于 epoch 的历元秒。

* cartesian (仅位置)
格式：[Time1, X1, Y1, Z1, Time2, X2, Y2, Z2, ...]
单位：Time(s), X/Y/Z(m)
* cartesianVelocity (位置 + 速度)
格式：[Time1, X1, Y1, Z1, dX1, dY1, dZ1, Time2, X2, ...]
单位：Time(s), X/Y/Z(m), dX/dY/dZ(m/s)

------------------------------
### JSON示例



