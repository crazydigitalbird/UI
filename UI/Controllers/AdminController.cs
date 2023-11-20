using Core.Models.Agencies;
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
    [ServiceFilter(typeof(UpdateSessionAttribute))]
    public class AdminController : Controller
    {
        private readonly IAdminClient _adminClient;

        private readonly IAdminAgencyClient _adminAgencyClient;

        private readonly IAuthenticationClient _authenticationClient;

        private readonly ILogger<AdminController> _logger;

        private readonly List<SelectListItem> _roles = new List<SelectListItem> { new("Admin Agency", "1"), new("Operator", "2"), new("User", "3") };

        public AdminController(IAdminClient adminClient, IAdminAgencyClient adminAgencyClient, IAuthenticationClient authenticationClient, ILogger<AdminController> logger)
        {
            _adminClient = adminClient;
            _adminAgencyClient = adminAgencyClient;
            _authenticationClient = authenticationClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var agencys = await _adminClient.GetAgencies();
            if (agencys is null)
            {
                TempData["Error"] = "Error getting all the agencies";
                return View();
            }
            return View(agencys);
        }

        public async Task<IActionResult> AddAgency()
        {
            ViewData["FreeUsers"] = await _adminAgencyClient.GetNonAgencyUsers();
            ViewData["Roles"] = _roles;
            return View("Edit");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAgency(AgencyView agency)
        {
            if (ModelState.IsValid)
            {
                if (await _adminClient.AddAgency(agency))
                {
                    TempData["Message"] = $"Add agency '{agency.Name}'";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = $"Error adding a agency '{agency.Name}'";
            }
            ViewData["FreeUsers"] = await _adminAgencyClient.GetNonAgencyUsers();
            ViewData["Roles"] = _roles;
            return View("Edit", agency);
        }

        public async Task<IActionResult> EditAgency(int agencyId)
        {
            AgencyView agency = await _adminAgencyClient.GetAgencyById(agencyId);
            if (agency is null)
            {
                TempData["Error"] = $"Agency with id '{agencyId}' not found";
                return RedirectToAction(nameof(Index));
            }

            ViewData["agencyId"] = agencyId;
            ViewData["FreeUsers"] = await _adminAgencyClient.GetNonAgencyUsers();
            ViewData["Roles"] = _roles;

            return View("Edit", agency);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAgency(AgencyView agency, List<ApplicationUser> originalUsers)
        {
            if (ModelState.IsValid)
            {
                if (await _adminAgencyClient.UpdateAgency(agency, originalUsers))
                {
                    TempData["Message"] = $"The '{agency.Name}' agency has been updated";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = $"'{agency.Name}' agency update error";
            }
            ViewData["agencyId"] = agency.Id;
            ViewData["FreeUsers"] = await _adminAgencyClient.GetNonAgencyUsers();
            ViewData["Roles"] = _roles;
            return View("Edit", await _adminAgencyClient.GetAgencyById(agency.Id));
        }

        public async Task<IActionResult> DeleteAgency(int agencyId)
        {
            if (await _adminClient.DeleteAgency(agencyId))
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
            return View(await _adminClient.GetUsers());
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin([Required] int userId)
        {
            if (ModelState.IsValid)
            {
                if (await _adminClient.AddAdmin(userId))
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
            return View(await _adminClient.GetUsers());
        }

        public async Task<IActionResult> DeleteAdmin(int adminId, string email)
        {
            if (await _adminClient.DeleteAdmin(adminId))
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
