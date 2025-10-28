# 登入畫面與路由安全設計計劃

## 🔐 資安原則

### 1. 預設拒絕原則
- **所有路由預設需要認證**
- **明確標記允許匿名存取的頁面**
- **分層權限控制**

### 2. 認證流程
- **Azure AD 單一登入**
- **自動重導向到登入頁面**
- **登入後返回原始頁面**

---

## 🛣️ 路由設計

### 1. 公開路由 (AllowAnonymous)
```
/Account/Login          - 登入頁面
/Account/SignIn         - Azure AD 登入重導向
/Account/SignOut        - 登出處理
/Account/AccessDenied   - 權限不足頁面
/Error                  - 錯誤頁面
/Health                 - 健康檢查 (可選)
```

### 2. 受保護路由 (需要認證)
```
/Home/Index             - 首頁儀表板
/Case/*                 - 個案管理 (所有功能)
/Attendance/*           - 考勤管理 (未來)
/Inventory/*            - 物資管理 (未來)
/Reports/*              - 報表系統 (未來)
/Admin/*                - 系統管理 (需要 Admin 角色)
```

### 3. API 路由
```
/api/case/*             - 個案 API
/api/attendance/*       - 考勤 API
/api/admin/*            - 管理 API
```

---

## 🎨 登入畫面設計

### 1. 頁面結構
```html
<!DOCTYPE html>
<html>
<head>
    <title>CanLove 協會管理系統 - 登入</title>
    <link href="~/css/login.css" rel="stylesheet" />
</head>
<body class="login-body">
    <div class="login-container">
        <div class="login-card">
            <div class="login-header">
                <h1>CanLove 協會管理系統</h1>
                <p>請登入以存取系統功能</p>
            </div>
            <div class="login-content">
                <a href="/Account/SignIn" class="btn-login">
                    <i class="fab fa-microsoft"></i>
                    使用 Microsoft 帳戶登入
                </a>
            </div>
        </div>
    </div>
</body>
</html>
```

### 2. CSS 樣式 (淺綠色系)
```css
/* 登入頁面專用樣式 */
.login-body {
    background: linear-gradient(135deg, #E8F5E8 0%, #C8E6C9 100%);
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    font-family: 'Microsoft JhengHei', sans-serif;
}

.login-container {
    width: 100%;
    max-width: 400px;
    padding: 20px;
}

.login-card {
    background: white;
    border-radius: 12px;
    box-shadow: 0 8px 32px rgba(46, 125, 50, 0.1);
    border: 1px solid #C8E6C9;
    overflow: hidden;
}

.login-header {
    background: linear-gradient(135deg, #4CAF50 0%, #66BB6A 100%);
    color: white;
    padding: 30px 20px;
    text-align: center;
}

.login-header h1 {
    margin: 0 0 10px 0;
    font-size: 24px;
    font-weight: 600;
}

.login-header p {
    margin: 0;
    opacity: 0.9;
    font-size: 14px;
}

.login-content {
    padding: 40px 30px;
}

.btn-login {
    display: block;
    width: 100%;
    background: #4CAF50;
    color: white;
    text-decoration: none;
    padding: 15px 20px;
    border-radius: 8px;
    text-align: center;
    font-size: 16px;
    font-weight: 500;
    transition: all 0.3s ease;
    border: none;
    cursor: pointer;
}

.btn-login:hover {
    background: #45A049;
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(76, 175, 80, 0.3);
}

.btn-login i {
    margin-right: 10px;
    font-size: 18px;
}
```

---

## 🔧 實作建議

### 1. 更新 Program.cs

```csharp
// 在 Program.cs 中添加全域認證要求
builder.Services.AddControllersWithViews(options =>
{
    // 全域認證要求
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddMicrosoftIdentityUI();

// 添加認證中介軟體
app.UseAuthentication();
app.UseAuthorization();

// 添加認證重導向中介軟體
app.Use(async (context, next) =>
{
    if (!context.User.Identity?.IsAuthenticated == true && 
        !context.Request.Path.StartsWithSegments("/Account") &&
        !context.Request.Path.StartsWithSegments("/Error") &&
        !context.Request.Path.StartsWithSegments("/Health"))
    {
        context.Response.Redirect("/Account/Login?returnUrl=" + 
            Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
        return;
    }
    await next();
});
```

