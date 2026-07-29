---
name: terrain-mask
description: 计算地面站/月面站等固定点的方位–仰角地形遮罩(AzElMask)。当用户需要地形遮挡高度角、月球坑缘遮罩、生成 Access/光照用 AzElMaskData、或调用 /Terrain/AzElMask 时使用。
---

# 地形遮罩计算 (Terrain Mask / AzElMask)

通过 Astrox WebAPI 计算观测点四周 360° 各方位角上由地形造成的最大仰角(遮罩),可用于后续 Access、光照等约束。

## 核心指令 (Core Instructions)

1. **输入解析**:识别计算点 `sitePosition`(中心天体、经纬高、是否贴地)及可选 `TerrainMaskPara`(地形服务 URL、步长、搜索距离)。
2. **接口选择**:
   - 需要完整距离–仰角剖面:`POST /Terrain/AzElMask`
   - 仅需扁平方位–仰角对(供 Access/`lighting-times` 约束):`POST /Terrain/AzElMaskSimple`
3. **参数校验**:`sitePosition.cartographicDegrees` 必填;`StepSize` 单位为 m,`MaxSearchRange` 单位为 km;角度输出为 rad。
4. **API 调用逻辑**:向 `{BASE_URL}/Terrain/AzElMask`(或 `AzElMaskSimple`)发送 `POST`,`Content-Type: application/json`。

## API 规范 (Tool Definition)

### 接口地址

| 方法 | 路径 | 说明 |
| --- | --- | --- |
| `POST` | `/Terrain/AzElMask` | 完整输出:`AzElMaskData[]` 含 `Items` 距离剖面 |
| `POST` | `/Terrain/AzElMaskSimple` | 简化输出:`AzElMaskData` 为扁平 `[az, el, az, el, ...]`(rad) |

两接口请求体均为 `AzimuthElevationMaskInput`。若 `TerrainMaskPara` 为 `null`/省略,则使用服务端 `appsettings.json` 缺省配置。当前地形数据支持:地球、月球、火星、月球南极。

### 输入参数结构 (AzimuthElevationMaskInput)


| 参数名               | 类型     | 必须  | 说明                                                                 |
| ----------------- | ------ | --- | ------------------------------------------------------------------ |
| `Text`            | string | 否   | 说明                                                                 |
| `sitePosition`    | object | 是   | 计算点位置,见 `shared-docs/api-schemas/EntityPositionSite.md`             |
| `TerrainMaskPara` | object | 否   | 地形遮罩参数;为 null 时用缺省配置,见 `shared-docs/api-schemas/TerrainMaskConfig.md` |


#### sitePosition 要点

