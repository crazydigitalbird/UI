using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    public class AdminAgencyController : Controller
    {
        private readonly IAdminAgencyClient _adminAgencyClient;

        private readonly ILogger<AdminAgencyController> _logger;

        private readonly List<SelectListItem> _roles = new List<SelectListItem> { new("Admin Agency", "1"), new("Operator", "2"), new("User", "3") };

        public AdminAgencyController(IAdminAgencyClient adminAgencyClient, ILogger<AdminAgencyController> logger)
        {
            _adminAgencyClient = adminAgencyClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var groups = await _adminAgencyClient.GetGroups();
            var shifts = await _adminAgencyClient.GetShifts();
            var cabinets = await _adminAgencyClient.GetCabinets();
            var profiles = await _adminAgencyClient.GetProfiles();

            ViewData["Groups"] = groups.Select((g, i) => new SelectListItem(g, $"{i + 1}"));
            ViewData["Shifts"] = shifts.Select((s, i) => new SelectListItem(s, $"{i + 1}"));
            ViewData["Cabinets"] = cabinets.Select((c, i) => new SelectListItem(c, $"{i + 1}"));

            return View(profiles);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProfiles(int[] profilesId)
        {
            var response = await _adminAgencyClient.DeleteProfiles(profilesId);
            if (response?.IsSuccessStatusCode ?? false)
            {
                return Ok();
            }
            return StatusCode(500, "Error deleting profiles");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeGroup(int profileId, int groupId)
        {
            var response = await _adminAgencyClient.ChangeGroup(profileId, groupId);
            if (response?.IsSuccessStatusCode ?? false)
            {
                return Ok();
            }
            return StatusCode(500, $"For profile id: {profileId}, the group value has not been changed");
        }

        [HttpPost]
        public async Task<IActionResult> AddGroup([Required] string nameGroup)
        {
            if (ModelState.IsValid)
            {
                var response = await _adminAgencyClient.AddGroup(nameGroup);
                if (response?.IsSuccessStatusCode ?? false)
                {
                    return Ok(await response.Content.ReadAsStringAsync());
                }
            }
            return BadRequest(nameGroup);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeShift(int profileId, int shiftId)
        {
            var response = await _adminAgencyClient.ChangeShift(profileId, shiftId);
            if (response?.IsSuccessStatusCode ?? false)
            {
                return Ok();
            }
            return StatusCode(500, $"For profile id: {profileId}, the shift value has not been changed");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeCabinet(int profileId, int cabinetId)
        {
            var response = await _adminAgencyClient.ChangeCabinet(profileId, cabinetId);
            if (response?.IsSuccessStatusCode ?? false)
            {
                return Ok();
            }
            return StatusCode(500, $"For profile id: {profileId}, the cabinet value has not been changed");
        }

        [HttpPost]
        public IActionResult GetOperators(int profileId)
        {
            return ViewComponent("Operators", profileId);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOperatorFromProfile(int operatorId, int profileId)
        {
            var response = await _adminAgencyClient.DeleteOperatorFromProfile(operatorId, profileId);
            if (response?.IsSuccessStatusCode ?? false)
            {
                return Ok();
            }
            return StatusCode(500, $"For profile id: {profileId}, the operator id: {operatorId} has not been deleted");
        }

        [HttpPost]
        public async Task<IActionResult> AddOperatorFromProfile(int operatorId, int profileId)
        {
            var response = await _adminAgencyClient.AddOperatorFromProfile(operatorId, profileId);
            if (response?.IsSuccessStatusCode ?? false)
            {
                return Ok(User.Identity.Name);
            }
            return StatusCode(500, $"For profile id: {profileId}, the operator id: {operatorId} has not been added");
        }

        public async Task<IActionResult> Users()
        {
            Agency agency = await _adminAgencyClient.GetAgency();
            ViewData["FreeUsers"] = await _adminAgencyClient.GetFreeUsers();
            ViewData["Roles"] = _roles;
            return View(agency);
        }

        [HttpPost]
        public async Task<IActionResult> Users(Agency agency, List<ApplicationUser> originalUsers)
        {
            if (ModelState.IsValid)
            {
                var deleteUsers = originalUsers.ExceptBy(agency.Users.Select(u => u.Id), ou => ou.Id).ToList();

                var addUsers = agency.Users.ExceptBy(originalUsers.Select(ou => ou.Id), u => u.Id).ToList();

                var updateUser = agency.Users.Intersect(originalUsers, new MyApplicationUserIsUpdateRoleComparer()).ToList();

                var response = await _adminAgencyClient.UpdateAgency(agency);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = $"Updated the list of users of the '{agency.Name}' agency";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = $"Error updating the list of users of the '{agency.Name}' anency";
            }
            agency = await _adminAgencyClient.GetAgency();
            ViewData["FreeUsers"] = await _adminAgencyClient.GetFreeUsers();
            ViewData["Roles"] = _roles;
            return View(agency);
        }
    }
}
