using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UI.Infrastructure.API;
using UI.Infrastructure.Filters;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    [APIAuthorize]
    public class AdminAgencyController : Controller
    {
        private readonly IAdminAgencyClient _adminAgencyClient;
        private readonly IAuthenticationClient _authenticationClient;
        private readonly ISheetClient _sheetClient;
        private readonly IBalanceClient _balanceClient;
        private readonly IGroupClient _groupClient;
        private readonly ISiteClient _siteClient;
        private readonly IStatisticClient _statisticClient;
        private readonly ILogger<AdminAgencyController> _logger;

        private readonly List<SelectListItem> _roles = new() { new("Admin Agency", "1"), new("Operator", "2")/*, new("User", "3")*/ };

        public AdminAgencyController(IAdminAgencyClient adminAgencyClient,
            IAuthenticationClient authenticationClient,
            ISheetClient sheetClient,
            IBalanceClient balanceClient,
            IGroupClient groupClient,
            ISiteClient siteClient,
            IStatisticClient statisticClient,
            ILogger<AdminAgencyController> logger)
        {
            _adminAgencyClient = adminAgencyClient;
            _authenticationClient = authenticationClient;
            _sheetClient = sheetClient;
            _balanceClient = balanceClient;
            _groupClient = groupClient;
            _siteClient = siteClient;
            _statisticClient = statisticClient;
            _logger = logger;
        }

        [ServiceFilter(typeof(GetAgencyIdFilter))]
        public async Task<IActionResult> Index(int agencyId)
        {
            var adminAgencyView = await _adminAgencyClient.GetAdminAgencyViewById(agencyId);
            return View(adminAgencyView);
        }

        [ServiceFilter(typeof(GetAgencyIdFilter))]
        public async Task<IActionResult> Statistics(int agencyId)
        {
            var balanceStatisticAgencyTask = _balanceClient.GetBalanceStatisticAgencyAsync(agencyId);
            var sheetsStatisticTask = _statisticClient.GetSheetsStatisticAsync(agencyId);
            var agencyMetrikTask = _statisticClient.GetAgencyMetrikAsync(agencyId);
            await Task.WhenAll(balanceStatisticAgencyTask, sheetsStatisticTask, agencyMetrikTask);

            var statisticsViewModel = new AdminAgencyStatisticsViewModel
            {
                BalanceStatisticAgency = balanceStatisticAgencyTask.Result,
                AgencySheetsStatistic = sheetsStatisticTask.Result,
                Metrik = agencyMetrikTask.Result
            };

            return View(statisticsViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddSheet(int siteId, string login, string password)
        {
            var sheet = await _sheetClient.AddAsync(siteId, login, password);
            if (sheet != null)
            {
                return Ok(sheet);
            }
            return StatusCode(500, $"Error adding a {login} sheet");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSheets(int[] sheetsId)
        {
            var errorDelitingSheets = new List<int>();
            foreach (var sheetId in sheetsId)
            {
                if (!await _adminAgencyClient.DeleteSheet(sheetId))
                {
                    errorDelitingSheets.Add(sheetId);
                }
            }
            return Ok(errorDelitingSheets);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeGroup(int sheetId, int groupId)
        {
            if (await _groupClient.ChangeGroupAsync(sheetId, groupId))
            {
                return Ok();
            }
            return StatusCode(500, $"For sheet id: {sheetId}, the group value has not been changed");
        }

        [HttpPost]
        public async Task<IActionResult> AddGroup(int agencyId, [Required] string nameGroup)
        {
            if (ModelState.IsValid)
            {
                var group = await _groupClient.AddGroupAsync(agencyId, nameGroup);
                if (group != null)
                {
                    return Ok(group); //await response.Content.ReadAsStringAsync()
                }
            }
            return BadRequest(nameGroup);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeCabinet(int sheetId, int cabinetId)
        {

            if (await _adminAgencyClient.ChangeCabinet(sheetId, cabinetId))
            {
                return Ok();
            }
            return StatusCode(500, $"For sheet id: {sheetId}, the cabinet value has not been changed");
        }

        [HttpPost]
        [ServiceFilter(typeof(GetAgencyIdFilter))]
        public IActionResult GetOperators(int sheetId, int agencyId)
        {
            return ViewComponent("Operators", new { sheetId, agencyId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOperatorFromSheet(int operatorId, int sheetId)
        {
            if (await _adminAgencyClient.DeleteOperatorFromSheet(operatorId, sheetId))
            {
                return Ok();
            }
            return StatusCode(500, $"For sheet id: {sheetId}, the operator id: {operatorId} has not been deleted");
        }

        [HttpPost]
        public async Task<IActionResult> AddOperatorFromSheet(int agencyId, int sheetId, int operatorId)
        {
            if (await _adminAgencyClient.AddOperatorFromSheet(agencyId, sheetId, operatorId))
            {
                return Ok(User.Identity.Name);
            }
            return StatusCode(500, $"For sheet id: {sheetId}, the operator id: {operatorId} has not been added");
        }

        [ServiceFilter(typeof(GetAgencyIdFilter))]
        public async Task<IActionResult> Users(int agencyId)
        {
            AgencyView agency = await _adminAgencyClient.GetAgencyById(agencyId);
            ViewData["agencyId"] = agencyId;
            ViewData["FreeUsers"] = await _adminAgencyClient.GetNonAgencyUsers();
            ViewData["Roles"] = _roles;
            ViewData["returnUrl"] = Request.Headers.Referer.ToString();
            return View(agency);
        }

        [HttpPost]
        public async Task<IActionResult> Users(AgencyView agency, List<ApplicationUser> originalUsers, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (await _adminAgencyClient.UpdateAgency(agency, originalUsers))
                {
                    TempData["Message"] = $"The '{agency.Name}' agency has been updated";
                    return Redirect(returnUrl);
                }
                TempData["Error"] = $"'{agency.Name}' agency update error";
            }
            ViewData["agencyId"] = agency.Id;
            ViewData["FreeUsers"] = await _adminAgencyClient.GetNonAgencyUsers();
            ViewData["Roles"] = _roles;
            return View("Edit", await _adminAgencyClient.GetAgencyById(agency.Id));
        }

        [HttpPost]
        [ServiceFilter(typeof(GetAgencyIdFilter))]
        public async Task<IActionResult> AddUser(int agencyId, string userName, [EmailAddress] string email, [Required] string pwd, Role role)
        {
            if (ModelState.IsValid)
            {
                var user = await _authenticationClient.AddUserAsync(userName, email, pwd);

                if (user != null && agencyId != 0)
                {
                    ApplicationUser appUser = new()
                    {
                        Id = user.Id,
                        UserName = userName,
                        Email = email,
                        Role = role
                    };

                    await _adminAgencyClient.AddUserAgency(agencyId, appUser);
                    return Ok(appUser);
                }
            }

            return StatusCode(500, $"Error adding user");
        }
    }
}
