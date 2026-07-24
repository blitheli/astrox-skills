---
name: rocket-trajectory-optim
description: 火箭方案弹道优化(级模型),用于基于实际火箭总体参数、飞行时序和优化 Profiles 计算入轨弹道、关键点、全程时序和子级落点。支持 CZ-2D/3B/4B/4C/8、Falcon9 与 TwoStage(如 CZ-12)。默认 Web API 为 http://astrox.cn:8764。
---

# 火箭方案弹道优化技能 (Rocket Trajectory Optim)

## 核心指令 (Core Instructions)

1. **输入解析**:识别用户提供的火箭型号、发射点、目标轨道、级质量、发动机参数、飞行时序和优化配置。
2. **默认服务**:本技能默认使用 `http://astrox.cn:8764`(见 `rocket-web-api.json`),不同于仓库全局 Astrox 默认服务。
3. **型号判定**:`RocketInput` 必须包含 `$type`,当前支持 `CZ-2D`、`CZ-3B`、`CZ-4B`、`CZ-4C`、`CZ-8`、`Falcon9`、`TwoStage`(CZ-12 等无独立类型的二级火箭用 `TwoStage`)。
4. **API 调用逻辑**:向 `{BASE_URL}/Rocket/TrajectoryOptim` 发送 `POST`,`Content-Type: application/json`。
5. **运行模式**:
   - `RunProfiles=false`:按 `RocketInput` 当前参数仅运行一次完整弹道。
   - `RunProfiles=true`:运行 `Profiles` 中的优化文件,并返回更新后的 `Profiles`。
6. **模板辅助(可选)**:可用 `GET /templates` 列出服务端示例方案,再用 `GET /templates/{filename}` 拉取完整请求体作为起点。

## API 规范 (Tool Definition)

### 接口地址

`POST /Rocket/TrajectoryOptim`

### 默认服务

`http://astrox.cn:8764`

契约来源:`rocket-web-api.json`(`Rocket Web API`, version `2026-07-24`)。

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
| `$type` | string | 是 | `CZ-2D` / `CZ-3B` / `CZ-4B` / `CZ-4C` / `CZ-8` / `Falcon9` / `TwoStage` |
| `Name` | string | 否 | 方案名称 |
| `Text` / `Text2` / `Text3` | string | 否 | 方案说明 |
| `Name_FaSheDian` | string | 否 | 发射点名称 |
| `FaSheDianLLA` | number[] | 否 | 发射点经度(deg)、纬度(deg)、高度(m);未给发射点名称时使用 |
| `Gw` | number | 是 | 有效载荷质量(kg) |
| `FairingMass` | number | 是 | 整流罩质量(kg) |
| `A0` | number | 否 | 发射方位角(deg);缺省哨兵值 `-1001` 时服务按目标倾角估初值 |
| `T1` | number | 是 | 转弯开始时刻(s) |
| `Alpham` | number | 是 | 大气飞行段最大攻角(deg) |
| `Sm` | number | 是 | 一级飞行段气动面积(m^2)(芯级+助推总横截面积) |
| `Sm2` | number | 否 | 二级飞行段(含子级)气动面积(m^2) |
| `SmFairing` | number | 否 | 整流罩坠落段气动面积(m^2) |
| `sma0` | number | 是 | 目标轨道半长轴(m) |
| `ecc0` | number | 是 | 目标轨道偏心率 |
| `inc0` | number | 是 | 目标轨道倾角(deg) |
| `omg0` | number | 是 | 目标轨道近地点幅角(deg) |

### `RocketInput` 型号字段摘要

