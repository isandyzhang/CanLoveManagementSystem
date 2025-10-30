using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = WebApplication.CreateBuilder(args);

// 先嘗試載入 Key Vault（需先在環境提供 KeyVault:VaultUri，App Service 建議用受控身分）
var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}

// 啟用 Microsoft Identity Web（OpenIdConnect）
builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

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
