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
        private readonly ICommentClient _commentClient;

        public OperatorController(IOperatorClient operatorClient, ISheetClient sheetClient, IBalanceClient balanceClient, ICommentClient commentClient)
        {
            _operatorClient = operatorClient;
            _sheetClient = sheetClient;
            _balanceClient = balanceClient;
            _commentClient = commentClient;
        }

        public async Task<IActionResult> Index()
        {
            var sheets = await _operatorClient.GetSheetsAsync();
            if(sheets == null)
            {
                return View();
            }

            await _sheetClient.GettingStatusAndMedia(sheets);

            //var sheetsIsNewComments = new List<int>();
            //var operatorId = await _operatorClient.GetOperatorIdAsync();
            //foreach (var sheet in sheets)
            //{
            //    var endDateTime = DateTime.Now;
            //    var beginDateTime = endDateTime - TimeSpan.FromDays(30);
            //    //sheet.Balance = (await _balanceClient.GetOperatorBalance( operatorId, beginDateTime, endDateTime)).Sum(ob => ob.Cash);

            //    var newCommentsCount = await _commentClient.GetNewSheetCommentsCountAsync(sheet.Id);
            //    if(newCommentsCount > 0)
            //    {
            //        sheetsIsNewComments.Add(sheet.Id);
            //    }
            //}
            //ViewData["sheetsIsNewComments"] = sheetsIsNewComments;
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

        public IActionResult Comments(int sheetId)
        {
            return ViewComponent("Comments", sheetId);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int sheetId, string text)
        {
            var comment = await _commentClient.AddCommentAsync(sheetId, text);
            if (comment != null)
            {
                return Ok(comment);
            }
            return StatusCode(500, $"Error creating comment for sheet with id: {sheetId}");
        }
    }
}
