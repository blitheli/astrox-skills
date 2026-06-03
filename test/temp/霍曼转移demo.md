# 霍曼转移与固定时长转移 Demo

本文记录使用 **Astrox Astrogator**（`POST /Astrogator/RunMCS`）完成的两类轨道机动算例：

1. **霍曼转移**：300 km 圆轨道 → 1000 km 圆轨道（燃料最优类两脉冲转移）
2. **固定时长转移**：相同起终轨道，转移时间 **2000 s**（非霍曼）

相关请求/响应 JSON 均保存在本目录 `test/temp/` 下。

---

## 1. 问题设定


| 项目           | 数值                                            |
| ------------ | --------------------------------------------- |
| 中心天体         | 地球                                            |
| 引力常数 μ       | 3.986004415×10¹⁴ m³/s²                        |
| 地球半径（用于高度换算） | 6378137 m                                     |
| 初始轨道         | 高度 **300 km** 圆轨道 → 半长轴 **a₁ = 6 678 137 m**  |
| 目标轨道         | 高度 **1000 km** 圆轨道 → 半长轴 **a₂ = 7 378 137 m** |
| 倾角等          | i = 28.5°，Ω = ω = ν = 0                       |
| 初始偏心率        | **1×10⁻¹⁰**（近圆，避免 e=0 奇异）                     |
| 推力坐标系        | `VNC(Earth)`，笛卡尔分量 X/Y/Z                      |
| 递推器          | `Earth_Point_Mass`（二体）                        |


---

## 2. 工具与参考

- **技能**：`skills/astrogator/SKILL.md`
- **霍曼模板**：`raw/Astrogator/Target/Hohmann_Transfer2_250609.json`
- **固定时长停止条件示例**：`raw/Astrogator/Target/ImpulsiveMnvr_ThrustVector_GEO_250624.json`（`Duration` 停止）
- **API**：`http://astrox.cn:8765/Astrogator/RunMCS`

PowerShell 调用示例：

```powershell
$body = Get-Content ".\hohmann-300to1000-request.json" -Raw
$resp = Invoke-RestMethod -Uri "http://astrox.cn:8765/Astrogator/RunMCS" `
  -Method POST -ContentType "application/json" -Body $body
$resp | ConvertTo-Json -Depth 15 | Set-Content ".\hohmann-300to1000-response.json" -Encoding UTF8
```

---

## 3. 霍曼转移

### 3.1 任务段结构

```
Initial_State (300 km 圆)
    → [TargetSequence: DV1_Target]
         DV1 脉冲
    → Propagate (Transfer_Orbit)
         停止条件: Apoapsis（远地点）
    → [TargetSequence: DV2_Target]
         DV2 脉冲
