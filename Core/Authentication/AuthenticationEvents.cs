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
        if (principal == null)
        {
            _logger.LogError("Principal 為 null，無法處理員工建立/更新");
            context.HandleResponse();
            context.Response.Redirect("/Account/Login?error=staff_creation_failed&message=" + 
                Uri.EscapeDataString("認證資訊不完整，請重新登入"));
            return;
        }

        var objectId = principal.GetObjectId();
        if (string.IsNullOrEmpty(objectId))
        {
            _logger.LogError("無法取得 ObjectId，無法處理員工建立/更新");
            context.HandleResponse();
            context.Response.Redirect("/Account/Login?error=staff_creation_failed&message=" + 
                Uri.EscapeDataString("無法取得使用者識別碼，請檢查 Azure AD 設定"));
            return;
        }

        // 記錄所有 Claims（用於診斷 Azure AD 設定問題）
        LogAllClaims(principal, objectId);

        try
        {
            // 從DI容器取得服務（避免循環依賴）
            using var scope = _serviceScopeFactory.CreateScope();
            var staffService = scope.ServiceProvider.GetRequiredService<IStaffService>();
            
            var staff = await staffService.GetStaffByAzureObjectIdAsync(objectId);
            
            if (staff == null)
            {
                // 首次登入，建立員工（會自動從Azure AD取得頭像URL）
                _logger.LogInformation("首次登入，建立新員工。ObjectId: {ObjectId}", objectId);
                
                try
                {
                    staff = await staffService.CreateStaffFromAzureAsync(principal);
                    _logger.LogInformation("成功建立新員工。StaffId: {StaffId}, ObjectId: {ObjectId}", staff.StaffId, objectId);
                }
                catch (Exception ex)
                {
                    // 記錄詳細錯誤資訊
                    _logger.LogError(ex, 
                        "建立新員工失敗。ObjectId: {ObjectId}, Error: {Error}, StackTrace: {StackTrace}", 
                        objectId, ex.Message, ex.StackTrace);
                    
                    // 阻止認證繼續
                    context.HandleResponse();
                    context.Response.Redirect("/Account/Login?error=staff_creation_failed&message=" + 
                        Uri.EscapeDataString("無法建立使用者資料，請聯絡系統管理員"));
                    return;
                }
            }
            else
            {
                // 更新員工資訊（包含頭像URL）
                _logger.LogInformation("更新員工資訊。StaffId: {StaffId}, ObjectId: {ObjectId}", staff.StaffId, objectId);
                
                try
                {
                    staff = await staffService.UpdateStaffFromAzureAsync(staff, principal);
                }
                catch (Exception ex)
                {
                    // 更新失敗不應該阻止登入，但記錄錯誤
                    _logger.LogError(ex, 
                        "更新員工資訊失敗，但允許登入繼續。StaffId: {StaffId}, ObjectId: {ObjectId}, Error: {Error}", 
                        staff.StaffId, objectId, ex.Message);
                }
            }

            // 確保員工記錄存在
            if (staff == null)
            {
                _logger.LogError("員工記錄為 null，無法繼續認證。ObjectId: {ObjectId}", objectId);
                context.HandleResponse();
                context.Response.Redirect("/Account/Login?error=staff_creation_failed&message=" + 
                    Uri.EscapeDataString("無法取得使用者資料，請聯絡系統管理員"));
                return;
            }

            // 記錄登入
            var httpContext = context.HttpContext;
            try
            {
                await staffService.LogStaffLoginAsync(
                    staff.StaffId,
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    httpContext.Request.Headers["User-Agent"].ToString());
            }
            catch (Exception ex)
            {
                // 記錄登入失敗不應該阻止認證，但記錄錯誤
                _logger.LogWarning(ex, "記錄登入資訊失敗，但不影響認證。StaffId: {StaffId}", staff.StaffId);
            }
            
            _logger.LogInformation("登入記錄完成。StaffId: {StaffId}, ObjectId: {ObjectId}", staff.StaffId, objectId);
        }
        catch (Exception ex)
        {
            // 捕獲所有未預期的錯誤
            _logger.LogError(ex, 
                "處理員工建立/更新時發生未預期的錯誤。ObjectId: {ObjectId}, Error: {Error}, StackTrace: {StackTrace}", 
                objectId, ex.Message, ex.StackTrace);
            
            // 阻止認證繼續
            context.HandleResponse();
            context.Response.Redirect("/Account/Login?error=staff_creation_failed&message=" + 
                Uri.EscapeDataString("處理使用者資料時發生錯誤，請聯絡系統管理員"));
            return;
        }

        // 只有當所有步驟都成功時，才繼續認證流程
        await base.TokenValidated(context);
    }

    /// <summary>
    /// 記錄所有 Claims（用於診斷 Azure AD 設定問題）
    /// </summary>
    private void LogAllClaims(ClaimsPrincipal principal, string objectId)
    {
        try
        {
            var allClaims = principal.Claims
                .Select(c => $"{c.Type}={c.Value}")
                .ToList();
            
            _logger.LogInformation("=== 所有 Claims（ObjectId: {ObjectId}）===", objectId);
            foreach (var claim in principal.Claims)
            {
                _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
            _logger.LogInformation("=== Claims 記錄結束 ===");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "記錄 Claims 時發生錯誤，但不影響主要流程");
        }
    }
}

