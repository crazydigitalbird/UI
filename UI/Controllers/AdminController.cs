using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IAdminClient _adminClient;

        private readonly ILogger<AdminController> _logger;

        private readonly List<SelectListItem> _roles = new List<SelectListItem> { new("Admin Agency", "1"), new("Operator", "2"), new("User", "3") };

        public AdminController(IAdminClient adminClient, ILogger<AdminController> logger)
        {
            _adminClient = adminClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _adminClient.GetAgencies());
        }

        public async Task<IActionResult> AddAgency()
        {
            ViewData["FreeUsers"] = await _adminClient.GetFreeUsers();
            ViewData["Roles"] = _roles;
            return View("Edit");
        }

        [HttpPost]
        public async Task<IActionResult> AddAgency(Agency agency)
        {
            if (ModelState.IsValid)
            {
                var response = await _adminClient.AddAgency(agency);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = $"Add agency '{agency.Name}'";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = $"Error adding a agency '{agency.Name}'";
            }
            ViewData["FreeUsers"] = await _adminClient.GetFreeUsers();
            ViewData["Roles"] = _roles;
            return View("Edit", agency);
        }

        public async Task<IActionResult> EditAgency(int agencyId)
        {
            Agency agency = await _adminClient.GetAgencyById(agencyId);
            if (agency == null)
            {
                TempData["Error"] = $"Agency with id '{agencyId}' not found";
                return RedirectToAction(nameof(Index));
            }
            ViewData["FreeUsers"] = await _adminClient.GetFreeUsers();
            ViewData["Roles"] = _roles;
            return View("Edit", agency);
        }

        [HttpPost]
        public async Task<IActionResult> EditAgency(Agency agency, List<ApplicationUser> originalUsers)
        {
            if (ModelState.IsValid)
            {
                var deleteUsers = originalUsers.ExceptBy(agency.Users.Select(u => u.Id), ou => ou.Id).ToList();

                var addUsers = agency.Users.ExceptBy(originalUsers.Select(ou => ou.Id), u => u.Id).ToList();

                var updateUser = agency.Users.Intersect(originalUsers, new MyApplicationUserIsUpdateRoleComparer()).ToList();

                var response = await _adminClient.UpdateAgency(agency);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = $"The '{agency.Name}' agency has been updated";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = $"'{agency.Name}' agency update error";
            }
            ViewData["FreeUsers"] = await _adminClient.GetFreeUsers();
            ViewData["Roles"] = _roles;
            return View("Edit", await _adminClient.GetAgencyById(agency.Id));
        }

        public async Task<IActionResult> DeleteAgency(int agencyId)
        {
            var response = await _adminClient.DeleteAgency(agencyId);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = $"The agency has been deleted";
            }
            else
            {
                TempData["Error"] = $"Error deleting a agency with id '{agencyId}'";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Admins()
        {
            return View(await _adminClient.GetAdmins());
        }

        public async Task<IActionResult> AddAdmin()
        {
            return View(await _adminClient.GetFreeUsers());
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin([Required] int userId)
        {
            if (ModelState.IsValid)
            {
                var response = await _adminClient.AddAdmin(userId);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Added a new administrator";
                    return RedirectToAction(nameof(Admins));
                }
                TempData["Error"] = "Error adding a new admin";
            }
            else
            {
                TempData["Error"] = "Please select an existing user";
            }
            return View(await _adminClient.GetFreeUsers());
        }

        public async Task<IActionResult> DeleteAdmin(int adminId, string email)
        {
            var response = await _adminClient.DeleteAdmin(adminId);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = $"Admin {email} has been deleted";
            }
            else
            {
                TempData["Error"] = $"Error deleting admin {email}";
            }
            return RedirectToAction(nameof(Admins));
        }
    }
}
