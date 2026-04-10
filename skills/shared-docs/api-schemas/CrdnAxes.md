# CrdnAxes（坐标轴 / 姿态）多态结构

描述对象或传感器的**姿态（坐标轴定义）**。在 OpenAPI 中通过 `**$type`** 区分具体实现；与 `EntityPath` 等结构中的 `Orientation` 字段对应。

规范来源：`astrox-web-api.json` → `components.schemas.CrdnAxes` 及各派生 schema。规范文件中部分组件键名为 `CrdnAxesCrdnAxesLVLH` 等（`CrdnAxes` 片段重复）；下文 **OpenAPI schema 名称** 已去掉重复段，写作 `CrdnAxesLVLH` 等，与 `$ref` 指向的组件一一对应。

## 基类约定


| 要求                         | 说明                       |
| -------------------------- | ------------------------ |
| `**$type` 必填**（`CrdnAxes`） | 取值必须为下表之一                |
| 多态                         | 服务端按 `$type` 反序列化为对应派生结构 |


基类字段（派生类共有）


| 字段名           | 类型            | 说明                                                                         |
| ------------- | ------------- | -------------------------------------------------------------------------- |
| `Name`        | string / null | 坐标轴名称                                                                      |
| `Description` | string        | 坐标轴描述                                                                      |
| `Start`       | string / null | 多段姿态时才使用。本段开始时刻 (UTCG)，格式 `yyyy-MM-ddTHH:mm:ssZ`；默认 `1900-01-01T00:00:00Z` |
| `Stop`        | string / null | 多段姿态时才使用。本段结束时刻 (UTCG)；默认 `2199-01-01T00:00:00Z`                           |


## 派生类型一览


| `$type` 值               | OpenAPI schema 名称               | 说明                                            |
| ----------------------- | ------------------------------- | --------------------------------------------- |
| `LVLH`                  | `CrdnAxesLVLH`                  | LVLH坐标轴(X->地心指向飞行器;Z->轨道面法向;Y->速度方向(Z×X))     |
| `VVLH`                  | `CrdnAxesVVLH`                  | VVLH坐标轴(Z轴指向地心方向，X轴约束在中心天体惯性系速度方向，Y轴指向轨道面负法向) |
| `VNC`                   | `CrdnAxesVNC`                   | VNC坐标轴(X->惯性系速度方向; Y->轨道面法向; Z->约束在天顶R方向)     |
| `Fixed`                 | `CrdnAxesFixed`                 | 相对命名参考轴的固定姿态（方位/欧拉/四元数）                       |
| `FixedAtEpoch`          | `CrdnAxesFixedAtEpoch`          | 固定在参考时刻的坐标轴                                   |
| `CzmlOrientation`       | `CrdnAxesCzmlOrientation`       | CZML 风格四元数时间序列（可插值）                           |
| `Composite`             | `CrdnAxesComposite`             | 多段姿态，按时间区间组合                                  |
| `AlignedAndConstrained` | `CrdnAxesAlignedAndConstrained` | 主轴对齐某向量、次轴受另一向量约束                             |


---

## LVLH

**Schema**：`CrdnAxesLVLH`


| 字段名     | 类型     | 必须  | 说明         |
| ------- | ------ | --- | ---------- |
| `$type` | string | 是   | 固定为 `LVLH` |


---

## VVLH

**Schema**：`CrdnAxesVVLH`


| 字段名     | 类型     | 必须  | 说明         |
| ------- | ------ | --- | ---------- |
| `$type` | string | 是   | 固定为 `VVLH` |


---

## VNC

**Schema**：`CrdnAxesVNC`


| 字段名     | 类型     | 必须  | 说明        |
| ------- | ------ | --- | --------- |
| `$type` | string | 是   | 固定为 `VNC` |


---

## Fixed（相对参考轴的固定指向）

**Schema**：`CrdnAxesFixed`


| 字段名                 | 类型     | 必须    | 说明                                         |
| ------------------- | ------ | ----- | ------------------------------------------ |
| `$type`             | string | 是     | 固定为 `Fixed`                                |
| `ReferenceAxesName` | string | **是** | 参考坐标轴名称                                    |
| `FixedOrientation`  | object | **是** | 相对该参考轴的指向参数，见下节 **FixedOrientation（嵌套多态）** |


### FixedOrientation（嵌套多态）

`FixedOrientation` 为对象，**必须**含 `**$type`**，取值与字段如下（与 OpenAPI 中 `IOrientationOrientation`* 映射一致；部分实现可能在文档生成时以内联形式出现）。


| `$type`       | 字段                                                | 说明                                             |
| ------------- | ------------------------------------------------- | ---------------------------------------------- |
| `AzEl`        | `Azimuth` (number, deg)、`Elevation` (number, deg) | 方位角：父系 XY 平面内与 +X 夹角，+Y 为正；仰角：XY 平面与指向夹角，+Z 为正 |
| `EulerAngles` | `Sequence` (string)、`A` / `B` / `C` (number, deg) | 欧拉角序列及三步角                                      |
| `Quaternion`  | `QS`、`QX`、`QY`、`QZ` (number)                      | 单位四元数分量（标量 + 向量部分，含义见 OpenAPI 字段描述）            |


