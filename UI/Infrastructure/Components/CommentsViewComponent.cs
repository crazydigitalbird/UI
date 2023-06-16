using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;

namespace UI.Infrastructure.Components
{
    public class CommentsViewComponent : ViewComponent
    {
        private readonly ICommentClient _commentClient;

        private readonly ILogger<CommentsViewComponent> _logger;

        public CommentsViewComponent(ICommentClient commentClient, ILogger<CommentsViewComponent> logger)
        {
            _commentClient = commentClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(int sheetId, int idRegularUser)
        {
            var comments = await _commentClient.GetCommentsAsync(sheetId, idRegularUser);
            return View(comments);
        }
    }
}
