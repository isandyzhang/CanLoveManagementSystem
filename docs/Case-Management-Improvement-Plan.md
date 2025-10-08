# 個案管理頁面改進計畫

## 🎯 改進目標

1. **修復性別顯示問題** - 女性 badge 顏色改為可見
2. **顯示建立者姓名** - 不是 email，而是使用者的真實姓名
3. **調整操作權限** - 根據角色顯示正確的操作按鈕
4. **統一編輯頁面** - 複用 Create 頁面的美觀設計

## 📋 問題分析

### 問題 1：性別顯示
**現況**：
```cshtml
<span class="badge bg-pink">女</span>
```
**問題**：`bg-pink` 不是 Bootstrap 標準類別，可能顯示白底

**解決方案**：改用標準 Bootstrap 顏色

### 問題 2：建立者顯示
**現況**：
```cshtml
<small class="text-muted">@(item.SubmittedBy ?? "系統")</small>
```
**問題**：顯示的是登入帳號（email），不是使用者姓名

**解決方案**：從 JWT Token 取得 `name` claim

### 問題 3：操作權限
**現況**：已實作基本權限控制

**需要調整**：
- Admin：編輯、刪除、審核
- SocialWorker：審核
- Assistant：提交審核（只能操作自己的）
- Viewer：只能查看

### 問題 4：編輯頁面
**現況**：Edit.cshtml 樣式簡陋

**解決方案**：複用 Create.cshtml 的設計，建立統一的表單樣式

## 🔧 實作步驟

### Step 1：修復性別顯示（立即修復）
- 優先級：⭐⭐⭐
- 難度：簡單
- 時間：5 分鐘

**修改內容**：
- 將 `bg-pink` 改為 `bg-danger` 或 `bg-warning`
- 或自訂 CSS 類別

### Step 2：顯示建立者姓名（重要）
- 優先級：⭐⭐⭐
- 難度：中等
- 時間：15 分鐘

**需要修改**：
1. Controller：取得使用者姓名
2. View：顯示姓名而非 email

**實作方式**：
```csharp
// 在 Controller 中
var userName = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "系統";
```

### Step 3：調整操作按鈕（功能調整）
- 優先級：⭐⭐⭐
- 難度：中等
- 時間：20 分鐘

**需要調整**：
- 移除 SocialWorker 的編輯按鈕（只保留審核）
- 確保 Admin 有所有權限
- Assistant 只能看到自己的個案

### Step 4：美化編輯頁面（體驗優化）
- 優先級：⭐⭐
- 難度：中等
- 時間：30 分鐘

**實作方式**：
- 選項 A：直接複製 Create.cshtml 改成 Edit
- 選項 B：建立共用的 Partial View
- **建議**：選項 A（較快速）

### Step 5：加入審核資訊顯示（額外優化）
- 優先級：⭐
- 難度：簡單
- 時間：10 分鐘

**顯示內容**：
- 審核者姓名
- 審核時間
- 審核狀態

## 📝 詳細實作計畫

### 🎨 修改 1：性別 Badge 顏色

**檔案**：`Views/Case/Index.cshtml`

**修改前**：
```cshtml
else if (item.Gender == "F")
{
    <span class="badge bg-pink">女</span>
}
```

**修改後**：
```cshtml
else if (item.Gender == "F" || item.Gender == "女")
{
    <span class="badge bg-danger">女</span>
}
```

**顏色選項**：
- `bg-danger`（紅色）
- `bg-warning`（黃色）
- `bg-info`（淺藍色）
- 自訂 `bg-female`（粉紅色）

---

### 👤 修改 2：顯示建立者姓名

**檔案**：`Views/Case/Index.cshtml`

**修改前**：
```cshtml
<td>
    <small class="text-muted">@(item.SubmittedBy ?? "系統")</small>
</td>
```

**修改後**：需要在 Controller 準備資料

**選項 A**：在 View 中直接處理（簡單但效能較差）
```cshtml
<td>
    @{
        var displayName = item.SubmittedBy ?? "系統";
        // 如果是 email，只顯示 @ 前面的部分
        if (displayName.Contains("@"))
        {
            displayName = displayName.Split('@')[0];
        }
    }
    <small class="text-muted">@displayName</small>
</td>
```

**選項 B**：在 Controller 建立 ViewModel（推薦）
```csharp
// 建立新的 ViewModel
public class CaseListItemViewModel
{
    public Case Case { get; set; }
    public string SubmittedByDisplayName { get; set; }
    public string ReviewedByDisplayName { get; set; }
}
```

**建議**：先用選項 A 快速修復，之後有時間再重構成選項 B

---

### 🔐 修改 3：調整操作權限

**檔案**：`Views/Case/Index.cshtml`

**需求分析**：
| 角色 | 查看 | 編輯 | 刪除 | 提交審核 | 審核 | 鎖定 |
|------|------|------|------|----------|------|------|
| Admin | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| SocialWorker | ✅ | ❌ | ❌ | ❌ | ✅ | ✅ |
| Assistant | ✅(自己的) | ❌ | ❌ | ✅(自己的) | ❌ | ❌ |
| Viewer | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |

