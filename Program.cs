using Azure.Identity;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 添加 Key Vault 配置（同時支援 ClientSecret 與 DefaultAzureCredential/Managed Identity）
var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
var clientId = builder.Configuration["KeyVault:ClientId"];
var clientSecret = builder.Configuration["KeyVault:ClientSecret"];
var tenantId = builder.Configuration["KeyVault:TenantId"];
var userAssignedManagedIdentityClientId = builder.Configuration["ManagedIdentityClientId"]; // 可選，用於 User-assigned MI

if (!string.IsNullOrEmpty(keyVaultUri))
{
    Azure.Core.TokenCredential credential;

    var hasClientSecret = !string.IsNullOrEmpty(clientId)
        && !string.IsNullOrEmpty(clientSecret)
        && !string.IsNullOrEmpty(tenantId);

    if (hasClientSecret)
    {
        credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
    }
    else
    {
        var defaultOptions = new DefaultAzureCredentialOptions();
        if (!string.IsNullOrEmpty(userAssignedManagedIdentityClientId))
        {
            defaultOptions.ManagedIdentityClientId = userAssignedManagedIdentityClientId;
        }
        credential = new DefaultAzureCredential(defaultOptions);
    }

    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);
}

// 添加服務
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
builder.Services.AddControllersWithViews();

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

// API 路由
app.MapControllers();

// 保留原有的 API 端點
app.MapGet("/", () => Results.Redirect("/Home/Index"));

app.Run();
