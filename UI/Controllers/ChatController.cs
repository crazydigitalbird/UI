using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Infrastructure.Filters;

namespace UI.Controllers
{
    [Authorize]
    [APIAuthorize]
    public class ChatController : Controller
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatClient chatClient, ILogger<ChatController> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int sheetId)
        {
            var messanger = await _chatClient.GetMessangerAsync();
            return View(messanger);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int userId, int chatId, string message)
        {
            if(await _chatClient.SendMessageAsync(userId, chatId, message))
            {
                return Ok();
            }
            return StatusCode(500);
        }
    }
}
