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
    public class ChatController : Controller
    {
        private readonly IChatClient _chatClient;
        private readonly IOperatorClient _operatorClient;
        private readonly ISheetClient _sheetClient;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatClient chatClient, IOperatorClient operatorClient, ISheetClient sheetClient, ILogger<ChatController> logger)
        {
            _chatClient = chatClient;
            _operatorClient = operatorClient;
            _sheetClient = sheetClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int sheetId)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId) ?? new Core.Models.Sheets.Sheet();
            var sheetInfo = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
            ViewData["avatar"] = sheetInfo.Personal.Avatar;
            ViewData["stickers"] = await _chatClient.GetStickersAsync(sheet);
            //ViewData["photos"] = await _chatClient.GetPhotosAsync(sheet);
            //ViewData["videos"] = await _chatClient.GetVideosAsync(sheet);
            return View(sheet);
        }

        [HttpPost]
        public async Task<IActionResult> Dialogues(int sheetId, string criteria, string cursor = "")
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Dialogues", new { sheet, criteria, cursor });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> AllNewMessagesFromAllMen(int sheetId)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("AllNewMessagesFromAllMen", sheet);
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
        public async Task<IActionResult> FindDialogueById(int sheetId, int idRegularUser)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Dialogue", new { sheet, idRegularUser });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> LoadMessages(int sheetId, int chatId, long idLastMessage)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Messages", new { sheet, chatId, idLastMessage });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ManMessagesMails(int sheetId, int idRegularUser, string userName, string avatarUrl)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("ManMessagesMails", new { sheet, idRegularUser, userName, avatarUrl });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> MediaPhotos(int sheetId, string cursor = "")
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Photos", new { sheet, cursor });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> MediaVideos(int sheetId, string cursor = "")
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Videos", new { sheet, cursor });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int sheetId, int idRegularUser, MessageType messageType, string message, string ownerAvatar)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Message", new { sheet, idRegularUser, messageType, message, ownerAvatar });
            }
            return BadRequest();
        }
    }
}
