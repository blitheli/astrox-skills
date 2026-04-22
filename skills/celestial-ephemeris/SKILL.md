---
name: celestial-ephemeris
description: 根据输入目标天体、观测者名称与坐标系、时间范围和积分步长,计算目标天体在分析时段内的星历并输出 CZML。适用于行星/月球轨迹可视化与后续可见性分析输入。
---

# 天体星历计算技能 (Celestial Ephemeris)

## 核心指令 (Core Instructions)

1. **输入解析**:识别目标天体 `TargetName` 以及可选观测者 `ObserverName`、观测者坐标系 `ObserverFrame`、时间窗 `Start/Stop` 和步长 `Step`。
2. **默认值处理**:
  - `ObserverName` 缺省为 `Sun`
  - `ObserverFrame` 缺省为 `MEANECLPJ2000`
  - `Step` 缺省为 `86400`(s)
  - `Start/Stop` 缺省时由服务按年度默认值处理
3. **API 调用逻辑**:向 `{BASE_URL}/celestial/ephemeris` 发送 `POST`,`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址

`POST /celestial/ephemeris`

### 输入参数结构 (JSON)

| 参数名 | 类型 | 必须 | 说明 |
| --- | --- | --- | --- |
| `TargetName` | string | 是 | 目标名称,如 `"Moon","Mars","Venus","Mercury","Jupiter","Saturn","Uranus","Neptune"` |
| `ObserverName` | string | 否 | 观测者名称,默认 `"Sun"` |
| `ObserverFrame` | string | 否 | 观测者坐标系,可选: `FIXED`,`INERTIAL`,`MEANECLPJ2000`,`J2000`;默认 `MEANECLPJ2000` |
| `Start` | string | 否 | 分析开始时刻(UTCG),格式 `yyyy-MM-ddTHH:mm:ssZ` |
| `Stop` | string | 否 | 分析结束时刻(UTCG),格式 `yyyy-MM-ddTHH:mm:ssZ` |
| `Step` | number | 否 | 积分步长,单位秒(s),默认 `86400` |

### 响应数据结构 (CzmlPositionOut)

详见 `skills/shared-docs/api-schemas/CzmlPositionOut.md`。

## 注意事项

- 时间格式建议使用 ISO8601 UTC,例如 `2026-01-01T00:00:00Z`。
- 建议显式给出 `Start`、`Stop`、`Step`,保证可重复复现。
- `Step` 过大可能导致轨迹点过疏,过小会增加返回数据量。
- 结果判定需同时检查 HTTP 状态与业务字段 `IsSuccess`。

## 标准执行流程

1. 参数预检
  - 检查必填字段 `TargetName`
  - 若同时给出 `Start/Stop`,检查 `Start < Stop`
  - 检查 `Step > 0`
2. 请求构造
  - 保留用户提供字段,仅在缺省时使用接口默认值
3. 结果判定
  - 先判 HTTP 状态,再判 `IsSuccess`
  - `IsSuccess = false` 时优先返回 `Message`
4. 输出归一化
  - 给出输入摘要(目标/观测者/时间窗/步长)与响应结论

## 调用示例 (最小可运行)

**场景**:以太阳为观测者,在 J2000 平黄道系下计算火星 7 天星历。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/ephemeris" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "TargetName": "Mars",
    "ObserverName": "Sun",
    "ObserverFrame": "MEANECLPJ2000",
    "Start": "2026-01-01T00:00:00Z",
    "Stop": "2026-01-08T00:00:00Z",
    "Step": 86400
  }'
```

## 本地快速验证 (可选)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/ephemeris" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/celestial-ephemeris/fixtures/celestial-ephemeris-mars-min.json"
```

## 更多示例与测试数据 (fixtures)

| 文件 | 用途简述 |
| --- | --- |
| `skills/celestial-ephemeris/fixtures/celestial-ephemeris-mars-min.json` | 太阳观测火星,7 天步长 1 天 |
