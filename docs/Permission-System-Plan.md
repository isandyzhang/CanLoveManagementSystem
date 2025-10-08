# 權限系統設計計畫

## 🎯 角色權限定義

### 1. Admin（管理者）
**權限範圍：** 所有功能
- ✅ 查看所有個案
- ✅ 新增/編輯/刪除個案
- ✅ 審核個案
- ✅ 管理學校資料
- ✅ 管理使用者
- ✅ 系統設定
- ✅ 查看所有統計報表

### 2. SocialWorker（社工）
**權限範圍：** 個案管理 + 審核功能
- ✅ 查看所有個案
- ✅ 新增/編輯個案
- ✅ **審核 Assistant 提交的個案**
- ✅ 修改個案狀態
- ✅ 查看個案統計
- ❌ 刪除個案
- ❌ 管理學校資料
- ❌ 管理使用者

### 3. Assistant（助理）
**權限範圍：** 資料輸入 + 基本查看
- ✅ 查看個案清單
- ✅ **新增個案（草稿狀態）**
- ✅ 編輯自己建立的個案（未提交前）
- ✅ 提交個案給社工審核
- ❌ 審核個案
- ❌ 修改他人建立的個案
- ❌ 刪除個案
- ❌ 查看敏感統計資料

### 4. Viewer（檢視者）
**權限範圍：** 唯讀查看
- ✅ 查看個案清單
- ✅ 查看個案詳細資料
- ❌ 新增/編輯/刪除個案
- ❌ 審核個案
- ❌ 任何修改功能

---

## 📋 個案狀態流程

```
Assistant 建立 → 草稿 → 提交 → 社工審核 → 已審核/退回
```

### 個案狀態定義
1. **Draft（草稿）** - Assistant 正在編輯
2. **Submitted（已提交）** - Assistant 提交給社工審核
3. **UnderReview（審核中）** - 社工正在審核
4. **Approved（已審核）** - 社工審核通過
5. **Rejected（已退回）** - 社工退回給 Assistant 修改

---

## 🛠️ 實作計畫

### 階段 1: 資料庫修改
1. **修改 Case 模型**
   - 加入 `status` 欄位（個案狀態）
   - 加入 `assigned_to` 欄位（指派給哪個社工）
   - 加入 `reviewed_by` 欄位（審核者）
   - 加入 `reviewed_at` 欄位（審核時間）
   - 加入 `review_notes` 欄位（審核備註）

### 階段 2: 控制器權限設定
1. **CaseController 權限**
   - `Index` - 所有角色可查看（但資料不同）
   - `Create` - Assistant, SocialWorker, Admin
   - `Edit` - 根據狀態和角色決定
   - `Delete` - 只有 Admin
   - `Review` - SocialWorker, Admin（新增審核功能）

2. **SchoolController 權限**
   - 只有 Admin 可以管理

### 階段 3: 視圖修改
1. **個案清單頁面**
   - 根據角色顯示不同欄位
   - 根據角色顯示不同操作按鈕

2. **個案詳細頁面**
   - 根據角色和狀態顯示不同編輯選項
   - 社工可以看到審核功能

3. **導航列**
   - 根據角色顯示不同選單項目

### 階段 4: 業務邏輯
1. **個案建立流程**
   - Assistant 建立時自動設為 Draft 狀態
   - 提交時變更為 Submitted 狀態

2. **審核流程**
   - 社工可以審核 Submitted 狀態的個案
   - 審核通過變為 Approved
   - 審核退回變為 Rejected

3. **編輯權限**
   - Draft 狀態：只有建立者可以編輯
   - Submitted/UnderReview 狀態：只有社工和 Admin 可以編輯
   - Approved 狀態：只有 Admin 可以編輯

---

## 🔧 技術實作細節

### 1. 授權策略擴展
```csharp
// 在 Program.cs 中加入更多授權策略
options.AddPolicy("CanCreateCase", policy => 
    policy.RequireRole("assistant", "socialworker", "admin"));
options.AddPolicy("CanReviewCase", policy => 
    policy.RequireRole("socialworker", "admin"));
options.AddPolicy("CanDeleteCase", policy => 
    policy.RequireRole("admin"));
```

### 2. 個案狀態列舉
```csharp
public enum CaseStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    Rejected = 5
}
```

### 3. 視圖中的權限判斷
```csharp
@if (User.IsInRole("socialworker") || User.IsInRole("admin"))
{
    // 顯示審核按鈕
}
```

---

## 📊 資料顯示差異

### Assistant 看到的個案清單
- 自己建立的個案（所有狀態）
- 被退回的個案（需要修改）

### SocialWorker 看到的個案清單
- 所有個案
- 特別標示需要審核的個案
- 可以按狀態篩選

### Viewer 看到的個案清單
- 所有已審核的個案
- 只能查看，不能操作

### Admin 看到的個案清單
- 所有個案
- 所有操作權限

---

## 🎨 UI/UX 設計建議

### 1. 狀態指示器
- 用不同顏色的標籤顯示個案狀態
- Draft: 灰色
- Submitted: 藍色
- UnderReview: 黃色
- Approved: 綠色
- Rejected: 紅色

### 2. 操作按鈕
- 根據角色和狀態動態顯示按鈕
- 使用 Bootstrap 的按鈕樣式
- 加入圖示讓功能更清楚

### 3. 篩選功能
- 社工可以按狀態篩選個案
- 可以按建立者篩選
- 可以按日期範圍篩選

---

## 🚀 實作順序建議

1. **先修改資料庫模型**（加入狀態欄位）
2. **修改 CaseController 加入權限檢查**
3. **修改個案清單頁面**（根據角色顯示不同內容）
4. **加入審核功能**（社工專用）
5. **修改個案詳細頁面**（根據狀態顯示不同操作）
6. **加入狀態篩選功能**
7. **測試各種角色和狀態的組合**

---

## ❓ 需要確認的問題

1. **個案退回後，Assistant 可以重新提交嗎？**
2. **社工可以修改已審核的個案嗎？**
3. **需要記錄個案的修改歷史嗎？**
4. **個案刪除是軟刪除還是硬刪除？**
5. **需要個案指派功能嗎？（指定特定社工處理）**

這個計畫如何？有什麼需要調整的地方嗎？
