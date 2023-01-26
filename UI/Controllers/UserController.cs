using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserClient _userClient;

        private readonly ILogger<UserController> _logger;

        public UserController(IUserClient userClient, ILogger<UserController> logger)
        {
            _userClient = userClient;
            _logger = logger;
        }

         // GET: UserController
        public async Task<ActionResult> Index()
        {
            return View(await _userClient.GetProfiles());
        }

        // POST: UserController/AddProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddProfile(string email, string password)
        {
            var response = await _userClient.AddProfile(email, password);
            if(response?.IsSuccessStatusCode ?? false)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }
            return StatusCode(500, $"Error adding a {email} profile");
        }

        // POST: UserController/Edit/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ChangePassword(int profileId, string password)
        {
            return Ok();
        }

        // POST: UserController/DeleteProfile/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteProfile(int profileId)
        {
            return Ok();
        }
    }
}
