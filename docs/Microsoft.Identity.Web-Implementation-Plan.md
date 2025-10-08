# Microsoft.Identity.Web 實作計畫

## 📋 專案現況分析

### 目前專案狀態
- **框架**: ASP.NET Core 9.0 MVC + Web API
- **資料庫**: SQL Server (Azure)
- **已整合**: Azure Key Vault, AutoMapper, Entity Framework Core
- **租戶資訊**: 已有 Key Vault 設定，包含 TenantId: `d28bc843-6061-4db3-b8a8-16ec1daf4164`

### 目標
- 使用 Microsoft 365 帳號登入
- 登入後在導覽列顯示使用者資訊
- 基於角色進行授權控制

---

## 🏗️ 架構設計

### 1. 驗證流程
```
使用者 → 點擊登入 → Microsoft 登入頁 → 完成驗證 → 回調到 /signin-oidc → 產生 ClaimsPrincipal → 顯示使用者資訊
```

### 2. 角色策略選擇
**推薦使用 App Roles** (而非 Group Claims)
- 優點：更乾淨、易管理、與 M365 原生整合
- 角色定義：`Admin`, `SocialWorker`, `Viewer`

### 3. 資料流
```
Entra ID → Token (含 roles) → ClaimsPrincipal → 本地授權判斷 → UI 顯示
```

---

## 📦 需要安裝的套件

```xml
<PackageReference Include="Microsoft.Identity.Web" Version="2.17.1" />
<PackageReference Include="Microsoft.Identity.Web.UI" Version="2.17.1" />
```

---

## ⚙️ 設定步驟

### 1. Entra ID 應用程式註冊設定

#### 在 Azure Portal 中：
1. 進入 **Azure Active Directory** → **應用程式註冊**
2. 建立新註冊：
   - 名稱：`CanLove Case Management System`
   - 支援的帳戶類型：`僅此組織目錄中的帳戶`
   - 重新導向 URI：`https://localhost:5001/signin-oidc` (開發環境)

#### 設定 App Roles：
1. 在應用程式註冊中，點擊 **應用程式角色**
2. 新增角色：
   ```json
   {
     "id": "admin-role-id",
     "allowedMemberTypes": ["User"],
     "description": "系統管理員，可管理所有功能",
     "displayName": "Admin",
     "isEnabled": true,
     "value": "Admin"
   }
   ```
   ```json
   {
     "id": "socialworker-role-id", 
     "allowedMemberTypes": ["User"],
     "description": "社工，可管理個案資料",
     "displayName": "SocialWorker",
     "isEnabled": true,
     "value": "SocialWorker"
   }
   ```
   ```json
   {
     "id": "viewer-role-id",
     "allowedMemberTypes": ["User"], 
     "description": "檢視者，只能檢視資料",
     "displayName": "Viewer",
     "isEnabled": true,
     "value": "Viewer"
   }
   ```

#### 指派使用者到角色：
1. 在 **企業應用程式** 中找到你的應用程式
2. 點擊 **使用者和群組** → **新增使用者/群組**
3. 選擇使用者並指派對應角色

### 2. 應用程式設定

#### appsettings.json 新增：
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "canlove.org.tw",
    "TenantId": "d28bc843-6061-4db3-b8a8-16ec1daf4164",
    "ClientId": "你的應用程式 ClientId",
    "ClientSecret": "你的應用程式 ClientSecret",
    "CallbackPath": "/signin-oidc"
  }
}
```

#### 將 ClientSecret 移到 Key Vault：
- 在 Key Vault 中建立 secret：`CanLove-ClientSecret`
- 在 appsettings.json 中移除 ClientSecret，改為：
```json
"ClientSecret": "@Microsoft.KeyVault(SecretUri=https://canlove-case.vault.azure.net/secrets/CanLove-ClientSecret/)"
```

---

## 💻 程式碼實作

### 1. Program.cs 修改

```csharp
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// 添加 Key Vault 配置
builder.Configuration.AddAzureKeyVaultIfProduction(builder.Environment);

