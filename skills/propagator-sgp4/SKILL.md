---
name: propagator-sgp4
description: 通过 Astrox WebAPI 的 POST /Propagator/sgp4,依据两行根数 TLE 使用 SGP4 模型外推星历,输出 CzmlPositionOut(CZML 结构的位置序列)。当用户需要 SGP4、TLE、两行根数、NORAD 星历、近地轨道 TLE 递推时使用。
---

# SGP4 轨道递推技能 (SGP4 Propagator)

## 核心指令 (Core Instructions)

1. **输入解析**:识别用户提供的 TLE 两行(Line1、Line2)、分析时间窗 `Start`/`Stop`,以及可选的采样步长 `Step`、卫星编号 `SatelliteNumber`。
2. **TLE 格式**:`TLEs` 为**恰好两个字符串**的数组:`[line1, line2]`,与 NORAD 标准两行根数一致(含行首的 `1`  / `2`  及校验位等,按用户给出的原文传递)。
3. **时间**:`Start`、`Stop` 为 UTC,推荐 `yyyy-MM-ddTHH:mm:ssZ`(或接口可接受的 ISO8601 变体)。
4. **API 调用逻辑**:向 `{BASE_URL}/Propagator/sgp4` 发送 `POST`,`Content-Type: application/json`。若无特别指定,可用 `curl`;亦可用与用户环境一致的 HTTP 客户端(如 `Invoke-RestMethod`、Python `requests`)。

**注意**:OpenAPI 中该路径为 `**/Propagator/sgp4`**(`sgp4` 为小写),与 `/Propagator/TwoBody` 的命名风格不同,调用时请与规范一致。

## API 规范 (Tool Definition)

### 接口地址

`POST /Propagator/sgp4`

### 输入参数结构 (JSON) — `Sgp4Input`


| 参数名               | 类型              | 必须  | 说明                                     |
| ----------------- | --------------- | --- | -------------------------------------- |
| `Start`           | string          | 是   | 分析开始时刻 (UTCG),如 `yyyy-MM-ddTHH:mm:ssZ` |
| `Stop`            | string          | 是   | 分析结束时刻 (UTCG),如 `yyyy-MM-ddTHH:mm:ssZ` |
| `TLEs`            | array of string | 是   | 两行 TLE:`["tle-line1", "tle-line2"]`    |
| `Step`            | number | null   | 否   | 输出步长 (s);为 `null` 时由服务按目标自动设置          |
| `SatelliteNumber` | string | null   | 否   | 卫星 SSC number(目录号),可与 TLE 内编号一致以便核对    |


### 响应数据结构(CzmlPositionOut)

详见 `shared-docs/api-schemas/CzmlPositionOut.md`

## 注意事项

- `TLEs` 长度必须为 **2**(第一行、第二行各一个字符串);不要把两行合并成一个带 `\n` 的字符串,除非接口另有说明(本接口 schema 为 `items: string` 的数组)。
- SGP4 适用于 TLE 所定义的近地/深空模型语境;超出 TLE 有效范围或格式错误时,`IsSuccess` 可能为 false,应阅读 `Message`。
- `Step` 与二体接口类似,单位为秒;省略或置 `null` 时使用服务端默认策略。

## 标准执行流程

1. **参数预检**
  - `Start`、`Stop`、`TLEs` 必填
  - 检查 `Start < Stop`
  - 检查 `TLEs.length === 2`,且每行非空
2. **请求构造**
  - 按接口契约原样传参,不修改 TLE 内容(除非用户要求修正格式)
  - 需要固定采样间隔时设置 `Step`;否则可传 `null` 或不传(以服务端行为为准)
3. **结果判定**
  - 先判 HTTP 状态,再判 `IsSuccess`
  - `IsSuccess = false` 时优先返回 `Message`
4. **输出归一化**
  - 给出关键输入摘要(时间窗、步长策略、卫星号)、执行状态、核心输出、限制说明

## 调用示例(最小可运行)

**说明**:将 `BASE_URL` 换成实际服务根地址。PowerShell 可用:`$env:BASE_URL='http://...'`,再对 `curl.exe` 使用同一 URL 拼接。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/sgp4" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Start": "2021-05-01T00:00:00Z",
    "Stop": "2021-05-01T06:00:00Z",
    "Step": 60,
    "SatelliteNumber": "25730",
    "TLEs": [
      "1 25730U 99025A   21120.62396556  .00000659  00000-0  35583-3 0  9997",
      "2 25730  99.0559 142.6068 0014039 175.9692 333.4962 14.16181681132327"
    ]
  }'
```

## 本地快速验证(可选)

用 fixture 可避免 PowerShell 下行内 JSON 转义问题:

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/sgp4" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@propagator-sgp4/fixtures/sgp4-min.json"
```

(路径相对于仓库根目录;若在 `propagator-sgp4` 目录下执行,请改为 `@fixtures/sgp4-min.json`。)

## 更多示例与测试数据(fixtures)


| 文件                                       | 用途简述                                     |
| ---------------------------------------- | ---------------------------------------- |
| `propagator-sgp4/fixtures/sgp4-min.json` | 与 OpenAPI 默认 TLE 一致的 6 h 窗口 + `Step: 60` |


从响应中取**末时刻**位置速度:按 `shared-docs/api-schemas/CzmlPositionOut.md` 中 `cartesianVelocity` 平铺格式,取最后一组 `[t, X, Y, Z, Vx, Vy, Vz]`。