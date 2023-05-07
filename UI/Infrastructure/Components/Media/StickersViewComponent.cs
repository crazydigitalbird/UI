using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class StickersViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<StickersViewComponent> _logger;

        public StickersViewComponent(IChatClient chatClient, ILogger<StickersViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet)
        {
            var stickersGroups = await _chatClient.GetStickersAsync(sheet);
            return View(stickersGroups);
        }
    }
}
