using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UI.Infrastructure.Filters
{
    public class APIAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if (!resultContext.HttpContext.User.Identity.IsAuthenticated)
            {
                await resultContext.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                resultContext.HttpContext.Items.Clear();
                resultContext.Result = new UnauthorizedResult();
            }
        }
    }
}
