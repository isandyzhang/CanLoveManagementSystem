// ============================================================================
// Program.cs - ASP.NET Core 應用程式啟動設定檔
// ============================================================================
// 此檔案負責：
// 1. 配置應用程式服務（依賴注入）
// 2. 設定認證與授權（OpenID Connect + Microsoft Identity）
// 3. 配置 HTTP 請求管道（中介軟體順序）
// 4. 設定路由規則
// ============================================================================

using CanLove_Backend.Infrastructure.Data.Contexts;
using CanLove_Backend.Core.Extensions;
using CanLove_Backend.Core.Authentication;
using CanLove_Backend.Core.DataProtection;
using CanLove_Backend.Domain.Case.Services.Basic;
using CanLove_Backend.Domain.Case.Services.Opening;
using CanLove_Backend.Domain.Case.Services.Opening.Steps;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Domain.Staff.Services;
using CanLove_Backend.Infrastructure.Storage.Blob;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CanLove_Backend.Infrastructure.Options.Services;
using CanLove_Backend.Application.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Azure.Identity;
using System.IO;

// 建立 WebApplicationBuilder，這是 .NET 6+ 的新方式（取代 Startup.cs）
var builder = WebApplication.CreateBuilder(args);

// 判斷是否為開發環境（用於條件式配置）
var isDevelopment = builder.Environment.IsDevelopment();

// ============================================================================
// 1. 配置 Kestrel Web 伺服器
// ============================================================================
// Kestrel 是 ASP.NET Core 內建的跨平台 Web 伺服器
// 這裡設定請求標頭大小限制，避免 HTTP 431 錯誤（Request Header Fields Too Large）
// 原因：OpenID Connect 認證流程中，URL 可能包含大量參數，導致標頭過大
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestHeadersTotalSize = 65536; // 64KB - 所有標頭的總大小限制
    options.Limits.MaxRequestHeaderCount = 100;        // 最多 100 個標頭
    options.Limits.MaxRequestLineSize = 16384;          // 16KB - 請求行的最大長度
});

// ============================================================================
// 2. 載入 Azure Key Vault（可選）
// ============================================================================
// Key Vault 用於安全儲存敏感資訊（如資料庫連接字串、API 金鑰等）
// 開發環境：連接失敗時只顯示警告，不中斷啟動
// 生產環境：連接失敗時拋出異常，確保敏感資訊安全
var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    try
    {
        // 使用 Managed Identity 或 Service Principal 連接 Key Vault
        builder.Configuration.AddAzureKeyVaultWithIdentity(builder.Environment);
    }
    catch (Exception ex)
    {
        if (isDevelopment)
        {
            // 開發環境：允許失敗（可以使用 appsettings.Development.json）
            Console.WriteLine($"[Warning] Key Vault 連接失敗（開發環境可忽略）: {ex.Message}");
        }
        else
        {
            // 生產環境：必須成功連接，否則無法取得敏感資訊
            throw new InvalidOperationException(
                $"無法連接 Azure Key Vault: {ex.Message}。請檢查 Managed Identity 設定。", ex);
        }
    }
}

// ============================================================================
// 3. 配置核心服務（依序執行，順序很重要）
// ============================================================================

// 3.1 Data Protection（資料保護）
// 用途：加密/解密 Cookie、Token 等敏感資料
// 重要：必須持久化金鑰，否則重啟後無法解密之前的 Cookie，導致 "Correlation failed"
ConfigureDataProtection(builder, keyVaultUri, isDevelopment);

// 3.2 認證（Authentication）
// 用途：驗證使用者身份（誰是使用者？）
// 包含：OpenID Connect 設定、Cookie 設定、事件處理器
ConfigureAuthentication(builder, isDevelopment);

// 3.3 授權（Authorization）
// 用途：決定使用者可以做什麼（使用者有什麼權限？）
// 注意：必須在認證之後配置
ConfigureAuthorization(builder);

// 3.4 資料庫
// 用途：註冊 Entity Framework Core DbContext
ConfigureDatabase(builder);

