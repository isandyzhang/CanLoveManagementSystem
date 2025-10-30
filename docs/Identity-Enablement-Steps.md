## 目的
在 .NET 9 專案中直接啟用 Azure AD（Microsoft.Identity.Web）登入與授權，作為 Phase 1。

## 要點總結
- 問題重點：是否需要「在資料庫開表」？
  - 使用 Azure AD（Entra ID）+ Microsoft.Identity.Web 只負責「外部登入（OIDC）」與授權，不需要 ASP.NET Core Identity 的 `AspNetUsers` 等本地資料表。
  - 只有在你要使用「本地會員系統（ASP.NET Core Identity）」或要在本地儲存使用者/角色細節時，才需要建立本地資料表與遷移。
- 因此：若僅需 AAD 登入與授權，不必在資料庫建立使用者表；如需角色控管，建議先用「Entra App Roles / Groups」對應授權。

---

## 前置需求
- 專案：TargetFramework `net9.0` 已就緒
- 套件：`Microsoft.Identity.Web` / `Microsoft.Identity.Web.UI` 已在 csproj（目前為 4.*）
- 環境：本地用 `dotnet user-secrets`，雲端用 App Settings /（後續 Phase 2）Key Vault

## 步驟 1：Azure AD 應用註冊（Entra ID）
1. 建立 App Registration（Web）：
   - Redirect URI：`https://<你的網域或本機>/signin-oidc`
   - 登入平台選擇「Web」
2. 記錄：`TenantId`、`ClientId`
3. 產生 Client Secret（僅供後端使用），記錄值
4. （選用）App Roles：若要以 AAD App Roles 控制授權，於「App roles」新增角色，並在 Enterprise Applications 指派給使用者/群組

## 步驟 2：設定機密（勿放在 appsettings*.json）
- 本機（User Secrets）：
  ```bash
  dotnet user-secrets init
  dotnet user-secrets set "AzureAd:Instance" "https://login.microsoftonline.com/"
  dotnet user-secrets set "AzureAd:Domain" "<your_domain>"
  dotnet user-secrets set "AzureAd:TenantId" "<tenant_id>"
  dotnet user-secrets set "AzureAd:ClientId" "<client_id>"
  dotnet user-secrets set "AzureAd:ClientSecret" "<client_secret>"
  dotnet user-secrets set "AzureAd:CallbackPath" "/signin-oidc"
  ```
- 雲端（Azure App Service → Configuration → Application settings）：同名鍵值放入；後續 Phase 2 可改由 Key Vault 引用

## 步驟 3：Program.cs 啟用驗證與授權
- 服務註冊：
  - 加入：
    - `AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)`
    - `AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))`
  - 授權策略：把 Fallback 改為 RequireAuthenticatedUser（或在 Controller 標註 `[Authorize]`）
- 中介軟體順序：
  - `app.UseAuthentication();`
  - `app.UseAuthorization();`

範例（邏輯示意，請依現有 `Program.cs` 整合）：
```csharp
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authorization;

// builder
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// app
app.UseAuthentication();
app.UseAuthorization();
```

> 提示：若要保留部分匿名頁面，對該控制器/動作加上 `[AllowAnonymous]`。

## 步驟 4：（選用）UI 與登入/登出流程
- 若需要現成都會頁（如登入/登出、拒絕存取），可加入：
  - `Microsoft.Identity.Web.UI`（已引入）
  - `services.AddRazorPages();` 並在路由中 `app.MapRazorPages();`
- 登入可透過挑戰：
  ```csharp
  return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
  ```

## 步驟 5：授權策略（角色/群組）
- AAD App Roles：
  - 在 App Registration 定義角色 → 企業應用中指派給使用者/群組
  - Token 會帶 `roles` 聲明，可在策略中要求角色
- AAD 群組：啟用群組宣告或透過 Graph 查群組
- 本地資料表（若有業務需要）：
  - 非必要；若要做細緻權限或人員主檔，可建本地 UserProfile 表，依 `oid`（使用者物件識別碼）做對應

## 步驟 6：測試與回退
- 本地：
  ```bash
  dotnet restore
  dotnet build -c Release
  # 啟動並走一次登入流程
  ```
- 雲端：
  - 先於 App Service 設定 AzureAd* 鍵值
  - 發佈後驗證 `/`（未登入應會導向登入）
- 回退：
  - 臨時關閉 Auth：移除 `UseAuthentication` / 將 Fallback Policy 改回允許匿名

## 常見問題
- 問：要不要在資料庫建立使用者表？
  - 答：僅用 AAD 外部登入時不需要。只有當你使用 ASP.NET Core Identity（本地帳號/密碼）或需要持久化本地使用者資料時，才需要。
- 問：CallbackPath 是什麼？
  - 答：OIDC 的回呼端點，預設常見為 `/signin-oidc`，需與 Azure AD App 註冊一致。
- 問：如何控管角色？
  - 答：優先考慮 AAD App Roles 或群組宣告，避免自行維護本地角色表。
