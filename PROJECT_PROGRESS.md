# CanLove Backend 專案進度規劃

## 📋 專案概述

**目標**：建立一個同時支援 MVC 和 Web API 的個案管理系統後端

**技術棧**：
- .NET 9.0
- Entity Framework Core (Code First)
- Azure SQL Database
- Azure Key Vault
- ASP.NET Core MVC + Web API

---

## ✅ 已完成項目

### 1. 基礎架構設定
- [x] 專案建立 (.NET 9.0)
- [x] Entity Framework Core 設定
- [x] Azure Key Vault 整合
- [x] 資料庫連線設定

### 2. 資料模型重構
- [x] Models 資料夾重新分類
  - `Core/` - 核心實體 (Case)
  - `CaseDetails/` - 個案詳細資訊
  - `Options/` - 選項表 (City, District, School 等)
  - `History/` - 歷史記錄
  - `Audit/` - 審計記錄
- [x] Namespace 統一整理
- [x] Code First Migration 建立

### 3. 資料庫設定
- [x] 從 Database First 轉換為 Code First
- [x] 初始 Migration 建立
- [x] Azure SQL 連線設定
- [x] Key Vault 連線字串管理

---

## 🚧 進行中項目

### 目前狀態
- 基礎架構完成
- 資料模型就緒
- 準備開始建立 Controllers 和 Services

---

## 📝 待完成項目

### 1. Controllers 層 (高優先級)
```
Controllers/
├── Api/
│   ├── CaseController.cs          # 個案管理 API
│   ├── CaseDetailController.cs    # 個案詳細資訊 API
│   ├── CaseHistoryController.cs   # 個案歷史 API
│   ├── OptionSetController.cs     # 選項表 API
│   └── UserController.cs          # 使用者管理 API
├── Mvc/
│   ├── HomeController.cs          # 首頁
│   ├── CaseController.cs          # 個案管理頁面
│   ├── CaseDetailController.cs    # 個案詳細頁面
│   └── ReportController.cs        # 報表頁面
└── Shared/
    └── BaseController.cs          # 基礎 Controller
```

### 2. Services 層 (高優先級)
```
Services/
├── ICaseService.cs                # 個案服務介面
├── CaseService.cs                 # 個案服務實作
├── ICaseDetailService.cs          # 個案詳細服務介面
├── CaseDetailService.cs           # 個案詳細服務實作
├── IOptionSetService.cs           # 選項表服務介面
├── OptionSetService.cs            # 選項表服務實作
├── IFileStorageService.cs         # 檔案儲存服務
├── FileStorageService.cs          # 檔案儲存服務實作
└── IAuditService.cs               # 審計服務
    └── AuditService.cs            # 審計服務實作
```

### 3. DTOs 和驗證 (中優先級)
```
DTOs/
├── Case/
│   ├── CreateCaseDto.cs
│   ├── UpdateCaseDto.cs
│   ├── CaseResponseDto.cs
│   └── CaseListDto.cs
├── CaseDetail/
│   ├── CreateCaseDetailDto.cs
│   ├── UpdateCaseDetailDto.cs
│   └── CaseDetailResponseDto.cs
└── Validators/
    ├── CreateCaseDtoValidator.cs
    ├── UpdateCaseDtoValidator.cs
    └── CaseDetailDtoValidator.cs
```

### 4. 認證和授權 (中優先級)
```
Auth/
├── JwtHelper.cs                   # JWT 工具類
├── PasswordService.cs             # 密碼服務
├── IAuthService.cs                # 認證服務介面
├── AuthService.cs                 # 認證服務實作
└── Middleware/
    └── JwtMiddleware.cs           # JWT 中介軟體
```

### 5. 配置和中介軟體 (中優先級)
```
Middleware/
├── ExceptionMiddleware.cs         # 例外處理中介軟體
├── LoggingMiddleware.cs           # 日誌中介軟體
└── RequestValidationMiddleware.cs # 請求驗證中介軟體
```

### 6. 工具類和擴展 (低優先級)
```
Utils/
├── DateTimeHelper.cs              # 日期時間工具
├── StringHelper.cs                # 字串工具
└── ValidationHelper.cs            # 驗證工具

Extensions/
├── ServiceCollectionExtensions.cs # 服務註冊擴展
├── ApplicationBuilderExtensions.cs # 應用程式建構擴展
└── DbContextExtensions.cs         # DbContext 擴展
```

---

## 🎯 實作優先順序

### 第一階段：核心功能 (1-2 週)
1. **建立 Controllers 目錄結構**
2. **實作個案管理 API (CRUD)**
3. **建立 Services 層**
4. **基本 DTOs 和驗證**

### 第二階段：進階功能 (1-2 週)
1. **認證和授權系統**
2. **個案詳細資訊管理**
3. **檔案上傳功能**
4. **審計記錄**

### 第三階段：完善功能 (1 週)
1. **MVC 頁面**
2. **報表功能**
3. **效能優化**
4. **錯誤處理**

---

## 🏗️ 架構設計

### MVC + Web API 混合架構
```
┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Mobile App    │
│   (React)       │    │   (Future)      │
└─────────┬───────┘    └─────────┬───────┘
          │                      │
          └──────────┬───────────┘
                     │
          ┌──────────▼───────────┐
          │   Controllers        │
          │   ├── Api/           │
          │   └── Mvc/           │
          └──────────┬───────────┘
                     │
          ┌──────────▼───────────┐
          │   Services           │
          │   (Business Logic)   │
          └──────────┬───────────┘
                     │
          ┌──────────▼───────────┐
          │   Data Layer         │
          │   ├── DbContext      │
          │   └── Models         │
          └──────────────────────┘
```

### API 路由設計
```
/api/v1/
├── cases/                    # 個案管理
├── case-details/            # 個案詳細
├── case-history/            # 個案歷史
├── option-sets/             # 選項表
└── users/                   # 使用者管理

/mvc/
├── /                        # 首頁
├── /cases                   # 個案列表
├── /cases/{id}              # 個案詳情
└── /reports                 # 報表
```

---

## 📊 資料庫表結構

### 核心表
- `Cases` - 個案主表
- `CaseDetail` - 個案詳細資訊
- `CaseHistory` - 個案歷史記錄

### 選項表
- `Cities` - 城市
- `Districts` - 區域
- `Schools` - 學校
- `OptionSets` - 選項集
- `OptionSetValues` - 選項值

### 審計表
- `DataChangeLog` - 資料變更記錄
- `UserActivityLog` - 使用者活動記錄

---

## 🔧 技術需求

### 已安裝套件
- Entity Framework Core
- Azure Key Vault 相關套件

### 需要新增套件
- FluentValidation (資料驗證)
- AutoMapper (物件對應)
- Swagger/OpenAPI (API 文件)
- Serilog (日誌記錄)
- JWT 認證套件

---

## 📅 時程規劃

| 週次 | 主要任務 | 預期產出 |
|------|----------|----------|
| 1 | Controllers + Services | 基本 CRUD API |
| 2 | DTOs + 驗證 | 完整 API 功能 |
| 3 | 認證系統 | 安全機制 |
| 4 | MVC 頁面 | 管理介面 |
| 5 | 測試 + 優化 | 完整系統 |

---

## 🎯 成功指標

- [ ] 所有個案管理功能 API 完成
- [ ] MVC 管理介面可正常使用
- [ ] 認證和授權機制運作正常
- [ ] 資料驗證完整
- [ ] 錯誤處理完善
- [ ] 效能符合需求

---

*最後更新：2024-09-08*
