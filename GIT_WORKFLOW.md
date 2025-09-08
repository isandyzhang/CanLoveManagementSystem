# CanLove Backend Git Workflow

## 🌿 分支策略

### 主要分支
- **`main`** - 生產環境分支，只包含穩定的發布版本
- **`develop`** - 開發主分支，整合所有功能開發

### 支援分支
- **`feature/*`** - 功能開發分支
- **`release/*`** - 發布準備分支
- **`hotfix/*`** - 緊急修復分支

## 📋 分支命名規範

### Feature 分支
```
feature/case-management-mvc
feature/case-detail-forms
feature/authentication-system
feature/api-endpoints
feature/database-models
```

### Release 分支
```
release/v1.0.0
release/v1.1.0
release/v2.0.0
```

### Hotfix 分支
```
hotfix/critical-security-fix
hotfix/database-connection-issue
hotfix/authentication-bug
```

## 🔄 工作流程

### 1. 功能開發流程
```bash
# 1. 從 develop 建立 feature 分支
git checkout develop
git pull origin develop
git checkout -b feature/case-management-mvc

# 2. 開發功能
# ... 進行開發工作 ...

# 3. 提交變更
git add .
git commit -m "feat: 實作個案管理 MVC 控制器

- 新增 CaseController
- 實作 CRUD 操作
- 建立基本 Views
- 添加資料驗證"

# 4. 推送到遠端
git push origin feature/case-management-mvc

# 5. 建立 Pull Request 到 develop
# 在 GitHub 上建立 PR: feature/case-management-mvc → develop
```

### 2. 發布流程
```bash
# 1. 從 develop 建立 release 分支
git checkout develop
git pull origin develop
git checkout -b release/v1.0.0

# 2. 更新版本號和文件
# 更新版本號、CHANGELOG.md 等

# 3. 推送 release 分支
git push origin release/v1.0.0

# 4. 建立 Pull Request: release/v1.0.0 → main
# 5. 審核通過後合併到 main
# 6. 標記版本
git tag v1.0.0
git push origin v1.0.0

# 7. 合併回 develop
git checkout develop
git merge release/v1.0.0
git push origin develop
```

### 3. 緊急修復流程
```bash
# 1. 從 main 建立 hotfix 分支
git checkout main
git pull origin main
git checkout -b hotfix/critical-bug-fix

# 2. 修復問題
# ... 進行修復 ...

# 3. 提交修復
git add .
git commit -m "fix: 修復資料庫連線問題

- 修正連線字串設定
- 添加錯誤處理
- 更新連線重試邏輯"

# 4. 推送並建立 PR
git push origin hotfix/critical-bug-fix
# 建立 PR: hotfix/critical-bug-fix → main

# 5. 合併到 main 後，也要合併回 develop
git checkout develop
git merge hotfix/critical-bug-fix
git push origin develop
```

## 📝 提交訊息規範

### 格式
```
<type>(<scope>): <subject>

<body>

<footer>
```

### Type 類型
- **feat**: 新功能
- **fix**: 修復問題
- **docs**: 文件更新
- **style**: 程式碼格式調整
- **refactor**: 重構程式碼
- **test**: 測試相關
- **chore**: 建置過程或輔助工具的變動

### 範例
```
feat(case): 新增個案管理功能

- 實作個案 CRUD 操作
- 建立個案列表頁面
- 添加個案搜尋功能
- 實作個案詳細頁面

Closes #123
```

## 🛡️ 分支保護規則

### main 分支
- ✅ 需要 Pull Request 審核
- ✅ 需要至少 1 個審核者
- ✅ 需要通過 CI/CD 檢查
- ✅ 禁止直接推送

### develop 分支
- ✅ 需要 Pull Request 審核
- ✅ 需要至少 1 個審核者
- ✅ 禁止直接推送

## 🔧 常用 Git 指令

### 分支管理
```bash
# 查看所有分支
git branch -a

# 切換分支
git checkout <branch-name>

# 建立並切換分支
git checkout -b <branch-name>

# 刪除本地分支
git branch -d <branch-name>

# 刪除遠端分支
git push origin --delete <branch-name>
```

### 同步更新
```bash
# 更新 develop 分支
git checkout develop
git pull origin develop

# 更新 feature 分支
git checkout feature/your-feature
git merge develop
# 或
git rebase develop
```

### 清理分支
```bash
# 清理已合併的分支
git branch --merged | grep -v "\*\|main\|develop" | xargs -n 1 git branch -d

# 清理遠端已刪除的分支
git remote prune origin
```

## 📊 專案里程碑

### v1.0.0 - 基礎功能
- [ ] 個案管理 MVC
- [ ] 基本 CRUD 操作
- [ ] 資料驗證
- [ ] 使用者認證

### v1.1.0 - 進階功能
- [ ] 個案詳細表單
- [ ] 檔案上傳
- [ ] 報表功能
- [ ] API 端點

### v2.0.0 - 完整系統
- [ ] 進階搜尋
- [ ] 權限管理
- [ ] 審計記錄
- [ ] 效能優化

## 🚀 開始使用

1. **設定本地環境**
   ```bash
   git checkout develop
   git pull origin develop
   ```

2. **開始新功能**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **提交變更**
   ```bash
   git add .
   git commit -m "feat: 你的功能描述"
   git push origin feature/your-feature-name
   ```

4. **建立 Pull Request**
   - 前往 GitHub 建立 PR
   - 選擇正確的目標分支
   - 填寫詳細的 PR 描述

---

*最後更新：2024-09-08*
