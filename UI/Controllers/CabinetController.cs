using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using UI.Infrastructure.API;
using UI.Infrastructure.Filters;

namespace UI.Controllers
{
    [Authorize]
    [APIAuthorize]
    [ServiceFilter(typeof(UpdateSessionAttribute))]
    public class CabinetController : Controller
    {
        private readonly ICabinetClient _cabinetClient;
        private readonly IAdminAgencyClient _adminAgencyClient;
        private readonly ILogger<CabinetController> _logger;

        public CabinetController(ICabinetClient cabinetClient, IAdminAgencyClient adminAgencyClient, ILogger<CabinetController> logger)
        {
            _cabinetClient = cabinetClient;
            _adminAgencyClient = adminAgencyClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int agencyId = 0)
        {
            if (agencyId == 0)
            {
                agencyId = await _adminAgencyClient.GetAgencyId();
            }
            ViewData["agencyId"] = agencyId;
            ViewData["operators"] = await _adminAgencyClient.GetAgencyOperators(agencyId);
            var cabinets = await _cabinetClient.GetCabinetsAsync(agencyId);
            return View(cabinets);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int agencyId, [Required] string name)
        {
            if (ModelState.IsValid)
            {
                var cabinet = await _cabinetClient.AddAsync(agencyId, name);
                if (cabinet != null)
                {
                    return Ok(cabinet);
                }
                return StatusCode(500, $"Error adding the {name} cabinet");
            }
            else
            {
                return BadRequest("Cabinet name is empty");
            }
        }

        [HttpPost]
        public async Task<IActionResult> BindCabinetsToUser(int agencyId, List<int> cabinets, [Required] int operatorId, [Required] string userName)
        {
            if (ModelState.IsValid)
            {
                foreach (var cabinet in cabinets)
                {
                    if (!await _cabinetClient.BindCabinetToOperatorAsync(agencyId, cabinet, operatorId))
                    {
                        return BadRequest($"Error when binding cabinets from the user {userName}");
                    }
                }
                var context = new { userId = operatorId, userName = userName, cabinets = cabinets };
                var contextJson = JsonConvert.SerializeObject(context);
                return Ok(contextJson);
            }
            return BadRequest($"Error when binding cabinets from the user {userName}");
        }

        [HttpPost]
        public async Task<IActionResult> UnbindCabinetToUser([Required] int cabinetId, [Required] int operatorId)
        {
            if (ModelState.IsValid)
            {
                if (await _cabinetClient.UnbindCabinetToUserAsync(cabinetId, operatorId))
                {
                    return Ok();
                }
            }
            return BadRequest($"Error when unbinding cabinet C{cabinetId} from the user");
        }
    }
}
