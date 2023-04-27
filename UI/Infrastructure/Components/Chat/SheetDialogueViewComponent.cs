using Core.Models.Sheets;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class SheetDialogueViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<PhotosViewComponent> _logger;

        public SheetDialogueViewComponent(IChatClient chatClient, ILogger<PhotosViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idRegularUser)
        {
            var dialogue = await _chatClient.FindDialogueById(sheet, idRegularUser);
            if (dialogue == null)
            {
                return Content(string.Empty);
            }
            return View(dialogue);
        }
    }
}
