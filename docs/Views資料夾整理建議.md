# Views 資料夾整理建議

## 目前結構分析

### 1. 主要資料夾結構
```
Views/
├── Account/              # 認證相關
├── Attendance/           # 考勤管理
├── CaseBasic/            # 個案基本資料（包含子模組）
│   ├── CareVisitRecord/  # 關懷訪視記錄表
│   ├── Consultation/     # 會談服務紀錄表
│   └── Partials/         # 個案基本資料的 Partials
├── CaseOpening/          # 開案紀錄表
│   └── Partials/         # 開案紀錄表的 Partials
├── Home/                 # 首頁
├── Shared/               # 共用元件
│   └── Partials/         # 共用 Partials
├── Staff/                # 員工管理
└── Supply/                # 物資管理
```

## 問題分析

### 1. 可刪除的檔案

#### ❌ 已廢棄但仍存在的檔案
- **`CaseBasic/ReviewItem.cshtml`** 
  - 狀態：已被 `_ReviewCaseBasicItem.cshtml` Partial 取代
  - 建議：刪除（Controller 中的 `ReviewItem` 方法可保留作為備用，但 View 已不需要）
  
- **`CaseOpening/ReviewItem.cshtml`**
  - 狀態：已被 `_ReviewCaseOpeningItem.cshtml` Partial 取代
  - 建議：刪除（Controller 中的 `ReviewItem` 方法可保留作為備用，但 View 已不需要）

#### ❌ 空資料夾
- **`CaseBasic/CareVisitRecord/Partials/`** - 空資料夾
- **`CaseBasic/Consultation/Partials/`** - 空資料夾
  - 建議：刪除空資料夾（未來需要時再建立）

#### ❌ 未使用的 View（Controller 有方法但 View 不存在）
- **`Home/About.cshtml`** - 不存在，但 `HomeController.About()` 有引用
- **`Home/Contact.cshtml`** - 不存在，但 `HomeController.Contact()` 有引用
  - 建議：建立這些 View 或移除 Controller 中的方法

### 2. 需要整理的檔案

#### ⚠️ 內容混亂的檔案
- **`CaseBasic/SearchForOpenCase.cshtml`**
  - 問題：檔案中有重複的內容（第 26-94 行是重複的「新增開案紀錄表」內容）
  - 建議：清理重複內容，只保留查詢入口的功能

#### ⚠️ 功能重複的檔案
- **`CaseBasic/ReviewForm.cshtml`**
  - 狀態：與 `ReviewItem.cshtml` 功能類似，但使用場景不同
  - 建議：確認是否還需要，如果不需要可以刪除

### 3. 建議的資料夾結構

#### ✅ 推薦的整理方式

```
Views/
├── Account/                    # 認證相關
│   ├── AccessDenied.cshtml
│   └── Login.cshtml
│
├── Attendance/                 # 考勤管理
│   ├── LeaveRequest.cshtml
│   └── Record.cshtml
│
├── Case/                       # 個案相關（統一管理）
│   ├── Basic/                  # 個案基本資料
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   ├── Index.cshtml
│   │   ├── Review.cshtml
│   │   ├── ReviewForm.cshtml   # 確認是否還需要
│   │   ├── SearchBasic.cshtml
│   │   ├── SearchForOpenCase.cshtml  # 需要清理重複內容
│   │   ├── SearchOpening.cshtml
│   │   └── Partials/
│   │       ├── _CaseFormFields.cshtml
│   │       └── _ReviewCaseBasicItem.cshtml
│   │
│   ├── Opening/                # 開案紀錄表
│   │   ├── Complete.cshtml
│   │   ├── Review.cshtml
│   │   ├── SelectCase.cshtml
│   │   ├── Step1.cshtml ~ Step7.cshtml
│   │   ├── _WizardNavigation.cshtml
│   │   └── Partials/
│   │       ├── _CaseWizardFormActions.cshtml
│   │       ├── _ReviewCaseOpeningItem.cshtml
│   │       └── _Step1FormFields.cshtml ~ _Step7FormFields.cshtml
│   │
│   ├── CareVisitRecord/        # 關懷訪視記錄表（未來功能）
│   │   ├── CareVisitRecord.cshtml
│   │   └── SearchCareVisit.cshtml
│   │
│   └── Consultation/            # 會談服務紀錄表（未來功能）
│       ├── ConsultationRecord.cshtml
│       └── SearchConsultation.cshtml
│
├── Home/                       # 首頁
│   ├── Index.cshtml
│   ├── About.cshtml            # 需要建立或移除 Controller 方法
│   └── Contact.cshtml          # 需要建立或移除 Controller 方法
│
├── Shared/                     # 共用元件
│   ├── _Breadcrumb.cshtml
│   ├── _Layout.cshtml
│   ├── _LoginLayout.cshtml
│   ├── _Sidebar.cshtml
│   ├── _Topbar.cshtml
│   ├── NotFound.cshtml
│   └── Partials/
│       ├── _AlertMessage.cshtml
│       ├── _CaseFormActions.cshtml
│       ├── _CaseTabs.cshtml
│       ├── _EmptyState.cshtml
│       ├── _NotImplemented.cshtml
│       ├── _SearchCase.cshtml
│       └── _ValidationSummary.cshtml
│
├── Staff/                      # 員工管理
│   ├── Edit.cshtml
│   └── Index.cshtml
│
└── Supply/                     # 物資管理
    ├── Inventory.cshtml
    └── StockIn.cshtml
```

