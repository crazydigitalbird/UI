using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Infrastructure.Filters;

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

        [ServiceFilter(typeof(UpdateSessionAttribute))]
        public async Task<IActionResult> Index()
        {
            var sheets = await _operatorClient.GetSheetsViewAsync();
            if (sheets == null)
            {
                return View();
            }

            Task statusAndMediaTask = _sheetClient.GettingStatusAndMedia(sheets);
            Task<int> operatorIdTask = _operatorClient.GetOperatorIdAsync();
            await Task.WhenAll(statusAndMediaTask, operatorIdTask);

            var operatorId = operatorIdTask.Result;

            var endDateTime = DateTime.Now;
            var currentMonth = endDateTime.Month;
            var days = endDateTime.Day == 31 ? 31 : 30;
            var beginDateTime = endDateTime - TimeSpan.FromDays(days);
            var balances = await _balanceClient.GetOperatorBalances(operatorId, beginDateTime, endDateTime);
#if DEBUGOFFLINE
            balances.ForEach(b => b.Sheet.Id = 1);
#endif            
            foreach (var sheet in sheets)
            {
                sheet.Balance = balances.Where(b => b.Sheet.Id == sheet.Id).Sum(ob => ob.Cash);
            }

            var currentMonthBalances = balances.Where(b => b.Date.Month == currentMonth).ToArray();
            var balancesByMessageType = new decimal[7];
            balancesByMessageType[0] = currentMonthBalances.Where(b => b.Type == "chat_message").Sum(b => b.Cash);
            balancesByMessageType[1] = currentMonthBalances.Where(b => b.Type == "chat_photo").Sum(b => b.Cash);
            balancesByMessageType[2] = currentMonthBalances.Where(b => b.Type == "chat_video").Sum(b => b.Cash);
            balancesByMessageType[3] = currentMonthBalances.Where(b => b.Type == "chat_audio").Sum(b => b.Cash);
            balancesByMessageType[4] = currentMonthBalances.Where(b => b.Type == "chat_sticker").Sum(b => b.Cash);
            balancesByMessageType[5] = currentMonthBalances.Where(b => b.Type == "read_mail").Sum(b => b.Cash);
            balancesByMessageType[6] = currentMonthBalances.Sum(b => b.Cash) - balancesByMessageType.Take(6).Sum();

            var balancesMonth = new decimal[endDateTime.Day];
            for(int i = 0; i < balancesMonth.Length; i++)
            {
                balancesMonth[i] = currentMonthBalances.Where(b => b.Date.Day == i + 1).Sum(b => b.Cash); ;
            }

            ViewData["balancesByMessageType"] = string.Join(";", balancesByMessageType).Replace(",", ".");
            ViewData["balancesMonth"] = string.Join(";", balancesMonth).Replace(",", ".");
            ViewData["balancesAll"] = balances.Sum(b => b.Cash);

            return View(sheets);
        }

        [HttpGet]
        public async Task<IActionResult> BalancesType(int month, int year)
        {
            try
            {
                var operatorId = await _operatorClient.GetOperatorIdAsync();

                var beginDateTime = new DateTime(year, month, 1);
                var days = DateTime.DaysInMonth(year, month);
                var endDateTime = new DateTime(year, month, days);
                var balances = await _balanceClient.GetOperatorBalances(operatorId, beginDateTime, endDateTime);

                var balancesByMessageType = new decimal[7];
                balancesByMessageType[0] = balances.Where(b => b.Type == "chat_message").Sum(b => b.Cash);
                balancesByMessageType[1] = balances.Where(b => b.Type == "chat_photo").Sum(b => b.Cash);
                balancesByMessageType[2] = balances.Where(b => b.Type == "chat_video").Sum(b => b.Cash);
                balancesByMessageType[3] = balances.Where(b => b.Type == "chat_audio").Sum(b => b.Cash);
                balancesByMessageType[4] = balances.Where(b => b.Type == "chat_sticker").Sum(b => b.Cash);
                balancesByMessageType[5] = balances.Where(b => b.Type == "read_mail").Sum(b => b.Cash);
                balancesByMessageType[6] = balances.Sum(b => b.Cash) - balancesByMessageType.Take(6).Sum();

                return Ok(balancesByMessageType);
            }
            catch (Exception)
            {
            }
            return StatusCode(500, $"Error getting a balance type for a month {month}, year {year}");
        }

        [HttpGet]
        public async Task<IActionResult> BalancesAllMonth(int month, int year)
        {
            try
            {
                var operatorId = await _operatorClient.GetOperatorIdAsync();

                var beginDateTime = new DateTime(year, month, 1);
                var days = DateTime.DaysInMonth(year, month);
                var endDateTime = new DateTime(year, month, days);
                var balances = await _balanceClient.GetOperatorBalances(operatorId, beginDateTime, endDateTime);

                var balancesMonth = new decimal[days];
                for (int i = 0; i < balancesMonth.Length; i++)
                {
                    balancesMonth[i] = balances.Where(b => b.Date.Day == i + 1).Sum(b => b.Cash); ;
                }

                return Ok(balancesMonth);
            }
            catch (Exception)
            {
            }
            return StatusCode(500, $"Error getting a balance all month for a month {month}, year {year}");
        }        
    }
}
