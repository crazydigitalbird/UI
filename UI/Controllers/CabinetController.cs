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
    [AutoValidateAntiforgeryToken]
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

        [ServiceFilter(typeof(GetAgencyIdFilter))]
        public async Task<IActionResult> Index(int agencyId = 0)
        {
            var getAgencyOperatorsTask = _adminAgencyClient.GetAgencyOperators(agencyId);
            var getCabinetsTask = _cabinetClient.GetCabinetsAsync(agencyId);
            await Task.WhenAll(getAgencyOperatorsTask, getCabinetsTask);
            ViewData["agencyId"] = agencyId;
            ViewData["operators"] = getAgencyOperatorsTask.Result;
            return View(getCabinetsTask.Result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([Required]int agencyId, [Required] string nameCabinet)
        {
            if (ModelState.IsValid)
            {
                var cabinet = await _cabinetClient.AddAsync(agencyId, nameCabinet);
                if (cabinet != null)
                {
                    return Ok(cabinet);
                }
                return StatusCode(500, $"Error adding the {nameCabinet} cabinet");
            }
            else
            {
                return BadRequest("Cabinet name is empty");
            }
        }

        [HttpPost]
        public async Task<IActionResult> BindCabinetsToUser([Required] int agencyId, List<int> cabinets, [Required] int operatorId, [Required] string userName)
        {
            if (ModelState.IsValid)
            {
                if(cabinets.Count == 0)
                {
                    return Ok();
                }
                var bindCabinetsToOperatorTasks = cabinets.Select(cabinetId => _cabinetClient.BindCabinetToOperatorAsync(agencyId, cabinetId, operatorId)).ToArray();
                await Task.WhenAll(bindCabinetsToOperatorTasks);

                var bindedCabinets = new List<int>();
                foreach (var task in bindCabinetsToOperatorTasks)
                {
                    if (task.Result > 0)
                    {
                        bindedCabinets.Add(task.Result);
                    }
                }
                if (bindedCabinets.Any())
                {
                    var errorBindCabinets = cabinets.Except(bindedCabinets);
                    var context = new { userId = operatorId, userName, cabinets, errorBindCabinets };
                    return Ok(context);
                }
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
