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
        private readonly IUserClient _userClient;
        private readonly IStatisticClient _statisticClient;
        private readonly ILogger<AdminAgencyController> _logger;

        private readonly List<SelectListItem> _roles = new() { new("Admin Agency", "1"), new("Operator", "2")/*, new("User", "3")*/ };

        public AdminAgencyController(IAdminAgencyClient adminAgencyClient,
            IAuthenticationClient authenticationClient,
            ISheetClient sheetClient,
            IBalanceClient balanceClient,
            IGroupClient groupClient,
            IUserClient userClient,
            IStatisticClient statisticClient,
            ILogger<AdminAgencyController> logger)
        {
            _adminAgencyClient = adminAgencyClient;
            _authenticationClient = authenticationClient;
            _sheetClient = sheetClient;
            _balanceClient = balanceClient;
            _groupClient = groupClient;
            _userClient = userClient;
            _statisticClient = statisticClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int agencyId = 0)
        {
            if (agencyId == 0)
            {
                agencyId = await _adminAgencyClient.GetAgencyId();
                if (agencyId == 0)
                {
                    return BadRequest();
                }
            }
            var sheets = await _adminAgencyClient.GetSheets(agencyId);

            var endDateTime = DateTime.Now;
            var beginDatetime = endDateTime - TimeSpan.FromDays(30);

            var statusAndMediaTask = _sheetClient.GettingStatusAndMedia(sheets);
            var groupsTask = _groupClient.GetGroupsAsync(agencyId);
            var cabinetsTask = _adminAgencyClient.GetCabinets(agencyId);
            var balancesTask = _balanceClient.GetAgencyBalance(agencyId, beginDatetime, endDateTime);
            var sitesTask = _userClient.GetSites();
            var opTask = _sheetClient.GetSheetAgencyOperatorSessionsCount(sheets);
            await Task.WhenAll(statusAndMediaTask, groupsTask, cabinetsTask, balancesTask, sitesTask, opTask);
            //var shifts = await _adminAgencyClient.GetShifts();

            var groups = groupsTask.Result;
            var cabinets = cabinetsTask.Result;
            var balances = balancesTask.Result;
            var sites = sitesTask.Result?.Where(s => s.IsActive).ToList(); ;

            if (sheets != null)
            {
                foreach (var sheet in sheets)
                {
#if !DEBUGOFFLINE
                    sheet.Balance = balances.Where(b => b.Sheet.Id == sheet.Id).Sum(sb => sb.Cash);
#endif

                    if (cabinets?.Any(c => c.Sheets?.Any(acs => acs.Sheet.Id == sheet.Id) ?? false) ?? false)
                    {
                        sheet.Cabinet = (Cabinet)cabinets.FirstOrDefault(c => c.Sheets.Any(acs => acs.Sheet.Id == sheet.Id));
                    }
                    if (groups?.Any(g => g.Sheets?.Any(ags => ags.Sheet.Id == sheet.Id) ?? false) ?? false)
                    {
                        sheet.Group = groups.FirstOrDefault(g => g.Sheets.Any(ags => ags.Sheet.Id == sheet.Id));
                    }
                }
            }

            ViewData["agencyId"] = agencyId;
            ViewData["Groups"] = groups?.Select(g => new SelectListItem(g.Description, g.Id.ToString()));
            ViewData["Cabinets"] = cabinets?.Select(c => new SelectListItem(c.Name, c.Id.ToString()));
            ViewData["Sites"] = sites;
            //ViewData["Shifts"] = shifts?.Select((s, i) => new SelectListItem(s, $"{i + 1}"));

            return View(sheets);
        }

        public async Task<IActionResult> Statistics(int agencyId)
        {
            if (agencyId == 0)
            {
                agencyId = await _adminAgencyClient.GetAgencyId();
                if (agencyId == 0)
                {
                    return BadRequest();
                }
            }

            var balanceStatisticAgencyTask = _balanceClient.GetBalanceStatisticAgencyAsync(agencyId);
            var sheetsStatisticTask = _statisticClient.GetSheetsStatisticAsync(agencyId);
            var statisticTimeMetricTask = _statisticClient.GetAgencyAverageResponseTimeAsync(agencyId);
            await Task.WhenAll(balanceStatisticAgencyTask, sheetsStatisticTask, statisticTimeMetricTask);

            var statisticsViewModel = new AdminAgencyStatisticsViewModel
            {
                BalanceStatisticAgency = balanceStatisticAgencyTask.Result,
                AgencySheetsStatistic = sheetsStatisticTask.Result,
                AverageResponseTime = statisticTimeMetricTask.Result
            };

            return View(statisticsViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddSheet(int siteId, string login, string password)
        {
            var sheet = await _userClient.AddSheet(siteId, login, password);
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
        public async Task<IActionResult> ChangeShift(int sheetId, int shiftId)
        {
            if (await _adminAgencyClient.ChangeShift(sheetId, shiftId))
            {
                return Ok();
            }
            return StatusCode(500, $"For sheet id: {sheetId}, the shift value has not been changed");
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

        public async Task<IActionResult> Users(int agencyId)
        {
            if (agencyId == 0)
            {
                agencyId = await _adminAgencyClient.GetAgencyId();
                if (agencyId == 0)
                {
                    return View();
                }
            }
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
        public async Task<IActionResult> AddUser(int agencyId, string userName, [EmailAddress] string email, [Required] string pwd, Role role)
        {
            if (ModelState.IsValid)
            {
                var addUserTask = _authenticationClient.AddUserAsync(userName, email, pwd);
                Task<int> agencyIdTask;
                if (agencyId == 0)
                {
                    agencyIdTask = _adminAgencyClient.GetAgencyId();
                }
                else
                {
                    agencyIdTask = Task.FromResult(agencyId);
                }

                await Task.WhenAll(addUserTask, agencyIdTask);

                agencyId = agencyIdTask.Result;
                var user = addUserTask.Result;

                if (user != null && agencyId != 0)
                {
                    ApplicationUser appUser = new ApplicationUser()
                    {
                        Id = addUserTask.Result.Id,
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
