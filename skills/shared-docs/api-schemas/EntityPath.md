# EntityPath 系列结构

OpenAPI 中与链路/访问分析相关的**实体路径**对象，用于描述带位置（及可选姿态、传感器、约束等）的分析对象。根字段结构在 `EntityPath`、`EntityPath2`、`EntityPath3` 中一致；区别主要在于**语义**（例如发送端与接收端）以及部分 schema 分支上是否写出显式 discriminator（实际 JSON 仍通过各多态类型的 `$type` 区分）。

规范来源：`astrox-web-api.json` 中 `components.schemas.EntityPath` / `EntityPath2` / `EntityPath3`。

## 变体对照

| Schema 名称     | 典型用途（OpenAPI 描述） | 备注 |
| -------------- | ------------------- | --- |
| `EntityPath`   | 发送对象；常作为信号的**发送**平台 | 例如访问/链路输入中的 `FromObjectPath` |
| `EntityPath2`  | 接收对象；常作为信号的**接收**平台 | 例如 `ToObjectPath`；schema 中对 `Position`、`Orientation`、`Sensor` 等给出了显式 discriminator 映射 |
| `EntityPath3`  | 与 `EntityPath` 字段集相同 | 用于对象列表项（如 `EntityPathGroup.AssignedObjects` 的元素） |

在 `IEntityObject` 等多态分支中，单对象形式对应 `IEntityObjectEntityPath`：`$type` 固定为 **`EntityPath`**，其余字段与下表一致。

## 字段说明（EntityPath / EntityPath2 / EntityPath3）

| 字段名 | 类型 | 必须 | 说明 |
| ----- | ---- | --- | --- |
| `$type` | string | 见下文 | 仅在使用 **`IEntityObjectEntityPath`**（`IEntityObject` 的 `EntityPath` 分支）时出现，取值必须为 `EntityPath`。直接使用 `EntityPath` / `EntityPath2` / `EntityPath3` schema 的 JSON **通常不写**该字段（以具体接口示例为准）。 |
| `Name` | string | 否 | 对象名称；OpenAPI 默认示例：`Satellite/Sat1` |
| `Description` | string | 否 | 对象描述 |
| `Vgt` | object | 否 | VGT 提供者（`CrdnProvider`），用于自定义点、向量、坐标轴等 |
| `Position` | object | **是** | 对象位置，多态类型 **`IEntityPosition`**；通过 **`$type`** 区分具体实现（如 `SGP4`、`J2`、`SitePosition` 等）。详见 [IEntityPosition.md](IEntityPosition.md) |
| `Orientation` | object / null | 否 | 对象姿态（`CrdnAxes2`）；若 `Sensor` 非空，则表示**传感器**的姿态 |
| `Sensor` | object / null | 否 | 传感器（`ISensor`）；非空时服务端可能默认施加传感器相关约束 |
| `SensorPointing` | object / null | 否 | 传感器指向（`ISensorPointing`）；仅当存在 `Sensor` 时有效 |
| `Constraints` | array / null | 否 | 约束集合，元素为实现 **`IContraint`** 的具体类型 |
| `Lighting` | string / null | 否 | 光照约束。取值：`DirectSun`、`Penumbra`、`Umbra`；默认 `null` |
| `OccultationBodies` | string[] / null | 否 | 遮挡天体名称列表；**第 1 个**为中心天体。`null` 时的默认语义见 OpenAPI 描述（地/月中心体与仅地球、其它中心天体、太阳等情形不同） |

## JSON 示例（最小：仅必填位置）

以下示例使用 SGP4 位置；可按业务替换为 [IEntityPosition.md](IEntityPosition.md) 中任意 `$type`。

```json
{
  "Name": "Satellite/Sat1",
  "Position": {
    "$type": "SGP4",
    "SatelliteNumber": "25730",
    "TLEs": [
      "1 25730U 99025A   21120.62396556  .00000659  00000-0  35583-3 0  9997",
      "2 25730  99.0559 142.6068 0014039 175.9692 333.4962 14.16181681132327"
    ]
  }
}
```

## 使用 `IEntityObject` 时的写法

若在需要 **`IEntityObject`** 的接口中传入单一路径对象，应使用 **`IEntityObjectEntityPath`** 形态，在对象上增加 **`$type": "EntityPath"`**（与 `EntityPathGroup` 分支区分），例如：

```json
{
  "$type": "EntityPath",
  "Name": "Satellite/Sat1",
  "Position": {
    "$type": "SGP4",
    "SatelliteNumber": "25730",
    "TLEs": ["1 25730U ...", "2 25730 ..."]
  }
}
```

## 注意事项

- **`Position` 为必填**；未提供合法位置时请求无法按契约构造。
- `EntityPath2` 的 OpenAPI 中对 `Orientation`、`Sensor`、`SensorPointing` 等给出了与 `Position` 类似的 **`$type` 分支映射**；实现复杂场景时请对照仓库根目录下的 **`astrox-web-api.json`** 中对应 schema 的 `discriminator.mapping`。
- 姿态、传感器、约束等类型的完整字段以 OpenAPI 为准；本文件只归纳 `EntityPath` 系列的公共外壳。
