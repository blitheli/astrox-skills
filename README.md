# ASTROX Skills 库

面向 AI 编码助手的**航天动力学技能文档库**。每个技能对应 ASTROX Web API 的一类能力,包含调用说明、参数约定和可直接运行的测试输入 (fixtures)。

## 这是什么

- **技能 (Skill)**: 目录 `skills/<skill-name>/SKILL.md` 中的结构化文档,指导 AI 助手理解场景、构造请求并解析响应。
- **Fixtures**: 各技能 `fixtures/` 下的最小 JSON 示例,可用于快速验证 API 或作为请求模板。
- **公共文档**: `skills/shared-docs/` 中的跨技能协议说明 (如 CZML 输出格式、公共 schema)。

本仓库为纯文档项目,不包含可执行应用代码。实际计算由远程 [ASTROX Web API](http://astrox.cn:8765) 完成。

## 如何使用

### 在 Cursor / Claude 等 AI 助手中

1. 将本仓库加入工作区,或让助手读取对应技能的 `SKILL.md`。
2. 用自然语言描述任务 (例如「用 SGP4 递推 ISS 的 TLE,输出 24 小时星历」)。
3. 助手会根据技能文档选择端点、组装 JSON,并调用 API;需要时可参考 `fixtures/` 中的示例。

技能注册列表见 [`claude.json`](claude.json) 的 `skills[]` 字段;维护约定见 [`CLAUDE.md`](CLAUDE.md)。

### 直接调用 API

默认服务地址 (见 `claude.json` → `defaultServer`):

```text
http://astrox.cn:8765
```

**POST 示例** (SGP4 轨道递推):

```bash
curl http://astrox.cn:8765/Propagator/sgp4 \
  -X POST \
  -H 'Content-Type: application/json' \
  --data-binary @skills/propagator/fixtures/sgp4/sgp4-min.json
```

**GET 示例** (查询城市):

```bash
curl "http://astrox.cn:8765/city?cityName=Beijing"
```

**成功判定**: HTTP 200,且响应 JSON 中 `IsSuccess` 为 `true`。

> 远程 API 可能未开启;连接失败 (timeout / connection reset) 通常表示服务端未运行,而非本仓库配置问题。

## 技能一览

以下按功能分类;完整列表以 [`claude.json`](claude.json) 为准。

### 轨道递推与弹道

| 技能 | 说明 |
| :--- | :--- |
| [`propagator`](skills/propagator/SKILL.md) | 轨道递推/星历外推: TwoBody、J2、HPOP、SGP4 (TLE),输出 CzmlPositionOut |
| [`propagator-simple-ascent`](skills/propagator-simple-ascent/SKILL.md) | 火箭主动上升段轨迹 |
| [`propagator-ballistic`](skills/propagator-ballistic/SKILL.md) | 导弹弹道轨迹 |

### 轨道设计与机动

| 技能 | 说明 |
| :--- | :--- |
| [`astrogator`](skills/astrogator/SKILL.md) | 轨道机动序列 (MCS): 脉冲/有限推力、目标序列、地月平动点 Halo、DRO、霍曼转移等 |
| [`lambert`](skills/lambert/SKILL.md) | Lambert 问题: 始末位置与飞行时间 → 速度增量 (DV1、DV2) |
| [`celestial-transfer`](skills/celestial-transfer/SKILL.md) | 行星/小行星间 Lambert 转移 (日心系) |
| [`rocket-trajectory-optim`](skills/rocket-trajectory-optim/SKILL.md) | 多级火箭弹道优化 (默认 API: `http://www.astrox.cn:8764`) |

### 轨道根数 ↔ 状态向量

| 技能 | 说明 |
| :--- | :--- |
| [`kepler2rv`](skills/kepler2rv/SKILL.md) | 开普勒六根数 → 地心惯性系位置速度 |
| [`rv2kepler`](skills/rv2kepler/SKILL.md) | 地心惯性系位置速度 → 开普勒六根数 |

### 星座 / 初始轨道生成 (Orbit Wizard)

| 技能 | 说明 |
| :--- | :--- |
| [`orbitwizard-sso`](skills/orbitwizard-sso/SKILL.md) | 太阳同步轨道 (SSO) |
| [`orbitwizard-geo`](skills/orbitwizard-geo/SKILL.md) | 地球同步轨道 (GEO) |
| [`orbitwizard-molniya`](skills/orbitwizard-molniya/SKILL.md) | 莫尔尼亚轨道 |
| [`orbitwizard-walker`](skills/orbitwizard-walker/SKILL.md) | Walker 星座 |

### 可见性、光照与访问

| 技能 | 说明 |
| :--- | :--- |
| [`access`](skills/access/SKILL.md) | 两对象间可见性/Access 弧段,AER 采样 |
| [`lighting-times`](skills/lighting-times/SKILL.md) | 光照/阴影时间 (含地形遮罩) |
| [`lighting-solar-aer`](skills/lighting-solar-aer/SKILL.md) | 相对视太阳的方位角、高度角、距离 |

### 天体星历与坐标

| 技能 | 说明 |
| :--- | :--- |
| [`celestial-ephemeris`](skills/celestial-ephemeris/SKILL.md) | 行星/月球等天体星历 (CZML Position) |
| [`celestial-mpc`](skills/celestial-mpc/SKILL.md) | 小行星 MPC 轨道根数与星历 |
| [`celestial-cbaxes-rotation`](skills/celestial-cbaxes-rotation/SKILL.md) | 天体坐标系间旋转四元数与角速度 |
| [`convert-czml-position`](skills/convert-czml-position/SKILL.md) | CZML 位置序列参考系转换 (DE430) |

### 数据查询

| 技能 | 说明 |
| :--- | :--- |
| [`query-city`](skills/query-city/SKILL.md) | 城市信息与坐标 |
| [`query-facility`](skills/query-facility/SKILL.md) | 地面站/测控设施 |
| [`query-tle`](skills/query-tle/SKILL.md) | 卫星 TLE 查询 |

## 目录结构 (简要)

```text
skills/
  shared-docs/           # 公共 schema 与协议说明
  <skill-name>/
    SKILL.md             # 技能主文档
    fixtures/            # 最小可运行 JSON 示例
claude.json              # 技能注册与默认 API 地址
astrox-web-api.json      # OpenAPI 规范 (维护技能时参考)
```

## 相关文档

| 文档 | 面向读者 | 内容 |
| :--- | :--- | :--- |
| 本文件 (`README.md`) | 使用者 | 功能说明、技能索引、快速上手 |
| [`CLAUDE.md`](CLAUDE.md) | 维护者 | 仓库约定、新增技能流程 |
| [`AGENTS.md`](AGENTS.md) | Cloud Agent / 维护者 | 目录规范、技能模板、测试与开发说明 |