// 3.5 應用程式服務
// 用途：註冊所有業務邏輯服務（Service、Repository 等）
// 使用依賴注入（DI）模式，讓服務可以自動注入到 Controller 中
RegisterApplicationServices(builder);

// ============================================================================
// 4. 建立應用程式實例
// ============================================================================
var app = builder.Build();

// ============================================================================
// 5. 配置 HTTP 請求管道（中介軟體順序非常重要！）
// ============================================================================
// 中介軟體執行順序：由上到下，請求時正向執行，回應時反向執行
// 順序錯誤可能導致認證失敗、路由無法匹配等問題

// 5.1 開發環境專用工具
if (app.Environment.IsDevelopment())
{
    // Swagger：API 文件工具（僅開發環境使用）
    app.UseSwagger();      // 產生 OpenAPI JSON 文件
    app.UseSwaggerUI();    // 提供 Swagger UI 介面（瀏覽器可訪問）
    
    // 開發者例外頁面：顯示詳細錯誤資訊（僅開發環境）
    // 生產環境應該使用自訂錯誤頁面，避免洩露敏感資訊
    app.UseDeveloperExceptionPage();
}

// 5.2 HTTPS 重定向
// 重要：使用 FormPost 模式時，必須使用 HTTPS
// 原因：
//   - HTTP + FormPost + SameSite=Lax = Correlation Cookie 無法傳遞（瀏覽器安全限制）
//   - HTTPS + FormPost + SameSite=None = 正常工作
// 解決方案：使用 https://localhost:7217 而不是 http://localhost:5239
app.UseHttpsRedirection();

// 5.3 靜態檔案（CSS、JS、圖片等）
// 從 wwwroot 目錄提供靜態檔案
app.UseStaticFiles();

// 5.4 路由
// 必須在 UseAuthentication/UseAuthorization 之前
// 因為路由需要知道請求要送到哪個 Controller/Action
app.UseRouting();

// 5.5 認證與授權（順序不可對調！）
// UseAuthentication：驗證使用者身份（設定 User.Identity）
// UseAuthorization：檢查使用者權限（必須在 UseAuthentication 之後）
// 為什麼順序重要？
//   - Authorization 需要知道使用者是誰（由 Authentication 提供）
//   - 如果順序錯誤，User.Identity 會是 null，導致授權失敗
app.UseAuthentication();
app.UseAuthorization();

// 5.6 CORS（跨來源資源共享）
// 允許前端應用程式（不同域名）訪問此 API
app.UseCors("AllowAll");

// ============================================================================
// 6. 路由配置（由上到下匹配，第一個匹配的路由會被使用）
// ============================================================================

// 6.1 根路徑（/）的特殊處理
// 用途：根據登入狀態決定導向首頁或登入頁
// 必須在其他路由之前，因為它優先匹配根路徑
app.MapGet("/", (HttpContext context) =>
{
    // 檢查使用者是否已登入（由 UseAuthentication 中介軟體設定）
    if (context.User.Identity?.IsAuthenticated == true)
    {
        // 已登入：導向首頁
        return Results.Redirect("/Home/Index");
    }
    // 未登入：導向登入頁面
    return Results.Redirect("/Account/Login");
});

// 6.2 個案基本資料路由（自訂路由）
// 模式：CaseBasic/{action=Query}/{id?}
// 範例：/CaseBasic/Query、/CaseBasic/Edit/123
app.MapControllerRoute(
    name: "caseBasic",
    pattern: "CaseBasic/{action=Query}/{id?}",
    defaults: new { controller = "CaseBasic" });

// 6.3 預設控制器路由（MVC 標準路由）
// 模式：{controller=Home}/{action=Index}/{id?}
// 範例：/Home/Index、/Account/Login、/CaseBasic/Edit/123
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 6.4 API 路由（用於 API Controller）
// 使用屬性路由（[Route]、[HttpGet] 等）
app.MapControllers();

app.Run();

// ========== 輔助方法 ==========

