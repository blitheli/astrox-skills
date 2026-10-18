---
name: close-approach
description: use this when the user needs close-approach or conjunction screening between one SGP4 satellite and TLE targets (近距离交会, 碰撞筛选, 最近距离, 相对速度, 碰撞概率). Calls POST /CAT/CA_ComputeV5.
---

# 近距离交会技能 (Close Approach / CA_ComputeV5)

用 SGP4 比较一颗主星和一组 TLE 目标, 在给定时间窗内筛选近距离交会. 网站说明写明本接口取代 `CA_ComputeV3`: 主星和目标都走 SGP4, 交会前先做近地点/远地点包络筛和距离向的时间筛.

上游实现仓库 `https://github.com/blitheli/ASTROX.AeroSpace` 在本次维护时无法读取 (GitHub API 返回 404). 字段含义以 `http://astrox.cn:8765/openapi/v1.json` (版本 `2026-10-12`) 为准, 并已同步到仓库根目录 `astrox-web-api.json` 的 `/CAT/CA_ComputeV5`, `CAInput5`, `CAOutput5`. 源码才能确定的细节标为待确认.

## 核心指令 (Core Instructions)

1. **输入解析**: 必填分析窗口 `Start_UTCG`/`Stop_UTCG` 和主星 `SAT1` (`TleInfo`). 目标列表 `Targets` 建议显式给出. 省略或为 null 时, 服务端从数据库 `Data/DataBases/Satellite` 读取全部目标.
2. **参数校验**: 时间使用 ISO8601 UTC. 距离阈值单位是 km, 不是 m. `Start_UTCG` 必须早于 `Stop_UTCG`. 每条 TLE 的 `SAT_Name`, `SAT_Number`, `TLE_Line1`, `TLE_Line2` 都要填.
3. **调用逻辑**: `POST {BASE_URL}/CAT/CA_ComputeV5`, `Content-Type: application/json`. 先看 HTTP 状态, 再看 `IsSuccess`.
4. **结果读取**: `CA_Results` 是交会事件列表. 同一对目标在一个窗口内可以有多条 (每次局部最近距离一条). 同时看 `TotalNumber`, `AfterApoPeriFilterNumber`, `AfterTimeFilterNumber`, 判断目标是被筛掉还是确实没有进入距离阈值.

## API 规范 (Tool Definition)

### 接口地址

`POST /CAT/CA_ComputeV5`

默认服务根地址见 `claude.json` 的 `defaultServer` (`http://astrox.cn:8765/`).

网站摘要: 空间碎片接近计算 V5. 轨道面夹角阈值缺省 1 deg. TimeFilter 改为距离判断且并行. `Targets` 可为 null, 则从数据库读取所有目标.

### 输入参数结构 (CAInput5)

| 参数名 | 类型 | 必须 | 默认值 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| `Start_UTCG` | string | 是 | 无 | 分析开始 (UTC), OpenAPI 格式 `yyyy-MM-ddTHH:mm:ss.fffZ` |
| `Stop_UTCG` | string | 是 | 无 | 分析结束 (UTC), 格式同上 |
| `TolMaxDistance` | number | 否 | `5` | 判定为交会的最大距离 (km) |
| `ApoPeriPad` | number | 否 | `30` | 近地点/远地点径向包络垫量 (km). OpenAPI 写明对应 STK `apoPeriPad` |
| `TimeDistPad` | number | 否 | `30` | 时间筛的距离垫量 (km). OpenAPI 写明对应 STK `timeDistPad` |
| `UseApoPeriFilter` | boolean | 否 | `true` | 是否做 Apo/Peri 包络筛. OpenAPI: 主星 300 s 采样, 次星取窗口端点 T0/T1 |
| `UseTimeOrCrossFilter` | boolean | 否 | `true` | 是否做交点时间筛. OpenAPI: V5 实现名为 `CrossTimePreFilter5` |
| `TolTheta` | number | 否 | `1` | 交点筛选的轨道面夹角阈值 (deg) |
| `SAT1` | TleInfo | 是 | 无 | 主星 (飞行器), SGP4/TLE |
| `Targets` | TleInfo[] | 否 | null | 目标列表. null 或省略表示读数据库全量目标 |

`TleInfo` 在 OpenAPI 里没有 `required` 数组, 但服务端对空对象返回 HTTP 400. 实测 `SAT1` 的 `SAT_Name`, `SAT_Number`, `TLE_Line1`, `TLE_Line2` 都是必填. `Targets[]` 至少要求 `SAT_Name`, `TLE_Line1`, `TLE_Line2`. `Targets[]` 缺 `SAT_Number` 时本次 400 列表里没有该项, 是否允许省略标为待确认. 调用时四个字段都填.

OpenAPI 给 `TleInfo` 填了 ISS (`25544`) 示例 default. 空字段不会落到这些示例, 而是 400.

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| `SAT_Name` | string | 是 | 名称 |
| `SAT_Number` | string | 是 (见上) | NORAD 编号, 字符串 |
| `TLE_Line1` | string | 是 | TLE 第一行 |
| `TLE_Line2` | string | 是 | TLE 第二行 |

