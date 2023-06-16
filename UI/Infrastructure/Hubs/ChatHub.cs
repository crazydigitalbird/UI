using Core.Models.Sheets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Immutable;
using UI.Infrastructure.API;
using UI.Infrastructure.Repository;

namespace UI.Infrastructure.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IDictionaryRepository<SheetDialogKey, NewMessage> _dictionary;
        private readonly IOperatorClient _operatorClient;
        private readonly IChatClient _chatClient;
        private readonly IRazorPartialToStringRenderer _renderer;

        //private readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();

        public ChatHub(IDictionaryRepository<SheetDialogKey, NewMessage> dictionary, IOperatorClient operatorClient, IChatClient chatClient, IRazorPartialToStringRenderer renderer)
        {
            _dictionary = dictionary;
            _operatorClient = operatorClient;
            _chatClient = chatClient;
            _renderer = renderer;
        }

        public async Task SendInitialAllNewMessagesFromAllMen()
        {
            var sheets = await _operatorClient.GetSheetsAsync();
            if (sheets == null)
            {
                return;
            }

            ImmutableArray<NewMessage> allMessagesImmutable = new ImmutableArray<NewMessage>();
            Task[] changeNumberOfUsersOnlineTasks;

            lock (_dictionary)
            {
                var allMessages = _dictionary.Active.Values.Where(v => sheets.Any(sheet => sheet.Id == v.SheetInfo.SheetId) && !v.IsDeleted)
                    .OrderByDescending(m => m.Dialogue.LastMessage.DateCreated);
                allMessagesImmutable = ImmutableArray.CreateRange(allMessages);
                changeNumberOfUsersOnlineTasks = _dictionary.Online.Select(kvp => Clients.Group($"{kvp.Key}").SendAsync("ChangeNumberOfUsersOnline", kvp.Key, kvp.Value)).ToArray();
            }

            //var timers = await _chatClient.Timers(allMessagesImmutable);

            var body = await _renderer.RenderPartialToStringAsync("_AllNewMessagesFromAllMen", allMessagesImmutable); //, "timers", timers

            await Clients.Caller.SendAsync("ReceiveInitialAllNewMessagesFromAllMen", body);

            await Task.WhenAll(changeNumberOfUsersOnlineTasks);
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

