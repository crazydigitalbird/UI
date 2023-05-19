using Core.Models.Sheets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UI.Infrastructure.API;
using UI.Infrastructure.Repository;

namespace UI.Infrastructure.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IDictionaryRepository<SheetDialogKey, NewMessage> _dictionary;
        private readonly IOperatorClient _operatorClient;
        private readonly IRazorPartialToStringRenderer _renderer;

        //private readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();

        public ChatHub(IDictionaryRepository<SheetDialogKey, NewMessage> dictionary, IOperatorClient operatorClient, IRazorPartialToStringRenderer renderer)
        {
            _dictionary = dictionary;
            _operatorClient = operatorClient;
            _renderer = renderer;
        }

        public async Task SendInitialAllNewMessagesFromAllMen()
        {
            var sheets = await _operatorClient.GetSheetsAsync();
            if (sheets == null)
            {
                return;
            }

            var allMessages = _dictionary.Active.Values.Where(v => sheets.Any(sheet => sheet.Id == v.SheetInfo.SheetId))
                .OrderByDescending(m => m.Dialogue.LastMessage.DateCreated);

            var body = await _renderer.RenderPartialToStringAsync("_AllNewMessagesFromAllMen", allMessages);

            await Clients.Caller.SendAsync("ReceiveInitialAllNewMessagesFromAllMen", body);
        }

        #region LiveChat Add and Remove Group

        public async Task AddToGroup(int sheetId, int idInterlocutor)
        {
            string groupName = $"{sheetId}-{idInterlocutor}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task RemoveFromGroup(int sheetId, int idInterlocutor)
        {
            string groupName = $"{sheetId}-{idInterlocutor}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        #endregion

        public override async Task OnConnectedAsync()
        {
            var sheets = await _operatorClient.GetSheetsAsync();
            if (sheets != null)
            {
                await AddToGroups(sheets);
            }
            await base.OnConnectedAsync();
        }

        private async Task AddToGroups(List<Sheet> sheets)
        {
            foreach (var sheet in sheets)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"{sheet.Id}");
            }
        }
    }
}