**修改重點**：
```cshtml
<!-- 編輯按鈕 - 只有 Admin 可以 -->
@if (isAdmin)
{
    <a href="@Url.Action("Edit", "Case", new { id = item.CaseId })" 
       class="btn btn-sm btn-outline-primary" title="編輯">
        <i class="bi bi-pencil"></i>
    </a>
}

<!-- 審核按鈕 - Admin 和 SocialWorker 可以 -->
@if ((isSocialWorker || isAdmin) && item.SubmittedAt != null && item.ReviewedAt == null)
{
    <!-- 審核下拉選單 -->
}
```

---

### 🎨 修改 4：美化編輯頁面

**目標**：讓 Edit.cshtml 與 Create.cshtml 有一致的外觀

**實作方式**：

#### 選項 A：直接複製修改（推薦）
1. 複製 `Create.cshtml` 的 HTML 結構
2. 修改標題為「編輯個案」
3. 修改表單 action 為 `Edit`
4. 加入隱藏欄位 `CaseId`
5. 預填現有資料

#### 選項 B：建立共用 Partial View
1. 建立 `_CaseForm.cshtml`
2. 在 Create 和 Edit 中引用
3. 較複雜但可重用

**建議**：選項 A（快速且直觀）

**需要的 ViewModel**：
```csharp
public class CaseEditViewModel
{
    public Case Case { get; set; }
    public List<City> Cities { get; set; }
    public List<District> Districts { get; set; }
    public List<School> Schools { get; set; }
    public List<SelectListItem> GenderOptions { get; set; }
}
```

---

### 📊 修改 5：加入審核資訊顯示

**檔案**：`Views/Case/Index.cshtml`

**在狀態欄位加入工具提示**：
```cshtml
<td>
    @{
        var statusClass = "";
        var statusText = "";
        var tooltipText = "";
        
        if (item.ReviewedAt != null)
        {
            statusClass = "bg-success";
            statusText = "已審核";
            tooltipText = $"審核者：{item.ReviewedBy}\n審核時間：{item.ReviewedAt:yyyy/MM/dd HH:mm}";
        }
        // ... 其他狀態
    }
    <span class="badge @statusClass" 
          data-bs-toggle="tooltip" 
          data-bs-placement="top" 
          title="@tooltipText">
        @statusText
    </span>
</td>
```

## 🚀 實作順序建議

### 第一階段：緊急修復（30分鐘）
1. ✅ 修復性別 Badge 顏色
2. ✅ 調整建立者顯示（暫時方案）
3. ✅ 調整操作按鈕權限

### 第二階段：功能完善（1小時）
4. ✅ 美化編輯頁面
5. ✅ 完善審核資訊顯示
6. ✅ 測試所有角色的權限

### 第三階段：體驗優化（視需求）
7. 建立 ViewModel 重構
8. 加入載入動畫
9. 加入操作確認對話框
10. 加入篩選和搜尋功能

## 🧪 測試檢查清單

### 顯示測試
- [ ] 男性顯示藍色 badge
- [ ] 女性顯示可見顏色 badge（非白色）
- [ ] 建立者顯示使用者姓名
- [ ] 審核狀態正確顯示
- [ ] 審核資訊顯示完整

### 權限測試
- [ ] Admin 可以編輯和刪除
- [ ] SocialWorker 只能審核，無法編輯刪除
- [ ] Assistant 只能提交自己的個案
- [ ] Viewer 只有查看按鈕

### 編輯頁面測試
- [ ] 編輯頁面樣式美觀
- [ ] 城市/地區聯動正常
- [ ] 資料正確預填
- [ ] 儲存功能正常

## 📚 相關檔案

### 需要修改的檔案
1. `Views/Case/Index.cshtml` - 列表頁面
2. `Views/Case/Edit.cshtml` - 編輯頁面
3. `Controllers/Mvc/CaseController.cs` - 控制器
4. `wwwroot/css/site.css` - 自訂樣式（如需要）

### 參考檔案
1. `Views/Case/Create.cshtml` - 參考設計
2. `Views/Shared/_Layout.cshtml` - Bootstrap 版本確認

## 💡 額外建議

### CSS 自訂顏色（如果需要粉紅色）
```css
/* wwwroot/css/site.css */
.bg-female {
    background-color: #ff69b4 !important; /* 粉紅色 */
    color: white;
}
```

### 顯示名稱處理函數
```javascript
// 在 View 中使用 JavaScript 處理
function getDisplayName(email) {
    if (!email || !email.includes('@')) return email || '系統';
    return email.split('@')[0];
}
```

## ✅ 完成標準

- [x] 性別 badge 顏色清晰可見
- [x] 建立者顯示使用者姓名而非 email
- [x] 權限按鈕符合角色需求
- [x] 編輯頁面美觀且功能完整
- [x] 所有角色測試通過

---

準備好開始實作了嗎？我們可以一步一步來！
