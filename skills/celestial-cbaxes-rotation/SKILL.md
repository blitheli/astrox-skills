---
name: celestial-cbaxes-rotation
description: 计算某历元时刻天体 A 坐标轴到天体 B 坐标轴的旋转四元数及角速度。坐标系可选 FIXED、INERTIAL、J2000、ICRF、MeanEclpJ2000。涉及月球 Fixed 系时默认加载 JPL DE430 历表。用户需要天体坐标系旋转、四元数、惯性系到固定系转换、Earth-Moon 姿态时使用。
---

# 天体坐标系旋转技能 (CbAxesRotation)

通过 Astrox WebAPI 的 `POST /celestial/CbAxesRotation`,计算指定历元时刻从起始中心天体坐标系到目标中心天体坐标系的旋转四元数,可选同时输出角速度。

## 核心指令 (Core Instructions)

1. **输入解析**:识别历元 `Epoch`、起始天体 `FromCbName` 及其坐标系 `FromCbFrame`、目标天体 `ToCbName` 及其坐标系 `ToCbFrame`,以及输出阶数 `Order`。
2. **坐标系约定**:
   - `FIXED`: 中心天体固定系(本体系/固连系)
   - `INERTIAL`: 中心天体惯性系
   - `MeanEclpJ2000`: J2000 平黄道坐标系
   - `J2000`: J2000 坐标系
   - `ICRF`: ICRF 坐标系
   - 坐标系名称不区分大小写;缺省 `FromCbFrame` 为 `INERTIAL`,`ToCbFrame` 为 `FIXED`。
3. **历表依赖**:涉及月球 Fixed 系时,服务端默认加载 JPL DE430 历表以保证旋转精度。
4. **API 调用逻辑**:向 `{BASE_URL}/celestial/CbAxesRotation` 发送 `POST`,`Content-Type: application/json`。
5. **结果解读**:
   - `Order = 0`: `Rotation` 为 4 元数组 `[qx, qy, qz, qw]`,表示从 `FromCbFrame` 到 `ToCbFrame` 的旋转四元数。
   - `Order = 1`: `Rotation` 为 7 元数组 `[qx, qy, qz, qw, Wx, Wy, Wz]`,后 3 个分量为角速度,单位 **rad/s**。
   - 四元数分量顺序为 **qx, qy, qz, qw**(标量部 qw 在第四位)。若需构造旋转矩阵,注意与部分库(标量部在首)的排列差异。

## API 规范 (Tool Definition)

### 接口地址

`POST /celestial/CbAxesRotation`

### 输入参数结构 (JSON)

| 参数名 | 类型 | 必须 | 缺省值 | 说明 |
| --- | --- | --- | --- | --- |
| `Epoch` | string | 是 | `2020-09-10T04:00:00.000Z` | 历元时刻(UTCG),格式 `yyyy-MM-ddTHH:mm:ssZ` |
| `FromCbName` | string | 是 | `Earth` | 起始中心天体名称,如 `Earth`、`Moon`、`Mars` |
| `FromCbFrame` | string | 否 | `INERTIAL` | 起始中心天体坐标系,见上文枚举 |
| `ToCbName` | string | 是 | `Moon` | 目标中心天体名称 |
| `ToCbFrame` | string | 否 | `FIXED` | 目标中心天体坐标系,见上文枚举 |
| `Order` | int32 | 否 | `0` | 旋转运动阶数: `0` 仅四元数, `1` 四元数 + 角速度(rad/s) |

### 输出说明

| 字段名 | 类型 | 说明 |
| --- | --- | --- |
| `IsSuccess` | boolean | 结果(True:成功;False:失败) |
| `Message` | string | 结果信息(失败原因等) |
| `Rotation` | number[] \| null | `Order=0` 时为 `[qx,qy,qz,qw]`; `Order=1` 时为 `[qx,qy,qz,qw,Wx,Wy,Wz]` |

## 注意事项

- 时间格式必须为 UTC ISO8601,例如 `2020-09-10T04:00:00.000Z`。
- 同一天体不同坐标系(如 Earth `INERTIAL` → Earth `FIXED`)表示该天体惯性系到固定系的姿态变换。
- 不同天体(如 Earth `INERTIAL` → Moon `FIXED`)表示跨天体的坐标轴旋转关系。
- 判定成功:HTTP 200 且 `IsSuccess` 为 `true`。
- 角速度仅在 `Order = 1` 时返回,单位为 **rad/s**。

## 标准执行流程

1. 参数预检:确认 `Epoch`、`FromCbName`、`ToCbName` 非空;坐标系枚举拼写正确。
2. 请求构造:按字段名构造 JSON;未指定坐标系时可依赖服务端缺省(`FromCbFrame=INERTIAL`,`ToCbFrame=FIXED`)。
3. 结果判定:HTTP 200 + `IsSuccess === true`,且 `Rotation` 非空。
4. 输出归一化:摘要历元、起止天体与坐标系、`Order`;返回四元数(及角速度);必要时说明四元数分量顺序。

## 调用示例

### 示例 1:Earth 惯性系 → Moon 固定系(仅四元数)

对应 C# 测试 `Earth2MoonFixedFrameRotation_20260625`。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/CbAxesRotation" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/celestial-cbaxes-rotation/fixtures/earth-inertial-to-moon-fixed.json
```

### 示例 2:Earth 惯性系 → Earth 固定系

对应 C# 测试 `EarthIcrf2FixedAxes_260625`(输入为 Earth Inertial → Earth Fixed)。

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/CbAxesRotation" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data-binary @skills/celestial-cbaxes-rotation/fixtures/earth-inertial-to-earth-fixed.json
```

### 示例 3:含角速度(Order=1)

```bash
export BASE_URL=http://astrox.cn:8765
curl "${BASE_URL}/celestial/CbAxesRotation" \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
  "Epoch": "2020-09-10T04:00:00.000Z",
  "FromCbName": "Earth",
  "FromCbFrame": "INERTIAL",
  "ToCbName": "Moon",
  "ToCbFrame": "FIXED",
  "Order": 1
}'
```

## Fixtures

- `skills/celestial-cbaxes-rotation/fixtures/cbaxes-rotation-min.json`:最小可运行请求(依赖服务端缺省坐标系)。
- `skills/celestial-cbaxes-rotation/fixtures/earth-inertial-to-moon-fixed.json`:Earth Inertial → Moon Fixed,`Order=0`。
- `skills/celestial-cbaxes-rotation/fixtures/earth-inertial-to-earth-fixed.json`:Earth Inertial → Earth Fixed,`Order=0`。
