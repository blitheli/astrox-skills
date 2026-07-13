---
name: libration
description: 计算圆型限制性三体问题(CRTBP)的平动点位置与归一化单位,生成地月 L1/L2 Halo、NRHO、DRO 周期轨道初值,对已有 XZ 平面穿越初值做固定 X 的周期轨道微分修正,或在无量纲会合坐标系中积分轨迹。用户提到 Lagrange 点、平动点、CRTBP、Halo、NRHO、DRO、微分修正或地月会合系时使用。
---

# 平动点与 CRTBP 轨道技能 (Libration)

本技能调用 Astrox Web API 的 7 个 `/libration/*` 端点,处理平动点、CRTBP 单位、无量纲轨迹积分、地月周期轨道族及固定 X 的周期轨道微分修正。相关概念、坐标系和换算公式见 [docs/libration-knowledge.md](docs/libration-knowledge.md)。

## 核心指令 (Core Instructions)

1. 先区分任务:
  - 求 L1-L5 位置: `GET /libration/positions`
  - 求无量纲与 SI 单位换算: `GET /libration/unit`
  - 积分已有 CRTBP 初值: `POST /libration/crtbp-trajectory`
  - 固定 X 求 XZ 对称周期轨道: `POST /libration/crtbp-period-orbit-fixed-x`
  - 生成地月 L1 Halo: `GET /libration/em-l1-halo`
  - 生成地月 L2 Halo/NRHO: `GET /libration/em-l2-halo`
  - 生成地月平面 DRO: `GET /libration/em-dro`
2. 明确坐标约定:
  - `/positions` 使用质心原点会合系.
  - `crtbp-trajectory`、`crtbp-period-orbit-fixed-x` 由 `IsBarycentric` 选择质心原点或主天体原点.
  - `em-l1-halo`、`em-l2-halo`、`em-dro` 返回主天体原点会合系无量纲状态,地月场景中月球位于 `x=1`.
3. 不得把无量纲会合系状态直接当作 m、m/s 或惯性系状态. 使用 `/libration/unit` 返回的 `UnitL`、`UnitT`、`UnitV` 换算.
4. `positions` 和 `unit` 没有 `IsSuccess` 包装. 其余端点先检查 HTTP 状态,再检查 `IsSuccess`.
5. 轨道族与微分修正接口返回的 `X0` 顺序为 `[x,y,z,vx,vy,vz]`,`Period` 为无量纲周期.
6. 需要自定义振幅/质量比下的 Halo/DRO 初值时,优先用 `crtbp-period-orbit-fixed-x`;地月标准族查表用 `em-*-halo` / `em-dro`.



## 接口选择


| 需求              | 方法与路径                                       | 主要输入                    | 主要输出               |
| --------------- | ------------------------------------------- | ----------------------- | ------------------ |
| L1-L5 位置        | `GET /libration/positions`                  | 质量比 `u`                 | 10 元裸数组            |
| 会合系单位           | `GET /libration/unit`                       | `gm1`,`gm2`,`meanRange` | `LibrationUnit`    |
| CRTBP 积分        | `POST /libration/crtbp-trajectory`          | `RV0` 等                 | 展平轨迹 `Positions`   |
| 固定 X 周期轨道修正     | `POST /libration/crtbp-period-orbit-fixed-x` | `RV0`,`TEnd` 等          | `HaloOrbitResults` |
| 地月 L2 Halo/NRHO | `GET /libration/em-l2-halo`                 | `ax`,`isSouth`          | `HaloOrbitResults` |
| 地月 L1 Halo      | `GET /libration/em-l1-halo`                 | `az`,`isSouth`          | `HaloOrbitResults` |
| 地月 DRO          | `GET /libration/em-dro`                     | `ax`                    | `HaloOrbitResults` |



## API 规范 (Tool Definition)



### 1. 平动点位置

`GET /libration/positions`


| 参数  | 类型     | 必须     | 说明               |
| --- | ------ | ------ | ---------------- |
| `u` | number | 建议显式提供 | 质量比 `m2/(m1+m2)` |


典型质量比:

- 日地系统: `3.003143144634591e-6`
- 地月系统: `0.01215058560962404`

响应为 10 元 `number[]`,无 `IsSuccess`:


| 索引        | 含义                    |
| --------- | --------------------- |
| 0,1,2     | L1,L2,L3 距附近天体的无量纲距离  |
| 3,4,5,6,7 | L1,L2,L3,L4,L5 的无量纲 x |
| 8,9       | L4,L5 的无量纲 y          |


成功判定:HTTP 200,数组长度为 10,元素均为有限数值. OpenAPI 未定义 `u` 的默认值,不要省略.

### 2. 会合坐标系单位

`GET /libration/unit`


| 参数          | 类型     | 必须  | 默认值               | 单位      |
| ----------- | ------ | --- | ----------------- | ------- |
| `gm1`       | number | 否   | `398600441800000` | m^3/s^2 |
| `gm2`       | number | 否   | `4904869500000`   | m^3/s^2 |
| `meanRange` | number | 否   | `384400000`       | m       |