/// <summary>
/// 配置 Data Protection（資料保護）
/// </summary>
/// <param name="builder">WebApplicationBuilder</param>
/// <param name="keyVaultUri">Key Vault URI（可選）</param>
/// <param name="isDevelopment">是否為開發環境</param>
/// 
/// <remarks>
/// Data Protection 用途：
/// 1. 加密/解密 Cookie（認證 Cookie、Correlation Cookie 等）
/// 2. 保護敏感資料（如 CSRF Token）
/// 
/// 為什麼需要持久化金鑰？
/// - 如果金鑰只存在記憶體中，應用程式重啟後會產生新金鑰
/// - 新金鑰無法解密舊 Cookie，導致 "Correlation failed" 錯誤
/// - 持久化金鑰可以跨重啟使用，避免此問題
/// 
/// 金鑰儲存位置：
/// - 開發環境：本地檔案系統（DataProtection-Keys 目錄）
/// - 生產環境：建議使用 Azure Key Vault 或共享儲存（多伺服器環境）
/// </remarks>
static void ConfigureDataProtection(WebApplicationBuilder builder, string? keyVaultUri, bool isDevelopment)
{
    // 建立金鑰儲存目錄
    var keysDirectory = new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys"));
    
    // 配置 Data Protection
    builder.Services.AddDataProtection()
        .SetApplicationName("CanLove_Backend")  // 應用程式名稱（用於金鑰隔離）
        .PersistKeysToFileSystem(keysDirectory); // 持久化到檔案系統
    
    Console.WriteLine("[DataProtection] 使用簡化配置：金鑰儲存在本地檔案系統");
    
    // 未來可以改為使用 Key Vault（生產環境推薦）
    // if (!string.IsNullOrWhiteSpace(keyVaultUri)) { ... }
}

/// <summary>
/// 取得 Azure Token Credential（用於連接 Azure 服務）
/// </summary>
/// <param name="configuration">設定檔</param>
/// <returns>TokenCredential 實例</returns>
/// 
/// <remarks>
/// 此函數目前未使用，但保留作為未來擴展用（如連接 Key Vault）
/// 
/// 認證方式優先順序：
/// 1. Service Principal（ClientSecretCredential）：使用 ClientId + ClientSecret
/// 2. Managed Identity（DefaultAzureCredential）：使用 Azure 資源的受控識別
/// 
/// Managed Identity 優點：
/// - 不需要儲存密碼或金鑰
/// - 自動輪換憑證
/// - 更安全（符合零信任原則）
/// </remarks>
#pragma warning disable CS8321 // 此函數預留給未來使用
static Azure.Core.TokenCredential GetTokenCredential(IConfiguration configuration)
{
    // 優先嘗試使用 Service Principal（如果設定檔中有提供）
    var clientId = configuration["KeyVault:ClientId"];
    var clientSecret = configuration["KeyVault:ClientSecret"];
    var tenantId = configuration["KeyVault:TenantId"];

    if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(tenantId))
    {
        // 使用 Service Principal 認證
        return new ClientSecretCredential(tenantId, clientId, clientSecret);
    }

    // 否則使用 Managed Identity（推薦方式）
    var defaultOptions = new DefaultAzureCredentialOptions();
    var userAssignedManagedIdentityClientId = configuration["ManagedIdentityClientId"];
    if (!string.IsNullOrEmpty(userAssignedManagedIdentityClientId))
    {
        // 指定使用特定的 User-Assigned Managed Identity
        defaultOptions.ManagedIdentityClientId = userAssignedManagedIdentityClientId;
    }
    // 使用 DefaultAzureCredential（會自動嘗試多種認證方式）
    return new DefaultAzureCredential(defaultOptions);
}

