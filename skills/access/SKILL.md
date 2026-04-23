---
name: access
description: 计算两对象间可见性/访问弧段。当用户需要测站对卫星可见窗口、卫星与卫星之间Access弧段、AER 采样时使用。
---

# 访问计算技能 (Access V2)

## 核心指令 (Core Instructions)

1. **输入解析**:识别分析时间范围、`FromObjectPath`(转发端)、`ToObjectPath`(接收端),以及可选的 `OutStep`、`ComputeAER`、`UseLightTimeDelay`。
2. **位置对象匹配**:`Position` 使用 `IEntityPosition` 多态(如 `SitePosition`、`SGP4`、`J2`、`TwoBody`、`CzmlPosition`、`CzmlPositions`、`CentralBody` 等),详见 `skills/shared-docs/api-schemas/IEntityPosition.md`。
3. **API 调用逻辑**:向 `{BASE_URL}/access/AccessComputeV2` 发送 `POST`,`Content-Type: application/json`。若无特别指定,可用 `curl`;亦可用与用户环境一致的 HTTP 客户端。
4. **约束**:可对FromObjectPath和ToObjectPath分别添加约束,例如地面站最小仰角,光照约束等。

## API 规范 (Tool Definition)

### 接口地址

`POST /access/AccessComputeV2`

De430 精密历表,可光延迟;`FromObjectPath` 为转发端,`ToObjectPath` 为接收端。支持多种位置/姿态/传感器组合;输出中若 `From` 非地面物体,方位角等字段意义可能受限(以服务端实现为准)。

### 输入参数结构 (JSON)


| 参数名                 | 类型      | 必须  | 说明                                                           |
| ------------------- | ------- | --- | ------------------------------------------------------------ |
| `Description`       | string  | 否   | 说明                                                           |
| `Start`             | string  | 是   | 分析开始时刻 (UTCG) (`yyyy-MM-ddTHH:mm:ssZ`)                       |
| `Stop`              | string  | 是   | 分析结束时刻 (UTCG) (`yyyy-MM-ddTHH:mm:ssZ`)                       |
| `OutStep`           | number  | 否   | 输出时间步长 (s),缺省 60                                             |
| `FromObjectPath`    | object  | 是   | `EntityPath`:含 `Position`,可选 `Name`、`Orientation`、`Sensor` 等 |
| `ToObjectPath`      | object  | 是   | `EntityPath`:同上                                              |
| `ComputeAER`        | boolean | 否   | 是否计算 AER,缺省 `false`                                          |
| `UseLightTimeDelay` | boolean | 否   | 是否使用光延迟,缺省 `false`                                           |


### 对象类型 (FromObjectPath / ToObjectPath)

`EntityPath`的定义不在本技能文档内展开字段定义。统一引用:`skills/shared-docs/api-schemas/EntityPath.md`。

### 响应数据结构(AccessOutput)

| 字段名         | 类型      | 说明                           |
| ----------- | ------- | ---------------------------- |
| `IsSuccess` | boolean | 结果标识(`true`: 成功;`false`: 失败) |
| `Message`   | string  | 错误信息(失败时存储失败原因)              |
| `Passes`    | array   | 按时间排序的 `AccessData` 弧段列表     |


### AccessData 结构(`Passes` 元素)

单段可见弧对应一条 `AccessData`,字段如下:


| 字段名                | 类型        | 说明                                        |
| ------------------ | --------- | ----------------------------------------- |
| `AccessStart`      | string    | 本弧段开始时刻 (UTCG),`yyyy-MM-ddTHH:mm:ss.fffZ` |
| `AccessStop`       | string    | 本弧段结束时刻 (UTCG),`yyyy-MM-ddTHH:mm:ss.fffZ` |
| `Duration`         | number    | 本弧段时长 (s)                                 |
| `MinElevationData` | AccessAER | null                                      |
| `MaxElevationData` | AccessAER | null                                      |
| `MinRangeData`     | AccessAER | null                                      |
| `MaxRangeData`     | AccessAER | null                                      |
| `AccessBeginData`  | AccessAER | null                                      |
| `AccessEndData`    | AccessAER | null                                      |
| `AllDatas`         | array     | null                                      |


