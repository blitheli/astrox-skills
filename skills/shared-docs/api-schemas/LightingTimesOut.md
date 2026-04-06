# LightingTimesOut

光照时间计算输出参数结构。

## 字段说明

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `IsSuccess` | boolean | 结果标识（true: 成功；false: 失败） |
| `Message` | string | 错误信息（失败时存储失败原因） |
| `SunLight` | [LightTimeData](LightTimeData.md) | 全光照时间参数 |
| `Penumbra` | [LightTimeData](LightTimeData.md) | 半影时间参数 |
| `Umbra` | [LightTimeData](LightTimeData.md) | 本影（无光照）时间参数 |

## JSON 示例

```json
{
  "IsSuccess": true,
  "SunLight": {
    "Intervals": [
      {
        "Start": "2022-09-05T13:40:58.370Z",
        "Stop": "2022-09-06T02:32:56.676Z",
        "Duration": 46318.3
      }
    ],
    "MinDuration": { "Start": "...", "Stop": "...", "Duration": 46128.403 },
    "MaxDuration": { "Start": "...", "Stop": "...", "Duration": 46318.302 },
    "MeanDuration": 46223.352,
    "TotalDuration": 92446.705
  },
  "Penumbra": {
    "Intervals": [
      {
        "Start": "2022-09-05T13:37:52.308Z",
        "Stop": "2022-09-05T13:40:58.370Z",
        "Duration": 186.066
      }
    ],
    "MeanDuration": 185.676,
    "TotalDuration": 742.706
  },
  "Umbra": {
    "Intervals": [
      {
        "Start": "2022-09-05T04:00:00.000Z",
        "Stop": "2022-09-05T13:37:52.308Z",
        "Duration": 34672.308
      }
    ],
    "MeanDuration": 19906.863,
    "TotalDuration": 79610.589
  }
}
```