## 具體建議

### 方案 A：最小改動（推薦）
保持現有結構，只做清理：
1. 刪除 `CaseBasic/ReviewItem.cshtml` 和 `CaseOpening/ReviewItem.cshtml`
2. 刪除空的 `Partials` 資料夾
3. 清理 `SearchForOpenCase.cshtml` 的重複內容
4. 建立 `Home/About.cshtml` 和 `Home/Contact.cshtml` 或移除 Controller 方法

### 方案 B：重構整理（較大改動）
將 `CaseBasic` 和 `CaseOpening` 合併到 `Case` 資料夾下：
1. 建立 `Views/Case/` 資料夾
2. 移動 `CaseBasic/` → `Case/Basic/`
3. 移動 `CaseOpening/` → `Case/Opening/`
4. 移動 `CareVisitRecord/` 和 `Consultation/` 到 `Case/` 下
5. 更新所有 Controller 中的 View 路徑

## 檔案使用狀態

### ✅ 正在使用的檔案
- `CaseBasic/Create.cshtml` - 新增個案
- `CaseBasic/Edit.cshtml` - 編輯個案
- `CaseBasic/Index.cshtml` - 個案列表
- `CaseBasic/Review.cshtml` - 審核列表（已重構）
- `CaseBasic/ReviewForm.cshtml` - 審核表單（需確認）
- `CaseBasic/SearchBasic.cshtml` - 查詢個案基本資料
- `CaseBasic/SearchOpening.cshtml` - 查詢開案紀錄
- `CaseBasic/SearchForOpenCase.cshtml` - 查詢入口（需清理）
- `CaseOpening/Step1.cshtml ~ Step7.cshtml` - 開案步驟
- `CaseOpening/Review.cshtml` - 開案審核列表（已重構）
- `CaseOpening/SelectCase.cshtml` - 選擇個案
- `CaseOpening/Complete.cshtml` - 完成頁面

### ❓ 需要確認的檔案
- `CaseBasic/ReviewItem.cshtml` - 已被 Partial 取代，但 Controller 還有方法
- `CaseOpening/ReviewItem.cshtml` - 已被 Partial 取代，但 Controller 還有方法
- `CaseBasic/ReviewForm.cshtml` - 功能與 ReviewItem 類似，需確認是否還需要

### 🗑️ 可刪除的檔案
- `CaseBasic/ReviewItem.cshtml` - 已廢棄
- `CaseOpening/ReviewItem.cshtml` - 已廢棄
- 空的 `Partials` 資料夾

## 建議的整理步驟

1. **立即刪除**：廢棄的 ReviewItem.cshtml 檔案
2. **清理內容**：SearchForOpenCase.cshtml 的重複內容
3. **處理缺失**：建立 Home/About.cshtml 和 Home/Contact.cshtml 或移除 Controller 方法
4. **未來考慮**：是否要重構為 Case/ 資料夾結構