| 型号 | 构型 | 主要附加字段 |
| --- | --- | --- |
| `CZ-2D` | 二级(主机+游机) | `Stage1/2_Mass`、`Stage1/2_FuelMass`、`Stage1_Engine`、`Stage2_MainEngine`、`Stage2_VernierEngine`、`Tk_1`、`Dt_k12f`、`Tk_F`、`Tk_2z`、`Tk_2u`、`Dt_xjfl`、`PhicxDot_2z`、`PhicxDot_2u`、`PsicxDot_2` |
| `CZ-3B` | 助推+三级(三级二次工作) | 助推:`Booster_*`、`NumberOfBooster`、`Booster_Engine`、`Tk_zt`、`Dt_ztf`;芯级时序:`Tk_1`、`Tk_F`、`Tk_2z`、`Tk_2u`、`Dt_k23f`、`Tk_3`、`Dt_hx`、`Tk_32`、`Dt_msxz`、`Dt_xjfl` 及对应程序角变化率 |
| `CZ-4B` | 三级一次工作 | `Stage1/2/3_*`、`Stage2_MainEngine`/`Stage2_VernierEngine`/`Stage3_Engine`、`Tk_1`、`Tk_F`、`Tk_2z`、`Tk_2u`、`Dt_k23f`、`Tk_3`、`Dt_msxz`、`Dt_xjfl`、`PhicxDot_2z`/`PsicxDot_2`/`PhicxDot_3`/`PsicxDot_3` |
| `CZ-4C` | 三级二次工作 | 在 CZ-4B 基础上用 `Dt_hx`、`Tk_32`(二次工作时长)、`Phicx_DotHx`;偏航程序角字段为 `PsicxDot_2z` |
| `CZ-8` | 助推+二级二次工作 | `Booster_*`、`NumberOfBooster`、`Booster_Engine`、`Stage1_Engine`、`Stage2_Engine`、`Stage2_Engine2`、`Tk_zt`、`Tk_1`、`Tk_F`、`Tk_2`、`Dt_hx`、`Tk_22`、`Dt_xjfl` 及 `PhicxDot_2/hx/22` 等 |
| `Falcon9` | 二级二次工作 | `Stage1/2_Mass`、`Stage1_Engine`、`Stage2_Engine`、`Stage2_Engine2`、`Tk_1`、`Dt_k12f`、`Dt_dh2`、`Tk_F`、`Tk_2`、`Dt_hx`、`Tk_22`、`Dt_xjfl` |
| `TwoStage` | 通用二级二次工作 | 与 `Falcon9` 同构;CZ-12 等无独立 `$type` 的二级火箭统一用本类型,详见下方专节 |

说明:

- 模板索引里可能出现 `CZ-8A` 名称,但其请求体 `$type` 仍为 `CZ-8`。
- CZ-4C 二次工作时长字段为 `Tk_32`,不要使用旧名 `Tk_3b`。
- CZ-12 **没有**独立 `$type`,请求中必须写 `"$type": "TwoStage"`;方案名可用 `Name`/`Text` 标注为 CZ-12。

### `TwoStage` 专节(以 CZ-12 为例)

`TwoStage` 是通用「一级 + 二级一次/二次点火」构型。典型飞行剖面:

起飞 → 一级关机(`Tk_1`) → 一二级分离(`Dt_k12f`) → 二级点火(`Dt_dh2`) → 抛罩(`Tk_F`) → 二级一次关机(`Tk_2`) → 滑行(`Dt_hx`) → 二级二次工作(`Tk_22`) → 星箭分离前滑行(`Dt_xjfl`)。

与 `Falcon9` 字段同构,区别主要在质量、发动机和时序数值。CZ-12 服务端模板:`CZ12_LEO_260722.json`、`CZ12_SSO_260722.json`。

#### TwoStage / CZ-12 专有字段

