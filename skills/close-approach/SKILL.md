---
name: close-approach
description: use this when the user needs close-approach screening between one SGP4 satellite and TLE targets (近距离交会, 最近距离, TEME 相对速度). Calls POST /CAT/CA_ComputeV5. V5 does not compute collision probability.
---

# 近距离交会技能 (Close Approach / CA_ComputeV5)

用 SGP4 比较一颗主星和一组 TLE 目标, 在给定时间窗内筛选近距离交会. 主星和目标都在地球 TEME 下传播. 流程是 Apo/Peri 径向包络筛, 再做 `CrossTimePreFilter5` 距离筛, 然后对每一段距离 Access 各出一条结果. 一条 `CA_Results` 是一次接近, 不是一对卫星. 同一对可以有多条, 没有条数上限.

字段契约以网站 OpenAPI (`http://astrox.cn:8765/openapi/v1.json`, 版本 `2026-10-12`) 为准, 已同步到 `astrox-web-api.json` 的 `/CAT/CA_ComputeV5`, `CAInput5`, `CAOutput5`. 算法语义对照私有仓库 `blitheli/ASTROX.AeroSpace` 的 `master` `7af74e3` (实现提交 `44877a4`, 同一颗次星的多次接近都保留). 入口是 `ASTROX.WebApi/Controllers/CATController.cs` 的 `CA_ComputeV5`, 实现是 `ASTROX.AeroSpace/CAT/CAT.CA_ComputeV5.cs`. Scalar / OpenAPI 操作说明仍可能写成一对一条, 以源码和本技能为准. 不一致处见文末「已知限制/注意事项」. 不改快照里的网站原文.

## 核心指令 (Core Instructions)

1. **输入解析**: 必填 `Start_UTCG`, `Stop_UTCG`, `SAT1`. 传播只用两行 TLE; `SAT_Name` 写入根数名称. `SAT_Number` 算法不读. `Targets` 省略或为 `[]` 时走卫星库 `stkSatDbAll` 全库.
2. **参数校验**: 时间用 ISO8601 UTC, 且 `Start_UTCG` < `Stop_UTCG`. 距离阈值单位是 km. `TolMaxDistance` 是最终 Access 上限. Filter5 的放行门是 `TolMaxDistance + TimeDistPad`, 过门后继续扫, 不会停在第一扇窗.
3. **调用逻辑**: `POST {BASE_URL}/CAT/CA_ComputeV5`, `Content-Type: application/json`. 先看 HTTP 状态, 再看 `IsSuccess`. JSON 属性名保持 C# 原名.
4. **结果读取**: 同一对卫星在数组里可以连续出现多条, 按该对 Access 区间起点升序. 不同次星之间的顺序不保证. 不要把 `CA_Probability` 当成碰撞概率, V5 不计算, 恒为 `0`. `CA_Theta` 是该条时刻两速度方向的夹角, 不是轨道面夹角.

## API 规范 (Tool Definition)

### 接口地址

`POST /CAT/CA_ComputeV5`

默认服务根地址见 `claude.json` 的 `defaultServer` (`http://astrox.cn:8765/`).

网站摘要写「取代 V3, 轨道面夹角缺省 1°, TimeFilter 改为距离判断且并行, `Targets` 可为 null 则读库」. 源码里 V3 路由仍在. `TolTheta` 的缺省值就是 `1` deg, 不是从 3° 再改一次.

### 输入参数结构 (CAInput5)

运行时缺省来自属性初始化器. OpenAPI / `[DefaultValue]` 不会在反序列化时补上省略字段.

