using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class InformationPersonViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<InformationPersonViewComponent> _logger;

        public InformationPersonViewComponent(IChatClient chatClient, ILogger<InformationPersonViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idUser)
        {
            var sheetInfo = await _chatClient.GetManProfile(sheet, idUser);
            if (sheetInfo == null)
            {
                return Content(string.Empty);
            }
            ViewData["photosPerson"] = await _chatClient.GetManPublicPhoto(sheet, idUser);
            return View(sheetInfo);
        }
    }
}