// 添加 Microsoft Identity Web 驗證
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

// 添加授權策略
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => 
        policy.RequireRole("Admin"));
    options.AddPolicy("RequireSocialWorker", policy => 
        policy.RequireRole("SocialWorker", "Admin"));
    options.AddPolicy("RequireViewer", policy => 
        policy.RequireRole("Viewer", "SocialWorker", "Admin"));
});

// 添加 Microsoft Identity Web UI (提供登入/登出頁面)
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

// 現有的服務註冊...
builder.Services.AddDbContext<CanLoveDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ... 其他現有服務

var app = builder.Build();

// 配置 HTTP 請求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 添加驗證和授權中介軟體
app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAll");

// 路由設定
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "case",
    pattern: "Case/{action=Index}/{id?}",
    defaults: new { controller = "Case" });

app.MapControllers();

app.Run();
```

### 2. 新增 AccountController

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanLove_Backend.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult SignIn(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("Index", "Home", new { area = "" });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            var callbackUrl = Url.Action("Index", "Home", new { area = "" });
            return SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
```

### 3. 修改現有控制器加入授權

#### HomeController.cs
```csharp
[Authorize] // 需要登入才能訪問
public class HomeController : Controller
{
    // 現有方法...
}
```

#### CaseController.cs
```csharp
[Authorize(Policy = "RequireSocialWorker")] // 需要社工或管理員權限
public class CaseController : Controller
{
    // 現有方法...
}
```

#### SchoolController.cs
```csharp
[Authorize(Policy = "RequireAdmin")] // 需要管理員權限
public class SchoolController : Controller
{
    // 現有方法...
}
```

### 4. 修改 _Layout.cshtml

```html
<!DOCTYPE html>
<html lang="zh-TW">
<head>
    <!-- 現有的 head 內容 -->
</head>
<body>
    <header>
        <nav class="navbar navbar-expand-sm navbar-dark bg-primary">
            <div class="container">
                <a class="navbar-brand" href="@Url.Action("Index", "Home")">
                    <i class="bi bi-heart-fill me-2"></i>CanLove
                </a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target=".navbar-collapse">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="navbar-collapse collapse d-sm-inline-flex justify-content-between">
                    <ul class="navbar-nav flex-grow-1">
                        <li class="nav-item">
                            <a class="nav-link" href="@Url.Action("Index", "Home")">
                                <i class="bi bi-house me-1"></i>首頁
                            </a>
                        </li>
                        
                        @if (User?.Identity?.IsAuthenticated ?? false)
                        {
                            <li class="nav-item">
                                <a class="nav-link" href="@Url.Action("Index", "Case")">
                                    <i class="bi bi-people me-1"></i>個案管理
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" href="@Url.Action("Create", "Case")">
                                    <i class="bi bi-plus-circle me-1"></i>新增個案
                                </a>
                            </li>
                            
                            @if (User.IsInRole("Admin"))
                            {
                                <li class="nav-item">
                                    <a class="nav-link" href="@Url.Action("Index", "School")">
                                        <i class="bi bi-building me-1"></i>學校管理
                                    </a>
                                </li>
                            }
                        }
                    </ul>
                    
                    <!-- 使用者資訊區域 -->
                    <ul class="navbar-nav">
                        @if (User?.Identity?.IsAuthenticated ?? false)
                        {
                            <li class="nav-item dropdown">
                                <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">
                                    <i class="bi bi-person-circle me-1"></i>
                                    @(User.FindFirst("name")?.Value ?? User.Identity?.Name ?? "使用者")
                                </a>
                                <ul class="dropdown-menu">
                                    <li><span class="dropdown-item-text">
                                        <small class="text-muted">
                                            角色: @string.Join(", ", User.FindAll("roles").Select(c => c.Value))
                                        </small>
                                    </span></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a class="dropdown-item" href="@Url.Action("SignOut", "Account")">
                                        <i class="bi bi-box-arrow-right me-1"></i>登出
                                    </a></li>
                                </ul>
                            </li>
                        }
                        else
                        {
                            <li class="nav-item">
                                <a class="nav-link" href="@Url.Action("SignIn", "Account")">
                                    <i class="bi bi-box-arrow-in-right me-1"></i>登入
                                </a>
                            </li>
                        }
                    </ul>
                </div>
            </div>
        </nav>
    </header>
    
    <!-- 現有的 body 內容 -->
</body>
</html>
```

