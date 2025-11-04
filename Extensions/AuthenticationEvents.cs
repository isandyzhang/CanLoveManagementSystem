using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;
using CanLove_Backend.Services.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace CanLove_Backend.Extensions;

public class AuthenticationEvents : OpenIdConnectEvents
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AuthenticationEvents(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
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
                    staff = await staffService.CreateStaffFromAzureAsync(principal);
                }
                else
                {
                    // 更新員工資訊（包含頭像URL）
                    staff = await staffService.UpdateStaffFromAzureAsync(staff, principal);
                }

                // 記錄登入
                var httpContext = context.HttpContext;
                await staffService.LogStaffLoginAsync(
                    staff.StaffId,
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    httpContext.Request.Headers["User-Agent"].ToString());
            }
        }

        await base.TokenValidated(context);
    }
}

