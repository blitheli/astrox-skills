---
name: rocket-trajectory-optim
description: 火箭方案弹道优化(级模型),用于基于实际火箭总体参数、飞行时序和优化 Profiles 计算入轨弹道、关键点、全程时序和子级落点。默认 Web API 为 http://www.astrox.cn:8764。
---

# 火箭方案弹道优化技能 (Rocket Trajectory Optim)

## 核心指令 (Core Instructions)

1. **输入解析**:识别用户提供的火箭型号、发射点、目标轨道、级质量、发动机参数、飞行时序和优化配置。
2. **默认服务**:本技能默认使用 `http://www.astrox.cn:8764`,不同于仓库全局默认服务。
3. **型号判定**:`RocketInput` 必须包含 `$type`,当前支持 `CZ-2D`、`CZ-4B`、`CZ-4C`。
4. **API 调用逻辑**:向 `{BASE_URL}/Rocket/TrajectoryOptim` 发送 `POST`,`Content-Type: application/json`。
5. **运行模式**:
  - `RunProfiles=false`:按 `RocketInput` 当前参数仅运行一次完整弹道。
  - `RunProfiles=true`:运行 `Profiles` 中的优化文件,并返回更新后的 `Profiles`。

## API 规范 (Tool Definition)

### 接口地址

`POST /Rocket/TrajectoryOptim`

### 默认服务

`http://www.astrox.cn:8764`

### 输入参数结构 (JSON)

| 参数名 | 类型 | 必须 | 说明 |
| --- | --- | --- | --- |
| `RocketInput` | object | 是 | 火箭弹道输入参数,首字段使用 `$type` 标记型号 |
| `Profiles` | array | 否 | 优化文件集合,仅计算时可为空数组 `[]` |
| `RunProfiles` | boolean | 否 | 是否运行优化 Profiles,默认 `true` |
| `GetAllData` | boolean | 否 | 是否返回所有点弹道参数,默认 `false` |
| `GetKeyData` | boolean | 否 | 是否返回特征点弹道参数,默认 `true` |

### `RocketInput` 通用字段

| 参数名 | 类型 | 必须 | 单位/说明 |
| --- | --- | --- | --- |
| `$type` | string | 是 | `CZ-2D`、`CZ-4B` 或 `CZ-4C` |
| `Name` | string | 否 | 方案名称 |
| `Text` | string | 否 | 方案说明 |
| `Name_FaSheDian` | string | 否 | 发射点名称 |
| `FaSheDianLLA` | number[] | 否 | 发射点经度(deg)、纬度(deg)、高度(m),未给发射点名称时使用 |
| `Gw` | number | 是 | 有效载荷质量(kg) |
| `FairingMass` | number | 是 | 整流罩质量(kg) |
| `A0` | number | 否 | 发射方位角(deg),未给时服务可按目标倾角估初值 |
| `T1` | number | 是 | 转弯开始时刻(s) |
| `Alpham` | number | 是 | 大气飞行段最大攻角(deg) |
| `Sm` | number | 是 | 一级飞行段气动面积(m^2) |
| `sma0` | number | 是 | 目标轨道半长轴(m) |
| `ecc0` | number | 是 | 目标轨道偏心率 |
| `inc0` | number | 是 | 目标轨道倾角(deg) |
| `omg0` | number | 是 | 目标轨道近地点幅角(deg) |

### `RocketInput` 型号字段摘要

| 型号 | 级数 | 主要附加字段 |
| --- | --- | --- |
| `CZ-2D` | 二级 | `Stage1_Mass`、`Stage1_FuelMass`、`Stage2_Mass`、`Stage2_FuelMass`、`Stage1_Engine`、`Stage2_MainEngine`、`Stage2_VernierEngine`、`Tk_1`、`Tk_F`、`Tk_2z`、`Tk_2u` |
| `CZ-4B` | 三级 | CZ-2D 通用字段加 `Stage3_Mass`、`Stage3_FuelMass`、`Stage3_Engine`、`Dt_k23f`、`Tk_3`、`Dt_msxz`、`Dt_xjfl` |
| `CZ-4C` | 三级二次工作 | CZ-4B 通用字段加 `Dt_hx`、`Tk_3b`、`Phicx_DotHx` |

### 发动机字段

