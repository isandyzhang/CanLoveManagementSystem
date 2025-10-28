using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanLove_Backend.Controllers.Mvc
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        /// <summary>
        /// 登入頁面
        /// </summary>
        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            // 如果已經登入，重導向到首頁
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Azure AD 登入重導向
        /// </summary>
        [HttpGet]
        public IActionResult SignIn(string returnUrl = "/")
        {
            var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : Url.Action("Index", "Home");
            var properties = new AuthenticationProperties 
            { 
                RedirectUri = redirectUrl,
                Items = { { "returnUrl", redirectUrl } }
            };
            
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// 登出處理
        /// </summary>
        [HttpGet]
        public new IActionResult SignOut()
        {
            var callbackUrl = Url.Action("Login", "Account", new { area = "" }, Request.Scheme);
            return base.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
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

