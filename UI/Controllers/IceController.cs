using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Infrastructure.Filters;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    [APIAuthorize]
    [ServiceFilter(typeof(UpdateSessionAttribute))]
    public class IceController : Controller
    {
        private readonly IIcebreakersClient _icebreakersClient;
        private readonly IOperatorClient _operatorClient;
        private readonly ISheetClient _sheetClient;
        private readonly ILogger<IceController> _logger;

        public IceController(IIcebreakersClient icebreakersClient, IOperatorClient operatorClient, ISheetClient sheetClient, ILogger<IceController> logger)
        {
            _icebreakersClient = icebreakersClient;
            _operatorClient = operatorClient;
            _sheetClient = sheetClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var sheets = await _operatorClient.GetSheetsAsync();
            ViewData["icesInfo"] = await _icebreakersClient.IcesInfo(sheets) ?? new List<IceInfo>();
            return View(sheets);
        }

        [HttpGet]
        public async Task<IActionResult> Ices(int sheetId, int idLast = 0)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Ices", new { sheet, idLast });
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Progress(int sheetId)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Progress", new { sheet });
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Trash(int sheetId)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("Trash", new { sheet });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int sheetId, string content, IceType iceType)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var iceId = await _icebreakersClient.Create(sheet, iceType, content);
                    if (iceId > 0)
                    {
                        return Ok();
                    }
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Switch(int sheetId, long iceId, string status)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                await _icebreakersClient.Switch(sheet, iceId, status);
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int sheetId, long iceId)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (await _icebreakersClient.Delete(iceId))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Reply(int sheetId, long iceId)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (await _icebreakersClient.Reply(iceId))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }
    }
}
