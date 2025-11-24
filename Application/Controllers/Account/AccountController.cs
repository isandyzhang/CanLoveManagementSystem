using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CanLove_Backend.Application.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        /// <summary>
        /// 登入頁面
        /// 如果用戶已登入，自動重導向到首頁以改善用戶體驗
        /// </summary>
        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            // 如果用戶已登入，直接重導向到首頁
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Azure AD 登入重導向
        /// 允許使用現有的 Azure AD 會話以改善用戶體驗
        /// 若需要強制重新認證，可將 prompt 參數改為 "login"
        /// 若需要讓用戶選擇帳號，可將 prompt 參數改為 "select_account"
        /// 
        /// 注意：不在這裡清除 Cookie，因為：
        /// 1. 登入流程會自動設置新的 Correlation Cookie
        /// 2. 清除 Cookie 可能干擾新的認證流程
        /// 3. 舊的 Correlation Cookie 會自動過期（30 分鐘）
        /// 4. 如果用戶已登入，應該在登出時清除，而不是在登入時
        /// </summary>
        [HttpGet]
        public IActionResult SignIn(string returnUrl = "/")
        {
            // 簡化重導向 URL，使用固定路徑避免 URL 過長導致 HTTP 431 錯誤
            // 登入成功後統一重導向到首頁，避免在狀態參數中存儲過長的 URL
            var redirectUrl = "/Home/Index"; // 使用固定路徑，避免 URL 過長
            
            var properties = new AuthenticationProperties 
            { 
                RedirectUri = redirectUrl
                // 不將 returnUrl 存儲在 Items 中，減少狀態參數大小
                // 不設定 prompt 參數，允許使用現有 Azure AD 會話（改善 UX）
                // 若需要強制重新認證，可取消註解下一行：
                // Parameters = { { "prompt", "login" } }
            };
            
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// 登出處理
        /// 注意：不在這裡清除 Cookie，因為：
        /// 1. base.SignOut() 會處理 Azure AD 登出流程
        /// 2. Cookie 會在登出流程完成後自動清除
        /// 3. 提前清除 Cookie 可能導致登出流程失敗
        /// </summary>
        [HttpGet]
        public new IActionResult SignOut()
        {
            // 不在此處清除 Cookie，讓 OpenID Connect 處理器處理完整的登出流程
            // Cookie 會在登出回調後自動清除
            
            var callbackUrl = Url.Action("Login", "Account", new { area = "" }, Request.Scheme);
            return base.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// 清除所有認證相關的 Cookie
        /// 僅用於登出時清理，確保完全登出
        /// 
        /// 注意：不在登入時使用此方法，因為：
        /// 1. 登入流程需要設置新的 Correlation Cookie
        /// 2. 清除 Cookie 可能干擾新的認證流程
        /// 3. OpenID Connect 處理器會自動管理 Correlation Cookie 的生命週期
        /// </summary>
        private void ClearAuthenticationCookies()
        {
            // 清除認證 Cookie
            Response.Cookies.Delete(".AspNetCore.Cookies", new CookieOptions
            {
                Path = "/",
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                HttpOnly = true
            });
            
            // 清除所有 Correlation 和 Nonce Cookie（名稱是動態的）
            // 實際的 Cookie 名稱可能是 .AspNetCore.Correlation.xxx 或 .AspNetCore.Nonce.xxx
            // 我們需要清除所有匹配的 Cookie
            var cookieOptions = new CookieOptions
            {
                Path = "/",
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1) // 設置為過去時間，確保刪除
            };
            
            foreach (var cookieName in Request.Cookies.Keys.ToList())
            {
                if (cookieName.StartsWith(".AspNetCore.Correlation", StringComparison.OrdinalIgnoreCase) ||
                    cookieName.StartsWith(".AspNetCore.Nonce", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Cookies.Delete(cookieName, cookieOptions);
                }
            }
        }

        /// <summary>
        /// 權限不足頁面
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

