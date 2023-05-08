﻿using Core.Models.Sheets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        private readonly IOperatorClient _operatorClient;
        private readonly ISheetClient _sheetClient;
        private readonly ILogger<ChatController> _logger;

        public ChatsController(IChatClient chatClient, IOperatorClient operatorClient, ISheetClient sheetClient, ILogger<ChatController> logger)
        {
            _chatClient = chatClient;
            _operatorClient = operatorClient;
            _sheetClient = sheetClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int sheetId)
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
                        return ViewComponent("SheetDialogues", new { sheet, online, cursor });

                    case "history":
                        break;

                    case "bookmarked":
                        return ViewComponent("SheetDialogues", new { sheet, criteria, online, cursor });

                    case "premium":
                        return ViewComponent("SheetDialogues", new { sheet, online, cursor, filter = criteria });

                    case "trash":
                        return Content(string.Empty);

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
        public async Task<IActionResult> LoadMessages(int sheetId, int idInterlocutor, long idLastMessage)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Messages", new { sheet, idInterlocutor, idLastMessage });
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
        public async Task<IActionResult> MediaPhotos(int sheetId, int idUser, bool exclusivePost, string cursor = "")
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (exclusivePost)
                {
                    var tags = "erotic";
                    return ViewComponent("MediaPhotos", new { sheet, tags, cursor });
                }
                else
                {
                    var statuses = "approved,approved_by_ai";
                    var excludeTags = "erotic";
                    return ViewComponent("MediaPhotos", new { sheet, statuses, excludeTags, cursor });
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> MediaVideos(int sheetId, int idUser, bool exclusivePost, string cursor = "")
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (exclusivePost)
                {
                    return ViewComponent("MediaVideos", new { sheet, tags = "erotic", cursor });
                }
                else
                {
                    return ViewComponent("MediaVideos", new { sheet, statuses = "approved", excludeTags = "erotic",cursor });
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int sheetId, int idRegularUser, MessageType messageType, string message, long idLastMessage = 0, string ownerAvatar = "")
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Message", new { sheet, idRegularUser, messageType, message, idLastMessage, ownerAvatar });
            }
            return BadRequest();
        }
    }
}
