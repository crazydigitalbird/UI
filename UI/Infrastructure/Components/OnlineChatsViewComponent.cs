using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class OnlineChatsViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;

        private readonly ILogger<OnlineChatsViewComponent> _logger;

        public OnlineChatsViewComponent(IChatClient chatClient, ILogger<OnlineChatsViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(int sheetId)
        {
            //var messanger = await _chatClient.GetChatsOnline(sheetId);
            return View();
        }
    }
}