| 参数名 | 类型 | 单位/说明 | CZ-12 LEO 样例量级 |
| --- | --- | --- | --- |
| `Stage1_Mass` / `Stage1_FuelMass` | number | 一级总质量 / 推进剂质量(kg) | ~355t / ~333t |
| `Stage2_Mass` / `Stage2_FuelMass` | number | 二级总质量 / 推进剂质量(kg) | ~54t / ~48t |
| `Stage1_Engine` | object | 一级发动机(如 4×YF-100J) | `Force=1.25e6 N`,`Ips≈2960` |
| `Stage2_Engine` | object | 二级一次工作发动机(如 2×YF-115B) | `Force=1.8e5 N`,`IsVacuum=true` |
| `Stage2_Engine2` | object | 二级二次工作发动机(可与一次同型号) | 同上 |
| `Tk_1` | number | 一级关机时刻(s,从起飞) | ~196 |
| `Dt_k12f` | number | 一级关机 → 一二级分离(s) | ~2 |
| `Dt_dh2` | number | 一二级分离 → 二级点火(s) | ~2 |
| `Tk_F` | number | 二级点火 → 整流罩分离(s) | ~8.7 |
| `Tk_2` | number | 二级一次工作总时长(s,从二级点火;须大于 `Tk_F`) | ~423(常固定) |
| `Dt_hx` | number | 二级一次关机后滑行(s) | ~1000 |
| `Tk_22` | number | 二级二次工作时长(s) | ~18 |
| `Dt_xjfl` | number | 二次关机 → 星箭分离前滑行(s) | ~90 |
| `PhicxDot_2` / `PsicxDot_2` | number | 抛罩后一次工作段俯仰/偏航程序角变化率(deg/s) | 优化自变量 |
| `PhicxDot_hx` / `PsicxDot_hx` | number | 滑行段俯仰/偏航程序角变化率(deg/s) | 优化自变量 |

注意:

- `$type` 只能是 `TwoStage`,不要写成 `CZ-12`。
- 二级二次工作时长是 `Tk_22`,不要与 CZ-4C 的 `Tk_32` 混淆。
- 优化时常固定 `Tk_2`,调节 `Gw`、`A0`、`Alpham`、`PhicxDot_2`、`PhicxDot_hx`(必要时再开 `Tk_22`/`Dt_hx`)。

### 发动机字段 (`RocketEngine`)

| 参数名 | 类型 | 必须 | 单位/说明 |
| --- | --- | --- | --- |
| `Name` | string | 否 | 发动机名称 |
| `Text` | string | 否 | 发动机说明 |
| `NumberOfEngines` | integer | 否 | 发动机台数,默认 1 |
| `IsBooster` | boolean | 否 | 是否助推器,默认 `false` |
| `CantAngle` | number | 否 | 安装偏角(deg),默认 0 |
| `Force` | number | 是 | 单台发动机额定推力(N) |
| `Ips` | number | 是 | 单台发动机额定比冲(m/s),缺省常见为 3000 |
| `Sa` | number | 否 | 单台发动机喷口面积(m^2) |
| `IsVacuum` | boolean | 否 | 是否真空段发动机,默认 `false` |
| `ThrustThrottling` | number[] | 否 | 推力节流序列 `[t1,F_th1,t2,F_th2,...]`;实际推力=`Force*F_th` |
| `IpsThrottling` | number[] | 否 | 比冲节流序列 `[t1,Ips_th1,...]`;实际比冲=`Ips*Ips_th` |

### 优化 Profile

`Profiles` 使用 `$type` 区分优化器:

| `$type` | 说明 |
| --- | --- |
| `AlglibOptimizer` | ALGLIB 数值差分优化(常用) |
| `DifferentialCorrector` | 微分修正器 |

#### 公共字段

| 参数名 | 类型 | 说明 |
| --- | --- | --- |
| `Name` / `Text` | string | 控制文件名称/说明 |
| `IsActive` | boolean | 本控制文件是否有效 |
| `IsIterate` | boolean | `true`=运行优化;`false`=仅按当前自变量计算一次 |
| `Controls` | array | 自变量表格 |
| `Results` | array | 约束或目标函数表格 |

#### `AlglibOptimizer` 专有

