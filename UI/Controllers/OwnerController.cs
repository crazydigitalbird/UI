using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    public class OwnerController : Controller
    {
        private readonly IOwnerClient _ownerClient;

        private readonly ILogger<OwnerController> _logger;

        public OwnerController(IOwnerClient ownerClient, ILogger<OwnerController> logger)
        {
            _ownerClient = ownerClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _ownerClient.GetAgencies());
        }

        public IActionResult AddAgency()
        {
            ViewData["Title"] = "Add Agency";
            return View("Edit");
        }

        [HttpPost]
        public async Task<IActionResult> AddAgency(Agency agency)
        {
            if (ModelState.IsValid)
            {
                var response = await _ownerClient.AddAgency(agency);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = $"Add agency '{agency.Name}'";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = response.StatusCode.ToString();
            }
            ViewData["Title"] = "Add Agency";
            return View("Edit", agency);
        }

        public async Task<IActionResult> EditAgency(int id)
        {
            ViewData["Title"] = "Edit Agency";
            Agency agency = await _ownerClient.GetAgencyById(id);
            if (agency == null)
            {
                TempData["Error"] = "Agency not found";
                return RedirectToAction(nameof(Index));
            }
            return View("Edit", agency);
        }

        [HttpPost]
        public async Task<IActionResult> EditAgency(Agency agency)
        {
            if (ModelState.IsValid)
            {
                var response = await _ownerClient.UpdateAgency(agency);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = $"Update agency '{agency.Name}'";
                    return RedirectToAction(nameof(Index));
                }
                TempData["Error"] = response.StatusCode.ToString();
            }
            ViewData["Title"] = "Edit Agency";
            return View("Edit", agency);
        }

        public async Task<IActionResult> DeleteAgency(int id)
        {
            var response = await _ownerClient.DeleteAgency(id);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = $"Deleted agency";
            }
            else
            {
                TempData["Error"] = response.StatusCode.ToString();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
