using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class DialogueViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<PhotosViewComponent> _logger;

        public DialogueViewComponent(IChatClient chatClient, ILogger<PhotosViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idRegularUser)
        {
            var dialogue = await _chatClient.FindDialogueById(sheet, idRegularUser);
            return View(dialogue);
        }
    }
}
