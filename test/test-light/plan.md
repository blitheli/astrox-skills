# 上海-天宫固定时间窗受光约束Access计算计划

## 目标
- 在 `test/test-light/` 下使用 Python 脚本计算固定时间窗内（`2026-04-23T00:00:00Z` 到 `2026-04-26T00:00:00Z`）上海与天宫空间站的 Access。
- 优先使用 Access 对象级光照约束实现条件过滤：`FromObjectPath.Lighting=Umbra`、`ToObjectPath.Lighting=DirectSun`。
- 不使用“先算光照时间段再做交集”作为主输出条件。
- 保存所有 Web API 请求与响应 JSON 文件，以及最终结果 JSON。

## 目录与文件
- `run_access_light.py`：主计算脚本（调用 query-city/query-tle/access/lighting-times，并进行区间求交）。
- `inputs/`：保存固定时间窗的请求模板 JSON。
- `outputs/`：保存所有请求与响应 JSON、最终结果 JSON。
- `README.md`：运行与输出说明。

## 执行流程
1. 通过 `GET /city?cityName=Shanghai` 查询上海坐标。
2. 通过 `GET /ssc?sscName=...` 查询天宫空间站 TLE（按 TIANGONG/TIANHE/CSS 候选匹配）。
3. 通过 `POST /access/AccessComputeV2` 计算上海到天宫的 Access 区间（基线结果）。
4. 再次调用 `POST /access/AccessComputeV2`，并在对象上设置光照约束：
   - `FromObjectPath.Lighting = Umbra`（地面站黑夜）
   - `ToObjectPath.Lighting = DirectSun`（空间站光照）
5. 以第4步结果作为主输出，生成 `outputs/final_access_with_light_constraints.json`。

## 验证标准
- API 返回满足 HTTP 200 且响应 JSON `IsSuccess=true`（在服务可用时）。
- 输出目录包含所有 `request_*.json`、`response_*.json` 和最终结果文件。
- 最终结果至少包含字段：
  - `window`、`input_sources`、`raw_counts`
  - `access_with_object_lighting_constraints`
  - `constrained_passes`、`summary`（保留为对照项）
