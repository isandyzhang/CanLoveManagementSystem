# CanLove 個案管理系統後端

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-512BD4?logo=asp.net)](https://dotnet.microsoft.com/apps/aspnet)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4?logo=entity-framework)](https://docs.microsoft.com/ef/core/)
[![Azure](https://img.shields.io/badge/Azure-Cloud-0078D4?logo=microsoft-azure)](https://azure.microsoft.com/)

CanLove 個案管理系統是一個專為非政府組織（NGO）設計的後端應用程式，用於管理個案資料、員工資訊、出勤記錄和物資管理等核心業務功能。

## 📋 目錄

- [功能特色](#功能特色)
- [技術架構](#技術架構)
- [系統需求](#系統需求)
- [快速開始](#快速開始)
- [專案結構](#專案結構)
- [環境設定](#環境設定)
- [開發指南](#開發指南)
- [部署說明](#部署說明)
- [授權](#授權)

## ✨ 功能特色

### 核心模組

- **個案管理（Case Management）**
  - 個案基本資料管理
  - 開案流程（7 步驟精靈式流程）
    - 個案詳細資料
    - 社會工作服務內容
    - 經濟狀況評估
    - 健康狀況評估
    - 學業表現評估
    - 情緒評估
    - 最後評估表
  - 關懷訪視記錄表
  - 會談服務紀錄表
  - 審核功能

- **員工管理（Staff Management）**
  - 員工資料維護
  - 權限管理

- **出勤管理（Attendance Management）**
  - 出勤記錄追蹤

- **物資管理（Supply Management）**
  - 物資庫存管理

### 安全功能

- **Azure AD 整合認證**
  - OpenID Connect 單一登入（SSO）
  - Microsoft Identity Web 整合
  - Cookie 基礎會話管理

- **資料保護**
  - 敏感資料加密（如身分證字號）
  - Data Protection API 金鑰持久化
  - Azure Key Vault 整合（生產環境）

- **授權機制**
  - 基於角色的存取控制（RBAC）
  - 權限常數管理

## 🏗️ 技術架構

### 後端技術棧

- **框架**: ASP.NET Core 9.0
- **資料庫**: SQL Server（透過 Entity Framework Core）
- **ORM**: Entity Framework Core 9.0
- **認證**: Microsoft Identity Web 4.0
- **物件對應**: AutoMapper
- **API 文件**: Swagger/OpenAPI

### 前端技術棧

- **CSS 框架**: Tailwind CSS 4.1
- **建置工具**: Vite 7.1

### Azure 服務整合

- **Azure Active Directory**: 身份驗證與授權
- **Azure Key Vault**: 敏感資訊儲存
- **Azure Blob Storage**: 檔案儲存服務

### 開發工具

- **.NET SDK**: 9.0.304
- **Hot Reload**: 啟用（開發環境）

## 📦 系統需求

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) 或更新版本
- [SQL Server](https://www.microsoft.com/sql-server) 2019 或更新版本（或 Azure SQL Database）
- [Node.js](https://nodejs.org/) 18.0 或更新版本（用於 Tailwind CSS 建置）
- Azure 訂閱（用於 Azure AD、Key Vault、Blob Storage）

## 🚀 快速開始

### 1. 複製專案

```bash
git clone https://github.com/isandyzhang/CanLove_Backend.git
cd CanLove_Backend
```

### 2. 安裝依賴

```bash
# 還原 .NET 套件
dotnet restore

# 安裝 Node.js 依賴（用於 Tailwind CSS）
npm install
```

### 3. 設定環境變數

複製 `appsettings.json` 並建立 `appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CanLoveDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "AzureBlobStorage": "你的 Azure Blob Storage 連接字串"
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "canlove.org.tw",
    "TenantId": "你的 Tenant ID",
    "ClientId": "你的 Client ID",
    "ClientSecret": "你的 Client Secret",
    "CallbackPath": "/signin-oidc"
  },
  "KeyVault": {
    "VaultUri": "https://canlove-case.vault.azure.net/"
  }
}
```

### 4. 執行資料庫遷移

```bash
dotnet ef database update --project . --startup-project .
```

### 5. 建置前端樣式（可選）

```bash
# 開發模式（監聽檔案變更）
npm run dev:css

# 或建置生產版本
npm run build:css
```

### 6. 執行應用程式

```bash
dotnet run
```

應用程式將在 `https://localhost:7217` 啟動（或查看 `Properties/launchSettings.json` 中的設定）。

## 📁 專案結構

```
CanLove_Backend/
├── Application/              # 應用程式層
│   ├── Controllers/         # MVC 控制器
│   │   ├── Account/         # 認證控制器
│   │   ├── Attendance/     # 出勤管理控制器
│   │   ├── Case/           # 個案管理控制器
│   │   ├── Home/           # 首頁控制器
│   │   ├── Staff/          # 員工管理控制器
│   │   └── Supply/         # 物資管理控制器
│   ├── Mappings/           # AutoMapper 設定檔
│   └── ViewComponents/     # 視圖元件
│
├── Core/                    # 核心功能層
│   ├── Authentication/     # 認證相關（事件處理、Cookie 輔助）
│   ├── DataProtection/     # 資料保護設定
│   ├── Extensions/         # 擴充方法
│   ├── Middleware/         # 自訂中介軟體（錯誤處理）
│   └── TagHelpers/         # 標籤輔助程式
│
├── Domain/                  # 領域層（業務邏輯）
│   ├── Case/               # 個案領域
│   │   ├── Models/         # 領域模型
│   │   ├── Services/       # 業務服務
│   │   └── ViewModels/     # 視圖模型
│   └── Staff/              # 員工領域
│
├── Infrastructure/          # 基礎設施層
│   ├── Data/               # 資料存取
│   │   ├── Contexts/       # DbContext
│   │   ├── Migrations/     # EF Core 遷移檔
│   │   ├── Audit/          # 審計日誌
│   │   └── History/        # 歷史記錄
│   ├── Options/            # 選項設定
│   └── Storage/            # 儲存服務（Blob、加密）
│
├── Models/                  # 共用模型
│   ├── Api/                # API 請求/回應模型
│   └── Mvc/                # MVC 模型
│
├── Views/                   # Razor 視圖
│   ├── Account/            # 認證視圖
│   ├── Attendance/         # 出勤管理視圖
│   ├── Case/               # 個案管理視圖
│   ├── Home/               # 首頁視圖
│   ├── Shared/             # 共用視圖元件
│   ├── Staff/              # 員工管理視圖
│   └── Supply/             # 物資管理視圖
│
├── wwwroot/                 # 靜態檔案
│   ├── css/                # 樣式表（Tailwind CSS）
│   ├── js/                 # JavaScript 檔案
│   └── ...                 # 圖片等其他靜態資源
│
├── docs/                    # 文件
├── Program.cs               # 應用程式啟動點
├── appsettings.json        # 應用程式設定檔
└── CanLove_Backend.csproj  # 專案檔
```

## ⚙️ 環境設定

### 開發環境

1. **資料庫連接字串**
   - 設定 `ConnectionStrings:DefaultConnection` 指向本地 SQL Server

2. **Azure AD 設定**
   - 在 Azure Portal 註冊應用程式
   - 設定回調 URL：`https://localhost:7217/signin-oidc`
   - 取得 `TenantId`、`ClientId`、`ClientSecret`

3. **Azure Blob Storage（可選）**
   - 建立儲存體帳戶
   - 取得連接字串

4. **Azure Key Vault（可選）**
   - 開發環境可跳過，使用 `appsettings.Development.json` 即可

### 生產環境

1. **Azure Key Vault**
   - 將敏感資訊（連接字串、Client Secret 等）儲存在 Key Vault
   - 設定 Managed Identity 或 Service Principal

2. **Data Protection 金鑰**
   - 生產環境建議使用 Azure Key Vault 或共享儲存
   - 多伺服器環境必須共享金鑰

3. **HTTPS**
   - 必須使用 HTTPS（FormPost 模式要求）
   - 設定 SSL 憑證

## 💻 開發指南

### 資料庫遷移

```bash
# 建立新的遷移
dotnet ef migrations add MigrationName --project . --startup-project .

# 套用遷移
dotnet ef database update --project . --startup-project .

# 檢視 SQL 腳本（不執行）
dotnet ef migrations script --project . --startup-project .
```

### 前端樣式開發

```bash
# 開發模式（自動重新編譯）
npm run dev:css

# 建置生產版本
npm run build:css
```

### 認證流程說明

1. 使用者訪問受保護的頁面
2. 重定向到 Azure AD 登入頁
3. 使用者完成認證
4. Azure AD 回調到應用程式（FormPost 模式）
5. 應用程式建立認證 Cookie
6. 後續請求使用 Cookie 識別使用者

### 服務註冊

所有業務服務在 `Program.cs` 的 `RegisterApplicationServices()` 方法中註冊，使用依賴注入（DI）模式。

生命週期：
- **Scoped**：每個 HTTP 請求建立一個實例（最常用）
- **Singleton**：應用程式啟動時建立一次
- **Transient**：每次注入時建立新實例

## 🚢 部署說明

### Azure App Service 部署

1. **建立 App Service**
   - 選擇 .NET 9.0 執行階段堆疊
   - 啟用 Managed Identity

2. **設定應用程式設定**
   - 在 App Service 設定中新增必要的設定值
   - 或使用 Azure Key Vault 參考

3. **設定資料庫**
   - 建立 Azure SQL Database
   - 更新連接字串

4. **部署應用程式**
   ```bash
   dotnet publish -c Release
   # 使用 Azure CLI 或 Visual Studio 部署
   ```

### CI/CD

專案包含 GitHub Actions 工作流程（`.github/workflows/main_canlovemanagementsystem.yml`），可自動化建置和部署流程。

## 📝 重要注意事項

### Cookie 設定

- **Correlation Cookie**：使用 `SameSite=None` + `Secure`（FormPost 模式要求）
- **認證 Cookie**：使用 `SameSite=Lax`（一般使用）
- 生產環境必須使用 HTTPS

### Data Protection 金鑰

- 必須持久化金鑰，否則重啟後無法解密 Cookie
- 多伺服器環境必須共享金鑰

### 認證中介軟體順序

中介軟體順序非常重要：
1. `UseRouting()`
2. `UseAuthentication()`
3. `UseAuthorization()`

## 🤝 貢獻

歡迎提交 Issue 和 Pull Request！

## 📄 授權

本專案為 CanLove 組織所有。

## 📞 聯絡資訊

- **GitHub**: [isandyzhang/CanLove_Backend](https://github.com/isandyzhang/CanLove_Backend)
- **Issues**: [GitHub Issues](https://github.com/isandyzhang/CanLove_Backend/issues)

---

**注意**：此專案包含敏感資訊，請勿將 `appsettings.json` 中的實際設定值提交到版本控制系統。使用 `appsettings.Development.json`（已加入 .gitignore）進行本地開發。
