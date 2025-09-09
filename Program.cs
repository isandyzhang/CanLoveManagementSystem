using Azure.Identity;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 添加 Key Vault 配置
builder.Configuration.AddAzureKeyVault(
    new Uri("https://canlove-case.vault.azure.net/"),
    new DefaultAzureCredential());

// 添加服務
builder.Services.AddDbContext<CanLoveDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 註冊自定義服務
builder.Services.AddScoped<SchoolService>();
builder.Services.AddScoped<CaseService>();
builder.Services.AddScoped<AddressService>();

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

// API 路由
app.MapControllers();

// 保留原有的 API 端點
app.MapGet("/", () => Results.Redirect("/Home/Index"));

app.Run();
