using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class SheetDialogueViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<SheetDialogueViewComponent> _logger;

        public SheetDialogueViewComponent(IChatClient chatClient, ILogger<SheetDialogueViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idRegularUser, string criteria)
        {
            var dialogue = await _chatClient.FindDialogueById(sheet, idRegularUser);
            if (dialogue == null)
            {
                return Content(string.Empty);
            }
            ViewData["criteria"] = criteria;
            ViewData["sheetId"] = sheet.Id;
            return View(dialogue);
        }
    }
}
