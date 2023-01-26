using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class OperatorsViewComponent : ViewComponent
    {
        private readonly IAdminAgencyClient _adminAgencyClient;

        private readonly ILogger<OperatorsViewComponent> _logger;

        public OperatorsViewComponent(IAdminAgencyClient adminAgencyClient, ILogger<OperatorsViewComponent> logger)
        {
            _adminAgencyClient = adminAgencyClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(int profileId)
        {
            ViewData["profileId"] = profileId;
            List<Operator> operators = (await _adminAgencyClient.GetOperatorsProfile(profileId)).ToList();
            return View(operators);
        }
    }
}
