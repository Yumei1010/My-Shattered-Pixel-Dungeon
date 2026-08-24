---
name: commit
description: 按逻辑单元分组并提交所有更改
---

# 提交技能

你的任务是将当前未提交的更改，按逻辑拆分为多次有意义的变更块。

1.  **分析逻辑单元**：运行 `git status` 和 `git diff`，完整审查所有更改并列出逻辑上独立的单元。
2.  **生成提交群组计划**：**严禁使用 `git add .`**，为每个逻辑单元规划具体的文件列表[reference:4]。
    *   *群组1 (UI样式修复)*: `src/styles/main.css`, `src/styles/theme.css`
    *   *群组2 (后端API更新)*: `handlers/user.go`, `models/user.go`
    *   *群组3 (文档)*: `README.md`
3.  **逐步校验与提交**：对每个计划，向用户展示并简要说明，经确认后，**仅暂存该组文件**并生成规范的提交信息（建议遵循 Conventional Commits 格式）[reference:5]。