# IContraint（约束）多态结构

`EntityPath` 等对象中 `Constraints` 数组的元素类型在 OpenAPI 里定义为 **`IContraint`**（拼写为 Contr**ai**nt，与英文 *constraint* 不同；请求 JSON 中仍通过 **`$type`** 区分具体分支）。

规范来源：`astrox-web-api.json` → `components.schemas.IContraint` 及其派生 schema。

## 基类约定

| 要求 | 说明 |
| --- | --- |
| **`$type` 必填** | 取值必须为下表之一：`Range`、`AzElMask`、`ElevationAngle` |
| 多态 | 服务端按 `$type` 反序列化为对应派生结构 |

## 派生类型一览

| `$type` 值 | OpenAPI schema 名称 | 说明 |
| ---------- | ------------------- | --- |
| `Range` | `IContraintConstraintRange` | 距离（斜距）上下限约束，单位 km |
| `AzElMask` | `IContraintConstraintAzElMask` | 方位角–仰角遮罩（成对采样点），角度单位 rad |
| `ElevationAngle` | `IContraintConstraintElevationAngle` | 仰角上下限约束，单位 deg |

---

## Range（距离约束）

**Schema**：`IContraintConstraintRange`

| 字段名 | 类型 | 必须 | 默认值 | 说明 |
| ------ | ---- | --- | ------ | --- |
| `$type` | string | 是 | — | 固定为 `Range` |
| `Text` | string / null | 否 | — | 说明文字 |
| `MinimumValue` | number | 否 | `0` | 最小距离 (km) |
| `MaximumValue` | number | 否 | `1e20` | 最大距离 (km)；**仅当** `IsMaximumEnabled === true` 时参与约束 |
| `IsMaximumEnabled` | boolean | 否 | `false` | 是否启用最大距离 |

### JSON 示例

```json
{
  "$type": "Range",
  "Text": "距离约束示例",
  "MinimumValue": 0,
  "MaximumValue": 50000,
  "IsMaximumEnabled": true
}
```

---

## AzElMask（方位–仰角遮罩）

**Schema**：`IContraintConstraintAzElMask`

| 字段名 | 类型 | 必须 | 说明 |
| ------ | ---- | --- | --- |
| `$type` | string | 是 | 固定为 `AzElMask` |
| `Text` | string / null | 否 | 说明 |
| `MaxRange` | number | 否 | 遮罩最大距离 (km)，**仅作说明**，不参与核心算法字段的必填性 |
| `AzElMaskData` | number[] | **是** | 按顺序交替存放：**方位角、仰角、方位角、仰角…** 单位均为 **rad** |

### JSON 示例

```json
{
  "$type": "AzElMask",
  "Text": "地面站天线包络",
  "MaxRange": 40000,
  "AzElMaskData": [0, 0.1745, 1.5708, 1.0472]
}
```

---

## ElevationAngle（仰角约束）

**Schema**：`IContraintConstraintElevationAngle`

| 字段名 | 类型 | 必须 | 默认值 | 说明 |
| ------ | ---- | --- | ------ | --- |
| `$type` | string | 是 | — | 固定为 `ElevationAngle` |
| `Text` | string / null | 否 | — | 说明 |
| `MinimumValue` | number | 否 | `0` | 仰角最小值 (deg) |
| `MaximumValue` | number | 否 | `90` | 仰角最大值 (deg)；**仅当** `IsMaximumEnabled === true` 时有效 |
| `IsMaximumEnabled` | boolean | 否 | `false` | 是否启用最大仰角 |

### JSON 示例

```json
{
  "$type": "ElevationAngle",
  "Text": "最低仰角 5°",
  "MinimumValue": 5,
  "MaximumValue": 90,
  "IsMaximumEnabled": false
}
```

---

## 在 `Constraints` 数组中的用法

`EntityPath` / `EntityPath2` / `EntityPath3` 等处的 `Constraints` 为上述对象的数组，可混合多种 `$type`，例如：

```json
"Constraints": [
  {
    "$type": "ElevationAngle",
    "MinimumValue": 5
  },
  {
    "$type": "Range",
    "MinimumValue": 100,
    "MaximumValue": 42000,
    "IsMaximumEnabled": true
  }
]
```

## 注意事项

- 请求体中 **`$type` 字符串必须与上表完全一致**（大小写敏感）。
- `Range` 与 `ElevationAngle` 的「最大值」是否生效由 **`IsMaximumEnabled`** 控制；未启用时不要依赖 `MaximumValue` 的约束语义。
- `AzElMask` 的 `AzElMaskData` 长度应为 **偶数**（成对的方位角、仰角）；具体插值或闭合规则以实现为准，复杂包络请对照服务文档或实测。
