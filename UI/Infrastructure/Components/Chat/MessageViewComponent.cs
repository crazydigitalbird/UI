using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UI.Infrastructure.API;
using UI.Infrastructure.Hubs;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class MessageViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<MessagesViewComponent> _logger;
        private readonly IChatHub _chatHub;

        public MessageViewComponent(IChatClient chatClient, ILogger<MessagesViewComponent> logger, IChatHub chatHub)
        {
            _chatClient = chatClient;
            _logger = logger;
            _chatHub = chatHub;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, Message newMessage, long idLastMessage)
        {
            long? idNewMessage = null;
            switch (newMessage.Type)
            {
                case MessageType.Message:
                    idNewMessage = await _chatClient.SendMessageAsync(sheet, newMessage.IdUserTo, MessageType.Message, newMessage.Content.Message, idLastMessage);
                    break;

                case MessageType.Sticker:
                    idNewMessage = await _chatClient.SendMessageAsync(sheet, newMessage.IdUserTo, MessageType.Sticker, $"{newMessage.Content.Id}", idLastMessage);
                    break;

                case MessageType.Virtual_Gift:
                    idNewMessage = await _chatClient.SendGiftAsync(sheet, newMessage.IdUserTo, $"{newMessage.Content.Id}", newMessage.Content.Message, idLastMessage);
                    break;

                case MessageType.Photo:
                    idNewMessage = await _chatClient.SendMessageAsync(sheet, newMessage.IdUserTo, MessageType.Photo, $"{newMessage.Content.IdPhoto}", idLastMessage);
                    break;

                case MessageType.Video:
                    idNewMessage = await _chatClient.SendMessageAsync(sheet, newMessage.IdUserTo, MessageType.Video, $"{newMessage.Content.IdPhoto}", idLastMessage);
                    break;

                case MessageType.Post:
                    var text = newMessage.Content.TextPreview;
                    var videos = newMessage.Content.Videos.Select(v => v.Id).ToList();
                    var photos = newMessage.Content.Photos.Select(p => p.Id).ToList();
                    idNewMessage = await _chatClient.SendPostAsync(sheet, newMessage.IdUserTo, text, videos, photos);
                    break;
            }

            if (idNewMessage.HasValue)
            {
                await _chatHub.ReplyToNewMessage(sheet.Id, newMessage.IdUserTo, idLastMessage, idNewMessage.Value);

                newMessage.Id = idNewMessage.Value;

                Messenger messenger = new Messenger
                {
                    Dialogs = new List<Dialogue> { new Dialogue
                    {
                        Messages= new List<Message> { newMessage }
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
