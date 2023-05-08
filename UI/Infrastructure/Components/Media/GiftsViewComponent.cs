using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class GiftsViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<GiftsViewComponent> _logger;

        public GiftsViewComponent(IChatClient chatClient, ILogger<GiftsViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet)
        {
            var giftData = await _chatClient.GetGiftsAsync(sheet);
            return View(giftData.Gifts);
        }
    }
}