/// <summary>
/// 配置認證（Authentication）
/// </summary>
/// <param name="builder">WebApplicationBuilder</param>
/// <param name="isDevelopment">是否為開發環境</param>
/// 
/// <remarks>
/// 認證流程說明（OpenID Connect）：
/// 1. 使用者訪問受保護的頁面 → 重定向到 Azure AD 登入頁
/// 2. 使用者輸入帳密 → Azure AD 驗證
/// 3. Azure AD 回調到應用程式（FormPost）→ 帶回授權碼
/// 4. 應用程式用授權碼換取 Token → 建立認證 Cookie
/// 5. 後續請求使用認證 Cookie 識別使用者
/// 
/// Cookie 類型說明：
/// - Correlation Cookie：用於驗證回調請求的合法性（防止 CSRF）
/// - Nonce Cookie：用於防止重放攻擊
/// - 認證 Cookie：儲存使用者身份資訊（持久化）
/// </remarks>
static void ConfigureAuthentication(WebApplicationBuilder builder, bool isDevelopment)
{
    // ========================================================================
    // 步驟 1：註冊 OpenID Connect 認證服務
    // ========================================================================
    // 使用 Microsoft Identity Web 套件，簡化 Azure AD 整合
    builder.Services
        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)  // 預設認證方案
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd")); // 從 appsettings.json 讀取 Azure AD 設定

    // ========================================================================
    // 步驟 2：定義 Cookie 過期時間和安全性策略
    // ========================================================================
    // 注意：不同類型的 Cookie 有不同的過期時間需求
    var correlationCookieExpiration = TimeSpan.FromMinutes(15);  // Correlation Cookie：認證流程用，15 分鐘足夠
    var authenticationCookieExpiration = TimeSpan.FromMinutes(30); // 認證 Cookie：會話用，30 分鐘
    
    // Secure Policy：決定 Cookie 是否只能在 HTTPS 下傳輸
    var cookieSecurePolicy = isDevelopment
        ? CookieSecurePolicy.SameAsRequest  // 開發環境：允許 HTTP 和 HTTPS（方便測試）
        : CookieSecurePolicy.Always;        // 生產環境：只允許 HTTPS（安全性）

    // ========================================================================
    // 步驟 3：註冊認證事件處理器
    // ========================================================================
    // 用途：在認證流程的各個階段執行自訂邏輯
    // 例如：自動建立/更新員工資料、記錄登入日誌
    builder.Services.AddScoped<AuthenticationEvents>();

    // ========================================================================
    // 步驟 4：配置 OpenID Connect 選項
    // ========================================================================
    builder.Services.Configure<OpenIdConnectOptions>(
        OpenIdConnectDefaults.AuthenticationScheme,
        options =>
        {
            // 4.1 指定事件處理器類型
            options.EventsType = typeof(AuthenticationEvents);
            
            // 4.2 PKCE（Proof Key for Code Exchange）
            // 用途：提升安全性，防止授權碼被竊取
            // 原理：使用動態生成的 code_verifier 和 code_challenge
            options.UsePkce = true;
            
            // 4.3 回應模式：FormPost
            // 為什麼使用 FormPost 而不是 Query？
            // - Query 模式：參數放在 URL 中，可能超過瀏覽器限制（HTTP 431 錯誤）
            // - FormPost 模式：參數放在 POST body 中，沒有長度限制
            // 注意：FormPost 需要配合 SameSite=None 才能正常工作
            options.ResponseMode = OpenIdConnectResponseMode.FormPost;
            
            // 4.4 跳過無法識別的請求
            // 用途：優雅處理邊緣情況（如直接訪問回調 URL、Cookie 丟失等）
            // 效果：避免拋出 "Correlation failed" 異常，改為跳過請求
            options.SkipUnrecognizedRequests = true;
            
            // 4.5 協議驗證器設定
            // RequireState：驗證 State 參數（防止 CSRF 攻擊）
            // RequireNonce：驗證 Nonce 參數（防止重放攻擊）
            options.ProtocolValidator.RequireState = true;
            options.ProtocolValidator.RequireNonce = true;
            
            // ====================================================================
            // 4.6 Correlation Cookie 設定（關鍵！）
            // ====================================================================
            // SameSite 設定說明：
            // - Lax：同站請求會發送 Cookie，跨站 GET 會發送，跨站 POST 不會發送
            // - None：所有請求都會發送 Cookie（需要 Secure，即 HTTPS）
            // - Strict：只有同站請求會發送 Cookie（最嚴格）
            //
            // 為什麼使用 None？
            // - FormPost 回調是跨站 POST 請求（從 Azure AD 回調到應用程式）
            // - 如果使用 Lax，跨站 POST 不會發送 Cookie，導致 Correlation failed
            // - 使用 None 可以確保跨站 POST 也能發送 Cookie
            // - 但必須配合 Secure（HTTPS），否則瀏覽器會拒絕
            var correlationSameSite = SameSiteMode.None; // 使用 None 以確保 FormPost 正常工作（需要 HTTPS）
            
            // Correlation Cookie 設定
            options.CorrelationCookie.SameSite = correlationSameSite;
            options.CorrelationCookie.HttpOnly = true;  // 防止 JavaScript 存取（防 XSS）
            options.CorrelationCookie.Path = "/";        // Cookie 路徑
            options.CorrelationCookie.Expiration = correlationCookieExpiration; // 過期時間
            options.CorrelationCookie.SecurePolicy = cookieSecurePolicy; // Secure 策略
            
            // Nonce Cookie 設定（與 Correlation Cookie 相同）
            options.NonceCookie.SameSite = correlationSameSite;
            options.NonceCookie.HttpOnly = true;
            options.NonceCookie.Path = "/";
            options.NonceCookie.Expiration = correlationCookieExpiration;
            options.NonceCookie.SecurePolicy = cookieSecurePolicy;
        });

    // ========================================================================
    // 步驟 5：配置 Cookie 認證選項
    // ========================================================================
    // 用途：設定認證 Cookie 的行為（會話管理）
    builder.Services.Configure<CookieAuthenticationOptions>(
        CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            // 5.1 路由設定
            options.LoginPath = "/Account/Login";        // 未登入時導向的登入頁面
            options.AccessDeniedPath = "/Account/AccessDenied"; // 權限不足時導向的頁面
            
            // 5.2 Cookie 安全性設定
            options.Cookie.HttpOnly = true;  // 防止 JavaScript 存取 Cookie（防 XSS 攻擊）
            options.Cookie.SameSite = SameSiteMode.Lax;  // 防 CSRF 攻擊（認證 Cookie 使用 Lax 即可）
            options.Cookie.SecurePolicy = cookieSecurePolicy; // Secure 策略
            options.Cookie.Path = "/"; // Cookie 路徑
            
            // 5.3 會話管理
            options.ExpireTimeSpan = authenticationCookieExpiration; // Cookie 過期時間（30 分鐘）
            options.SlidingExpiration = true; // 滑動過期：每次請求後重新計算過期時間
            // 例如：使用者每 10 分鐘訪問一次，Cookie 永遠不會過期
        });
}

