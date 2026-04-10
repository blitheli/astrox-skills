# AGENTS.md

## Cursor Cloud specific instructions

### 仓库概述

本仓库 (`astrox-skills`) 是一个**纯文档/知识库**项目，不包含可构建的应用代码。它为 AI 编码助手维护航天动力学 ASTROX Web API 的技能文档 (SKILL.md)、JSON 测试 fixture 和 OpenAPI 规范。

### 无构建/无依赖

- 没有 `package.json`、`requirements.txt`、`Makefile`、`Dockerfile` 或任何构建系统。
- 唯一的系统依赖是 `curl`（发送 API 请求）和 `jq`（格式化 JSON 输出），两者在 Cloud Agent 环境中已预装。

### 如何测试/验证

所有"测试"都是通过 `curl` 向远程 ASTROX Web API 发送 HTTP 请求完成的。

- **API 地址**：`http://astrox.cn:8765`（定义在 `claude.json` → `defaultServer`）
- **POST 端点示例**：`curl http://astrox.cn:8765/Propagator/sgp4 -X POST -H 'Content-Type: application/json' --data-binary @skills/propagator-sgp4/fixtures/sgp4-min.json`
- **GET 端点示例**：`curl "http://astrox.cn:8765/city?cityName=Beijing"`
- 判定标准：HTTP 200 + 响应 JSON 中 `IsSuccess` 为 `true`

### 注意事项

- 使用英文符号:,()
- 创建的SKILL.md要符合skill格式,尤其是yaml头
- 远程 API 服务器可能未开启。遇到连接失败（Connection reset / timeout）时，不代表环境配置有误，只是服务器侧未运行。
- 技能目录结构与注册信息参见 `CLAUDE.md` 和 `claude.json`。
- `raw/` 目录下的 C# 文件是上游参考代码，不需要在本地编译。

