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
        private readonly ICommentClient _commentClient;

        public OperatorController(IOperatorClient operatorClient, ICommentClient commentClient)
        {
            _operatorClient = operatorClient;
            _commentClient = commentClient;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<SheetView> sheets = await _operatorClient.GetSheetsAsync();
            if(sheets == null)
            {
                return View();
            }

            var sheetsIsNewComments = new List<int>();
            foreach (var sheet in sheets)
            {
                var newCommentsCount = await _commentClient.GetNewSheetCommentsCountAsync(sheet.Id);
                if(newCommentsCount > 0)
                {
                    sheetsIsNewComments.Add(sheet.Id);
                }
            }
            ViewData["sheetsIsNewComments"] = sheetsIsNewComments;
            return View(sheets);
        }

        public async Task<IActionResult> Balance(Interval interval)
        {
            Dictionary<int, int> balance = await _operatorClient.GetBalanceAsync(User.Identity.Name, interval);
            if (balance != null)
            {
                return Ok(balance);
            }
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
