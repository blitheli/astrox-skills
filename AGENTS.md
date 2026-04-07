# AGENTS.md

## Cursor Cloud specific instructions

### 仓库性质

这是一个**纯文档/知识库仓库**（`astrox-skills`），不包含可编译运行的应用代码。仓库维护航天动力学 AI 技能定义（SKILL.md）、API fixture 测试数据（JSON）和 OpenAPI 规范文档。

### 核心依赖

- **curl** + **jq**：用于验证 fixture JSON 和调用远程 API
- **python3**：用于运行验证脚本
- 无需安装额外包管理器或语言运行时

### 远程 API

所有技能均依赖远程 ASTROX Web API（`http://astrox.cn:8765/`）。该服务器位于中国大陆，从 Cursor Cloud 环境可能因网络原因无法直接访问（TCP 连接可建立但数据传输被重置）。若 API 不可达，仍可在本地完成 JSON 语法校验和技能结构一致性检查。

### 本地验证方法

1. **JSON 语法校验**：`find skills -name '*.json' -path '*/fixtures/*' -exec jq empty {} \;`
2. **技能注册一致性检查**：验证 `claude.json` 中注册的技能与磁盘目录是否一致
3. **API fixture 调用**（需网络可达）：
   ```bash
   export BASE_URL=http://astrox.cn:8765
   curl "${BASE_URL}/Propagator/TwoBody" -X POST -H 'Content-Type: application/json' \
     -d @skills/propagator-twobody/fixtures/twobody-classical-min.json
   ```

### 注意事项

- `claude.json` 是技能注册中心，新增技能必须在此文件中添加条目
- 部分技能目录（`query-city`、`query-facility`、`query-tle`）存在于磁盘但尚未注册到 `claude.json`
- 详细的目录结构和命名规范见 `README.md` 和 `CLAUDE.md`
