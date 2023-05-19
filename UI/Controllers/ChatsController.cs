﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Infrastructure.Filters;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    [APIAuthorize]
    public class ChatsController : Controller
    {
        private readonly IChatClient _chatClient;
        private readonly IMailClient _mailClient;
        private readonly IOperatorClient _operatorClient;
        private readonly ISheetClient _sheetClient;
        private readonly ILogger<ChatController> _logger;

        public ChatsController(IChatClient chatClient, IMailClient mailClient, IOperatorClient operatorClient, ISheetClient sheetClient, ILogger<ChatController> logger)
        {
            _chatClient = chatClient;
            _mailClient = mailClient;
            _operatorClient = operatorClient;
            _sheetClient = sheetClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var sheets = await _operatorClient.GetSheetsAsync() ?? new();
            //ViewData["stickers"] = await _chatClient.GetStickersAsync(sheet);
            return View(sheets);
        }

        [HttpPost]
        public async Task<IActionResult> Dialogues(int sheetId, string criteria, bool online, string cursor = "")
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                switch (criteria)
                {
                    case "active":
                    case "bookmarked":
                    case "premium":
                    case "trash":
                        return ViewComponent("SheetDialogues", new { sheet, criteria, online, cursor });

                    case "history":
                        break;
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> FindDialogueById(int sheetId, int idRegularUser, string criteria)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("SheetDialogue", new { sheet, idRegularUser, criteria });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> AllNewMessagesFromAllMen()
        {
            var sheets = await _operatorClient.GetSheetsAsync();
            if (sheets != null)
            {
                return ViewComponent("AllNewMessagesFromAllMen", sheets);
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ManMessagesMails(int sheetId, int idRegularUser)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                var messagesAndMailsLeft = await _chatClient.GetManMessagesMails(sheet, idRegularUser);
                return Ok(new { messagesAndMailsLeft?.MessagesLeft, messagesAndMailsLeft?.MailsLeft });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> InformationPerson(int sheetId, int idUser)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("InformationPerson", new { sheet, idUser });
            }
            return BadRequest();
        }

        #region Pin Bookmark Premium Trash
        [HttpPost]
        public async Task<IActionResult> ChangePin(int sheetId, int idRegularUser, bool addPin)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (await _chatClient.ChangePinAsync(sheet, idRegularUser, addPin))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ChangeBookmark(int sheetId, int idRegularUser, bool addBookmark)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (await _chatClient.ChangeBookmarkAsync(sheet, idRegularUser, addBookmark))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePremium(int sheetId, int idRegularUser, bool addPremium)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (await _chatClient.ChangePremiumAsync(sheet, idRegularUser, addPremium))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ChangeTrash(int sheetId, int idRegularUser, bool addTrash)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (await _chatClient.ChangeTrashAsync(sheet, idRegularUser, addTrash))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> LoadMessages(int sheetId, int idInterlocutor, long idLastMessage, bool isNew = false)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Messages", new { sheet, idInterlocutor, idLastMessage, isNew});
            }
            return BadRequest();
        }


        [HttpPost]
        public async Task<IActionResult> Stickers(int sheetId)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Stickers", sheet);
            }
            return BadRequest();
        }

        #region Gifts
        [HttpPost]
        public async Task<IActionResult> CheckGifts(int sheetId, int idInterlocutor)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return Ok(await _chatClient.CheckGiftsAsync(sheet, idInterlocutor));
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Gifts(int sheetId)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Gifts", sheet);
            }
            return BadRequest();
        }
        #endregion

        #region Post
        [HttpPost]
        public async Task<IActionResult> CheckPost(int sheetId, int idInterlocutor)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return Ok(await _chatClient.CheckPostAsync(sheet, idInterlocutor));
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Posts(int sheetId, int idInterlocutor, long idLastMessage)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("HistoryPosts", new { sheet, idInterlocutor, idLastMessage });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> SendPost(int sheetId, int idRegularUser, long idLastMessage, string text, List<PhotoVideo> videos, List<PhotoVideo> photos)
        {
            if (text?.Length >= 200 && text?.Length <= 3500 && (videos?.Count >= 1 || photos?.Count >= 4))
            {
                var sheet = await _sheetClient.GetSheetAsync(sheetId);
                if (sheet != null)
                {
                    Message newMessage = new()
                    {
                        DateCreated = DateTime.Now,
                        IdUserFrom = sheet.User.Id,
                        IdUserTo = idRegularUser,
                        Type = MessageType.Post,
                        Content = new Content
                        {
                            TextPreview = text,
                            Photos = photos,
                            Videos = videos
                        }
                    };
                    return ViewComponent("Message", new { sheet, newMessage, idLastMessage });
                }
            }
            return BadRequest();
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> Sheets(string criteria, string cursor = "")
        {
            var sheets = await _operatorClient.GetSheetsViewAsync();
            await _sheetClient.GettingStatusAndMedia(sheets);
            if (sheets != null)
            {
                return ViewComponent("Sheets", new { sheets, criteria, cursor });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> MediaPhotos(int sheetId, int idRegularUser, bool exclusivePost, string cursor = "")
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                var statuses = "approved,approved_by_ai";
                if (exclusivePost)
                {
                    return ViewComponent("MediaPhotos", new { sheet, idRegularUser, statuses, cursor });
                }
                else
                {
                    var excludeTags = "erotic";
                    return ViewComponent("MediaPhotos", new { sheet, idRegularUser, statuses, excludeTags, cursor });
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> MediaVideos(int sheetId, int idRegularUser, bool exclusivePost, string cursor = "")
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (exclusivePost)
                {
                    return ViewComponent("MediaVideos", new { sheet, idRegularUser, statuses = "approved", cursor });
                }
                else
                {
                    return ViewComponent("MediaVideos", new { sheet, idRegularUser, statuses = "approved", excludeTags = "erotic", cursor });
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int sheetId, int idRegularUser, MessageType messageType, string message, long idLastMessage = 0)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                Message newMessage = new()
                {
                    DateCreated = DateTime.Now,
                    IdUserFrom = sheet.User.Id,
                    IdUserTo = idRegularUser,
                    Type = messageType
                };
                switch (messageType)
                {
                    case MessageType.Message:
                        newMessage.Content = new Content() { Message = message };
                        break;

                    case MessageType.Sticker:
                        var stickerOptions = message.Split(';');
                        newMessage.Content = new Content() { Id = int.Parse(stickerOptions[0]), Url = stickerOptions[1] };
                        break;

                    case MessageType.Virtual_Gift:
                        var giftOptions = message.Split(';');
                        newMessage.Content = new Content() { Id = int.Parse(giftOptions[0]), ImageSrc = giftOptions[1], Message = giftOptions[2] };
                        break;

                    case MessageType.Photo:
                        var photoOptions = message.Split(';');
                        newMessage.Content = new Content { IdPhoto = int.Parse(photoOptions[0]), Url = photoOptions[1] };
                        break;

                    case MessageType.Video:
                        var videoOptions = message.Split(';');
                        newMessage.Content = new Content { IdPhoto = int.Parse(videoOptions[0]), Url = videoOptions[1] };
                        break;
                }

                return ViewComponent("Message", new { sheet, newMessage, idLastMessage });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Timer(int sheetId, int idInterlocutor, long idLastMessage, MessageType messageType)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                var seconds = await _chatClient.Timer(sheet, idInterlocutor, idLastMessage, messageType);
                if (seconds.HasValue)
                {
                    return Ok(seconds.Value);
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> History(string cursor)
        {
            var sheets = await _operatorClient.GetSheetsAsync();
            if (sheets != null)
            {
                return ViewComponent("History", new {sheets, cursor});
            }
            return BadRequest();
        }
    }
}
