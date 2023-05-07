using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json.Serialization;
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

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idRegularUser, MessageType messageType, string message, long idLastMessage)
        {
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
                    var stickerOptions = message.Split(';');
                    message = stickerOptions[0];
                    sendMessage.Content = new Content() { Url = stickerOptions[1] };
                    break;

                case MessageType.Photo:
                    var photoOptions = message.Split(';');
                    message = photoOptions[0];
                    sendMessage.Content = new Content { IdPhoto = int.Parse(photoOptions[0]), Url = photoOptions[1] };
                    break;

                case MessageType.Video:
                    var videoOptions = message.Split(';');
                    message = videoOptions[0];
                    sendMessage.Content = new Content { IdPhoto = int.Parse(videoOptions[0]), Url = videoOptions[1] };
                    break;
            }
            var idNewMessage = await _chatClient.SendMessageAsync(sheet, idRegularUser, messageType, message, idLastMessage);
            if (idNewMessage.HasValue)
            {
                sendMessage.Id = idNewMessage;
                
                Messenger messenger = new Messenger
                {
                    Dialogs = new List<Dialogue> { new Dialogue
                    {
                        Messages= new List<Message> { sendMessage }
                    } },
                    Sheet = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info)
            };

                ViewData["newSendMessage"] = true;
                return View(messenger);
            }
            return null;
        }
    }
}
