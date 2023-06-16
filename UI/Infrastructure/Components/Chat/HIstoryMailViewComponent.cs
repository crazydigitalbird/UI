using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class HistoryMailViewComponent : ViewComponent
    {
        private readonly IMailClient _mailClient;
        private readonly IChatClient _chatClient;
        private readonly IOperatorClient _operatorClient;
        private readonly ILogger<HistoryMailViewComponent> _logger;

        public HistoryMailViewComponent(IMailClient mailClient, IChatClient chatClient, IOperatorClient operatorClient, ILogger<HistoryMailViewComponent> logger)
        {
            _mailClient = mailClient;
            _chatClient = chatClient;
            _operatorClient = operatorClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, int idRegularUser, long idCorrespondence = 0, int page = 1, int limit = 40)
        {
            var mailHistoryData = await _mailClient.HistoryAsync(sheet, idRegularUser);
            ViewData["ownerInfo"] = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
            ViewData["regularInfo"] = await _chatClient.GetManProfile(sheet, idRegularUser);
            if(mailHistoryData?.Data?.History?.Any(m => m.Attachments?.Videos?.Count > 0) ?? false)
            {

            }
            return View(mailHistoryData);
        }
    }
}