| 参数名 | 类型 | 说明 |
| --- | --- | --- |
| `DiffStep` | number | 数值差分步长(归一化) |
| `EpsX` | number | 自变量收敛精度(归一化) |
| `StepMax` | number | 自变量最大步长(归一化,0 表示不限制) |
| `MaxIts` | integer | 最大迭代次数,0 表示不限制 |
| `OptimX` / `OptimFG` | array | 优化后自变量/目标与约束残差 |
| `Converged` | boolean | 是否收敛 |
| `OptimTerminationType` | integer | ALGLIB 状态码(`2` 正常收敛,`7` 精度达标,`-3` 约束不可行,`0` 未优化) |
| `IterationCount` / `FvecCount` | integer | 迭代次数 / Fvec 调用次数 |

#### `DifferentialCorrector` 专有

| 参数名 | 类型 | 说明 |
| --- | --- | --- |
| `MaxIts` | integer | 最大迭代次数 |
| `OptimX` / `OptimF` | array | 最优自变量 / 约束残差(归一化) |
| `Converged` / `IterationCount` | boolean / integer | 收敛标志与迭代次数 |

#### `Controls` / `Results` 行字段

| 表 | 关键字段 |
| --- | --- |
| `Controls` | `Use`、`Name`、`CurrentValue`、`LowerBound`、`UpperBound`、`Scale`、`Object` |
| `Results` | `Use`、`Name`、`CurrentValue`、`DesiredValue`、`Scale`、`Object`、`Goal`(`Equality`/`Minimize`/`Maximize`)、`DltFG` |

### 响应数据结构

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `IsSuccess` | boolean | 是否计算成功 |
| `Message` | string | 结果信息或失败原因 |
| `Profiles` | array | 优化更新后的 Profiles |
| `DicShiXu` | object | 飞行时序(每段开始时刻),含 `text`、`tt` 等数组 |
| `DicAllData` | object | 所有点弹道参数,仅 `GetAllData=true` 时返回 |
| `DicKeyData` | object | 特征点弹道参数(每段首末状态) |
| `DicZJLD` | object | 子级落点数据(`text`/`tt`/`tdf`/`mass`/`sm`/`d_L`/`lambda`/`d_B` 等) |

## 注意事项

- 目标轨道半长轴 `sma0` 使用米(m),不要使用千米(km)。
- 推力单位为 N,质量单位为 kg,比冲单位为 m/s,时序单位为 s,角度单位为 deg。
- `RocketInput.$type` 必须与型号字段匹配;助推相关字段仅 `CZ-3B`/`CZ-8` 使用。
- CZ-12 使用 `$type: "TwoStage"`,不要传 `CZ-12`;二次工作时长字段为 `Tk_22`。
- `A0=-1001` 表示未赋值;正式优化前建议给出合理初值。
- 仅需快速弹道验证时,优先设置 `RunProfiles=false`,`Profiles=[]`。
- `GetAllData=true` 返回数据量较大,只在需要完整剖面分析时启用。
- 优化失败时优先查看 `Message`、`Profiles[].Converged`、`Profiles[].OptimTerminationType` 和残差字段。

## 标准执行流程

1. 参数预检
   - 检查 `RocketInput.$type` 是否存在且为支持型号。
   - 检查质量、推力、比冲、时长、目标半长轴均为正数。
   - 检查 `FaSheDianLLA` 长度为 3,单位为 deg、deg、m。
   - CZ-4C 确认使用 `Tk_32`,而非旧字段 `Tk_3b`。
   - CZ-12 / 通用二级确认 `$type=TwoStage`,二次工作字段为 `Tk_22`(不是 `Tk_32`)。
2. 请求构造
   - 默认 `BASE_URL=http://astrox.cn:8764`。
   - 按接口契约原样传参,不做单位隐式转换。
   - 快速验证时使用 `RunProfiles=false`、`GetKeyData=true`。
3. 结果判定
   - 先判 HTTP 状态,再判 `IsSuccess`。
   - `IsSuccess=false` 时返回 `Message` 和优化收敛信息。
