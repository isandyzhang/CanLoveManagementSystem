using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// 已停用 Azure Key Vault 載入，先確保應用可啟動部署

// 已停用 Microsoft Identity Web 驗證，先開放存取以利部署

// 授權改為全開放（忽略 [Authorize]），避免因權限阻擋導致無法存取
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAssertion(_ => true)
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

// 已停用驗證中介軟體，僅保留授權（已設為全開放）
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
