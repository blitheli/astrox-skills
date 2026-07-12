# 平动点与地月 CRTBP 实用知识

本文档只保留使用 `/libration/*` API 所需的概念、坐标约定和换算规则.

## 1. CRTBP 模型

圆型限制性三体问题(CRTBP)假设:

- 两个主天体以圆轨道绕共同质心运动.
- 第三个物体质量可忽略,不反作用于两个主天体.
- 在随主天体旋转的会合坐标系中,两个主天体固定.

质量比定义为:

```text
u = m2 / (m1 + m2)
```

其中 `m1` 为主天体,`m2` 为次天体. 常用值:

- 日地系统:`3.003143144634591e-6`
- 地月系统:`0.01215058560962404`

CRTBP 是理想模型. 它适合周期轨道族初值、快速设计和动力学分析,但不能替代含真实历元、星历摄动和非球形引力的任务级模型.

## 2. L1-L5 平动点

平动点是在旋转会合系中,引力和离心效应平衡的位置.

| 点 | 几何位置 | 典型用途 |
| --- | --- | --- |
| L1 | 两主天体之间 | 地月通道、L1 Halo/Lyapunov |
| L2 | 次天体背向主天体一侧 | L2 Halo、NRHO、月背任务 |
| L3 | 主天体背向次天体一侧 | 轨道族与全局动力学研究 |
| L4 | 与两主天体构成正三角形,领先约 60 deg | 稳定区与共振任务 |
| L5 | 与两主天体构成正三角形,滞后约 60 deg | 稳定区与共振任务 |

`GET /libration/positions` 使用质心原点,返回 10 元数组. 前 3 项是共线点距附近天体的距离,后 7 项是 L1-L5 的 x 和 L4/L5 的 y. 不要把前 3 项误当作坐标.

## 3. 两种会合系原点

### 3.1 质心原点

标准 CRTBP 常令系统质心位于原点. `/libration/positions` 使用此约定.`crtbp-trajectory` 在 `IsBarycentric=true` 时也使用此约定.

### 3.2 主天体原点

`crtbp-trajectory` 在 `IsBarycentric=false` 时原点位于主天体,次天体位于 `x=1`. 地月 L1/L2 Halo 和 DRO 族接口采用此类无量纲状态约定.

因此地月族接口中的:

```text
x = 1
```

表示月球位置,不是 L1 或 L2 位置. L2 与 DRO 的振幅常写为 `Ax=X0[0]-1`,即相对月球位置.

不要直接混合两种原点的状态. 请求 `crtbp-trajectory` 时应显式设置 `IsBarycentric`,并核对响应回显.

## 4. 归一化单位

给定主天体引力参数 `gm1`,次天体引力参数 `gm2` 和平均距离 `meanRange`:

```text
UnitL = meanRange
UnitT = sqrt(UnitL^3 / (gm1 + gm2))
UnitV = UnitL / UnitT
```

无量纲时间 `2π` 对应两个主天体的一圈公转周期.

通过 `GET /libration/unit` 获取单位,再换算:

```text
r_m   = r_nd * UnitL
v_mps = v_nd * UnitV
t_s   = t_nd * UnitT
```

API 默认地月参数近似给出:

```text
UnitL = 384400000 m
UnitT = 375189.29688375752 s
UnitV = 1024.5494826018351 m/s
```

这些比例只改变单位,不会把旋转系状态转换成惯性系状态. 旋转系速度和惯性系速度之间还涉及坐标旋转及角速度项.

## 5. Halo 与 NRHO

Halo 是 L1/L2 附近的三维周期轨道族. 常选 XZ 平面上的对称点作为初值:

```text
X0 = [x,0,z,0,vy,0]
```

北半球 Halo 的 `z>0`,南半球 Halo 的 `z<0`.

本 API 对两个轨道族使用不同的参数:

| 接口 | 族参数 | API 文档范围 |
| --- | --- | --- |
| `em-l1-halo` | `Az=X0[2]` | 约 `0.022-0.199` |
| `em-l2-halo` | `Ax=X0[0]-1` | 约 `0.05-0.1928` |

`isSouth=true` 通过翻转 z 分量生成南半球分支.

NRHO(Near Rectilinear Halo Orbit)是高振幅 Halo 族的一部分,具有近月段速度快、远月段停留久的特征. OpenAPI 给出 L2 NRHO 示例 `ax=0.026`,但该值低于同一文档声明的 `0.05` 下限. 这属于契约内部不一致,调用时必须以线上响应的 `IsSuccess` 为准.

## 6. DRO

DRO(Distant Retrograde Orbit)是绕次天体的远距逆行周期轨道族. 地月平面 DRO 的 API 初值位于相对月球的 +X 侧:

```text
X0 = [1+Ax,0,0,0,Vy,0]
Vy < 0
```

`Ax` 的文档范围约为 `0.078-0.520`,对应约 30000-200000 km:

```text
Ax = amplitude_m / UnitL
amplitude_m = Ax * UnitL
```

例如使用 `UnitL=384400 km` 时:

```text
70000 km  -> Ax ~= 0.1821
100000 km -> Ax ~= 0.26014568
```

OpenAPI 将典型 70000 km 写为 `Ax=0.1801`,与直接使用 384400 km 换算略有差异. 需要精确物理振幅时,应使用同一次 `/libration/unit` 返回的 `UnitL` 计算,不要混用近似常数.

DRO 通常具有较强的线性稳定性,而 L1/L2 Halo 通常不稳定. 稳定性结论会随轨道成员和力模型变化,不能仅凭轨道名称替代单值矩阵或长期摄动分析.

## 7. 周期轨道验证

Halo/DRO 接口返回 `Period`,`X0`,`ListT`,`ListX`. 基本检查:

1. `IsSuccess=true`.
2. `X0` 长度为 6.
3. `Period>0`.
4. `ListT` 与 `ListX` 非空且数量匹配.
5. `ListX` 最后一项与 `X0` 在所需容差内闭合.

将 `X0` 和 `Period` 再送入 `crtbp-trajectory` 时,必须保持相同的:

- 质量比 `U`.
- 原点约定 `IsBarycentric`.
- 状态顺序 `[x,y,z,vx,vy,vz]`.
- 无量纲单位.

## 8. 使用边界

使用本技能:

- 快速查询 L1-L5.
- 计算归一化单位.
- 获取地月 L1/L2 Halo、NRHO、DRO 的 CRTBP 初值.
- 对无量纲状态做纯 CRTBP 数值积分.

改用 `astrogator`:

- 需要明确 UTC 历元.
- 使用 m、m/s 输入并直接输出任务轨迹.
- 需要真实力模型、星历摄动、机动或 TargetSequence.
- 使用 `Moon L1`、`Moon L2`、`Moon EMLibration` 等任务坐标系.

两类接口的坐标原点、方向和速度符号可能不同. 仅做尺度乘除不足以在两者之间转换.