| 参数名 | 类型 | 必须 | 默认值 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| `Start_UTCG` | string | 是 | 无 | 分析开始 (UTC). 注释格式 `yyyy-MM-ddTHH:mm:ss.fffZ`, 用 `GregorianDate.Parse` |
| `Stop_UTCG` | string | 是 | 无 | 分析结束 (UTC), 格式同上 |
| `TolMaxDistance` | number | 否 | `5` | 距离上限 (km). Access 上限是该值换算成米, 下限 0, 不含 `TimeDistPad`. 字段 XML 仍写「Filter5 紧门早退阈值」, 实现已去掉早退 |
| `ApoPeriPad` | number | 否 | `30` | 径向包络垫量 (km). 实际每侧外扩 `(ApoPeriPad + TolMaxDistance)` km. 没有负值钳制 |
| `TimeDistPad` | number | 否 | `30` | 只放宽 Filter5. 放行门是 `TolMaxDistance + TimeDistPad` (km). `< 0` 时在进入 Filter5 前改成 `30`. 不放宽最终 Access |
| `UseApoPeriFilter` | boolean | 否 | `true` | `true` 时做主星 300 s 地心距扫描, 次星 T0/T1 端点包络, 以及 `CheckApoPeri`. `false` 时这三步都跳过. 同编号跳过和 TLE 解析失败仍然发生 |
| `UseTimeOrCrossFilter` | boolean | 否 | `true` | `true` 时对 Apo 通过者并行跑 `CrossTimePreFilter5`, 并保留多段窗. `false` 时不跑 Filter5, 只放一整段 `[T0, T1]`, 再由 Access 按距离拆成多条 |
| `TolTheta` | number | 否 | `1` | 单位 deg. 只用于近反共面整窗放行: 轨道面夹角 `theta > π - 3*TolTheta` (弧度) 时 Filter5 直接通过并保持 `[T0, T1]`. 缺省 1° 时大约是面夹角大于 177°. 不参与最终 `CA_Theta`. 近 0° 没有对应旁路 |
| `SAT1` | TleInfo | 是 | 无 | 主星. SGP4, 参考系地球 TEME |
| `Targets` | TleInfo[] | 否 | 空数组 | 次星. `null` 或长度 0 (含 JSON 省略和显式 `[]`) 都从库 `stkSatDbAll` 读取. 路径字符串是 `Data\DataBases\Satellite`. 跳过没有 TLE 的条目. 主星不在读库时预先剔除 |

`TleInfo` (`ASTROX.AeroSpace/CAT/TleInfo.cs`):

| 参数名 | 类型 | 算法是否读取 | 说明 |
| :--- | :--- | :--- | :--- |
| `SAT_Name` | string | 是 | 写成 `TwoLineElementSet.Name`. `[DefaultValue("ISS")]` 不会在反序列化时填上 |
| `SAT_Number` | string | 否 | 算法和 `ToTLE()` 都不读. `[DefaultValue("25544")]` 同样不参与运行时. 输出编号来自两行根数解析出的 NORAD 编号 |
| `TLE_Line1` | string | 是 | SGP4 第一行. 构造根数时默认不校验校验和 |
| `TLE_Line2` | string | 是 | 第二行 |

源码没有在这四个字段上标 `[Required]`. 主星两行解析失败会使整单 `IsSuccess=false`. 次星两行解析失败记入 Apo 拒绝并跳过该星. WebApi 是否把非空 `string` 当成隐式必填并返回 HTTP 400, 源码调研标为不确定, 见待确认. 2026-10-18 对空的 `SAT1` 实测为 HTTP 400, 错误列出 `SAT_Name`, `SAT_Number`, `TLE_Line1`, `TLE_Line2`.

### 处理流程 (源码)

