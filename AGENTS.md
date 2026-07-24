# AGENTS.md

## Cursor Cloud specific instructions

### 仓库概述

本仓库 (`astrox-skills`) 是一个**纯文档/知识库**项目,不包含可构建的应用代码。它为 AI 编码助手维护航天动力学 ASTROX Web API 的技能文档 (SKILL.md)、JSON 测试 fixture 和 OpenAPI 规范。

### 无构建/无依赖

- 没有 `package.json`、`requirements.txt`、`Makefile`、`Dockerfile` 或任何构建系统。
- 唯一的系统依赖是 `curl`(发送 API 请求)和 `jq`(格式化 JSON 输出),两者在 Cloud Agent 环境中已预装。

### 如何测试/验证

所有"测试"都是通过 `curl` 向远程 ASTROX Web API 发送 HTTP 请求完成的。

- **API 地址**:`http://astrox.cn:8765`(定义在 `claude.json` → `defaultServer`)
- **POST 端点示例**:`curl http://astrox.cn:8765/Propagator/sgp4 -X POST -H 'Content-Type: application/json' --data-binary @skills/propagator/fixtures/sgp4/sgp4-min.json`
- **GET 端点示例**:`curl "http://astrox.cn:8765/city?cityName=Beijing"`
- 判定标准:HTTP 200 + 响应 JSON 中 `IsSuccess` 为 `true`

### 注意事项

- 使用英文符号:,()
- SKILL.md 须符合技能文档规范,顶部使用标准 YAML frontmatter(`---`、`name`、`description`、`---`);不要随意改写已有技能中已约定的 frontmatter 字段含义。
- 远程 API 服务器可能未开启。遇到连接失败(Connection reset / timeout)时,不代表环境配置有误,只是服务器侧未运行。
- 技能目录结构与注册信息参见 `CLAUDE.md` 和 `claude.json`。
- `raw/` 目录下的 C# 文件是上游参考代码,不需要在本地编译。
- 使用技能时不必展开查询 `astrox-web-api.json`;该文件主要用于创建或维护技能、核对字段与 OpenAPI 契约。
- 火箭相关技能以仓库根目录 `rocket-web-api.json` 为准,默认服务为 `http://astrox.cn:8764`。

---

## 目录组织规范

当前仓库建议采用如下结构:

```text
skills/
  shared-docs/                  # 公共文档(跨 skill 复用)
    api-schemas/
      CzmlPositionOut.md
  <skill-name>/                 # 单个技能目录
    SKILL.md                    # 技能主说明
    fixtures/                   # 该技能专属测试输入
      *.json
```

### 设计原则

1. 每个 skill 独立维护
   - 每个技能都有自己的 `SKILL.md` 和 `fixtures/`。
2. 公共文档集中管理
   - 所有技能共享的协议说明、Schema、术语说明放在 `skills/shared-docs/`。
3. 文档引用路径统一
   - 在 `SKILL.md` 中统一使用相对 `skills/` 根目录可读的路径表达(例如 `shared-docs/api-schemas/CzmlPositionOut.md`)。

## 新建 Skill 模板

新增技能时建议复制下面模板并按需修改:

```md
---
name: <skill-name>
description: <一句话描述该技能在什么场景触发>
---

# <技能中文名> (<Skill English Name>)

## 核心指令 (Core Instructions)
1. 输入解析:说明必填输入、可选输入及默认值。
2. 参数校验:列出关键约束(长度、单位、格式、取值范围)。
3. 调用逻辑:说明请求方法、路径、Content-Type、超时/重试策略(如有)。

## API 规范 (Tool Definition)

### 接口地址
`<METHOD /path>`

### 输入参数结构 (JSON)

| 参数名 | 类型 | 必须 | 说明 |
| :--- | :--- | :--- | :--- |
| ... | ... | ... | ... |

### 响应数据结构

详见 `shared-docs/api-schemas/<SchemaName>.md`

## 注意事项

- 单位、坐标系、时间格式等易错点。
- 失败时如何判定(HTTP 状态、`IsSuccess`、`Message`)。

## 标准执行流程

1. 参数预检
2. 请求构造
3. 结果判定
4. 输出归一化

## 调用示例(最小可运行)

```bash
curl "${BASE_URL}/<path>" \
  --request <METHOD> \
  --header 'Content-Type: application/json' \
  --data '{ ... }'
```

## 本地快速验证(可选)

```bash
curl "${BASE_URL}/<path>" \
  --request <METHOD> \
  --header 'Content-Type: application/json' \
  --data-binary "@<skill-name>/fixtures/<sample>.json"
```

```

## 命名建议

- 技能目录:`kebab-case`(例如 `propagator`)
- fixture 文件:`<场景>-<输入类型>-<时长>.json`(例如 `moon-classical-4h.json`)
- 公共 schema:与接口响应类型保持一致(例如 `CzmlPositionOut.md`)
