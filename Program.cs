using Azure.Identity;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Services;
using Microsoft.EntityFrameworkCore;
using CanLove_Backend.Extensions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// 添加 Key Vault 配置（支援開發和正式環境）
// 暫時註解掉 Key Vault 進行測試
// builder.Configuration.AddAzureKeyVaultWithIdentity(builder.Environment);

// 添加 Microsoft Identity Web 驗證
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

// 添加授權策略
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => 
        policy.RequireRole("admin"));
    options.AddPolicy("RequireSocialWorker", policy => 
        policy.RequireRole("socialworker", "admin"));
    options.AddPolicy("RequireViewer", policy => 
        policy.RequireRole("viewer", "socialworker", "admin"));
    options.AddPolicy("RequireAssistant", policy => 
        policy.RequireRole("assistant", "socialworker", "admin"));
});

// 添加服務
// 添加 Entity Framework
builder.Services.AddDbContext<CanLoveDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 註冊自定義服務
// 共用服務
builder.Services.AddScoped<CanLove_Backend.Services.Shared.OptionService>();
builder.Services.AddScoped<CanLove_Backend.Services.Shared.SchoolService>();
builder.Services.AddScoped<CanLove_Backend.Services.Shared.AddressService>();

// Case 相關服務
builder.Services.AddScoped<CanLove_Backend.Services.Case.CaseService>();

// CaseWizardOpenCase 相關服務
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S1_CD_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S2_CSWC_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S3_CFQES_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S4_CHQHS_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S5_CIQAP_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S6_CEEE_Service>();
builder.Services.AddScoped<CanLove_Backend.Services.CaseWizardOpenCase.Steps.CaseWizard_S7_FAS_Service>();

// 添加 AutoMapper
builder.Services.AddAutoMapper(typeof(CanLove_Backend.Mappings.CaseMappingProfile));

// 添加 MVC 支援
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

// 添加 API 支援
builder.Services.AddControllers();

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

// MVC 路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 個案相關路由
app.MapControllerRoute(
    name: "case",
    pattern: "Case/{action=Index}/{id?}",
    defaults: new { controller = "Case" });

// Microsoft Identity Web 路由
app.MapControllerRoute(
    name: "microsoft-identity",
    pattern: "MicrosoftIdentity/{controller=Account}/{action=SignIn}/{id?}");

// API 路由
app.MapControllers();

// 保留原有的 API 端點
app.MapGet("/", () => Results.Redirect("/Home/Index"));

app.Run();