1. **准备**: 解析起止时刻. 需要时读 `stkSatDbAll`. 主星建 SGP4, 在 `T0` 和 `T1` 各求值一次, 失败抛 `"sc已坠落！"`, 整单失败.
2. **Apo/Peri** (`UseApoPeriFilter==true`):
   - 主星 `ComputePerigeeApogee`, 步长 300 s. 量是地心距 (m), 不是海拔. 只在 `r·v` 变号处用 Brent 求极值 (时间容差 0.1 s), 并看最后一个样本. 步长小于约半个轨道周期才不容易漏变号; 300 s 对 LEO 满足.
   - 次星不是 300 s, 也不是 900 s. 只在 `T0` 和 `T1` 各取一次瞬时开普勒: `rp=a(1-e)`, `ra=a(1+e)`, `μ=3.986004418e14` m³/s². 包络是两端 `rp` 的较小者和 `ra` 的较大者, 不含窗中点. `e<0` 或 `e>=1` 则剔除.
   - `pad_m = (ApoPeriPad + TolMaxDistance) * 1000`. 主星和次星的地心距区间都向外扩这一段. 两段间隙小于 `2 * (ApoPeriPad + TolMaxDistance)` km 时判为重叠.
   - TLE 行内 NORAD 编号与主星相同则 `continue`, 不计入 Apo 拒绝, 三个计数字段都不因此减小.
3. **Filter5** (`UseTimeOrCrossFilter==true`): 并行. 放行门仍是 `TolMaxDistance + TimeDistPad` (km). 非正或 NaN 时退回 `TolMaxDistance`, 再不行就 5 km. `TolTheta` 只打开近反共面旁路 (`theta > π - 3*TolTheta`), 近 0° 不整窗放行. 分析窗内每次过门都留一扇窗: 该时刻前 5 min 到后 5 min, 再钳进 `[T0, T1]`. 两周期差小于 30 s 时, 这扇窗接到 `T1` 并停止继续扫交点, 后面交给 Access 拆段. 没有「第一次进入 `TolMaxDistance` 就返回」. 扫完用 `MergeFilter5Windows` 把重叠或相接的窗合成一段. 近 180° 旁路仍是整段 `[T0, T1]` 一窗. 粗跳仍在: 远距时最多再跳 2 个半圈, 可能跨过一次接近. 诊断串是 `pass:multi nWin=…`.
4. **Access**: 对合并后的每一段窗分别按距离约束 `[0, TolMaxDistance]` km (换算成米, 不含 `TimeDistPad`) 求满足区间. 重叠或端点相接的满足区间再合成一段. 每一段合成后的区间做一次极值搜索, 最多出一条 `CA_Results`. 一段里多个极小值只留距离更小的那次. 没有报出极小值, 或最小距离小于 0, 这一段丢弃, 其它段仍可加入. 不再因为没找到极小值就用 `JulianDate.MinValue` 去求速度. 没有条数上限, 也不再判断极小值是否 `<= TolMaxDistance` 才收录 (区间本身来自距离约束的满足段). 全平坦距离时探索器会不会报出极小值, 待确认; 报不出就不会加入. V5 不采样区间端点或中点, 也没有 `ComputeMinRange` 回退.

单颗次星的解析, 端点, Filter5, Access 异常被吃掉, 不让整单失败. 未捕获异常则 `IsSuccess` 保持 `false`, `Message` 为异常信息加堆栈.

### 响应数据结构 (CAOutput5)

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `IsSuccess` | boolean | 成功路径末尾写 `true`. 未捕获异常时保持 `false` |
| `Message` | string | 成功为 `OK`. 失败为异常文本 |
| `TotalNumber` | integer | 读库之后的 `Targets.Length`, 含随后被同编号跳过的星. 不是事件数 |
| `AfterApoPeriFilterNumber` | integer | `TotalNumber - apoRejected`. 减去的是两行解析失败, 端点包络失败, `CheckApoPeri` 不重叠, 以及 Apo 循环里的其它异常. 同编号跳过不减. 比较的是地心距, 不是海拔 |
| `AfterTimeFilterNumber` | integer | 再减去 Filter5 返回 false 或抛错的次数. Access 失败, 0 段, 精搜抛错都不减. 同编号跳过也不减. V5 拒绝主因是距离门, 不是面夹角门 |
| `CA_Results` | CAResultInfo[] | 成功时为数组, 可以是空数组. 一条是一次接近. 同一对可多条, 无条数上限. OpenAPI 文字仍像一对一条 |
| `TimingPrepareMs` | integer | 墙钟 (ms): 解析窗口, 可选读库, 主星 SGP4, T0/T1 检查, 以及主星 300 s Apo 扫描 (开关打开时), 主星 `Platform` |
| `TimingApoPeriMs` | integer | 次星串行循环墙钟: `ToTLE`, 同编号跳过, 端点包络, `CheckApoPeri`. 不含主星 300 s 扫描 |
| `TimingTimeOrCrossMs` | integer | 并行 Filter5 的墙钟. 开关为 false 时仍会计这段循环 |
| `TimingAccessMinRangeMs` | integer | 串行 Access 加各段极值搜索的墙钟 |
| `TimingTotalMs` | integer | 从进入方法到返回的墙钟. 失败路径也会写. 四段相加接近总时间, 不必逐毫秒相等 |

