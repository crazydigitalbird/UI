using Core.Models.Agencies.Groups;
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

        private readonly IGroupClient _groupClient;

        private readonly ILogger<AdminAgencyController> _logger;

        private readonly List<SelectListItem> _roles = new List<SelectListItem> { new("Admin Agency", "1"), new("Operator", "2"), new("User", "3") };

        public AdminAgencyController(IAdminAgencyClient adminAgencyClient, IGroupClient groupClient, ILogger<AdminAgencyController> logger)
        {
            _adminAgencyClient = adminAgencyClient;
            _groupClient = groupClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int agencyId = 0)
        {
            if (agencyId == 0)
            {
                agencyId = await _adminAgencyClient.GetAgencyId();
                if (agencyId == 0)
                {
                    return View();
                }
            }
            var sheets = await _adminAgencyClient.GetSheets(agencyId);

            var groups = await _groupClient.GetGroupsAsync(agencyId);
            var shifts = await _adminAgencyClient.GetShifts();
            var cabinets = await _adminAgencyClient.GetCabinets(agencyId);

            if (sheets != null)
            {
                foreach (var sheet in sheets)
                {
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

            ViewData["Groups"] = groups?.Select(g => new SelectListItem(g.Description, g.Id.ToString()));
            ViewData["Shifts"] = shifts?.Select((s, i) => new SelectListItem(s, $"{i + 1}"));
            ViewData["Cabinets"] = cabinets?.Select(c => new SelectListItem(c.Name, c.Id.ToString()));
            ViewData["agencyId"] = agencyId;

            return View(sheets);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSheets(int[] sheetsId)
        {
            List<int> errorDelitingSheets = new List<int>();
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
            ViewData["FreeUsers"] = await _adminAgencyClient.GetNonAgencyUsers();
            ViewData["Roles"] = _roles;
            return View("Edit", await _adminAgencyClient.GetAgencyById(agency.Id));
        }
    }
}