本接口将 `sitePosition` 按 `EntityPositionSite` 直接反序列化,fixture 中可不写 `$type`(与上游 C# 用例一致)。关键字段:


| 字段名                   | 类型       | 必须  | 说明                                      |
| --------------------- | -------- | --- | --------------------------------------- |
| `CentralBody`         | string   | 否   | 中心天体,缺省 `Earth`;月面点用 `Moon`             |
| `cartographicDegrees` | number[] | 是   | `[经度(deg), 纬度(deg), 高度(m)]`             |
| `clampToGround`       | boolean  | 否   | 是否贴地形表面;为 `true` 时高度强制为实际地形高程           |
| `HeightAboveGround`   | number   | 否   | 相对地面高度(m),缺省 0                          |


#### TerrainMaskPara 要点


| 字段名                | 类型     | 缺省   | 说明                                      |
| ------------------ | ------ | ---- | --------------------------------------- |
| `TerrainServerUrl` | string | —    | 地形服务 URL(到 `layer.json` 之前)             |
| `PolarDemFileName` | string | —    | 两极 DEM 名;非空时优先于 `TerrainServerUrl`      |
| `StepSize`         | number | `30` | 径向搜索步长(**m**)                           |
| `MaxSearchRange`   | number | `15` | 最大搜索距离(**km**)                          |
| `TerrainZoomLevel` | int    | `-1` | 地形最大级别(`-1` 自动)                         |


**Web API 注意**:当前服务端模型将 `TerrainServerUrl`、`PolarDemFileName` 视为非空 `string`。一旦请求体包含 `TerrainMaskPara` 对象,HTTP 校验要求这两个字段都出现且非 `null`。`PolarDemFileName` 为 `""` 时可能在运行期按字典键查找失败;非空时会走极区 DEM 并忽略 `TerrainServerUrl`。中低纬地球/月球 tileset 场景建议**省略整个 `TerrainMaskPara`**,使用服务端缺省配置(Bruno 坑缺省结果与上游 Assert 一致)。


### 响应数据结构

详见 `shared-docs/api-schemas/AzimuthElevationMaskOut.md`。

#### `/Terrain/AzElMask` 摘要


| 字段名            | 类型      | 说明                                      |
| -------------- | ------- | --------------------------------------- |
| `IsSuccess`    | boolean | 成功标识                                    |
| `Message`      | string  | 失败原因                                    |
| `sitePosition` | object  | 回写点位(贴地时高程已更新)                          |
| `AzElMaskData` | array   | `ElevationMaskData[]`:每项含 `Azimuth`/`Elevation`(rad) 与可选 `Items` |


`ElevationMaskData.Items[]` 元素为 `{ Distance(m), Elevation(rad) }`。

#### `/Terrain/AzElMaskSimple` 摘要

顶层字段同完整接口,但 `AzElMaskData` 为 `number[]`:`[方位角1, 高度角1, 方位角2, 高度角2, ...]`(rad),可直接用于 Access 约束 `$type: AzElMask` 或 `lighting-times` 的 `AzElMaskData`。

## 注意事项

- **单位**:输入经纬度为 deg、高度为 m;`StepSize` 为 m;`MaxSearchRange` 为 km;输出方位角/仰角均为 **rad**(度 = rad × 180/π)。
- **贴地**:`clampToGround: true` 时以地形表面为观测点;响应中 `sitePosition.cartographicDegrees[2]` 可能被更新。
- **地形源**:优先省略 `TerrainMaskPara` 使用缺省。中低纬月球/地球走 tileset;月球南极(如沙克尔顿)缺省走极区 DEM(`Moon_LDEM_80s_20m`)。自定义时见上文 Web API 校验说明。上游 C# 库内可直接传 `TerrainServerUrl`+`StepSize`+`MaxSearchRange`(无需 `PolarDemFileName`),与 HTTP 校验不完全一致。
- **成功判定**:HTTP 200 且 `IsSuccess === true`;失败时优先返回 `Message`。
- **与其他技能衔接**:Simple 输出的扁平数组可喂给 `access`(约束 `AzElMask`)、`lighting-times`(`AzElMaskData`)。

## 标准执行流程

1. 参数预检
   - 确认 `sitePosition.cartographicDegrees` 长度为 3
   - 确认中心天体与地形服务匹配(如月面点用月球 tileset)
   - 确认 `StepSize > 0`、`MaxSearchRange > 0`(若提供)
2. 接口选择
   - 需要距离剖面或上游 C# `GetAzimuthElevationMask` 对照 → `/Terrain/AzElMask`
   - 仅生成约束用扁平遮罩 → `/Terrain/AzElMaskSimple`
3. 请求构造
   - 按契约原样传参;`TerrainMaskPara` 可省略以用缺省
4. 结果判定
   - 先判 HTTP 状态,再判 `IsSuccess`
5. 输出归一化
   - 摘要点位、贴地后高程、若干方位的仰角(deg)及最远 `Distance`(km)

## 调用示例(最小可运行)

### 示例 1:月球 Bruno 坑(中低纬 tileset)

与上游 C# 单元测试一致;省略 `TerrainMaskPara`,使用服务端缺省月球地形。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Terrain/AzElMask" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/terrain-mask/fixtures/moon-bruno-azelmask.json
```

期望(节选,角度已换算为 deg,容差约 ±0.01°):

| 索引 | Azimuth(deg) | Elevation(deg) | 最远 Distance(km,约) |
| --- | ------------ | -------------- | ------------------- |
| 0   | 0.000        | 13.394         | ~12.38              |
| 1   | 1.000        | 13.416         | ~12.57              |

上游 Assert:

- `AzElMaskData[0].Elevation * (180/π) ≈ 13.394` (±0.01)
- `AzElMaskData[1].Elevation * (180/π) ≈ 13.416` (±0.01)

### 示例 2:月球南极沙克尔顿坑(极区 DEM)

地面站位于沙克尔顿坑底部 `[126.544292, -89.732934, -2781]`,`clampToGround: true`。缺省走极区 DEM(`Moon_LDEM_80s_20m` / PolarStereoGraphic),无需显式 `TerrainMaskPara`。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Terrain/AzElMask" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/terrain-mask/fixtures/moon-sp-shackleton-azelmask.json
```

期望(节选,角度已换算为 deg,容差约 ±0.01°):

| 索引 | Azimuth(deg) | Elevation(deg) | 最远 Distance(km,约) |
| --- | ------------ | -------------- | ------------------- |
| 0   | 0.000        | ~17.065        | ~13.47              |
| 31  | 31.000       | 17.005         | —                   |
| 70  | 70.000       | 19.450         | —                   |

上游 Assert:

- `AzElMaskData[31].Elevation * (180/π) ≈ 17.005` (±0.01)
- `AzElMaskData[70].Elevation * (180/π) ≈ 19.450` (±0.01)

若需显式指定极区 DEM,可在 `TerrainMaskPara` 中设置 `PolarDemFileName: "Moon_LDEM_80s_20m"`(同时须给出非 null 的 `TerrainServerUrl`,见上文 Web API 注意)。

### 内联 JSON(Bruno,推荐 HTTP 形态)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Terrain/AzElMask" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "Text": "月球Bruno坑的地形遮罩",
    "sitePosition": {
      "CentralBody": "Moon",
      "cartographicDegrees": [102.91745, 35.911758, -2252],
      "clampToGround": true
    }
  }'
