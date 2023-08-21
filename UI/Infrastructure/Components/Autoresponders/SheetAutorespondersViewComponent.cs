using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;

namespace UI.Infrastructure.Components
{
    public class SheetAutorespondersViewComponent : ViewComponent
    {
        private readonly IAutorespondersClient _autorespondersClient;
        private readonly ILogger<SheetAutorespondersViewComponent> _logger;

        public SheetAutorespondersViewComponent(IAutorespondersClient autorespondersClient, ILogger<SheetAutorespondersViewComponent> logger)
        {
            _autorespondersClient = autorespondersClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet)
        {
            var autoresponder = await _autorespondersClient.GetAutorespondersAsync(sheet);
            if (autoresponder?.Data?.StackType != null)
            {
                return View(autoresponder.Data.StackType);
            }
            return Content(string.Empty);
        }
    }
}
