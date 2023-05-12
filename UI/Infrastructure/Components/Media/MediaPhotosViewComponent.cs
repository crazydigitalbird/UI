using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class MediaPhotosViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<MediaPhotosViewComponent> _logger;

        public MediaPhotosViewComponent(IChatClient chatClient, ILogger<MediaPhotosViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idRegularUser, string statuses = "", string tags = "", string excludeTags= "", string cursor = "")
        {
            var photos = await _chatClient.GetPhotosAsync(sheet, idRegularUser, statuses, tags, excludeTags, cursor);
            return View(photos);
        }
    }
}
