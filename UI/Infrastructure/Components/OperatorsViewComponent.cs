using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

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

        public async Task<IViewComponentResult> InvokeAsync(int sheetId, int agencyId)
        {
            ViewData["sheetId"] = sheetId;
            var operatorSessionsViews = await _adminAgencyClient.GetAgencyOperators(agencyId);
            return View(operatorSessionsViews);
        }
    }
}
