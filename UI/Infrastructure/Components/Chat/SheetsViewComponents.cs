using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class SheetsViewComponents : ViewComponent
    {
        private readonly IChatClient _chatClient;

        private readonly ILogger<SheetsViewComponents> _logger;

        public SheetsViewComponents(IChatClient chatClient, ILogger<SheetsViewComponents> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(IEnumerable<Sheet> sheets, string criteria, string cursor)
        {
            List<Messanger> messangers = new List<Messanger>();
            foreach (var sheet in sheets)
            {
                var messanger = await _chatClient.GetMessangerAsync(sheet, criteria, cursor, 20) ?? new Messanger();
                messanger.Sheet = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
                await _chatClient.GetManProfiles(sheet, messanger.Dialogs);
                messangers.Add(messanger);
            }
            return View(messangers);
        }
    }
}