### 响应数据结构 (CAOutput5)

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `IsSuccess` | boolean | `true` 成功, `false` 失败 |
| `Message` | string | 失败原因. 成功时实测为 `OK` |
| `TotalNumber` | integer | 目标总数 |
| `AfterApoPeriFilterNumber` | integer | Apo/Peri 过滤后的目标数 |
| `AfterTimeFilterNumber` | integer | OpenAPI 文字为“轨道面交点过滤后的总目标数”. 字段名是 TimeFilter, 与文字是否同一层筛选待确认 |
| `CA_Results` | CAResultInfo[] 或 null | 交会事件. 一个元素是一对目标的一次最近距离 |
| `TimingPrepareMs` | integer | 准备阶段墙钟 (ms) |
| `TimingApoPeriMs` | integer | Apo/Peri 筛累计墙钟 (ms) |
| `TimingTimeOrCrossMs` | integer | CrossTime 筛累计墙钟 (ms) |
| `TimingAccessMinRangeMs` | integer | Access 与 MinRange 终检累计墙钟 (ms) |
| `TimingTotalMs` | integer | OpenAPI 文字为“CA_ComputeV3总墙钟(ms)”. 出现在 V5 响应里, 是否为说明残留待确认 |

### CAResultInfo (`CA_Results` 元素)

与 V3 共用 `CAResultInfo`. 快照与 2026-10-12 网站 schema 一致.

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `SAT1_Name` | string | 主星名称 |
| `SAT2_Name` | string | 目标名称 |
| `SAT1_Number` | string | 主星编号 |
| `SAT2_Number` | string | 目标编号 |
| `SAT1_TLE_Line1` | string | 主星 TLE 第一行 |
| `SAT1_TLE_Line2` | string | 主星 TLE 第二行 |
| `SAT2_TLE_Line1` | string | 目标 TLE 第一行 |
| `SAT2_TLE_Line2` | string | 目标 TLE 第二行 |
| `CA_MinRange_Time` | string | 该次最近距离时刻 (UTCG). 实测格式 `yyyy-MM-ddTHH:mm:ss.fffZ` |
| `CA_MinRange` | number | 最近距离 (km) |
| `CA_Theta` | number | OpenAPI: 轨道面夹角 (deg). 见注意事项 |
| `CA_DeltaV` | number | 相对速度 (km/s). 未返回分量, 坐标系待确认 |
| `CA_Probability` | number | OpenAPI: 碰撞概率, `1` 表示完全碰撞. 计算公式待确认 |

## 注意事项

- 距离用 km, 速度用 km/s, 角度用 deg, 时间用 s 或 UTC. 不要把 `TolMaxDistance` 当成米.
- 时间窗要盖住 TLE 历元附近. 主星样例与 `skills/propagator/fixtures/sgp4/sgp4-min.json` 同为 NORAD `25730`, 历元约 `2021-04-30T15:00:00Z`.
- 不带毫秒的 `yyyy-MM-ddTHH:mm:ssZ` 本次也可被接受. 仍按 OpenAPI 写成 `.fffZ`.
- `Start_UTCG >= Stop_UTCG` 时, 实测仍是 HTTP 200 且 `IsSuccess=true`, `CA_Results` 为空. 调用方必须自己检查时间顺序. 不能只靠 `IsSuccess` 判断窗口有效.
- 省略 `Targets` 会扫描数据库全量. 2026-10-18 一次 10 分钟窗口实测 `TotalNumber=34725`. 这个数字随库变化. fixture 和日常调用都要显式传入 `Targets`, 避免打满全库.
- 筛选计数是漏斗: `TotalNumber` -> `AfterApoPeriFilterNumber` -> `AfterTimeFilterNumber`. 终检再按 `TolMaxDistance` 写入 `CA_Results`.
- 一对目标可以对应多条 `CA_Results`. `ca-computev5-raan5-4h.json` 在 4 小时内返回 4 条, 间隔约半个轨道周期.
- 完全相同的两行根数 (只改编号) 实测没有事件. 沿迹只差 `0.02 deg` 时有一条约 `2.515 km` 的事件. 零距离是否被当成无局部极小值, 待确认.
- `CA_Theta`: 倾角增大 `2 deg` 时实测约 `1.999`; 升交点赤经增大 `2 deg` 或 `5 deg` 时实测为 `2` 或 `5`, 与轨道面夹角一致. 只把平近点角增加 `0.02 deg` 时返回 `0.02`, 而轨道面夹角应接近 `0`. 该特例的精确定义待确认.
- `CA_DeltaV` 量级与相对速度一致: 共面沿迹小偏差约 `0.0026 km/s`; 倾角差 `2 deg` 约 `0.26 km/s`. 矢量所在坐标系 (TEME 或别的) 待确认.
- `CA_Probability` 在 `2.5 km` 和 `25 m` 两次交会里都是 `0`. 等效半径, 协方差和公式都待确认. 不要把 `0` 解读成“已证明不会碰撞”.
- `TolTheta` 在升交点差 `5 deg` 时, 设为 `1` 或 `10` 都没有改变是否进入终检. 它和 `CrossTimePreFilter5` 的组合规则待确认.
- 主星 `300 s` 扫描, 次星只取 T0/T1, 只来自 OpenAPI 对 `UseApoPeriFilter` 的描述, 没有源码核对.
- `99901`, `99902`, `99941` 是本技能构造的编号, 不是在轨目标. 近目标只把平近点角从 `333.4962 deg` 改为 `333.5162 deg` (沿迹约 `2.5 km`). `GEO-LIKE` 把平均运动改成 `1.00273791 rev/day`, 用来触发 Apo/Peri 筛除.

