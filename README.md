航天动力学算法 SKILLS

## 目录组织规范

当前仓库建议采用如下结构：

```text
skills/
	shared-docs/                  # 公共文档（跨 skill 复用）
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
	 - 在 `SKILL.md` 中统一使用相对 `skills/` 根目录可读的路径表达（例如 `shared-docs/api-schemas/CzmlPositionOut.md`）。

## 新建 Skill 模板

新增技能时建议复制下面模板并按需修改：

```md
---
name: <skill-name>
description: <一句话描述该技能在什么场景触发>
---

# <技能中文名> (<Skill English Name>)

## 核心指令 (Core Instructions)
1. 输入解析：说明必填输入、可选输入及默认值。
2. 参数校验：列出关键约束（长度、单位、格式、取值范围）。
3. 调用逻辑：说明请求方法、路径、Content-Type、超时/重试策略（如有）。

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
- 失败时如何判定（HTTP 状态、`IsSuccess`、`Message`）。

## 标准执行流程

1. 参数预检
2. 请求构造
3. 结果判定
4. 输出归一化

## 调用示例（最小可运行）

```bash
curl "${BASE_URL}/<path>" \
	--request <METHOD> \
	--header 'Content-Type: application/json' \
	--data '{ ... }'
```

## 本地快速验证（可选）

```bash
curl "${BASE_URL}/<path>" \
	--request <METHOD> \
	--header 'Content-Type: application/json' \
	--data-binary "@<skill-name>/fixtures/<sample>.json"
```
```

## 命名建议

- 技能目录：`kebab-case`（例如 `propagator-sgp4`）
- fixture 文件：`<场景>-<输入类型>-<时长>.json`（例如 `moon-classical-4h.json`）
- 公共 schema：与接口响应类型保持一致（例如 `CzmlPositionOut.md`）
