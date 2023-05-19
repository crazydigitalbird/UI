using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using System.Diagnostics;

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
            //Stopwatch sw = Stopwatch.StartNew();
            var sheetInfo = await _chatClient.GetManProfile(sheet, idUser);
            //sw.Stop();
            if (sheetInfo == null)
            {
                return Content(string.Empty);
            }
            //Stopwatch sw1 = Stopwatch.StartNew();
            ViewData["photosPerson"] = await _chatClient.GetManPublicPhoto(sheet, idUser);
            //sw1.Stop();
            return View(sheetInfo);
        }
    }
}