## 标准执行流程

1. 参数预检: `Start_UTCG`, `Stop_UTCG`, `SAT1` 四字段, 以及每个目标的 TLE. 确认 `Start < Stop`, 距离单位是 km.
2. 目标列表: 用户没给目标时不要默认扫全库, 先问清楚. 只有用户明确要求对库内全部目标做筛选时才省略 `Targets`.
3. 请求构造: 阈值缺省可用 OpenAPI 默认 (`5 km`, `30 km`, `30 km`, 两个筛开关 `true`, `TolTheta=1`).
4. 结果判定: HTTP 200 后读 `IsSuccess`. 失败看 `Message`. HTTP 400 是字段校验失败, 没有 `IsSuccess`.
5. 输出归一化: 报告窗口, 阈值, 三级计数, 每条交会的时刻, `CA_MinRange` (km), `CA_DeltaV` (km/s), `CA_Theta` (deg), `CA_Probability`. 概率为 0 时按待确认处理, 不要写成碰撞结论.

## Fixtures 目录

| 文件 | 说明 |
| :--- | :--- |
| `ca-computev5-min.json` | 只含必填时间和一对目标, 阈值走默认. 期望 1 条约 `2.515 km` 的交会 |
| `ca-computev5-filter-4h.json` | 4 小时, 显式阈值. 近目标保留, `GEO-LIKE` 被 Apo/Peri 筛掉 |
| `ca-computev5-raan5-4h.json` | 升交点赤经加 `5 deg`, 距离阈值 `200 km`. 4 小时内 4 次交会, 最近距离约 `99 km` |

## 调用示例

在仓库根目录执行. `BASE_URL` 末尾不要重复斜杠.

### 1. 最小交会 (默认 5 km)

主星 `25730`, 目标沿迹相位差 `0.02 deg`.

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/CAT/CA_ComputeV5" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/close-approach/fixtures/ca-computev5-min.json
```

请求体与 fixture 相同:

```json
{
  "Start_UTCG": "2021-04-30T14:00:00.000Z",
  "Stop_UTCG": "2021-04-30T18:00:00.000Z",
  "SAT1": {
    "SAT_Name": "FY-1C-25730",
    "SAT_Number": "25730",
    "TLE_Line1": "1 25730U 99025A   21120.62396556  .00000659  00000-0  35583-3 0  9997",
    "TLE_Line2": "2 25730  99.0559 142.6068 0014039 175.9692 333.4962 14.16181681132327"
  },
  "Targets": [
    {
      "SAT_Name": "NEAR-ALONG",
      "SAT_Number": "99901",
      "TLE_Line1": "1 99901U 99025A   21120.62396556  .00000659  00000-0  35583-3 0  9998",
      "TLE_Line2": "2 99901  99.0559 142.6068 0014039 175.9692 333.5162 14.16181681132321"
    }
  ]
}
```

2026-10-18 实测: HTTP 200, `IsSuccess=true`, `Message=OK`, 计数 `1 / 1 / 1`, 一条结果 `CA_MinRange_Time=2021-04-30T17:22:21.541Z`, `CA_MinRange=2.515`, `CA_DeltaV=0.002583`, `CA_Theta=0.02`, `CA_Probability=0`.

### 2. 近目标加一个会被筛掉的高轨目标

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/CAT/CA_ComputeV5" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/close-approach/fixtures/ca-computev5-filter-4h.json
```

2026-10-18 实测: 计数 `2 / 1 / 1`, `CA_Results` 只有 `NEAR-ALONG` 一条, 最近距离 `2.515 km`. `GEO-LIKE` 停在 Apo/Peri 筛.

### 3. 一个窗口内的多次交会

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/CAT/CA_ComputeV5" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/close-approach/fixtures/ca-computev5-raan5-4h.json
```

2026-10-18 实测: 4 条, 时刻依次为 `14:41:45.060Z`, `15:32:31.701Z`, `16:23:29.363Z`, `17:14:16.005Z`, 距离在 `99.039 km` 与 `99.262 km` 之间交替, `CA_Theta=5`, `CA_DeltaV` 约 `0.648 km/s`.

## 本地快速验证

```bash
export BASE_URL=http://astrox.cn:8765
curl --silent --show-error --write-out '\nHTTP %{http_code}\n' \
  "${BASE_URL}/CAT/CA_ComputeV5" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/close-approach/fixtures/ca-computev5-min.json
```

成功判定: HTTP 200, `IsSuccess` 为 `true`, `CA_Results` 中有一条 `CA_MinRange` 约 `2.5` (km) 的记录.
