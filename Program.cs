using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// 載入 Key Vault（生產環境使用，開發環境可選擇性使用）
// Azure App Service 建議使用 Managed Identity（DefaultAzureCredential）
// 本機開發：可將 KeyVault:VaultUri 設為空或使用 ClientSecret 認證
var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    try
    {
        // 使用現有的擴展方法，支援 ClientSecret 或 DefaultAzureCredential
        builder.Configuration.AddAzureKeyVaultWithIdentity(builder.Environment);
    }
    catch (Exception ex)
    {
        // 開發環境：連接失敗時記錄警告但繼續運行（配置已在 appsettings 中）
        // 生產環境：連接失敗時拋出異常（應檢查 Managed Identity 設定）
        if (builder.Environment.IsDevelopment())
        {
            Console.WriteLine($"[Warning] Key Vault 連接失敗（開發環境可忽略）: {ex.Message}");
            // 開發環境不拋出異常，繼續使用 appsettings 中的配置
        }
        else
        {
            // 生產環境連接失敗是嚴重問題，重新拋出異常
            throw new InvalidOperationException(
                $"無法連接 Azure Key Vault: {ex.Message}。請檢查 Managed Identity 設定。", ex);
        }
    }
}

// 配置 Data Protection（持久化密钥，避免重启后 Correlation failed）
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
    .SetApplicationName("CanLove_Backend");

// 啟用 Microsoft Identity Web（OpenIdConnect）
builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// 設定認證事件，用於登入時同步員工資料
// 使用Configure來設定事件，因為需要在服務註冊完成後才能取得IStaffService
builder.Services.Configure<Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions>(
    Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme,
    options =>
    {
        var serviceScopeFactory = builder.Services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
        options.Events = new CanLove_Backend.Extensions.AuthenticationEvents(serviceScopeFactory);
    });

// 開發環境：配置 Cookie 策略，避免 Correlation failed
if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions>(
        Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        });
}

// 授權：預設要求已驗證使用者；需要匿名的頁面請加 [AllowAnonymous]
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// 添加 Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("[Startup][Db] 未取得 ConnectionStrings:DefaultConnection，請確認 Key Vault 或 App Settings 覆寫。");
}

builder.Services.AddDbContext<CanLoveDbContext>(options =>
    options.UseSqlServer(connectionString));

// 註冊自定義服務
// === 共用服務 ===
builder.Services.AddScoped<CanLove_Backend.Services.Shared.OptionService>();
builder.Services.AddScoped<CanLove_Backend.Services.Shared.SchoolService>();
builder.Services.AddScoped<CanLove_Backend.Services.Shared.AddressService>();
builder.Services.AddScoped<CanLove_Backend.Services.Shared.IStaffService, CanLove_Backend.Services.Shared.StaffService>();
builder.Services.AddScoped<CanLove_Backend.Services.Shared.IBlobService, CanLove_Backend.Services.Shared.BlobService>();

// === Case 相關服務 ===
builder.Services.AddScoped<CanLove_Backend.Services.Case.CaseService>();

// === CaseWizardOpenCase 相關服務 ===
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S1_CD_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S2_CSWC_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S3_CFQES_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S4_CHQHS_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S5_CIQAP_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S6_CEEE_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S7_FAS_Service>();

// === 其他服務 ===
// 添加 AutoMapper
builder.Services.AddAutoMapper(typeof(CanLove_Backend.Mappings.CaseMappingProfile));

// === MVC 和 API 支援 ===
builder.Services.AddControllersWithViews();

// 添加 API 支援
builder.Services.AddControllers();

// === 其他配置 ===
// 添加 Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// === 配置 HTTP 請求管道 ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // 啟用開發工具（包含 Hot Reload）
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 驗證與授權中介軟體（順序不可對調）
app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAll");

// === 路由配置 ===
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 個案相關路由
app.MapControllerRoute(
    name: "case",
    pattern: "Case/{action=Index}/{id?}",
    defaults: new { controller = "Case" });

// 已移除 Microsoft Identity Web 路由

// API 路由
app.MapControllers();

// 預設路由重導向
app.MapGet("/", () => Results.Redirect("/Home/Index"));

app.Run();