4. 输出归一化
   - 给出型号、发射点、目标轨道、运行模式、执行状态。
   - 若存在 `DicShiXu`、`DicKeyData`、`DicZJLD`,摘要输出关键时序、末状态和落点。

## 调用示例

### CZ-2D 轨道优化

```bash
export BASE_URL=http://astrox.cn:8764
curl "${BASE_URL}/Rocket/TrajectoryOptim" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/rocket-trajectory-optim/fixtures/trajectory-optim-cz2d-optim.json"
```

### TwoStage / CZ-12 LEO(900km / 50°)

```bash
export BASE_URL=http://astrox.cn:8764
curl "${BASE_URL}/Rocket/TrajectoryOptim" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/rocket-trajectory-optim/fixtures/trajectory-optim-twostage-cz12-leo.json"
```

最小请求骨架(仅示意字段,数值以 fixture 为准):

```json
{
  "RocketInput": {
    "$type": "TwoStage",
    "Name": "CZ-12 LEO 900km 50°",
    "FaSheDianLLA": [109.8, 18.4, 20],
    "Gw": 3000,
    "FairingMass": 2000,
    "A0": 141.821,
    "T1": 18,
    "Alpham": 0.4263,
    "Sm": 11.341,
    "sma0": 7278137,
    "ecc0": 0.0,
    "inc0": 50.0,
    "omg0": 170,
    "Stage1_Mass": 355000,
    "Stage1_FuelMass": 333000,
    "Stage2_Mass": 54000,
    "Stage2_FuelMass": 48000,
    "Tk_1": 196.315,
    "Dt_k12f": 2.0,
    "Dt_dh2": 2.0,
    "Tk_F": 8.7,
    "Tk_2": 423,
    "Dt_hx": 1000,
    "Tk_22": 18,
    "Dt_xjfl": 90,
    "Stage1_Engine": { "Name": "4台YF-100J", "NumberOfEngines": 4, "Force": 1250000, "Ips": 2960.0, "Sa": 1.5 },
    "Stage2_Engine": { "Name": "2台YF-115B", "NumberOfEngines": 2, "Force": 180000, "Ips": 3349.0, "IsVacuum": true },
    "Stage2_Engine2": { "Name": "2台YF-115B二次", "NumberOfEngines": 2, "Force": 180000, "Ips": 3349.0, "IsVacuum": true }
  },
  "RunProfiles": false,
  "GetKeyData": true,
  "Profiles": []
}
```

### TwoStage / CZ-12 SSO(700km)

```bash
curl "${BASE_URL}/Rocket/TrajectoryOptim" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/rocket-trajectory-optim/fixtures/trajectory-optim-twostage-cz12-sso.json"
```

可选:从服务端模板起步

```bash
curl "${BASE_URL}/templates"
curl "${BASE_URL}/templates/CZ2D_LEO_260527.json"
curl "${BASE_URL}/templates/CZ12_LEO_260722.json"
curl "${BASE_URL}/templates/CZ12_SSO_260722.json"
```

## 更多示例与测试数据(fixtures)

| 文件 | 用途简述 |
| --- | --- |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-cz2d-optim.json` | CZ-2D LEO 优化样例 |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-cz3b-gto.json` | CZ-3B GTO 优化样例(含助推与三级二次工作) |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-cz4b-optim.json` | CZ-4B SSO 优化样例 |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-cz4c-optim.json` | CZ-4C SSO 优化样例(`Tk_32`) |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-cz8-sso.json` | CZ-8 SSO 优化样例 |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-falcon9-leo.json` | Falcon9 LEO 优化样例(含 Alglib + DifferentialCorrector) |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-twostage-cz12-leo.json` | TwoStage / CZ-12 LEO 900km 50° 优化样例 |
| `skills/rocket-trajectory-optim/fixtures/trajectory-optim-twostage-cz12-sso.json` | TwoStage / CZ-12 SSO 700km 优化样例 |