### 2. 更新 AccountController

```csharp
[AllowAnonymous]
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
    {
        // 如果已經登入，重導向到首頁
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpGet]
    public IActionResult SignIn(string returnUrl = "/")
    {
        var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/Home/Index";
        var properties = new AuthenticationProperties 
        { 
            RedirectUri = redirectUrl,
            Items = { { "returnUrl", redirectUrl } }
        };
        
        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public new IActionResult SignOut()
    {
        var callbackUrl = Url.Action("Login", "Account", new { area = "" }, Request.Scheme);
        return base.SignOut(
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
```

### 3. 建立登入頁面

```html
@* Views/Account/Login.cshtml *@
@{
    ViewData["Title"] = "登入 - CanLove 協會管理系統";
    Layout = "_LoginLayout";
}

<div class="login-container">
    <div class="login-card">
        <div class="login-header">
            <h1>CanLove 協會管理系統</h1>
            <p>請登入以存取協會管理功能</p>
        </div>
        <div class="login-content">
            <a href="/Account/SignIn?returnUrl=@ViewBag.ReturnUrl" class="btn-login">
                <i class="fab fa-microsoft"></i>
                使用 Microsoft 帳戶登入
            </a>
            
            @if (!string.IsNullOrEmpty(ViewBag.ReturnUrl))
            {
                <p class="return-notice">
                    登入後將返回：<code>@ViewBag.ReturnUrl</code>
                </p>
            }
        </div>
    </div>
</div>
```

### 4. 建立登入專用 Layout

```html
@* Views/Shared/_LoginLayout.cshtml *@
<!DOCTYPE html>
<html lang="zh-TW">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"]</title>
    <link href="~/css/login.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
</head>
<body class="login-body">
    @RenderBody()
</body>
</html>
```

---

## 🛡️ 安全強化建議

### 1. 認證保護
```csharp
// 在需要特定角色的控制器上使用
[Authorize(Policy = "RequireAdmin")]
public class AdminController : Controller
{
    // 管理功能
}

[Authorize(Policy = "RequireSocialWorker")]
public class CaseController : Controller
{
    // 個案管理功能
}
```

### 2. 防止直接 URL 存取
```csharp
// 在 Program.cs 中添加
app.Use(async (context, next) =>
{
    // 檢查是否為 AJAX 請求
    if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" && 
        !context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.StatusCode = 401;
        return;
    }
    await next();
});
```

### 3. 安全標頭
```csharp
// 添加安全標頭
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

---

## 📱 響應式設計

### 手機版優化
```css
@media (max-width: 768px) {
    .login-container {
        padding: 10px;
    }
    
    .login-card {
        border-radius: 8px;
    }
    
    .login-header {
        padding: 20px 15px;
    }
    
    .login-header h1 {
        font-size: 20px;
    }
    
    .login-content {
        padding: 30px 20px;
    }
}
```

---

## 🔄 登入流程

1. **使用者存取受保護頁面**
2. **系統檢查認證狀態**
3. **未認證 → 重導向到 `/Account/Login`**
4. **使用者點擊登入按鈕**
5. **重導向到 Azure AD 登入**
6. **Azure AD 認證成功**
7. **重導向回原始頁面**

---

## ⚠️ 注意事項

1. **HTTPS 強制**：生產環境必須使用 HTTPS
2. **Session 管理**：設定適當的 Session 超時時間
3. **錯誤處理**：妥善處理認證失敗情況
4. **日誌記錄**：記錄所有登入嘗試
5. **測試**：完整測試各種登入情境

---

*這個設計確保了系統的安全性，同時提供了良好的使用者體驗。*