### CAResultInfo (`CA_Results` 元素)

V5 复用 V3 的 `CAResultInfo`. 下表是 V5 的实际赋值, 不是字段注释的字面意思.

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| `SAT1_Name` / `SAT2_Name` | string | 来自 `TleInfo.SAT_Name` (写入根数 `Name`) |
| `SAT1_Number` / `SAT2_Number` | string | 来自两行根数解析出的 NORAD 编号, 不是 JSON 的 `SAT_Number` |
| `SAT*_TLE_Line1` / `SAT*_TLE_Line2` | string | 参与传播的两行 |
| `CA_MinRange_Time` | string | 该段 Access 上更近的那次极小值时刻, `yyyy-MM-ddTHH:mm:ss.fffZ`. 同一对多条按区间起点升序, 不是按距离从小到大, 也不是整窗全局最小 |
| `CA_MinRange` | number | 该次极小距离, `Math.Round(..., 3)`, 单位 km. TEME 下两颗 SGP4 位置差的模. 不是 Filter5 的线性距离 |
| `CA_Theta` | number | 该条自己的时刻上, 两速度方向夹角 (deg), 保留 3 位小数: `round(acos(normalize(V_sc)·normalize(V_tg))*180/π, 3)`. TEME. 多条不共用一个面夹角. Filter5 的面夹角在 `tg.Theta`, 不写入本字段 |
| `CA_DeltaV` | number | 该条自己的时刻上, TEME 惯性相对速度的大小 (km/s): `round(\|V_sc-V_tg\|*0.001, 6)`. 没有转到 RIC 或 VVLH |
| `CA_Probability` | number | V5 没有赋值, `double` 默认 `0`. 不算 Pc, 没有等效半径, 没有协方差. `0` 表示没算, 不是算出来的概率 0 |

只改平近点角时 `CA_Theta` 会接近所改的角度差, 例如改 `0.02°` 就显示 `0.02`. 倾角和升交点赤经不变时, 轨道面夹角是 0, 但 V5 不输出面夹角. 近圆轨道上速度方向随纬度幅角转动, 两速度夹角接近真近点角差, 也就接近平近点角差. `Math.Round(x, 3)` 把约 `0.0195°`–`0.0205°` 显示成 `0.02`. 源码里没有写死 `0.02`. 点积因舍入略大于 1 时 `acos` 得到的是 NaN, 不是 `0.02`, 所以这不是「面夹角本应是 0, 被噪声抬出来」.

## 已知限制/注意事项

网站 OpenAPI, C# 注释和 V5 实现不一致. `astrox-web-api.json` 保留网站原文, 调用时以下面的源码行为为准.

