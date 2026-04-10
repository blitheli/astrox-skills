---
name: propagator-j2
description: 计算考虑 J2 项摄动的轨道轨迹,输出 CzmlPositionOut(CZML结构的位置序列)。当用户需要 J2 摄动、地球扁率影响、一阶带谐项轨道递推时使用。
---

# J2 摄动轨道积分器技能 (J2 Propagator)

## 核心指令 (Core Instructions)

1. **输入解析**:识别用户提供的轨道历元、开始/结束时间以及轨道根数(经典根数或笛卡尔坐标)。
2. **坐标系匹配**:默认使用 `Inertial` 系和 `Earth` 中心天体,除非用户明确指定其他行星。
3. **参数转换(OrbitalElements)**:
  - 若 `CoordType` 为 `Classical`,确保输入 6 个元素:[半长径(m),偏心率,轨道倾角(deg),近点角距(deg),升交点经度(deg),真近点角(deg)]。
  - 若 `CoordType` 为 `Cartesian`,确保输入 6 个元素:[X(m),Y(m),Z(m),Vx(m/s),Vy(m/s),Vz(m/s)]。
4. **API 调用逻辑**:向 `{BASE_URL}/Propagator/J2` 发送 `POST`,`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址

`POST /Propagator/J2`

### 输入参数结构 (JSON)


| 参数名                      | 类型     | 必须  | 说明                                         |
| ------------------------ | ------ | --- | ------------------------------------------ |
| `CentralBody`            | string | 否   | 缺省 "Earth"                                 |
| `CoordSystem`            | string | 否   | 缺省 "Inertial"                              |
| `GravitationalParameter` | number | 否   | 引力常数(m^3/s^2),缺省使用EGM2008:3.9860044150E+14 |
| `J2NormalizedValue`      | number | 否   | 归一化J2值,缺省: 地球EGM2008:0.000484165143790815  |
| `RefDistance`            | number | 否   | 参考距离(m),缺省: 地球EGM2008:6378136.3            |
| `CoordType`              | string | 否   | "Classical" 或 "Cartesian" (缺省 Classical)   |
| `OrbitalElements`        | array  | 是   | 轨道根数,具体根数由CoordType决定                      |
| `OrbitEpoch`             | string | 是   | 轨道历元(UTCG) ("yyyy-MM-ddTHH:mm:ssZ")        |
| `Start`                  | string | 是   | 分析开始时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ")     |
| `Stop`                   | string | 是   | 分析结束时刻 (UTCG) ("yyyy-MM-ddTHH:mm:ssZ")     |
| `Step`                   | number | 否   | 积分步长(s),默认 60                              |


### 响应数据结构(CzmlPositionOut)

详见 shared-docs/api-schemas/CzmlPositionOut.md

## 注意事项

- 单位检查:半长径必须是米(m)而非千米(km)。
- 时间格式:首选为ISO8601 格式
- `GravitationalParameter`:引力常数(m^3/s^2),地球默认 `3.986004415E14`;若缺省则按中心天体自动获取
- `J2NormalizedValue`:归一化 J2 值,地球默认 `0.000484165143790815`;若缺省则按中心天体自动获取
- `RefDistance`:参考距离(m),地球默认 `6378136.3`;若缺省则按中心天体自动获取

## 标准执行流程

1. 参数预检
  - 检查必填字段完整性
  - 检查 UTC 时间格式
  - 检查 `Start < Stop`
  - 检查 `OrbitalElements` 长度必须为 6
2. 模型判定
  - 未指定 `CoordType` 时默认 `Classical`
  - 若给出速度分量则优先使用 `Cartesian`
3. 请求构造
  - 按接口契约原样传参,不做单位隐式转换
  - 明确记录 `CoordType`、`CoordSystem`、`CentralBody`
4. 结果判定
  - 先判 HTTP 状态,再判 `IsSuccess`
  - `IsSuccess = false` 时优先返回 `Message`
5. 输出归一化
  - 给出关键输入摘要、执行状态、核心输出、限制说明

## 调用示例(最小可运行:Classical)

**场景**:地球轨道,高度约 300km,倾角 28.5°,传播 12 小时,使用 Earth 默认 J2 参数。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/J2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Start": "2022-04-18T04:00:00.000Z",
    "Stop": "2022-04-18T16:00:00.000Z",
    "CentralBody": "Earth",
    "OrbitEpoch": "2022-04-18T04:00:00.000Z",
    "CoordType": "Classical",
    "OrbitalElements": [6678140, 0, 28.5, 0, 0, 0]
  }'
```

## 显式指定 J2 参数

```bash
curl "${BASE_URL}/Propagator/J2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Start": "2022-04-18T04:00:00.000Z",
    "Stop": "2022-04-18T16:00:00.000Z",
    "CentralBody": "Earth",
    "OrbitEpoch": "2022-04-18T04:00:00.000Z",
    "GravitationalParameter": 3.986004415E14,
    "J2NormalizedValue": 0.000484165143790815,
    "RefDistance": 6378136.3,
    "CoordType": "Classical",
    "OrbitalElements": [6678140, 0, 28.5, 0, 0, 0]
  }'
```

## 本地快速验证(可选)

用 fixture 可避免 PowerShell 下行内 JSON 转义问题:

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Propagator/J2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary "@propagator-j2/fixtures/j2-earth-classical-min.json"
```

## 更多示例与测试数据(fixtures)


| 文件                                                   | 用途简述               |
| ---------------------------------------------------- | ------------------ |
| `propagator-j2/fixtures/j2-earth-classical-min.json` | 地球默认 J2 参数,12 小时传播 |
| `propagator-j2/fixtures/j2-mars-classical.json`      | 火星 J2 参数,24 小时传播   |


说明:部分 fixture 使用服务可接受的非 `Z` 结尾时间字符串;若接口校验只认 ISO8601,请改为 `yyyy-MM-ddTHH:mm:ssZ` 格式后再测。

从响应中取**末时刻**位置速度:按 `shared-docs/api-schemas/CzmlPositionOut.md` 中 `cartesianVelocity` 平铺格式,取最后一组 `[t, X, Y, Z, Vx, Vy, Vz]`。