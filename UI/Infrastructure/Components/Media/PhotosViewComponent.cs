using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class PhotosViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<PhotosViewComponent> _logger;

        public PhotosViewComponent(IChatClient chatClient, ILogger<PhotosViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, string cursor = "")
        {
            var photos = await _chatClient.GetPhotosAsync(sheet, cursor);
            return View(photos);
        }
    }
}
