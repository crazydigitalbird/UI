using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class ProgressViewComponent : ViewComponent
    {
        private readonly IIcebreakersClient _icebreakersClient;
        private readonly IOperatorClient _operatorClient;
        private readonly ILogger<ProgressViewComponent> _logger;

        public ProgressViewComponent(IIcebreakersClient icebreakersClient, IOperatorClient operatorClient, ILogger<ProgressViewComponent> logger)
        {
            _icebreakersClient = icebreakersClient;
            _operatorClient = operatorClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet)
        {
            var iceApprov = await _icebreakersClient.Progress(sheet);
            if (iceApprov?.Data?.Ices?.Count > 0)
            {
                var sheetInfo = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
                ViewData["avatarUrl"] = sheetInfo.Personal.AvatarSmall;
                return View(iceApprov?.Data?.Ices);
            }
            return Content(string.Empty);
        }
    }
}
