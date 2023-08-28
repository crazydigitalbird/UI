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
        private readonly ISheetClient _sheetClient;
        private readonly ISiteClient _siteClient;
        private readonly ILogger<UserController> _logger;

        public UserController(ISheetClient userClient, ISiteClient siteClient, ILogger<UserController> logger)
        {
            _sheetClient = userClient;
            _siteClient = siteClient;
            _logger = logger;
        }

         // GET: UserController
        public async Task<ActionResult> Index()
        {
            ViewData["Sites"] = await _siteClient.GetSites();
            return View(await _sheetClient.GetSheetsAsync());
        }

        // POST: UserController/AddProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddSheet(int siteId, string login, string password)
        {
            var sheet = await _sheetClient.AddAsync(siteId, login, password);
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
            var sheet = await _sheetClient.UpdateAsync(sheetId, password);
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
            if (await _sheetClient.DeleteAsync(sheetId))
            {
                return Ok();
            }
            return StatusCode(500);
        }
    }
}
