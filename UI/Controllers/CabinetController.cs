using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UI.Infrastructure.API;
using UI.Models;
using Newtonsoft.Json;
using UI.Infrastructure.Filters;

namespace UI.Controllers
{
    [Authorize]
    [APIAuthorize]
    public class CabinetController : Controller
    {
        private readonly ICabinetClient _cabinetClient;
        private readonly ILogger<CabinetController> _logger;

        public CabinetController(ICabinetClient cabinetClient, ILogger<CabinetController> logger)
        {
            _cabinetClient = cabinetClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Cabinet> cabinets = await _cabinetClient.GetCabinetsAsync();
            return View(cabinets);
        }

        [HttpPost]
        public async Task<IActionResult> Add([Required] string name)
        {
            if (ModelState.IsValid)
            {
                var response = await _cabinetClient.AddAsync(name);
                if (response?.IsSuccessStatusCode ?? false)
                {
                    return Ok(await response.Content.ReadAsStringAsync());
                }
                return StatusCode(500, $"Error adding the {name} cabinet");
            }
            else
            {
                return BadRequest("Cabinet name is empty");
            }
        }

        [HttpPost]
        public async Task<IActionResult> BindCabinetsToUser(List<int> cabinets, [Required] int userId, [Required] string userName)
        {
            if (ModelState.IsValid)
            {
                var response = await _cabinetClient.BindCabinetToUserAsync(cabinets, userId);
                if (response?.IsSuccessStatusCode ?? false)
                {
                    var context = new {userId = userId, userName = userName, cabinets = cabinets};
                    var contextJson = JsonConvert.SerializeObject(context);
                    return Ok(contextJson);
                }
            }
            return BadRequest($"Error when binding/unbinding cabinets from the user {userName}");
        }

        [HttpPost]
        public async Task<IActionResult> UnbindCabinetToUser([Required] int cabinetId, [Required] int userId)
        {
            if (ModelState.IsValid)
            {
                var response = await _cabinetClient.UnbindCabinetToUserAsync(cabinetId, userId);
                if (response?.IsSuccessStatusCode ?? false)
                {
                    return Ok();
                }
            }
            return BadRequest($"Error when unbinding cabinet C{cabinetId} from the user");
        }
    }
}