### 5. 新增 AccessDenied 頁面

#### Views/Account/AccessDenied.cshtml
```html
@{
    ViewData["Title"] = "存取被拒絕";
}

<div class="container mt-5">
    <div class="row justify-content-center">
        <div class="col-md-6">
            <div class="card">
                <div class="card-body text-center">
                    <i class="bi bi-shield-exclamation text-warning" style="font-size: 4rem;"></i>
                    <h2 class="mt-3">存取被拒絕</h2>
                    <p class="text-muted">您沒有權限存取此頁面。</p>
                    <a href="@Url.Action("Index", "Home")" class="btn btn-primary">
                        <i class="bi bi-house me-1"></i>回到首頁
                    </a>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## 🔧 本地開發設定

### 1. User Secrets (開發環境)
```bash
dotnet user-secrets set "AzureAd:ClientId" "你的應用程式ClientId"
dotnet user-secrets set "AzureAd:ClientSecret" "你的應用程式ClientSecret"
```

### 2. 本機測試 URL
- 應用程式 URL: `https://localhost:5001`
- 回調 URL: `https://localhost:5001/signin-oidc`

---

## 🚀 部署設定

### 1. 生產環境設定
- 在 Azure Portal 中新增生產環境的回調 URL
- 將 ClientSecret 存到 Key Vault
- 更新 appsettings.Production.json

### 2. GitHub Actions 部署
- 確保 Key Vault 權限設定正確
- 驗證環境變數設定

---

## 🧪 測試檢查清單

### 功能測試
- [ ] 未登入時訪問受保護頁面會重導向到登入
- [ ] 登入成功後顯示使用者姓名
- [ ] 登入後導覽列顯示對應權限的選單
- [ ] 不同角色看到不同的功能選單
- [ ] 登出功能正常運作
- [ ] 權限不足時顯示 AccessDenied 頁面

### 角色測試
- [ ] Admin 角色可看到所有功能
- [ ] SocialWorker 角色可管理個案但看不到學校管理
- [ ] Viewer 角色只能檢視資料

---

## 📝 後續擴展

### 1. 本地使用者對應 (可選)
如果需要追蹤本地使用者資料：
```csharp
public class User
{
    public string ObjectId { get; set; } // Entra ID 的 oid claim
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public DateTime LastSignInAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 2. 審計日誌整合
在現有的 `UserActivityLog` 中記錄：
- 登入/登出事件
- 使用者操作記錄
- 權限檢查記錄

### 3. 進階授權
- 基於資源的授權 (例如：只能編輯自己負責的個案)
- 時間基礎的授權
- 條件式授權

---

## ⚠️ 注意事項

1. **安全性**
   - 確保所有機密資訊都存放在 Key Vault
   - 使用 HTTPS 進行所有通訊
   - 定期輪換 ClientSecret

2. **效能**
   - Token 快取設定適當的過期時間
   - 考慮使用分散式快取 (Redis) 在生產環境

3. **監控**
   - 設定登入失敗的監控
   - 記錄權限檢查失敗的事件
   - 監控 Token 過期和重新整理

---

## 📚 參考資源

- [Microsoft.Identity.Web 官方文件](https://docs.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [ASP.NET Core 驗證和授權](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Azure AD App Roles](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps)
