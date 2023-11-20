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
    public class AutorespondersController : Controller
    {
        private readonly IOperatorClient _operatorClient;
        private readonly ISheetClient _sheetClient;
        private readonly IBalanceClient _balanceClient;
        private readonly IAutorespondersClient _autorespondersClient;

        public AutorespondersController(IOperatorClient operatorClient, ISheetClient sheetClient, IBalanceClient balanceClient, IAutorespondersClient autorespondersClient)
        {
            _operatorClient = operatorClient;
            _sheetClient = sheetClient;
            _balanceClient = balanceClient;
            _autorespondersClient = autorespondersClient;
        }

        public async Task<IActionResult> Index()
        {
            var sheets = await _operatorClient.GetSheetsAsync();
            Task<Dictionary<string, AutoresponderInfo>> AutorespondersInfoTask = _autorespondersClient.GetAutorespondersInfoAsync(sheets);
            Task<int> operatorIdTask = _operatorClient.GetOperatorIdAsync();
            await Task.WhenAll(AutorespondersInfoTask, operatorIdTask);

            var operatorId = operatorIdTask.Result;
            var endDateTime = DateTime.Now;
            var days = endDateTime.Day == 31 ? 31 : 30;
            var beginDateTime = endDateTime - TimeSpan.FromDays(days);
            var balances = await _balanceClient.GetOperatorBalances(operatorId, beginDateTime, endDateTime);
            ViewData["AutorespondersInfo"] = AutorespondersInfoTask.Result;
            ViewData["BalancesAll"] = balances.Sum(b => b.Cash);

            return View(sheets);
        }

        [HttpGet]
        public async Task<IActionResult> SheetAutoresponders(int sheetId)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                return ViewComponent("SheetAutoresponders", new { sheet });
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Clear(int sheetId, string stackType, AutoresponderMessages autoresponderMessages)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (await _autorespondersClient.ClearAutoresponderAsync(sheet, stackType.ToLower(), autoresponderMessages))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Update(int sheetId, string stackType, AutoresponderMessages autoresponderMessages, string message, int intervalStart, int intervalFinish, int limitMessage)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                if (await _autorespondersClient.UpdateAsync(sheet, stackType.ToLower(), autoresponderMessages, message, intervalStart, intervalFinish, limitMessage))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }
    }
}
