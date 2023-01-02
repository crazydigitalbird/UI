using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class OperatorsViewComponent : ViewComponent
    {
        private readonly ISuperAdminClient _superAdminClient;

        private readonly ILogger<OperatorsViewComponent> _logger;

        public OperatorsViewComponent(ISuperAdminClient superAdminClient, ILogger<OperatorsViewComponent> logger)
        {
            _superAdminClient = superAdminClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(int profileId)
        {
            ViewData["profileId"] = profileId;
            List<Operator> operators = (await _superAdminClient.GetOperatorsProfile(profileId)).ToList();
            return View(operators);
        }
    }
}