/// <summary>
/// 配置授權（Authorization）
/// </summary>
/// <param name="builder">WebApplicationBuilder</param>
/// 
/// <remarks>
/// 授權與認證的區別：
/// - 認證（Authentication）：驗證使用者身份（誰是使用者？）
/// - 授權（Authorization）：決定使用者權限（使用者可以做什麼？）
/// 
/// FallbackPolicy：
/// - 預設授權策略，適用於所有未明確指定授權的端點
/// - RequireAuthenticatedUser：要求使用者必須已登入
/// - 效果：未登入的使用者無法訪問任何頁面（除了明確允許的，如登入頁）
/// </remarks>
static void ConfigureAuthorization(WebApplicationBuilder builder)
{
    builder.Services.AddAuthorization(options =>
    {
        // 設定預設授權策略：所有端點都需要登入
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()  // 要求使用者必須已認證
            .Build();
    });
}

/// <summary>
/// 配置資料庫
/// </summary>
/// <param name="builder">WebApplicationBuilder</param>
/// 
/// <remarks>
/// Entity Framework Core 設定：
/// - AddDbContext：註冊 DbContext 為 Scoped 生命週期
/// - Scoped：每個 HTTP 請求建立一個 DbContext 實例，請求結束時自動釋放
/// - 為什麼使用 Scoped？確保每個請求使用獨立的資料庫連線，避免併發問題
/// </remarks>
static void ConfigureDatabase(WebApplicationBuilder builder)
{
    // 從設定檔讀取連接字串（可能來自 appsettings.json 或 Key Vault）
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Console.Error.WriteLine("[Startup][Db] 未取得 ConnectionStrings:DefaultConnection，請確認 Key Vault 或 App Settings 覆寫。");
    }

    // 註冊 DbContext（使用 SQL Server）
    builder.Services.AddDbContext<CanLoveDbContext>(options =>
        options.UseSqlServer(connectionString));
}