响应字段:


| 字段                          | 说明                           |
| --------------------------- | ---------------------------- |
| `GravitationalParameter1/2` | 输入引力参数                       |
| `U`                         | 质量比 `m2/(m1+m2)`             |
| `UnitL`                     | 单位长度(m)                      |
| `UnitT`                     | 单位时间(s),一个主次天体公转周期对应无量纲 `2π` |
| `UnitV`                     | 单位速度(m/s),等于 `UnitL/UnitT`   |


成功判定:HTTP 200,上述数值字段存在且为正. 本响应无 `IsSuccess`.

### 3. CRTBP 轨迹积分

`POST /libration/crtbp-trajectory`


| 参数              | 类型        | 必须  | 默认值                   | 说明                          |
| --------------- | --------- | --- | --------------------- | --------------------------- |
| `RV0`           | number[6] | 是   | OpenAPI 给出 L2 Halo 示例 | `[x,y,z,vx,vy,vz]`,无量纲      |
| `U`             | number    | 否   | `0.01215058560962404` | 质量比                         |
| `T0`            | number    | 否   | `0`                   | 初始无量纲时刻                     |
| `IsBarycentric` | boolean   | 否   | `false`               | `true` 为质心原点,`false` 为主天体原点 |
| `TEnd`          | number    | 否   | `3.384919254474086`   | 结束时刻,可小于 `T0` 以逆向积分         |
| `OutStep`       | number    | 否   | `0`                   | `0` 返回自适应节点,正数表示均匀输出步长      |


响应 `Trajectory_CRTBP_Output`:

- `IsSuccess`,`Message`:业务状态.
- `U`,`IsBarycentric`:实际使用的约定.
- `Positions`:展平数组,每 7 个值为一组:
`[t,x,y,z,vx,vy,vz]`.

必须检查 `Positions.length % 7 == 0`. OpenAPI 的输出描述称 `IsBarycentric` 默认 true,但输入 schema 和上游测试明确默认 false;以请求值及响应回显为准.

### 4. CRTBP 周期轨道微分修正(固定 X)

`POST /libration/crtbp-period-orbit-fixed-x`

在 CRTBP 中固定初始 `x`,通过微分修正 `z`、`Vy` 与半周期,求关于 XZ 平面对称的周期轨道(Halo/DRO).

请求体与 `crtbp-trajectory` 相同,均为 `Trajectory_CRTBP_Input`:


| 参数              | 类型        | 必须  | 默认值                   | 说明                                      |
| --------------- | --------- | --- | --------------------- | --------------------------------------- |
| `RV0`           | number[6] | 是   | OpenAPI L2 Halo 示例    | XZ 平面穿越初值,应满足 `y≈vx≈vz≈0`               |
| `U`             | number    | 否   | `0.01215058560962404` | 质量比                                     |
| `T0`            | number    | 否   | `0`                   | 初始无量纲时刻                                 |
| `IsBarycentric` | boolean   | 否   | `false`               | 输入/输出坐标系;内部始终在主天体原点求解                  |
| `TEnd`          | number    | 否   | `3.384919254474086`   | 周期初值取 `|TEnd-T0|`                       |
| `OutStep`       | number    | 否   | `0`                   | 与积分接口共用 schema;本端点返回 `ListT`/`ListX` |


关键约束:

- `RV0[0]`(x)在修正过程中保持不变.
- 初值应接近目标族成员;猜测过差时 `IsSuccess=false`.
- `IsBarycentric=true` 时按 `x_bary = x_m1 - U` 解释输入/输出;响应 `IsBarycentric` 与请求一致.
- 平面 DRO 可取 `z=0`;三维 Halo 取 `z≠0`.

响应为 `HaloOrbitResults`(见下方公共响应). 成功时核对:`X0[0]` 等于请求 `RV0[0]`,`Period>0`,周期闭合.

### 5. 地月 L2 Halo/NRHO

`GET /libration/em-l2-halo`


| 参数        | 类型      | 必须  | 默认值     | 说明                                 |
| --------- | ------- | --- | ------- | ---------------------------------- |
| `ax`      | number  | 否   | `0.192` | `Ax=X0[0]-1`,线上数据范围约 `[0.026000000000018453, 0.1928]`;NRHO 用 `0.0261` 等略大于下限的值,勿字面量 `0.026`(会因浮点边界被拒) |
| `isSouth` | boolean | 否   | `false` | `true` 返回南半球轨道,即 `X0[2]<0`         |


### 6. 地月 L1 Halo

`GET /libration/em-l1-halo`


| 参数        | 类型      | 必须  | 默认值     | 说明                             |
| --------- | ------- | --- | ------- | ------------------------------ |
| `az`      | number  | 否   | `0.05`  | `Az=X0[2]`,文档范围约 `0.022-0.199` |
| `isSouth` | boolean | 否   | `false` | `true` 时 `X0[2]` 为负            |




### 7. 地月 DRO

`GET /libration/em-dro`