当请求体中 `ComputeAER` 为 `false` 时,响应里可能仅含 `AccessStart`、`AccessStop`、`Duration`,上述 `*Data` / `AllDatas` 可能为 `null`、缺省或未填充(以实际 JSON 为准)。设为 `true` 时一般返回完整 AER 相关字段。

### AccessAER 点结构(`*Data` 与 `AllDatas[]`)


| 字段名         | 类型     | 说明                                     |
| ----------- | ------ | -------------------------------------- |
| `Time`      | string | 采样时刻 (UTCG),`yyyy-MM-ddTHH:mm:ss.fffZ` |
| `Azimuth`   | number | 方位角 (deg)                              |
| `Elevation` | number | 高度角 / 仰角 (deg)                         |
| `Range`     | number | 斜距 (m)                                 |
| `RangeDot`  | number | 距离变化率 (m/s)                            |


## 注意事项

- 时间格式:使用 ISO8601 UTC(`yyyy-MM-ddTHH:mm:ssZ`);确保 `Start` < `Stop`,且与星历/TLE 历元覆盖区间一致(例如 SGP4 与 TLE 历元附近)。
- 坐标单位:经纬度为度 (deg),高度为米 (m);弧段 `Duration` 为秒 (s)。AER 中 `Azimuth`/`Elevation` 为度 (deg),`Range` 为米 (m),`RangeDot` 为米每秒 (m/s)。
- `FromObjectPath` / `ToObjectPath` 为 `EntityPath`/`EntityPath2` 结构,**根级一般不需要 `$type`**,但内部的 `Position` 等多态子对象需要 `$type`。
- `skills/access/fixtures` 含最小用例及自 `raw/access` 移植的演示 JSON(与上游 C# demo 对应);部分历史用例含额外字段(如 `Text2`、`isPasses`),服务端可能忽略未知字段。完整契约以 `astrox-web-api.json` 为准。

## 标准执行流程

1. 参数预检
  - 检查必填字段:`Start`、`Stop`、`FromObjectPath`、`ToObjectPath`
  - 检查 UTC 时间格式
  - 检查 `Start < Stop`
2. 位置判定
  - 根据用户描述为转发端/接收端选择合适 `Position` 类型
  - 检查多态分支所需字段(如 `SitePosition` 的 `cartographicDegrees`、`SGP4` 的 `TLEs`)
3. 请求构造
  - 按接口契约原样传参
4. 结果判定
  - 先判 HTTP 状态,再判 `IsSuccess`
  - `IsSuccess = false` 时优先返回 `Message`
5. 输出归一化
  - 给出关键输入摘要、执行状态、`Passes` 弧段摘要

## Fixtures 目录


| 文件                                          | 说明                                                       |
| ------------------------------------------- | -------------------------------------------------------- |
| `access-min.json`                           | 地面站 `SitePosition` 对地球 `TwoBody`,最小字段                    |
| `access-compute-v2-site-sgp4-min.json`      | 北京附近站对 SGP4/TLE 25730                                    |
| `access-fac2-j2conic-sensor-composite.json` | Fac2 对 J2 卫星:多段 `Composite` 姿态、锥形传感器、距离约束                |
| `access-fac2-j2-rec-sensor-20260103.json`   | Fac2 对 J2:矩形传感器、`VVLH` 姿态                                |
| `access-fac2-j2-rec-sensor-20221230.json`   | Fac2 对 J2+矩形传感器+`CzmlOrientation`(大样本)                   |
| `access-fac2-j2conic-sensor2-20260103.json` | Fac2 对 J2+锥形传感器(20260103)                                |
| `access-fac2-j2conic-sensor2-20221120.json` | Fac2 对 J2+锥形传感器(20221120)                                |
| `access-earth-spin-sensor-czml.json`        | GEO `TwoBody` 对 LEO+`CzmlOrientation`+锥传感器(大样本 CZML 四元数) |
| `access-earth-fac-sun-czml-position.json`   | 三亚地面站对日心 ICRF `CzmlPosition`(长时序)                        |
| `access-sthelens-earth-sat-el-range.json`   | St.Helens 站对 `CzmlPosition`(ISS 样本)、仰角与距离约束              |
| `access-moon-bruno-terrain-mask.json`       | 月面 Bruno 坑 `AzElMask` 地形遮罩对月球 `TwoBody`                  |
| `access-rocket-sgp4-rec-sensor.json`        | 火箭轨迹 `CzmlPosition` 对 SGP4+矩形传感器(大样本)                    |


## 调用示例

**说明**:将 `BASE_URL` 换成实际 ASTROX 服务根地址(末尾可有或无 `/`)。PowerShell 可先执行 `$env:BASE_URL='http://astrox.cn:8765'`,再对 `curl.exe` 使用同一变量拼接路径。下列命令均在仓库根目录执行,通过 `--data-binary @...` 引用 fixture。

### 1. 地面站对 SGP4(TLE)

**场景**:北京附近地面站对 TLE 卫星 25730;分析窗口与 `skills/propagator-sgp4/fixtures/sgp4-min.json` 中 TLE 历元对齐。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/access/AccessComputeV2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/access/fixtures/access-compute-v2-site-sgp4-min.json
```

### 2. 地面站对 TwoBody(最小字段)

**场景**:地面 `SitePosition` 对地球经典根数 `TwoBody`,字段最少,适合快速连通性检查。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/access/AccessComputeV2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/access/fixtures/access-min.json
```

### 3. 地面站对 J2+复合姿态+锥传感器+距离约束

**场景**:Fac2 对 J2 传播卫星;`Composite` 多段姿态、锥形 `Sensor`、`SensorPointing` 固定指向、`Range` 约束。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/access/AccessComputeV2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/access/fixtures/access-fac2-j2conic-sensor-composite.json
```

### 4. 地面站对 J2+矩形传感器+VVLH

**场景**:Fac2 对 J2;矩形视场、`VVLH` 姿态与固定 Az/El 指向,无长 CZML 样本。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/access/AccessComputeV2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/access/fixtures/access-fac2-j2-rec-sensor-20260103.json
```

### 5. 地面站对 CzmlPosition+仰角与距离约束

**场景**:St.Helens 地面站对 `CzmlPosition`(ISS 轨迹样本);`ElevationAngle` 与 `Range` 约束,`ComputeAER` 为 `true`。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/access/AccessComputeV2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/access/fixtures/access-sthelens-earth-sat-el-range.json
```

### 6. 月面站对月球卫星+地形 AzEl 遮罩

**场景**:月球 Bruno 坑附近 `SitePosition`(`clampToGround`)+`AzElMask` 对月球 `TwoBody` 卫星。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/access/AccessComputeV2" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/access/fixtures/access-moon-bruno-terrain-mask.json
```

更长 CZML/日心轨迹等大体积用例见上节 Fixtures 表中的 `access-earth-fac-sun-czml-position.json`、`access-earth-spin-sensor-czml.json`、`access-rocket-sgp4-rec-sensor.json` 等,调用方式相同,仅替换 `--data-binary` 路径。

## 本地快速验证(可选)

用 fixture 可避免 PowerShell 下行内 JSON 转义问题(见上节 `--data-binary @...`)。

## 相关文档

- `skills/shared-docs/api-schemas/IEntityPosition.md`
- `skills/shared-docs/api-schemas/EntityPositionSite.md`
- `skills/shared-docs/api-schemas/EntityPositionSGP4.md`
- 完整字段以仓库根目录 `astrox-web-api.json` 为准。