/// <summary>
/// 註冊應用程式服務（依賴注入）
/// </summary>
/// <param name="builder">WebApplicationBuilder</param>
/// 
/// <remarks>
/// 依賴注入（DI）生命週期：
/// - Singleton：應用程式啟動時建立一次，整個生命週期共用同一個實例
/// - Scoped：每個 HTTP 請求建立一個實例，請求結束時釋放（最常用）
/// - Transient：每次注入時都建立新實例
/// 
/// 為什麼大部分服務使用 Scoped？
/// - 符合 HTTP 請求的生命週期
/// - 每個請求有獨立的服務實例，避免併發問題
/// - 請求結束時自動釋放資源（如資料庫連線）
/// </remarks>
static void RegisterApplicationServices(WebApplicationBuilder builder)
{
    var services = builder.Services;

    // ========================================================================
    // 共用服務（所有模組都會用到）
    // ========================================================================
    services.AddScoped<OptionService>();           // 選項資料服務（下拉選單等）
    services.AddScoped<SchoolService>();            // 學校資料服務
    services.AddScoped<IStaffService, StaffService>(); // 員工服務（使用介面，方便測試）
    services.AddScoped<IBlobService, BlobService>();    // Blob 儲存服務（檔案上傳）
    services.AddScoped<DataEncryptionService>();   // 資料加密服務（如身分證字號加密）

    // ========================================================================
    // Case 相關服務
    // ========================================================================
    services.AddScoped<CaseService>(); // 個案服務

    // ========================================================================
    // CaseOpening 相關服務（開案流程的 7 個步驟）
    // ========================================================================
    services.AddScoped<CaseWizard_S1_CD_Service>();      // 步驟 1：個案詳細資料
    services.AddScoped<CaseWizard_S2_CSWC_Service>();     // 步驟 2：社會工作服務內容
    services.AddScoped<CaseWizard_S3_CFQES_Service>();    // 步驟 3：經濟狀況評估
    services.AddScoped<CaseWizard_S4_CHQHS_Service>();    // 步驟 4：健康狀況評估
    services.AddScoped<CaseWizard_S5_CIQAP_Service>();    // 步驟 5：學業表現評估
    services.AddScoped<CaseWizard_S6_CEEE_Service>();     // 步驟 6：情緒評估
    services.AddScoped<CaseWizard_S7_FAS_Service>();      // 步驟 7：最後評估表

    // ========================================================================
    // 框架服務
    // ========================================================================
    // AutoMapper：物件對應（DTO ↔ Entity）
    services.AddAutoMapper(typeof(CaseMappingProfile));
    
    // MVC 服務（包含 Controller、View、Model Binding 等）
    services.AddControllersWithViews(); // 已包含 AddControllers() 的功能
    
    // Swagger/OpenAPI 服務（API 文件）
    services.AddEndpointsApiExplorer(); // 探索 API 端點
    services.AddSwaggerGen();           // 產生 Swagger 文件

    // ========================================================================
    // CORS（跨來源資源共享）
    // ========================================================================
    // 用途：允許前端應用程式（不同域名）訪問此 API
    // 注意：開發環境使用 AllowAll，生產環境應該限制特定域名
    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()    // 允許任何來源（開發環境）
                  .AllowAnyMethod()    // 允許任何 HTTP 方法（GET、POST 等）
                  .AllowAnyHeader();   // 允許任何標頭
        });
    });
}

