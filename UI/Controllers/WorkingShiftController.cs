using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Infrastructure.Filters;

namespace UI.Controllers
{
    [Authorize]
    [APIAuthorize]
    public class WorkingShiftController : Controller
    {
        private readonly IWorkingShiftClient _workingShiftClient;
        private readonly ILogger<WorkingShiftController> _logger;

        public WorkingShiftController(IWorkingShiftClient workingShiftClient, ILogger<WorkingShiftController> logger)
        {
            _workingShiftClient = workingShiftClient;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Start()
        {
            if (await _workingShiftClient.Start())
            {
                HttpContext.Session.SetInt32("OnShift", 1);
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Stop()
        {
            if (await _workingShiftClient.Stop())
            {
                HttpContext.Session.SetInt32("OnShift", 0);
                return Ok();
            }
            return BadRequest();
        }
    }
}