using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Infrastructure.Filters;

namespace UI.Controllers
{
    [Authorize]
    [APIAuthorize]
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
            ViewData["Sites"] = (await _userClient.GetSites())?.Where(s => s.IsActive).ToList();
            return View(await _userClient.GetSheets());
        }

        // POST: UserController/AddProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddSheet(int siteId, string login, string password)
        {
            var sheet = await _userClient.AddSheet(siteId, login, password);
            if(sheet != null)
            {
                return Ok(sheet);
            }
            return StatusCode(500, $"Error adding a {login} sheet");
        }

        // POST: UserController/Edit/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(int sheetId, string password)
        {
            var sheet = await _userClient.UpdateSheet(sheetId, password);
            if (sheet != null)
            {
                return Ok(sheet);
            }
            return StatusCode(500, $"Error updating a sheet with id: {sheetId}");
        }

        // POST: UserController/DeleteProfile/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteSheet(int sheetId)
        {
            if (await _userClient.DeleteSheet(sheetId))
            {
                return Ok();
            }
            return StatusCode(500);
        }
    }
}
