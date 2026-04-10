# EntityPositionCentralBody 数据结构

中心天体位置对象,含 `$type` 判别字段(值为 `CentralBody`)。

## 字段说明


| 字段名     | 类型     | 必须  | 说明                      |
| ------- | ------ | --- | ----------------------- |
| `$type` | string | 是   | 类型标识符,必须为 `CentralBody` |
| `Name`  | string | 是   | 中心天体名称                  |


## JSON 示例

```json
{
  "$type": "CentralBody",
  "Name": "Earth"
}
```

## 注意事项

- `$type` 必须为 `CentralBody`,否则无法作为该多态分支正确反序列化。
- 表示「位于某中心天体」的抽象位置,具体几何含义由调用接口决定。