```

### 上游 C# 库内用例中的 TerrainMaskPara(中低纬对照)

库内 `TerrainMaskCompute.GetAzimuthElevationMask` 对 Bruno 坑常用如下参数(缺省步长/搜索距与服务端缺省一致):

```json
{
  "TerrainServerUrl": "http://astrox.cn:8765/AstroxTerrain/v1/tilesets/Moon_V14_new/tiles/",
  "StepSize": 30,
  "MaxSearchRange": 15
}
```

经 HTTP 调用时勿原样省略 `PolarDemFileName`(会 400);见上文 Web API 注意。

### 简化接口

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/Terrain/AzElMaskSimple" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/terrain-mask/fixtures/moon-bruno-azelmask.json
```

月球南极 Simple 接口内部使用 `.lbl` DEM(以实现为准),输入可复用沙克尔顿 fixture:

```bash
curl "${BASE_URL}/Terrain/AzElMaskSimple" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/terrain-mask/fixtures/moon-sp-shackleton-azelmask.json
```

## Fixtures


| 文件                                                              | 说明                                                              |
| --------------------------------------------------------------- | --------------------------------------------------------------- |
| `skills/terrain-mask/fixtures/moon-bruno-azelmask.json`         | 月球 Bruno 坑(中低纬),省略 `TerrainMaskPara`,对齐上游 Assert             |
| `skills/terrain-mask/fixtures/moon-sp-shackleton-azelmask.json` | 月球南极沙克尔顿坑底部,缺省极区 DEM,对齐上游 Assert                              |

## 本地快速验证

```bash
export BASE_URL=http://astrox.cn:8765
# Bruno
curl "${BASE_URL}/Terrain/AzElMask" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/terrain-mask/fixtures/moon-bruno-azelmask.json \
  | jq '{IsSuccess, el0_deg: (.AzElMaskData[0].Elevation * 180 / 3.141592653589793), el1_deg: (.AzElMaskData[1].Elevation * 180 / 3.141592653589793)}'

# 沙克尔顿
curl "${BASE_URL}/Terrain/AzElMask" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/terrain-mask/fixtures/moon-sp-shackleton-azelmask.json \
  | jq '{IsSuccess, el31_deg: (.AzElMaskData[31].Elevation * 180 / 3.141592653589793), el70_deg: (.AzElMaskData[70].Elevation * 180 / 3.141592653589793)}'
```
