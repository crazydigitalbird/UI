using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAuthenticationClient _authenticationClient;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthenticationClient authenticationClient, ILogger<AccountController> logger)
        {
            _authenticationClient = authenticationClient;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn(LoginModel loginModel)
        {
            ModelState.Remove("ConfirmPassowrd");
            if (ModelState.IsValid)
            {
                var user = await AuthenticateUser(loginModel.Email, loginModel.Password);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(loginModel);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    //AllowRefresh = <bool>,
                    // Refreshing the authentication session should be allowed.

                    //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    // The time at which the authentication ticket expires. A 
                    // value set here overrides the ExpireTimeSpan option of 
                    // CookieAuthenticationOptions set with AddCookie.

                    //IsPersistent = true,
                    // Whether the authentication session is persisted across 
                    // multiple requests. When used with cookies, controls
                    // whether the cookie's lifetime is absolute (matching the
                    // lifetime of the authentication ticket) or session-based.

                    //IssuedUtc = <DateTimeOffset>,
                    // The time at which the authentication ticket was issued.

                    //RedirectUri = <string>
                    // The full path or absolute URI to be used as an http 
                    // redirect response value.
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                _logger.LogInformation("User {UserName} logged in at {Time}.", user.UserName, DateTime.UtcNow);

                return LocalRedirect("/");
            }

            return View("LogIn", loginModel);
        }

        public IActionResult ChangePassword(string returnUrl)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePassworViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            TempData["Message"] = "Password changed successfully";
            return View();
        }

        [AllowAnonymous]
        public IActionResult PasswordRecovery()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PasswordRecovery([EmailAddress][Required] string email)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(". ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View();
            }
            TempData["Message"] = "An email with instructions for changing the password has been sent to the specified email address";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(LoginModel loginModel/*, string referralCode*/)
        {
            if (!ModelState.IsValid)
            {
                //ViewData["referralCode"] = referralCode;
                ViewData["register"] = true;
                return View("LogIn", loginModel);
            }
            return LocalRedirect("/");
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(LogIn));
        }

        private async Task<ApplicationUser> AuthenticateUser(string login, string password)
        {
            string hashPassword = Hash.HashPassword(password);
            return await _authenticationClient.LogIn(login, hashPassword);
        }
    }
}
