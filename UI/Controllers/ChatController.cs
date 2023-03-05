using Microsoft.AspNetCore.Mvc;
using System.Net;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatClient chatClient, ILogger<ChatController> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var messanger = await _chatClient.GetMessangerAsync();
            if (messanger is null)
            {
                TempData["Error"] = "Error. Try again.";
                return View();
            }
            return View(messanger);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int userId, int chatId, string message)
        {
            return Ok();
        }
    }
}