- `CA_Theta` 的字段注释和 OpenAPI 仍写「轨道面夹角 (deg)」. V5 写的是 TEME 速度方向夹角. V3 才把 Filter 的面夹角写入该字段. V4 用角动量方向夹角, V5 精搜不走那条路径.
- `CAInput5` 注释写面夹角接近 0° 和 180° 时 Filter5 整窗放行. 代码只在 `theta > π - 3*TolTheta` (近 180°, 且是 3 倍阈值) 时整窗放行. 近 0° 只是把交线纬度幅角置 0, 然后继续按距离扫. `TolTheta=0` 时这条旁路打不开.
- `UseApoPeriFilter`, `UseTimeOrCrossFilter` 的注释写「当前始终执行, 开关预留」. 代码会读这两个开关, 设成 `false` 会跳过对应筛.
- 动作摘要里的「轨道面夹角缺省为 3°->1°」是 V3 说明的口气. 字段缺省就是 `1.0` deg. 控制器上 V5 的说明写缺省 1°, 与字段一致.
- `CA_Probability` 注释写「碰撞概率, 1 为完全碰撞」. V5 不计算, 留下默认 `0`.
- `AfterApoPeriFilterNumber` 的说明写「高度过滤」. 比较的是地心距, 而且两侧各加 `(ApoPeriPad+TolMaxDistance)`.
- `AfterTimeFilterNumber` 的说明仍写「轨道面交点过滤后的总目标数」. V5 减少它的是 Filter5 距离门失败或异常. 面夹角只参与近 180° 旁路.
- `TimingTotalMs` 的说明仍写「CA_ComputeV3 总墙钟」. 类型已经是 V5 输出.
- `TimingApoPeriMs` 的说明写「含 T1 瞬时 dh」. V5 次星是 T0/T1 的瞬时 `rp`/`ra`. 主星 300 s 扫描的耗时在 `TimingPrepareMs`, 不在这项. `TimingPrepareMs` 的短说明也没写上这段扫描.
- 读库路径字符串和控制器说明都是 `Data\DataBases\Satellite` (`DataBases` 的 B 大写). 磁盘目录和 WebApi 拷贝项是 `Data/Databases/Satellite`. 别的接口曾因 Linux 大小写改成 `Databases`, V5 这条没改.
- `CrossTimePreFilter5` 文件头仍写「由 Filter3 更名, 未完成」, 同文件后文和 `changes.md` 已把距离门和粗跳写成已实现.
- Filter5 一处注释把求值异常说成「视为不过滤通过」, 括号里才是代码行为: `Pass` 保持 false, 也就是不通过.
- 控制器类注释「V5 取代 V3」只是加了动作. `CA_ComputeV3` 仍注册在路由上. V3 仍是只探索第一段 Access, 每对一条.
- Scalar 操作说明 (`EndpointSummary` / `EndpointDescription`) 没有写「同一对可多条」. 提交 `44877a4` 没改控制器. 源码 XML 摘要已经写成每个距离区间一条, 但工程不生成 XML 文档, 这段摘要默认进不了 Scalar. `CA_Results` 的特性文本仍是「所有碰撞结果, 一个对象为两两碰撞结果」. 调用方以本技能和源码为准, 不要以当前 Scalar 操作说明为准.
- `TolMaxDistance` 的 XML 和 `CrossTimePreFilter5` 的一处参数说明仍写「紧门早退阈值」. 同文件后文写明不再在第一次进入 `TolMaxDistance` 后退出. 以实现为准: 每次过门留窗, 没有早退.
- 仓库里部分 LEO50 对照笔记仍写「第一段 Access」. 那是 `44877a4` 之前的行为.
- STK 的 `apoPeriPad` 是否也在两侧各加一次 pad, 本仓库没有 STK 源码, 待确认. 能确定的是 V5 的 `CheckApoPeri` 两侧都外扩.

调用时另外注意:

- 距离用 km, 速度用 km/s, 角度用 deg. 不要把 `TolMaxDistance` 当成米. 源码在 Access 和 pad 处自行乘 1000.
- 时间窗要盖住 TLE 历元附近. 主星样例与 `skills/propagator/fixtures/sgp4/sgp4-min.json` 同为行内 NORAD `25730`.
- 不带毫秒的 `yyyy-MM-ddTHH:mm:ssZ` 在 2026-10-18 也可被接受. 仍建议写成 `.fffZ`.
- `Start_UTCG >= Stop_UTCG` 时, 实测仍可能 HTTP 200 且 `IsSuccess=true`, 结果为空. 调用方自己检查时间顺序.
- 用户没明确要求扫全库时, 不要省略 `Targets`. 省略和 `[]` 都会读 `stkSatDbAll`. 2026-10-18 一次 10 分钟窗口实测 `TotalNumber=34725`, 该数字随库变化.
- 三个计数可以大于真正进入下一阶段的次星数, 差额至少包括同编号跳过. 事件数是 `CA_Results` 的长度, 可以多于次星数, 也可以更少.
- 同一颗次星的多条在数组里连续, 按距离区间起点升序. 不同次星来自 `ConcurrentBag`, 不是输入顺序, 也不是按编号或时刻的全局顺序. 接口不按时刻配对; 测试对照 STK 时用的 20 分钟就近配对不是接口规则.
- 少出事件的原因包括: Filter5 粗跳跨过一圈, 周期差小于 30 s 时停止继续扫交点, 重叠或相接的窗被合成一段, 该段没有极小值, 或单星异常被吃掉. 这些都不是条数上限.
- `99901`, `99902`, `99941` 是本技能构造的行内编号, 不是在轨目标. 近目标只把平近点角从 `333.4962` deg 改为 `333.5162` deg. 近圆, 地心距约 7000 km 时, 0.02° 相位差对应沿迹约 2.4 km, 落在缺省 5 km 门内. `GEO-LIKE` 把平均运动改成 `1.00273791` rev/day, 用来触发 Apo/Peri 筛除.
- 两行根数完全相同却没有事件, 分两种:
  - 只改 JSON 的 `SAT_Number`, 两行字符串不变: 解析出的 NORAD 相同, Apo 阶段直接跳过, 不跑 Filter5 和 Access. 三个计数都不减小. 若只有这一颗次星, 计数是 `1 / 1 / 1`, 事件数是 0.
  - 两行里的编号也改了, 轨道根数仍相同: 不会被同编号跳过. 短窗可能在 Filter5 飞不到假交点 (面夹角 `< 1e-10` rad 时交线幅角被置 0, 并不因此放行), 从而 `AfterTimeFilterNumber` 减少. 窗足够长时 Filter5 会因为距离 0 通过. 精搜的距离函数恒为 0. 若探索器不报告极小值, 该段直接丢弃, 不再用 `JulianDate.MinValue` 求速度. 全平坦函数是否一定不报极小值, 待确认. 2026-10-18 用 4 小时窗, 行内编号改成 `99913`, 根数与主星相同, 计数 `1 / 1 / 1` 且事件数为 0.

## 标准执行流程

1. 参数预检: `Start_UTCG`, `Stop_UTCG`, `SAT1` 的两行 TLE. 确认 `Start < Stop`, 距离单位是 km. `SAT_Number` 可填, 但不要靠它区分目标; 区分看两行里的 NORAD 编号.
2. 目标列表: 用户没给目标时不要省略 `Targets`, 先问清楚. 只有用户明确要求对 `stkSatDbAll` 全库筛选时才省略.
3. 请求构造: 阈值缺省为 `TolMaxDistance=5` km, `ApoPeriPad=30` km, `TimeDistPad=30` km, 两个筛开关 `true`, `TolTheta=1`. 记住 pad 只放宽筛, 不放宽最终 Access.
4. 结果判定: HTTP 200 后读 `IsSuccess`. 失败看 `Message`. HTTP 400 是进入算法前的校验失败, 没有 `IsSuccess`.
5. 输出归一化: 报告窗口, 阈值, 三级计数, 以及每一条接近的时刻, `CA_MinRange` (km), `CA_DeltaV` (TEME 相对速度大小, km/s), `CA_Theta` (该条时刻的速度方向夹角, deg). `CA_Probability` 固定按「未计算」报告, 不要写成碰撞结论. 同一对按多条接近列出, 不要合并成一行. 不要假设不同次星的全局时间顺序.

## Fixtures 目录

