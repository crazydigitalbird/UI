using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Infrastructure.Filters;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    [APIAuthorize]
    public class OperatorController : Controller
    {
        private readonly IOperatorClient _operatorClient;
        private readonly ISheetClient _sheetClient;
        private readonly IBalanceClient _balanceClient;

        public OperatorController(IOperatorClient operatorClient, ISheetClient sheetClient, IBalanceClient balanceClient)
        {
            _operatorClient = operatorClient;
            _sheetClient = sheetClient;
            _balanceClient = balanceClient;
        }

        public async Task<IActionResult> Index()
        {
            var sheets = await _operatorClient.GetSheetsViewAsync();
            if(sheets == null)
            {
                return View();
            }

            await _sheetClient.GettingStatusAndMedia(sheets);

            var operatorId = await _operatorClient.GetOperatorIdAsync();
            var endDateTime = DateTime.Now;
            var beginDateTime = endDateTime - TimeSpan.FromDays(30);
            var balances = await _balanceClient.GetOperatorBalance(operatorId, beginDateTime, endDateTime);
            foreach(var sheet in sheets)
            {
                sheet.Balance = balances.Where(b => b.Sheet.Id == sheet.Id).Sum(ob => ob.Cash);
            }

            return View(sheets);
        }

        public async Task<IActionResult> Balance(Interval interval)
        {
            //Dictionary<int, int> balance = await _operatorClient.GetBalanceAsync(User.Identity.Name, interval);
            //if (balance != null)
            //{
            //    return Ok(balance);
            //}
            return StatusCode(500, $"Error getting a balance for a {interval}");
        }
    }
}
