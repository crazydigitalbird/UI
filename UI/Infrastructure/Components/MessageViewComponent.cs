using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class MessageViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<MessagesViewComponent> _logger;

        public MessageViewComponent(IChatClient chatClient, ILogger<MessagesViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idRegularUser, MessageType messageType, string message, string ownerAvatar)
        {
            ViewData["ownerAvatar"] = ownerAvatar;
            Message sendMessage = new Message()
            {
                DateCreated = DateTime.Now,
                IdUserFrom = sheet.User.Id,
                IdUserTo = idRegularUser,
                Type = messageType
            };
            switch (messageType)
            {
                case MessageType.Message:
                    sendMessage.Content = new Content() { Message = message };
                    break;

                case MessageType.Sticker:
                    var options = message.Split(';');
                    message = options[0];
                    sendMessage.Content = new Content() { Url = options[1] };
                    break;
            }
            var m = await _chatClient.SendMessageAsync(sheet, idRegularUser, messageType, message);            
            if (m != null)
            {
                sendMessage.Id = m.IdMessage;
                return View(sendMessage);
            }
            return null;
        }
    }
}
