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
        [HttpGet]
        public IActionResult SignIn(string returnUrl = "/")
        {
            var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : Url.Action("Index", "Home");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public new IActionResult SignOut()
        {
            var callbackUrl = Url.Action("Index", "Home", new { area = "" }, Request.Scheme);
            return base.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

