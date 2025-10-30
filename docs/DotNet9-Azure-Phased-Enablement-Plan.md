## 目標
- 以 .NET 9 為基準，先上 Azure，功能逐步開啟：Identity、KeyVault、Azure SQL 連線等。
- 降低風險：透過 Feature Flag/設定分階段切換，隨時可回退。

## 現況快照（2025-10-30）
- TargetFramework: `net9.0`（`CanLove_Backend.csproj`）
- SDK: `global.json` → `9.0.304`
- Program 啟動：
  - 已關閉 Microsoft Identity Web 的驗證、授權改為 Fallback 全通過
  - KeyVault 載入暫停
  - EF Core SQL Server 連線從 `ConnectionStrings:DefaultConnection`
- 注意：`appsettings.Development.json` 含有敏感資訊（Azure AD 與 KeyVault client secret、DB 連線字串），需立即進行秘密旋轉與移出版本控管。

## 風險與原則
- 切勿在 Git 版本庫保留任何密碼/憑證（已存在者必須旋轉）。
- 所有機密在本機用 User-Secrets 儲存，雲端用 Azure App Settings/Key Vault。
- 分階段開啟功能，每階段皆有健康檢查與回退策略。

## 分階段計劃

### Phase 0：基線部署（無驗證，無 KeyVault）
- 目的：先把應用跑起來，驗證基本頁面與資料庫連線。
- 變更：維持 Program.cs 目前設定（授權全通過、不啟用 Identity、KeyVault）。
- 設定：
  - Azure App Service：設定 `ConnectionStrings:DefaultConnection`（App Settings），先用具最小權限的 SQL 帳號。
- 驗證清單：
  - /Home/Index 可開
  - /Case/Index 可開且列表/基本操作可用
  - EF 連線成功，無連線字串錯誤
- 監控：啟用 App Service 日誌、Application Insights（可選）
- 回退：如服務異常，立即回滾至前一版（或暫停流量）

### Phase 1：Identity（Microsoft.Identity.Web）
- 目的：引入 Azure AD 登入與授權控管。
- 變更：
  - 還原 `AddMicrosoftIdentityWebApp` 與 `UseAuthentication`、`UseAuthorization`，移除 Fallback 全通過策略。
  - 控制器逐步加回 `[Authorize]`。
- 設定：
  - Azure App Settings 設定：`AzureAd:Instance/Domain/TenantId/ClientId/ClientSecret/CallbackPath`
  - 本機：用 `dotnet user-secrets` 存放上述設定（不要放在 appsettings.Development.json）
- 驗證清單：
  - 登入流程（/Account/Login 或 OIDC 自動挑戰）可完成
  - 未登入訪問受保護頁面會導向登入
- 回退：移除中介軟體與授權標註，回到 Phase 0

### Phase 2：Key Vault（設定載入）
- 目的：用 Key Vault 管理機密（DB 連線字串、Client Secrets 等）。
- 變更：
  - 還原 `Azure.Extensions.AspNetCore.Configuration.Secrets` 與 `Azure.Identity` 載入序列
  - Program.cs 於 `ConfigurationBuilder` 增加 Key Vault provider
- 設定：
  - 在 Key Vault 建立 Secrets：`ConnectionStrings--DefaultConnection`、`AzureAd--ClientSecret` 等（`--` 會被映射為 `:`）
  - App Service 設定託管身分或使用機密型服務主體
- 驗證清單：
  - 啟動時成功從 Key Vault 讀取設定
  - 刻意從 App Settings 移除機密後仍可啟動
- 回退：停用 Key Vault provider，退回 App Settings

### Phase 3：資料庫與資料保護
- 目的：穩定資料庫連線與資料保護機制。
- 變更：
  - EF Core migrations 檢查、`dotnet ef database update`
  - 生產環境 Data Protection 金鑰改用外部儲存（Azure Blob 或 Key Vault），避免多機置換登入無效
- 設定：
  - Key Vault 或 Blob 儲存設定
- 驗證清單：
  - 多實例部署下維持登入 Cookie 有效

## 健檢與清理清單
- 秘密管理：
  - [必做] 旋轉下列已外洩秘密，並移出 repo：
    - `appsettings.Development.json` 中的 Azure AD ClientSecret、KeyVault ClientSecret、DB 密碼
  - 本機使用 User Secrets：
    ```bash
    dotnet user-secrets init
    dotnet user-secrets set "AzureAd:ClientSecret" "<value>"
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<value>"
    ```
  - 生產使用 App Settings / Key Vault
- 工作流程 / CI：
  - 舊 Azure WebApp workflows 已移除，待之後 GA/穩定後再新增針對 .NET 9 的 CI/CD
- 套件版本：
  - .NET 相關已對齊 9.x。建議執行：
    ```bash
    dotnet list package --outdated
    ```
    檢查非框架套件可否升級（如 Azure SDK、Swashbuckle）
- 目錄與檔案：
  - `DataProtection-Keys/` 僅供開發測試，生產請改用共享金鑰儲存
  - `node_modules/` 僅在確定前端資產需要時保留，否則可改由 CI 安裝

## 本地與部署指令
- 本地：
  ```bash
  dotnet restore
  dotnet build -c Release
  dotnet ef database update
  ```
- 自包含部署（可選）：
  ```bash
  dotnet publish CanLove_Backend.csproj -c Release -r linux-x64 --self-contained false /p:PublishTrimmed=false
  ```

## 回退策略
- 每個 Phase 切換前標註版號，必要時回退到上一個穩定版
- 保留基線（Phase 0）設定可快速回復服務

## 待辦與責任分工（建議）
- 秘密旋轉與搬移：DevOps/Infra
- Key Vault 建置與權限：DevOps/Infra
- 程式側載入與中介軟體切換：Backend
- 驗證與回退演練：共同
