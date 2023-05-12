using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class MediaVideosViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<MediaVideosViewComponent> _logger;

        public MediaVideosViewComponent(IChatClient chatClient, ILogger<MediaVideosViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idRegularUser, string statuses = "", string tags = "", string excludeTags = "", string cursor = "")
        {
            var videos = await _chatClient.GetVideosAsync(sheet, idRegularUser, statuses, tags, excludeTags, cursor);
            return View(videos);
        }
    }
}
