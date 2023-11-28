using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
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
        public async Task<IActionResult> LogIn(LoginModel loginModel, string returnUrl)
        {
            HttpContext.Session.Clear();
            if (!ModelState.IsValid)
            {
                return View(loginModel);
            }

            var user = await _authenticationClient.LogInAsync(loginModel.Login.Trim(), loginModel.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(new LoginRegisterModel { LoginModel = loginModel });
            }

            await SignInAsync(user);

            if(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(LoginRegisterModel loginRegisterModel/*, string referralCode*/)
        {
            if (!ModelState.IsValid)
            {
                //ViewData["referralCode"] = referralCode;
                ViewData["register"] = true;
                return View("LogIn", loginRegisterModel);
            }

            var isRegister = await _authenticationClient.RegisterAsync(loginRegisterModel.RegisterModel.Login.Trim(), loginRegisterModel.RegisterModel.Email.Trim(), loginRegisterModel.RegisterModel.Password);

            if (!isRegister)
            {
                ModelState.AddModelError(string.Empty, "Invalid register attempt.");
                ViewData["register"] = true;
                return View("LogIn", loginRegisterModel);
            }

            TempData["Message"] = "Registration completed successfully";

            return View(nameof(LogIn));
        }

        private async Task SignInAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
                {
                    new Claim("Id", $"{user.Id}"),
                    new Claim("OperatorId", $"{user.OperatorId}"),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, $"{user.Role}"),
                    new Claim("SessionGuid", user.SesstionGuid)
                };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

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
        }

        public async Task<IActionResult> LogOut()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(LogIn));
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

            var response = await _authenticationClient.ChangePassowrdAsync(model.Password, model.NewPassword);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Password changed successfully";
                return View();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return Unauthorized();
            }

            _logger.LogError("The password has not been changed. HttpStatusCode: {httpStatusCode}.", response.StatusCode);
            return StatusCode(500, "The password has not been changed");
        }

        [AllowAnonymous]
        public IActionResult PasswordRecovery()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordRecovery([EmailAddress][Required] string email)
        {
            if (ModelState.IsValid)
            {
                var response = await _authenticationClient.PasswordRecoveryAsync(email.Trim());
                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "An email with instructions for changing the password has been sent to the specified email address";
                    return View();
                }
                TempData["Error"] = "An error has occured. Try again later";
            }
            else
            {
                TempData["Error"] = string.Join(". ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }
            return View();
        }
    }
}
