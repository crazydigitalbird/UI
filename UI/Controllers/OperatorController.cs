using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    public class OperatorController : Controller
    {
        private readonly IOperatorClient _operatorClient;

        public OperatorController(IOperatorClient operatorClient)
        {
            _operatorClient = operatorClient;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Profile> profiles = await _operatorClient.GetProfilesAsync();
            return View(profiles);
        }

        public async Task<IActionResult> Balance(Interval interval)
        {
            Dictionary<int, int> balance = await _operatorClient.GetBalanceAsync(User.Identity.Name, interval);
            if (balance != null)
            {
                return Ok(balance);
            }
            return StatusCode(500, $"Error getting a balance for a {interval}");
        }

        public IActionResult Notes(int profileId)
        {
            return ViewComponent("Notes", profileId);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote(int profileId, string text)
        {
            HttpResponseMessage response = await _operatorClient.CreateNoteAsync(User.Identity.Name, profileId, text);
            if (response?.IsSuccessStatusCode ?? false)
            {
                return Ok(new { name = User.Identity.Name, date = DateTime.Now.ToString("HH:mm dd.MM.yyy") });
            }
            return StatusCode(500, $"Error creating note for profile with id: {profileId}");
        }
    }
}
