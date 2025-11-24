using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;
using CanLove_Backend.Infrastructure.Options.Services;
using CanLove_Backend.Infrastructure.Storage.Blob;
using CanLove_Backend.Infrastructure.Storage.Encryption;
using CanLove_Backend.Domain.Case.Shared.Services;
using CanLove_Backend.Domain.Staff.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CanLove_Backend.Core.Authentication;

public class AuthenticationEvents : OpenIdConnectEvents
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AuthenticationEvents> _logger;

    public AuthenticationEvents(IServiceScopeFactory serviceScopeFactory, ILogger<AuthenticationEvents> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public override Task RedirectToIdentityProvider(RedirectContext context)
    {
        // 記錄登入重定向
        _logger.LogInformation("=== 開始登入流程 ===");
        _logger.LogInformation("重定向到 Azure AD 進行登入。RequestId: {RequestId}, Scheme: {Scheme}", 
            context.Request.Path, context.Request.Scheme);
        
        // 記錄當前 Cookie 狀態
        var cookies = string.Join(", ", context.Request.Cookies.Keys);
        _logger.LogInformation("登入前的 Cookie: {Cookies}", cookies);
        
        return base.RedirectToIdentityProvider(context);
    }

    public override Task MessageReceived(MessageReceivedContext context)
    {
        // 記錄收到回調消息
        _logger.LogInformation("收到 Azure AD 回調消息。RequestId: {RequestId}, Method: {Method}", 
            context.Request.Path, context.Request.Method);
        
        // 記錄所有 Cookie（用於診斷）
        var cookies = string.Join(", ", context.Request.Cookies.Keys);
        _logger.LogInformation("當前請求的 Cookie: {Cookies}", cookies);
        
        // 記錄 Correlation Cookie 的狀態
        var correlationCookies = context.Request.Cookies.Keys
            .Where(k => k.StartsWith(".AspNetCore.Correlation", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (correlationCookies.Any())
        {
            _logger.LogInformation("找到 Correlation Cookie: {Cookies}", string.Join(", ", correlationCookies));
        }
        else
        {
            _logger.LogWarning("未找到 Correlation Cookie！這可能導致 Correlation failed 錯誤。");
        }
        
        return base.MessageReceived(context);
    }

    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        // 記錄認證失敗
        _logger.LogError(context.Exception, "認證失敗。RequestId: {RequestId}, Error: {Error}", 
            context.Request.Path, context.Exception?.Message);
        return base.AuthenticationFailed(context);
    }

    public override Task RedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        // 記錄登出重定向
        _logger.LogInformation("重定向到 Azure AD 進行登出。RequestId: {RequestId}", context.Request.Path);
        return base.RedirectToIdentityProviderForSignOut(context);
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        _logger.LogInformation("Token 驗證成功。RequestId: {RequestId}", context.Request.Path);
        
        var principal = context.Principal;
        if (principal != null)
        {
            var objectId = principal.GetObjectId();
            if (!string.IsNullOrEmpty(objectId))
            {
                // 從DI容器取得服務（避免循環依賴）
                using var scope = _serviceScopeFactory.CreateScope();
                var staffService = scope.ServiceProvider.GetRequiredService<IStaffService>();
                
                var staff = await staffService.GetStaffByAzureObjectIdAsync(objectId);
                
                if (staff == null)
                {
                    // 首次登入，建立員工（會自動從Azure AD取得頭像URL）
                    _logger.LogInformation("首次登入，建立新員工。ObjectId: {ObjectId}", objectId);
                    staff = await staffService.CreateStaffFromAzureAsync(principal);
                }
                else
                {
                    // 更新員工資訊（包含頭像URL）
                    _logger.LogInformation("更新員工資訊。StaffId: {StaffId}, ObjectId: {ObjectId}", staff.StaffId, objectId);
                    staff = await staffService.UpdateStaffFromAzureAsync(staff, principal);
                }

                // 記錄登入
                var httpContext = context.HttpContext;
                await staffService.LogStaffLoginAsync(
                    staff.StaffId,
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    httpContext.Request.Headers["User-Agent"].ToString());
                
                _logger.LogInformation("登入記錄完成。StaffId: {StaffId}", staff.StaffId);
            }
        }

        await base.TokenValidated(context);
    }
}

