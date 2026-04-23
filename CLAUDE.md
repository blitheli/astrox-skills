# CLAUDE.md

本文件用于约定本仓库中 Claude/技能资产的组织与维护方式。

## 仓库目标

- 维护航天动力学相关技能文档(SKILL.md)
- 维护可复用的公共文档(shared-docs)
- 维护可直接调用 API 的最小测试输入(fixtures)

## 目录约定

```text
skills/
  shared-docs/
    api-schemas/
      CzmlPositionOut.md
  propagator-sgp4/
    SKILL.md
    fixtures/
  propagator-twobody/
    SKILL.md
    fixtures/
```

## 配置文件

- 主配置文件:`claude.json`
- 关键字段:
  - `skillsRoot`: 技能根目录(当前为 `skills`)
  - `sharedDocsRoot`: 公共文档目录(当前为 `skills/shared-docs`)
  - `skills[]`: 已注册技能列表(名称、SKILL.md 路径、fixtures 路径)

## 新增技能流程

1. 先建git分支
2. 在 `skills/` 下创建新目录(建议 kebab-case 命名)
3. 新建 `SKILL.md`
4. 新建 `fixtures/` 并至少提供一个最小可运行 JSON
5. 如复用公共 schema,引用 `shared-docs/api-schemas/...`
6. 在 `claude.json` 的 `skills` 数组中追加该技能配置
7. 可参考propagator-j2的内容格式
8. webapi的相关接口说明,首先参考astrox-web-api.json,不清楚的预留

## 编写建议

- 路径使用仓库相对路径,避免硬编码本机绝对路径
- 时间优先使用 ISO8601 UTC(例如 `2024-05-01T12:00:00Z`)
- 明确输入单位(m、m/s、deg、s)并在 SKILL 文档中写清
- 响应判定建议同时检查 HTTP 状态与业务字段(例如 `IsSuccess`)

## 当前已注册技能

以 `claude.json` 的 `skills[]` 为准,当前包括:

- `propagator-sgp4`、`propagator-twobody`、`propagator-simple-ascent`、`propagator-ballistic`、`propagator-j2`、`propagator-hpop`
- `orbitwizard-sso`、`orbitwizard-geo`、`orbitwizard-molniya`、`orbitwizard-walker`
- `lighting-times`
- `query-city`、`query-facility`、`query-tle`
- `access`(目录 `skills/access/`)
- `celestial-ephemeris`(目录 `skills/celestial-ephemeris/`)
- `celestial-mpc`(目录 `skills/celestial-mpc/`)

