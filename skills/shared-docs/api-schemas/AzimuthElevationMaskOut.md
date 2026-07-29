# AzimuthElevationMaskOut 数据结构

`POST /Terrain/AzElMask` 的响应体:某观测点四周 360° 方位–仰角地形遮罩。

## 顶层字段


| 字段名            | 类型      | 说明                                                         |
| -------------- | ------- | ---------------------------------------------------------- |
| `IsSuccess`    | boolean | 结果标识(`true`: 成功;`false`: 失败)                              |
| `Message`      | string  | 结果信息(失败时存储失败原因)                                           |
| `sitePosition` | object  | 计算点位置(`EntityPositionSite`);若请求贴地,则 `cartographicDegrees` 中高程已更新 |
| `AzElMaskData` | array   | 各方位角对应的 `ElevationMaskData` 列表(通常按 1° 步进,共约 360 项)         |


## ElevationMaskData(某一方位角)


| 字段名         | 类型     | 说明                                      |
| ----------- | ------ | --------------------------------------- |
| `Azimuth`   | number | 方位角(rad)                                |
| `Elevation` | number | 该方位角上搜索范围内的最大仰角/遮挡高度角(rad)              |
| `Items`     | array  | 可选;不同距离处的最大高度角采样(`ElevationRiseData[]`) |


## ElevationRiseData(`Items` 元素)


| 字段名         | 类型     | 说明                         |
| ----------- | ------ | -------------------------- |
| `Distance`  | number | 距中心点的距离(m)(高度角连线上)         |
| `Elevation` | number | 当前距离处地形遮挡的最大高度角(rad)       |


## 简化输出对照

`POST /Terrain/AzElMaskSimple` 返回 `AzimuthElevationMaskSimpleOut`:顶层同样含 `IsSuccess`、`Message`、`sitePosition`,但 `AzElMaskData` 为扁平 `number[]`:

```text
[方位角1, 高度角1, 方位角2, 高度角2, ...]  (单位均为 rad)
```

该扁平数组可直接作为 `access` / `lighting-times` 约束中的 `AzElMaskData` 使用。

## 单位与约定

- 方位角、仰角:rad(换算度:`* 180 / π`)
- 距离:`Distance` 为 m;请求侧 `MaxSearchRange` 为 km,`StepSize` 为 m
- 成功判定:HTTP 200 且 `IsSuccess === true`