| 参数   | 类型     | 必须  | 默认值      | 说明                           |
| ---- | ------ | --- | -------- | ---------------------------- |
| `ax` | number | 否   | `0.1801` | `Ax=X0[0]-1`,约 `0.078-0.520` |


该范围约对应 30000-200000 km. 返回平面逆行周期轨道,+X 轴穿越且 `X0[4]`(Vy)小于 0. 物理振幅换算:

`Ax = amplitude_m / UnitL`

## 周期轨道公共响应

`em-l1-halo`、`em-l2-halo`、`em-dro`、`crtbp-period-orbit-fixed-x` 均返回 `HaloOrbitResults`:


| 字段                    | 说明             |
| --------------------- | -------------- |
| `IsSuccess`,`Message` | 业务状态           |
| `IsBarycentric`       | 原点约定;族接口通常为 false,固定 X 接口与请求一致 |
| `Period`              | 无量纲周期          |
| `X0`                  | 微分修正后的 6 元初值   |
| `InitialX0`           | 修正前初值          |
| `ListT`               | 一周期无量纲时刻       |
| `ListX`               | 一周期状态数组,每项 6 元 |


成功时至少检查:`X0.length == 6`,`Period > 0`,`ListT` 与 `ListX` 非空. 周期闭合可比较 `X0` 与 `ListX` 最后一项.

## 单位换算

从 `/libration/unit` 获取同一主次天体系统的单位后:

```text
r_m   = r_nd * UnitL
v_mps = v_nd * UnitV
t_s   = t_nd * UnitT
```

换算后仍是旋转会合坐标系量,不是惯性系量. 不要混用 `384400000 m` 与其它平均地月距离生成的单位.

## 标准执行流程

1. 确认主次天体、质量比和坐标原点.
2. 若需要物理量,先调用 `/libration/unit`.
3. 对振幅做范围预检,并保留 API 的 `IsSuccess` 作为最终判定.
4. 获取周期轨道时保存响应中的 `X0`,`Period`,并单独记录所采用的质量比 `U` 约定.
5. 自定义周期轨道:构造 XZ 穿越初值 `RV0`,设 `TEnd≈` 预估周期,调用 `crtbp-period-orbit-fixed-x`;可用 `em-*` 结果作初值猜测后再微调.
6. 将轨道初值送入 `crtbp-trajectory` 时保持相同 `U` 和坐标原点.
7. 输出时同时标明数值、单位、坐标系、原点和是否无量纲.



## 调用示例

```bash
export BASE_URL=http://astrox.cn:8765

# 地月 L1-L5 位置
curl "${BASE_URL}/libration/positions?u=0.01215058560962404"

# 地月会合系单位
curl "${BASE_URL}/libration/unit?gm1=398600441800000&gm2=4904869500000&meanRange=384400000"

# 地月 L2 Halo,北半球
curl "${BASE_URL}/libration/em-l2-halo?ax=0.191494&isSouth=false"

# 地月 L1 Halo,南半球
curl "${BASE_URL}/libration/em-l1-halo?az=0.10&isSouth=true"

# 100000 km 振幅 DRO
curl "${BASE_URL}/libration/em-dro?ax=0.26014568158168577"

# L2 Halo 初值积分一个周期
curl "${BASE_URL}/libration/crtbp-trajectory" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/libration/fixtures/crtbp-trajectory-min.json"

# 固定 X 的周期轨道微分修正
curl "${BASE_URL}/libration/crtbp-period-orbit-fixed-x" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@skills/libration/fixtures/crtbp-period-orbit-fixed-x-min.json"
```



## Fixtures

- `fixtures/cases.json`:7 个端点的声明式测试清单,包含查询参数和期望检查.
- `fixtures/crtbp-trajectory-min.json`:上游闭合测试使用的 L2 Halo 一周期积分请求.
- `fixtures/crtbp-period-orbit-fixed-x-min.json`:已知闭合 L2 Halo 初值的固定 X 修正请求.
- `fixtures/crtbp-period-orbit-fixed-x-perturbed.json`:在 `em-l2-halo` 附近扰动 z/周期后的修正请求.

验证时以 HTTP 200 和各端点的响应规则为准. 远程服务器不可用时,连接失败不表示 fixture 或本地配置错误.

`em-l1-halo`、`em-l2-halo`、`em-dro` 依赖服务端轨道族数据文件. 若 HTTP 200 但 `IsSuccess=false` 且 `Message` 指向 `EM-L1-Halo.json`、`EM-L2-Halo.json` 或 `EM-DRO.json`,表示服务端缺少数据文件,不是请求参数或本地 fixture 的 JSON 语法错误.

## 与 Astrogator 的边界

本技能处理纯 CRTBP、无量纲会合系和周期轨道族快速生成. 若任务需要历元、m/m/s 状态、真实力模型、机动序列或微分修正任务流程,应使用 `astrogator` 技能,不要直接把本技能的状态当作 `Moon L1`、`Moon L2` 或 `Moon EMLibration` 坐标系输入.
