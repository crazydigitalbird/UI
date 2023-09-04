using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Filters
{
    public class GetAgencyIdFilter : IAsyncActionFilter
    {
        private readonly IAdminAgencyClient _adminAgencyClient;

        public GetAgencyIdFilter(IAdminAgencyClient adminAgencyClient)
        {
            _adminAgencyClient= adminAgencyClient;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            int agencyId = 0;
            if (context.ActionArguments.TryGetValue("agencyId", out var agencyIdObject))
            {
                if (agencyIdObject is int)
                {
                    agencyId = (int)agencyIdObject;
                }
            }
            if (agencyId == 0)
            {
                agencyId = await _adminAgencyClient.GetAgencyId();
                if (agencyId == 0)
                {
                    context.Result = new BadRequestObjectResult("agencyId is 0");
                    return;
                }
                context.ActionArguments["agencyId"] = agencyId;
            }

            await next();
        }
    }
}
