using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class NotesViewComponent : ViewComponent
    {
        private readonly IOperatorClient _operatorClient;

        private readonly ILogger<NotesViewComponent> _logger;

        public NotesViewComponent(IOperatorClient operatorClient, ILogger<NotesViewComponent> logger)
        {
            _operatorClient = operatorClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(int profileId)
        {
            List<Note> notes= await _operatorClient.Notes(User.Identity.Name, profileId);
            ViewData["profileId"] = profileId;
            return View(notes);
        }
    }
}