| 参数名 | 类型 | 必须 | 单位/说明 |
| --- | --- | --- | --- |
| `Name` | string | 否 | 发动机名称 |
| `NumberOfEngines` | integer | 否 | 发动机台数,默认 1 |
| `CantAngle` | number | 否 | 安装偏角(deg),默认 0 |
| `Force` | number | 是 | 单台发动机额定推力(N) |
| `Ips` | number | 是 | 单台发动机额定比冲(m/s) |
| `Sa` | number | 是 | 单台发动机喷口面积(m^2) |
| `IsVacuum` | boolean | 否 | 是否真空段发动机 |
| `ThrustThrottling` | number[] | 否 | 推力节流序列 `[t1,F_th1,t2,F_th2,...]` |
| `IpsThrottling` | number[] | 否 | 比冲节流序列 `[t1,Ips_th1,t2,Ips_th2,...]` |

### 优化 Profile

`Profiles` 中的优化项使用 `$type: "AlglibOptimizer"`。

| 参数名 | 类型 | 说明 |
| --- | --- | --- |
| `IsActive` | boolean | 本控制文件是否有效 |
| `IsIterate` | boolean | 是否迭代优化,false 时按当前自变量计算 |
| `DiffStep` | number | 数值差分步长(归一化) |
| `EpsX` | number | 自变量收敛精度(归一化) |
| `StepMax` | number | 自变量最大步长(归一化,0 表示不限制) |
| `MaxIts` | integer | 最大迭代次数,0 表示不限制 |
| `Controls` | array | 自变量表格 |
| `Results` | array | 约束或目标函数表格 |

### 响应数据结构

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `IsSuccess` | boolean | 是否计算成功 |
| `Message` | string | 结果信息或失败原因 |
| `Profiles` | array | 优化更新后的 Profiles |
| `DicShiXu` | object | 飞行时序,包含每段开始时刻数据 |
| `DicAllData` | object | 所有点弹道参数,仅 `GetAllData=true` 时返回 |
| `DicKeyData` | object | 特征点弹道参数,每段首末状态 |
| `DicZJLD` | object | 子级落点数据 |

## 注意事项

- 目标轨道半长轴 `sma0` 使用米(m),不要使用千米(km)。
- 推力单位为 N,质量单位为 kg,比冲单位为 m/s,时序单位为 s。
- `RocketInput.$type` 必须与型号字段匹配,例如 `CZ-2D` 不应传三级字段。
- 仅需快速弹道验证时,优先设置 `RunProfiles=false`,`Profiles=[]`。
- `GetAllData=true` 返回数据量较大,只在需要完整剖面分析时启用。
- 优化失败时优先查看 `Message`、`Profiles[].OptimTerminationType` 和 `Profiles[].OptimFG`。

## 标准执行流程

1. 参数预检
  - 检查 `RocketInput.$type` 是否存在且为支持型号。
  - 检查质量、推力、比冲、时长、目标半长轴均为正数。
  - 检查 `FaSheDianLLA` 长度为 3,单位为 deg、deg、m。
2. 请求构造
  - 默认 `BASE_URL=http://www.astrox.cn:8764`。
  - 按接口契约原样传参,不做单位隐式转换。
  - 快速验证时使用 `RunProfiles=false`、`GetKeyData=true`。
3. 结果判定
  - 先判 HTTP 状态,再判 `IsSuccess`。
  - `IsSuccess=false` 时返回 `Message` 和优化收敛信息。
4. 输出归一化
  - 给出型号、发射点、目标轨道、运行模式、执行状态。
  - 若存在 `DicShiXu`、`DicKeyData`、`DicZJLD`,摘要输出关键时序、末状态和落点。

## 调用示例(CZ-2D 轨道优化)

```bash
export BASE_URL=http://www.astrox.cn:8764
curl "${BASE_URL}/Rocket/TrajectoryOptim" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/rocket-trajectory-optim/fixtures/trajectory-optim-cz2d-optim.json"
```

## 更多示例与测试数据(fixtures)

| 文件 | 用途简述 |
| --- | --- |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-cz2d-optim.json` | CZ-2D LEO 轨道优化样例 |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-cz4b-optim.json` | CZ-4B 700km SSO 轨道优化样例 |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-cz4c-optim.json` | CZ-4C 800km SSO 轨道优化样例 |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-cz2d-optim.json` | CZ-2D LEO 入轨优化样例,运行 AlglibOptimizer Profiles |
