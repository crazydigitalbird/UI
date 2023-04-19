using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class DialoguesViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<DialoguesViewComponent> _logger;

        public DialoguesViewComponent(IChatClient chatClient, ILogger<DialoguesViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, string criteria = "", string cursor = "")
        {
            var messanger = await _chatClient.GetMessangerAsync(sheet, criteria, cursor);
            if (messanger != null)
            {
                await _chatClient.GetManProfiles(sheet, messanger.Dialogs);
            }
            return View(messanger);
        }
    }
}
