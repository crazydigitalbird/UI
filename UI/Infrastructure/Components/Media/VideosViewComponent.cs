using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class VideosViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<VideosViewComponent> _logger;

        public VideosViewComponent(IChatClient chatClient, ILogger<VideosViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, string cursor = "")
        {
            var videos = await _chatClient.GetVideosAsync(sheet, cursor);
            return View(videos);
        }
    }
}