### JSON 示例（Fixed + AzEl）

```json
{
  "$type": "Fixed",
  "ReferenceAxesName": "Body",
  "FixedOrientation": {
    "$type": "AzEl",
    "Azimuth": 90,
    "Elevation": 0
  }
}
```

---

## FixedAtEpoch

**Schema**：`CrdnAxesFixedAtEpoch`


| 字段名                 | 类型     | 必须    | 说明                                      |
| ------------------- | ------ | ----- | --------------------------------------- |
| `$type`             | string | 是     | 固定为 `FixedAtEpoch`                      |
| `SourceAxesName`    | string | **是** | 源坐标轴名称                                  |
| `ReferenceAxesName` | string | **是** | 参考坐标轴名称                                 |
| `Epoch`             | string | **是** | 冻结姿态的参考时刻 (UTCG)，`yyyy-MM-ddTHH:mm:ssZ` |


---

## CzmlOrientation

**Schema**：`CrdnAxesCzmlOrientation`


| 字段名                      | 类型       | 必须    | 默认值                        | 说明                                                                                         |
| ------------------------ | -------- | ----- | -------------------------- | ------------------------------------------------------------------------------------------ |
| `$type`                  | string   | 是     | —                          | 固定为 `CzmlOrientation`                                                                      |
| `epoch`                  | string   | **是** | `2021-05-01T00:00:00.000Z` | 历元 (UTCG)                                                                                  |
| `unitQuaternion`         | number[] | **是** | —                          | 姿态四元数序列，Fixed 坐标系下分量顺序 **(x, y, z, w)**；多点时为 `[Time, X, Y, Z, W, …]`，`Time` 为相对 `epoch` 的秒 |
| `CentralBody`            | string   | 否     | `Earth`                    | 中心天体名，如 `Earth`、`Moon`                                                                     |
| `interpolationAlgorithm` | string   | 否     | `LINEAR`                   | `LINEAR`、`LAGRANGE`、`HERMITE`                                                              |
| `interpolationDegree`    | integer  | 否     | `1`                        | 插值阶数                                                                                       |


### JSON 示例

```json
{
      "$type": "CzmlOrientation",
      "CentralBody": "Earth",
      "interpolationAlgorithm": "LAGRANGE",
      "interpolationDegree": 1,
      "epoch": "2022-04-25T04:00:00.0000Z",
      "unitQuaternion": [
        0,  -0.486785, 0.511829, 0.487841, 0.512915,
        12, -0.582614, 0.399471, 0.583421, 0.400754,
        24, ...
      ]
    }
```

---

## Composite（多段姿态）

**Schema**：`CrdnAxesComposite`

`Intervals` 为**姿态段数组**；每个元素与 `**CrdnAxes`** 相同的多态约定（含 `$type`），可递归包含 `Composite`。


| 字段名         | 类型     | 必须    | 说明                    |
| ----------- | ------ | ----- | --------------------- |
| `$type`     | string | 是     | 固定为 `Composite`       |
| `Intervals` | array  | **是** | 各元素为 `CrdnAxes` 的任一分支 |


---

## AlignedAndConstrained

**Schema**：`CrdnAxesAlignedAndConstrained`


| 字段名             | 类型     | 必须    | 默认值  | 说明                                                     |
| --------------- | ------ | ----- | ---- | ------------------------------------------------------ |
| `$type`         | string | 是     | —    | 固定为 `AlignedAndConstrained`                            |
| `Principal`     | string | **是** | —    | 指向向量名称（VGT 等上下文中已命名的向量）                                |
| `PrincipalAxis` | string | 否     | `+X` | 本坐标系中要与 `Principal` 对齐的轴：`+X`、`-X`、`+Y`、`-Y`、`+Z`、`-Z` |
| `Reference`     | string | **是** | —    | 约束用向量名称                                                |
| `ReferenceAxis` | string | 否     | `+Z` | 与 `Reference` 对应的坐标轴（同上六种）                             |


---

## 在 `CrdnProvider` 中的用法

`CrdnProvider.Axes` 为 `**CrdnAxes`** 对象数组，用于声明自定义坐标轴（不含服务端缺省定义）。详见 `astrox-web-api.json` 中 `CrdnProvider`。

---

## 注意事项

- 请求 JSON 中 `**$type` 须与上表完全一致**（大小写敏感）。
- `ReferenceAxesName`、`SourceAxesName`、向量名等字符串需与同一请求中 **VGT / 内置轴命名** 一致，否则解析或运行期可能失败。
- `CzmlOrientation.unitQuaternion` 的布局与插值行为以实现为准；复杂序列建议对照 OpenAPI 字段说明或联调实测。

