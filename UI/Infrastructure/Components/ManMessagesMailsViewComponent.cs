using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class ManMessagesMailsViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;

        public ManMessagesMailsViewComponent(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idRegularUser, string userName, string avatarUrl)
        {
            var messagesAndMailsLeft = await _chatClient.GetManMessagesMails(sheet, idRegularUser);
            messagesAndMailsLeft.IdInterlocutor = idRegularUser;
            messagesAndMailsLeft.UserName = userName;
            messagesAndMailsLeft.Avatar = avatarUrl;
            return View(messagesAndMailsLeft);
        }
    }
}
