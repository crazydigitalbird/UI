using Microsoft.AspNetCore.Mvc.Filters;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Filters
{
    public class UpdateSessionAttribute : ActionFilterAttribute
    {
        private readonly IAuthenticationClient _authenticationClient;
        public UpdateSessionAttribute(IAuthenticationClient authenticationClient)
        {
            _authenticationClient = authenticationClient;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Task.Run(() => _authenticationClient.UpdateSession());
        }
    }
}