| 文件 | 说明 |
| :--- | :--- |
| `ca-computev5-min.json` | 必填时间加一对目标, 阈值走默认. 平近点角差 0.02°. 2026-10-18 实测 1 条, `CA_MinRange=2.515` km, `CA_Theta=0.02` |
| `ca-computev5-filter-4h.json` | 4 小时, 显式阈值. 近目标保留, `GEO-LIKE` 被 Apo/Peri 筛掉. 计数 `2 / 1 / 1` |
| `ca-computev5-raan5-4h.json` | 升交点赤经加 5°, `TolMaxDistance=200` km. 同一对 4 条接近 |

## 调用示例

在仓库根目录执行. `BASE_URL` 末尾不要重复斜杠.

### 1. 最小交会 (默认 5 km)

主星行内编号 `25730`, 目标平近点角大 `0.02` deg. `CA_Theta=0.02` 是两速度方向夹角, 与这次相位差一致, 不是轨道面夹角.

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

2026-10-18 实测: HTTP 200, `IsSuccess=true`, `Message=OK`, 计数 `1 / 1 / 1`, 一条结果 `CA_MinRange_Time=2021-04-30T17:22:21.541Z`, `CA_MinRange=2.515`, `CA_DeltaV=0.002583`, `CA_Theta=0.02`, `CA_Probability=0` (未计算).

### 2. 近目标加一个会被筛掉的高轨目标

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/CAT/CA_ComputeV5" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/close-approach/fixtures/ca-computev5-filter-4h.json
```

2026-10-18 实测: 计数 `2 / 1 / 1`, `CA_Results` 只有 `NEAR-ALONG` 一条, 最近距离 `2.515` km. `GEO-LIKE` 停在 Apo/Peri 筛.

### 3. 升交点赤经差 5°

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/CAT/CA_ComputeV5" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/close-approach/fixtures/ca-computev5-raan5-4h.json
```

5° 不是近反共面, `TolTheta` 不会把整窗放行. 每次过门留一扇约 ±5 min 的窗, Access 对合并后的每一段各出一条. 2026-10-19 再次调用: HTTP 200, `IsSuccess=true`, 计数 `1 / 1 / 1`, 同一对 4 条, 按时刻升序:

- `2021-04-30T14:41:45.060Z`, `99.039` km, `CA_Theta=5`, `CA_DeltaV=0.648568`
- `2021-04-30T15:32:31.701Z`, `99.262` km, `CA_Theta=5`, `CA_DeltaV=0.647112`
- `2021-04-30T16:23:29.363Z`, `99.039` km, `CA_Theta=5`, `CA_DeltaV=0.648572`
- `2021-04-30T17:14:16.005Z`, `99.262` km, `CA_Theta=5`, `CA_DeltaV=0.647109`

四条的 `CA_Probability` 都是 `0`. 这与「一条结果等于一次接近, 同一对可多条」一致.

## 本地快速验证

```bash
export BASE_URL=http://astrox.cn:8765
curl --silent --show-error --write-out '\nHTTP %{http_code}\n' \
  "${BASE_URL}/CAT/CA_ComputeV5" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/close-approach/fixtures/ca-computev5-min.json
```

成功判定: HTTP 200, `IsSuccess` 为 `true`, `CA_Results` 中有一条 `CA_MinRange` 约 `2.5` (km) 的记录, `CA_Probability` 为 `0`.

## 仍待确认

- WebApi 是否把 `TleInfo` 的非空 `string` (尤其 `SAT_Number`) 一律当成隐式必填. 算法不读 `SAT_Number`. 空 `SAT1` 在线上会 400, 缺字段的次星是否一律 400, 调研标为不确定.
- 距离函数全平坦时, 探索器是否一定不报告极小值. 报不出则该段不加入结果.
- Linux 上进程工作目录是否另有 `DataBases`, 使 `Data\DataBases\Satellite` 能对上磁盘的 `Databases`. 2026-10-18 的 `astrox.cn:8765` 省略 `Targets` 时读到了全库 (`TotalNumber=34725`), 不能外推到别的部署.
- STK `apoPeriPad + rangeConstraint` 是否也在两侧各加一次 pad.
