using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UI.Infrastructure.API;

namespace UI.Controllers
{
    public class SuperAdminController : Controller
    {
        private readonly ISuperAdminClient _superAdminClient;

        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(ISuperAdminClient superAdminClient, ILogger<SuperAdminController> logger)
        {
            _superAdminClient = superAdminClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var groups = await _superAdminClient.GetGroups();
            var shifts = await _superAdminClient.GetShifts();
            var cabinets = await _superAdminClient.GetCabinets();
            var profiles = await _superAdminClient.GetProfiles();

            ViewData["Groups"] = groups.Select((g, i) => new SelectListItem(g, $"{i + 1}"));
            ViewData["Shifts"] = shifts.Select((s, i) => new SelectListItem(s, $"{i + 1}"));
            ViewData["Cabinets"] = cabinets.Select((c, i) => new SelectListItem(c, $"{i + 1}"));

            return View(profiles);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProfiles(int[] profilesId)
        {
            var response = await _superAdminClient.DeleteProfiles(profilesId);
            if (response?.IsSuccessStatusCode ?? false)
            {
                return Ok();
            }
            return StatusCode(500, "Error deleting profiles");        
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangeGroup(int profileId, int groupId)
        {
            var response = await _superAdminClient.ChangeGroup(profileId, groupId);
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
                var response = await _superAdminClient.AddGroup(nameGroup);
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
            var response = await _superAdminClient.ChangeShift(profileId, shiftId);
            if (response?.IsSuccessStatusCode ?? false)
            {
                return Ok();
            }
            return StatusCode(500, $"For profile id: {profileId}, the shift value has not been changed");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeCabinet(int profileId, int cabinetId)
        {
            var response = await _superAdminClient.ChangeCabinet(profileId, cabinetId);
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
            var response = await _superAdminClient.DeleteOperatorFromProfile(operatorId, profileId);
            if (response?.IsSuccessStatusCode ?? false)
            {
                return Ok();
            }
            return StatusCode(500, $"For profile id: {profileId}, the operator id: {operatorId} has not been deleted");
        }

        [HttpPost]
        public async Task<IActionResult> AddOperatorFromProfile(int operatorId, int profileId)
        {
            var response = await _superAdminClient.AddOperatorFromProfile(operatorId, profileId);
            if (response?.IsSuccessStatusCode ?? false)
            {
                return Ok(User.Identity.Name);
            }
            return StatusCode(500, $"For profile id: {profileId}, the operator id: {operatorId} has not been added");
        }
    }
}