```

要点：**分两个 TargetSequence**，每段用微分修正器解 **1 个自变量、1 个约束**。

### 3.2 自变量与约束

#### 第一阶段 `DV1_Target`


| 类型      | 名称                          | ParentName | 说明                         |
| ------- | --------------------------- | ---------- | -------------------------- |
| **自变量** | `ImpulsiveMnvr.Cartesian.X` | DV1        | 第一次脉冲切向 ΔV（初值 200 m/s）     |
| **约束**  | `RadiusOfApoapsis`          | DV1        | 机动后远地点半径 = **7 378 137 m** |


`DV1` 段内须在 `Results` 中声明 `ModifiedKeplerianElement`（`RadiusOfApoapsis`），DC 才能读取该约束。

#### 中间递推 `Transfer_Orbit`


| 项目   | 设置                                 |
| ---- | ---------------------------------- |
| 停止条件 | `$type: Apoapsis`（飞到转移椭圆远地点）       |
| 转移时长 | **不固定**，由轨道力学决定（结果约 **2931.85 s**） |


#### 第二阶段 `DV2_Target`


| 类型      | 名称                          | ParentName | 说明                     |
| ------- | --------------------------- | ---------- | ---------------------- |
| **自变量** | `ImpulsiveMnvr.Cartesian.X` | DV2        | 第二次脉冲切向 ΔV（初值 200 m/s） |
| **约束**  | `Eccentricity`              | DV2        | 机动后偏心率 = **0**（圆轨道）    |


在远地点执行 DV2 时，切向加速即可同时满足目标圆轨道半径与 e=0，故只需 1 个自变量。

### 3.3 调试要点（首次失败经验）

早期单段 Target、或 `Results` 的 `ParentName` 指向错误时，API 返回 `InnerException`。成功配置的关键：

1. **两个独立** `TargetSequence`（DV1 / DV2 各一套 DC）
2. 约束的 `ParentName` 与机动段 `Name` 一致（`DV1` / `DV2`）
3. 各 `ManeuverImpulsive` 内声明 DC 要用的 `Results` 元素
4. `ThrustAxesName` 使用 `"VNC(Earth)"`
5. 初始圆轨道用 **e = 1×10⁻¹⁰**，勿用严格 0

### 3.4 计算结果


| 项目       | 数值                        |
| -------- | ------------------------- |
| DV1 ΔV   | **190.034 m/s**（纯切向）     |
| 转移飞行时间   | **2931.85 s**（≈ 48.9 min） |
| DV2 ΔV   | **185.355 m/s**（纯切向）     |
| **总 ΔV** | **375.389 m/s**           |
| DC 收敛    | 两阶段均 `Converged: true`    |
| 终轨偏心率    | ≈ 2.8×10⁻¹⁰               |


**文件**：`hohmann-300to1000-request.json`、`hohmann-300to1000-response.json`

---

## 4. 固定时长转移（2000 s）

### 4.1 需求

在相同 300 km → 1000 km 圆轨道之间，规定转移 **时长为 2000 s**（短于霍曼约 2932 s），求两次脉冲 ΔV。

### 4.2 任务段结构

```
Initial_State
    → [TargetSequence: Fixed_Time_Transfer]
         DV1 脉冲
         Propagate (Transfer_2000s)  — Duration.Trip = 2000
         DV2 脉冲
