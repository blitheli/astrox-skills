# LightTimeData

光照时间数据结构，用于 SunLight、Penumbra、Umbra。

## 字段说明

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `Intervals` | [TimeIntervalData](TimeIntervalData.md)[] | 所有时间段数组 |
| `MinDuration` | [TimeIntervalData](TimeIntervalData.md) | 最小持续时间 |
| `MaxDuration` | [TimeIntervalData](TimeIntervalData.md) | 最大持续时间 |
| `MeanDuration` | number | 平均时长，单位：秒 (s) |
| `TotalDuration` | number | 总时长，单位：秒 (s) |

## JSON 示例

```json
{
  "Intervals": [
    {
      "Start": "2022-09-05T13:40:58.370Z",
      "Stop": "2022-09-06T02:32:56.676Z",
      "Duration": 46318.3
    },
    {
      "Start": "2022-09-06T13:42:12.994Z",
      "Stop": "2022-09-07T02:31:01.405Z",
      "Duration": 46128.4
    }
  ],
  "MinDuration": {
    "Start": "2022-09-06T13:42:12.994Z",
    "Stop": "2022-09-07T02:31:01.405Z",
    "Duration": 46128.4
  },
  "MaxDuration": {
    "Start": "2022-09-05T13:40:58.370Z",
    "Stop": "2022-09-06T02:32:56.676Z",
    "Duration": 46318.3
  },
  "MeanDuration": 46223.352,
  "TotalDuration": 92446.705
}
```
