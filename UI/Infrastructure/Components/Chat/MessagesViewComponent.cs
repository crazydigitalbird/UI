using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class MessagesViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;

        private readonly ILogger<MessagesViewComponent> _logger;

        public MessagesViewComponent(IChatClient chatClient, ILogger<MessagesViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idInterlocutor, long idLastMessage, bool isNew = false)
        {
            if (isNew)
            {
                ViewData["newSendMessage"] = true;
                var messanger = await _chatClient.GetMessagesChatAsync(sheet, idInterlocutor, 0, 10);
                messanger.Dialogs.FirstOrDefault().Messages = messanger.Dialogs.FirstOrDefault().Messages?.Where(m => m.Id > idLastMessage).ToList();
                return View(messanger);
            }
            else
            {
                var messanger = await _chatClient.GetMessagesChatAsync(sheet, idInterlocutor, idLastMessage);
                return View(messanger);
            }
        }
    }
}
