# Propagator 公共约定

轨道递推类 Propagator 端点（TwoBody、J2、HPOP、SGP4）共享以下约定。

## 输出结构

所有端点均返回 `CzmlPositionOut`，详见 [CzmlPositionOut.md](CzmlPositionOut.md)。

从响应中取**末时刻**位置速度:按 `cartesianVelocity` 平铺格式,取最后一组 `[t, X, Y, Z, Vx, Vy, Vz]`。

## 时间格式

- 首选 ISO8601 UTC: `yyyy-MM-ddTHH:mm:ssZ` 或 `yyyy-MM-ddTHH:mm:ss.fffZ`
- 部分 fixture 使用服务可接受的非 `Z` 结尾时间字符串;若接口校验只认 ISO8601,请改为 `Z` 结尾后再测

## 单位约定

| 量 | 单位 |
| --- | --- |
| 半长径、位置 | m |
| 速度 | m/s |
| 角度 | deg |
| 时间步长 | s |
| 引力常数 | m³/s² |

## 公共时间窗字段

根数类端点（TwoBody、J2、HPOP）通常包含:

| 字段 | 必须 | 说明 |
| --- | --- | --- |
| `Start` | 是 | 分析开始时刻 (UTCG) |
| `Stop` | 是 | 分析结束时刻 (UTCG) |
| `OrbitEpoch` | 是 | 轨道历元 (UTCG) |
| `Step` | 否 | 输出步长 (s), TwoBody/J2 默认 60 |

SGP4 端点使用 `TLEs` 而非 `OrbitalElements`,详见 [EntityPositionSGP4.md](EntityPositionSGP4.md)。

## 标准执行流程

1. **参数预检**
   - 检查必填字段完整性
   - 检查 UTC 时间格式
   - 检查 `Start < Stop`
   - 根数类:检查 `OrbitalElements` 长度必须为 6
   - SGP4:检查 `TLEs.length === 2`
2. **模型判定**
   - 有 TLE 两行 → 使用 `/Propagator/sgp4`,禁止走 J2/HPOP/TwoBody
   - 未指定 `CoordType` 时默认 `Classical`
   - 若给出速度分量则优先使用 `Cartesian`
3. **请求构造**
   - 按接口契约原样传参,不做单位隐式转换
   - 明确记录端点、`CoordType`、`CoordSystem`、`CentralBody`
4. **结果判定**
   - 先判 HTTP 状态,再判 `IsSuccess`
   - `IsSuccess = false` 时优先返回 `Message`
5. **输出归一化**
   - 给出关键输入摘要、执行状态、核心输出、限制说明

## 调用模板

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/{Endpoint}" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@propagator/fixtures/{type}/{fixture}.json"
```

(路径相对于仓库根目录。)