```

**2000 s 写在 Propagate 的停止条件中，不是 DC 自变量。**

### 4.3 第一次打靶尝试（未完全收敛）


| 自变量（4 个）  | 约束（2 个）                            |
| --------- | ---------------------------------- |
| DV1: X, Z | DV2 后：RadiusOfApoapsis = 7378137 m |
| DV2: X, Z | DV2 后：Eccentricity = 0             |


问题：**4 自变量、2 约束 → 欠定**，101 次迭代后 `Converged: false`（e 仍约 1.9×10⁻⁴）。

另：API **不支持** 将 `SemiMajorAxis` 作为 DC 约束名（报错需用 `RadiusOfApoapsis` 等）。

### 4.4 最终打靶设置（3×3，已收敛）

#### 自变量（ControlParameters）


| #   | 参数                          | ParentName | 含义         | 初值      |
| --- | --------------------------- | ---------- | ---------- | ------- |
| 1   | `ImpulsiveMnvr.Cartesian.X` | **DV1**    | 第一次脉冲切向 ΔV | 400 m/s |
| 2   | `ImpulsiveMnvr.Cartesian.X` | **DV2**    | 第二次脉冲切向 ΔV | 200 m/s |
| 3   | `ImpulsiveMnvr.Cartesian.Z` | **DV2**    | 第二次脉冲径向 ΔV | 0 m/s   |


- DV1 仅切向（Y、Z 固定为 0）
- DV2 的 Y 固定为 0

#### 约束（Results）


| #   | 约束                 | ParentName         | 目标值         | 容差     | 含义                            |
| --- | ------------------ | ------------------ | ----------- | ------ | ----------------------------- |
| 1   | `RadiusMagnitude`  | **Transfer_2000s** | 7 378 137 m | 1 m    | 积分 2000 s 末地心距 = 1000 km 轨道半径 |
| 2   | `RadiusOfApoapsis` | **DV2**            | 7 378 137 m | 1 m    | 机动后远地点半径                      |
| 3   | `Eccentricity`     | **DV2**            | 0           | 1×10⁻⁶ | 终轨近圆                          |


`RadiusMagnitude` 须在 `Propagate` 段声明：

```json
"Results": [{
  "$type": "SphericalElement",
  "Name": "RadiusMagnitude",
  "CoordSystemName": "Earth Inertial",
  "ComponentName": "RadiusMagnitude"
}]
```

`RadiusOfApoapsis`、`Eccentricity` 须在 **DV2** 的 `ManeuverImpulsive.Results` 中声明。

#### 自变量与约束的物理对应


| 要满足的条件                    | 主要调节            |
| ------------------------- | --------------- |
| 2000 s 末到达 r = 1000 km 高度 | DV1 切向 + DV2 径向 |
| 终轨圆化、半径 1000 km           | DV2 切向 + DV2 径向 |


### 4.5 计算结果


| 项目         | 数值                        |
| ---------- | ------------------------- |
| DC 迭代      | **9 次**，`Converged: true` |
| DV1 ΔV     | **241.28 m/s**（纯切向）       |
| DV2 ΔV     | **407.97 m/s**（切向+径向）    |
| **总 ΔV**   | **649.25 m/s**            |
| 2000 s 末半径 | 7 378 137.0 m             |
| 终轨偏心率      | ≈ 1.35×10⁻⁷               |


**物理解释**：2000 s 末航天器位于 **1000 km 高度**，但通常 **尚未到达** 转移椭圆远地点，仍有较大径向速度；DV2 需大径向制动（VNC-Z 约 −390 m/s），故总 ΔV 显著高于霍曼。

**文件**：`transfer-2000s-request.json`、`transfer-2000s-response.json`

---

## 5. 两种方案对比


| 项目       | 霍曼转移                | 2000 s 固定时长             |
| -------- | ------------------- | ----------------------- |
| 转移时长     | 2931.9 s（由远地点停止决定）  | **2000 s**（Duration 停止） |
| 中间停止条件   | Apoapsis            | Duration = 2000         |
| DC 结构    | 2 段 ×（1 自变量 + 1 约束） | 1 段 ×（3 自变量 + 3 约束）     |
| ΔV₁      | 190.03 m/s（切向）      | 241.28 m/s（切向）          |
| ΔV₂      | 185.36 m/s（切向）      | 407.97 m/s（切向+径向）       |
| 总 ΔV     | **375.39 m/s**      | **649.25 m/s**          |
| 相对霍曼燃料代价 | —                   | 约 **+73%**              |


---

## 6. 霍曼 vs 固定时长：DC 设置对照表


|     | 霍曼 `DV1_Target` | 霍曼 `DV2_Target` | 固定 2000 s `Fixed_Time_Transfer`  |
| --- | --------------- | --------------- | -------------------------------- |
| 自变量 | DV1.X           | DV2.X           | DV1.X；DV2.X；DV2.Z                |
| 约束  | DV1 后 Ra        | DV2 后 e=0       | Propagate 末 r；DV2 后 Ra；DV2 后 e=0 |
| 时间  | 不固定             | —               | **固定 2000 s**                    |


---

## 7. 本目录相关文件


| 文件                                | 说明                      |
| --------------------------------- | ----------------------- |
| `hohmann-300to1000-request.json`  | 霍曼转移 MCS 请求             |
| `hohmann-300to1000-response.json` | 霍曼转移 API 响应             |
| `transfer-2000s-request.json`     | 2000 s 固定时长转移请求（3×3 DC） |
| `transfer-2000s-response.json`    | 2000 s 转移 API 响应        |
| `霍曼转移demo.md`                     | 本文档                     |


---

## 8. 简要结论

1. **霍曼**：两脉冲、各 1 自由度，递推至远地点再圆化；总 ΔV 最小（本算例 375 m/s）。
2. **固定 2000 s**：缩短转移时间约 32%，需 3 自由度打靶（含 DV2 径向）；总 ΔV 升至 649 m/s。
3. Astrogator 打靶时须保证 **自变量个数 = 独立约束个数**，并在对应段上预先声明 `Results` 供 DC 读取。

