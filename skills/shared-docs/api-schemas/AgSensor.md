# AgSensor（传感器）接口说明

描述传感器的类型定义。在 OpenAPI 中通过 `**$type`** 区分具体实现；与 `EntityPath` 等结构中的 `Sensor` 字段对应。

---

### 基类约定


| 要求             | 说明                       |
| -------------- | ------------------------ |
| `**$type` 必填** | 取值：`Rectangular`、`Conic` |
| 多态             | 按 `$type` 反序列化为对应结构      |


### 派生类型一览


| `$type` 值     | OpenAPI schema 名称   | 说明                    |
| ------------- | ------------------- | --------------------- |
| `Rectangular` | `RectangularSensor` | 矩形锥（两向半角定义）           |
| `Conic`       | `ConicSensor`       | 圆环扇形锥（内外半角 + 切面方位角范围） |


---

## Rectangular（矩形传感器）

**Schema**：`RectangularSensor`


| 字段名          | 类型            | 必须  | 默认值  | 说明                     |
| ------------ | ------------- | --- | ---- | ---------------------- |
| `$type`      | string        | 是   | —    | 固定为 `Rectangular`      |
| `Text`       | string / null | 否   | —    | 说明文字                   |
| `xHalfAngle` | number        | 否   | `45` | +Z 到 +X 的半锥角 (**deg**) |
| `yHalfAngle` | number        | 否   | `45` | +Z 到 +Y 的半锥角 (**deg**) |


### JSON 示例

```json
{
  "$type": "Rectangular",
  "Text": "载荷视场",
  "xHalfAngle": 5,
  "yHalfAngle": 5
}
```

---

## Conic（圆锥/环形锥传感器）

**Schema**：`ConicSensor`


| 字段名                 | 类型            | 必须  | 默认值   | 说明                                        |
| ------------------- | ------------- | --- | ----- | ----------------------------------------- |
| `$type`             | string        | 是   | —     | 固定为 `Conic`                               |
| `Text`              | string / null | 否   | —     | 说明文字                                      |
| `innerHalfAngle`    | number        | 否   | `0`   | 圆锥内半角 (**deg**)；OpenAPI 描述中字面为「dge」，语义为角度 |
| `outerHalfAngle`    | number        | 否   | `45`  | 圆锥外半角 (**deg**)                           |
| `minimumClockAngle` | number        | 否   | `0`   | 切面起始角 (**deg**)                           |
| `maximumClockAngle` | number        | 否   | `360` | 切面终止角 (**deg**)                           |


### JSON 示例

```json
{
  "$type": "Conic",
  "innerHalfAngle": 0,
  "outerHalfAngle": 8,
  "minimumClockAngle": -45,
  "maximumClockAngle": 45
}
```

---

## 在 `EntityPath` 中的配合用法

`EntityPath`（及同类路径对象）中与传感器相关的字段示例：

- `**Sensor**`：传感器多态（`Rectangular` / `Conic`），见上文派生类型。
- `**Orientation**`：对象或传感器本体姿态，类型为 `**CrdnAxes**`；若 `Sensor` 非空，OpenAPI 说明该姿态表示**传感器**的姿态。详见 [CrdnAxes.md](CrdnAxes.md)。

最小示例（含 `Sensor`）：

```json
{
  "Name": "Satellite/Sat1",
  "Position": { "$type": "SGP4", "SatelliteNumber": "25544", "TLEs": ["1 ...", "2 ..."] },
  "Sensor": {
    "$type": "Conic",
    "outerHalfAngle": 10
  }
}
```

---

## 其它 schema：`ConicSensor` / `RectangularSensor`

部分报表或覆盖类请求体使用 `**ConicSensor**`、`**RectangularSensor**`：字段与上表类似，但**无** `$type` 与 discriminator（独立对象，不是 `ISensor` 多态外壳）。若接口要求的是这两种类型名，请直接对照 `astrox-web-api.json` 中对应 schema，勿与 `ISensor`* 混用。

---

## 注意事项

- `**$type` 字符串须与上表完全一致**（大小写敏感）。
- 提供 `**Sensor`** 时，服务端可能**自动加入传感器相关约束**；行为以实现为准。
- 角度缺省与 OpenAPI `default` 一致；未写字段时勿假设服务端会回传与请求相同的省略策略。

