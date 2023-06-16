using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class HistoryViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;
        private readonly IOperatorClient _operatorClient;
        private readonly ILogger<HistoryViewComponent> _logger;

        public HistoryViewComponent(IChatClient chatClient, IOperatorClient operatorClient, ILogger<HistoryViewComponent> logger)
        {
            _chatClient = chatClient;
            _operatorClient = operatorClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(List<Sheet> sheets, string cursor = "", bool isNew = false, long idLastMessage = 0, int limit = 40)
        {
            ViewData["sheets"] = sheets;
            ViewData["isNew"] = isNew;

            var idUsers = string.Join(",", sheets.Select(s => s.Identity));
            Messenger messenger = await _chatClient.GetHistoryAsync(idUsers, cursor, limit);
            if(messenger == null)
            {
                return null;
            }

            if (isNew)
            {
                messenger.Dialogs = messenger.Dialogs.TakeWhile(d => d.LastMessage.Id != idLastMessage).ToList();
            }

            var idDialogues = messenger.Dialogs.Select(d => d.IdInterlocutor).Distinct().ToList();
            var sheetsInfo = await _chatClient.GetManProfiles(sheets.FirstOrDefault(), idDialogues);

            messenger.Dialogs.ForEach(d =>
            {
                var manSheetInfo = sheetsInfo.FirstOrDefault(i => i.Id == d.IdInterlocutor);
                if (manSheetInfo != null)
                {
                    d.UserName = manSheetInfo.Personal.Name;
                    d.Avatar = manSheetInfo.Personal.AvatarSmall;
                }
            });

            var operatorsId = messenger.Dialogs.Where(d => d.Operator > 0).Select(d => d.Operator).Distinct();
            var applicationUsers = await _operatorClient.GetUsersLogins(operatorsId);
            ViewData["applicationUsers"] = applicationUsers;

            return View(messenger);
        }
    }
}
