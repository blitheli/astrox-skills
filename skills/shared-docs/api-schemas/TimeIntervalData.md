# TimeIntervalData

时间间隔数据结构。

## 字段说明


| 字段名        | 类型     | 说明                                        |
| ---------- | ------ | ----------------------------------------- |
| `Start`    | string | 开始时刻 (UTCG)，格式：`yyyy-MM-ddTHH:mm:ss.fffZ` |
| `Stop`     | string | 结束时刻 (UTCG)，格式：`yyyy-MM-ddTHH:mm:ss.fffZ` |
| `Duration` | number | 持续时间，单位：秒 (s)                             |


## JSON 示例

```json
{
  "Start": "2022-09-05T13:40:58.370Z",
  "Stop": "2022-09-06T02:32:56.676Z",
  "Duration": 46318.3
}
```

