# test-celestial Lambert 转移分析计划

## 涉及 API

- `POST /celestial/ephemeris` - 地球星历（日心黄道 MeanEclpJ2000）
- `POST /celestial/mpc` - 小行星 2015 XF261 星历
- `POST /orbit/lambert` - Lambert 批量求解（太阳引力 Gm = 1.3271244004193938E20）

## 文件结构

```
test/test-celestial/
├── fetch_data.py          # 调用 API，保存原始 JSON
├── lambert_analysis.py    # 解析星历、批量 Lambert、绘图
├── requirements.txt       # requests, numpy, matplotlib
├── earth_ephemeris.json   # fetch_data.py 生成
├── asteroid_2015xf261.json
├── lambert_results.json   # lambert_analysis.py 生成
└── dv_plot.png
```

## fetch_data.py 逻辑

- 请求地球星历：
  - `TargetName: "Earth"`, `ObserverName: "Sun"`, `ObserverFrame: "MEANECLPJ2000"`
  - `Start: "2028-06-01T00:00:00Z"`, `Stop: "2029-02-01T00:00:00Z"`, `Step: 172800`（2 天）
  - 保存 `earth_ephemeris.json`
- 请求小行星星历：
  - `TargetName: "2015 XF261"`, `Start: ""`, `Stop: "2029-04-11T00:00:00Z"`（含 4/10 点）
  - 保存 `asteroid_2015xf261.json`

## lambert_analysis.py 逻辑

**解析 CZML 结构**

CZML `cartesianVelocity` 格式为平铺数组：`[t_offset, x, y, z, vx, vy, vz, ...]`，`t` 为相对 epoch 的秒数。

- 从地球星历提取各出发日期的 `[x, y, z, vx, vy, vz]`（2 天步长，与请求完全对齐）
- 从 MPC 星历找到 `2029-04-10T00:00:00Z` 时刻的状态向量作为固定 RV2

**出发时间范围**

2028-06-01 至 2029-01-31，步长 2 天，约 122 个时刻点

**批量 Lambert 请求**

所有算例合并为一次请求：
- `RV1`: 平铺所有出发时刻的地球状态（长度 6×N）
- `RV2`: 重复 N 次小行星在 2029-04-10 的状态（长度 6×N）
- `TOF`: 各出发时刻到 2029-04-10 的秒数（长度 N）
- `Gm`: 1.3271244004193938e+20（太阳）
- 保存 `lambert_results.json`

**绘图**

用 `matplotlib` 绘制双轴折线图：X 轴为出发日期，Y 轴为 |DV1|（km/s）和 |DV2|（km/s）

## requirements.txt

```
requests
numpy
matplotlib
```

## 执行顺序

1. `python fetch_data.py` → 保存两个 JSON
2. `python lambert_analysis.py` → 读取 JSON，调用 Lambert API，输出图

## 关键说明

- Lambert 为日心坐标系，Gm 使用太阳引力常数
- MPC 的 Stop 设为 `2029-04-11T00:00:00Z` 以确保包含 2029-04-10 的数据点
- 若 Lambert `IsSuccess=false`，该点 DV 标记为 NaN 并跳过绘图
