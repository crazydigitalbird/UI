using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class HistoryPostsViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<HistoryPostsViewComponent> _logger;

        public HistoryPostsViewComponent(IChatClient chatClient, ILogger<HistoryPostsViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idInterlocutor, long idLastMessage)
        {
            var photos = await _chatClient.ListPostAsync(sheet, idInterlocutor, idLastMessage);
            return View(photos);
        }
    }
}
