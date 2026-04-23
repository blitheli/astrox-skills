# test-light

本目录用于计算固定时间窗(2026-04-23T00:00:00Z 到 2026-04-26T00:00:00Z)内,上海与天宫空间站的 Access,并叠加约束:
- 上海为黑夜(Umbra)
- 天宫为空间站光照(SunLight)

当前脚本同时输出两种约束实现:
- 对象级光照约束(推荐): 在 Access 请求中直接设置:
  - `FromObjectPath.Lighting = "Umbra"`
  - `ToObjectPath.Lighting = "DirectSun"`
  - 不做时间段交集。
- 时间段交集约束(兼容保留): `Access ∩ Shanghai.Umbra ∩ Tiangong.SunLight`。

## 运行方式

在仓库根目录执行:

`python3 test/test-light/run_access_light.py`

## 目录说明

- `run_access_light.py`: 主脚本,负责调用 Web API、求交计算、结果输出。
- `inputs/`: 固定时间窗下的请求样例 JSON。
- `outputs/`: 所有请求/响应 JSON 与最终结果 JSON。

## 输出说明

- 最终结果:
  - `outputs/final_access_with_light_constraints.json`
- 对象级光照约束结果:
  - `final_access_with_light_constraints.json` 中 `access_with_object_lighting_constraints`
- 请求与响应(示例):
  - `outputs/request_access_compute_v2.json`
  - `outputs/response_access_compute_v2.json`
  - `outputs/request_access_compute_v2_object_lighting.json`
  - `outputs/response_access_compute_v2_object_lighting.json`
  - `outputs/request_lighting_shanghai.json`
  - `outputs/response_lighting_shanghai.json`
  - `outputs/request_lighting_tiangong.json`
  - `outputs/response_lighting_tiangong.json`
  - `outputs/request_query_city_shanghai.json`
  - `outputs/response_query_city_shanghai.json`
  - `outputs/request_query_tle_*.json`
  - `outputs/response_query_tle_*.json`
