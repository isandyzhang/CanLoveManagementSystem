using Azure.Identity;
using CanLove_Backend.Data.Contexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 添加 Key Vault 配置
builder.Configuration.AddAzureKeyVault(
    new Uri("https://canlove-case.vault.azure.net/"),
    new DefaultAzureCredential());

// 添加服務
builder.Services.AddDbContext<CanLoveDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


app.MapGet("/", () => "Hello World!");

app.Run();
