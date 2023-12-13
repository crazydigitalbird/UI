using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using UI.Infrastructure.Repository;

namespace UI.Infrastructure.Hubs
{
    public class CallingSideChatHub : IChatHub
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IRazorPartialToStringRenderer _renderer;
        private readonly IDictionaryRepository<SheetDialogKey, NewMessage> _dictionary;

        public CallingSideChatHub(IHubContext<ChatHub> hubContext, IRazorPartialToStringRenderer renderer, IDictionaryRepository<SheetDialogKey, NewMessage> dictionary)
        {
            _hubContext = hubContext;
            _renderer = renderer;
            _dictionary = dictionary;
        }

        public async Task DeleteDialogs(IEnumerable<KeyValuePair<SheetDialogKey, long>> sheetsDialogues)
        {
            var deleteDialogTasks = sheetsDialogues.Select(sheetDialogue => _hubContext.Clients.Group($"{sheetDialogue.Key.SheetId}").SendAsync("DeleteDialog", sheetDialogue.Key.SheetId, sheetDialogue.Key.IdInterlocutor, sheetDialogue.Value)).ToArray();
            await Task.WhenAll(deleteDialogTasks);
        }

        public async Task AddDialogs(ImmutableDictionary<SheetDialogKey, NewMessage> sheetsDialogues)
        {
            var addDialogTasks = sheetsDialogues.Select(AddDialog).ToArray();
            await Task.WhenAll(addDialogTasks);
        }

        private async Task AddDialog(KeyValuePair<SheetDialogKey, NewMessage> sheetDialogue)
        {
            var element = await _renderer.RenderPartialToStringAsync("_AllNewMessagesFromAllMen", new List<NewMessage> { sheetDialogue.Value });
            await _hubContext.Clients.Group($"{sheetDialogue.Key.SheetId}").SendAsync("AddDialog", element);

            await _hubContext.Clients
                .Group($"{sheetDialogue.Key.SheetId}-{sheetDialogue.Key.IdInterlocutor}")
                .SendAsync("NewMessage", sheetDialogue.Key.SheetId, sheetDialogue.Key.IdInterlocutor, sheetDialogue.Value.Dialogue.LastMessage.Id);
        }

        public async Task UpdateDialogs(IEnumerable<KeyValuePair<SheetDialogKey, long>> oldSheetsDialogues, ImmutableDictionary<SheetDialogKey, NewMessage> newSheetsDialogues)
        {
            await DeleteDialogs(oldSheetsDialogues);
            await AddDialogs(newSheetsDialogues);
        }

        public async Task ChangeNumberOfUsersOnline(ConcurrentDictionary<int, int> online)
        {
            var tasks = online.Select(kvp => _hubContext.Clients.Group($"{kvp.Key}").SendAsync("ChangeNumberOfUsersOnline", kvp.Key, kvp.Value)).ToArray();
            await Task.WhenAll(tasks);
        }

        public async Task ReplyToNewMessage(int sheetId, int idInterlocutor, long idLastMessage, long idNewMessage)
        {
            var key = new SheetDialogKey(sheetId, idInterlocutor);
            lock (_dictionary)
            {
                if (_dictionary.Active.ContainsKey(key) && _dictionary.Active[key].Dialogue?.LastMessage?.Id.Value <= idLastMessage) //????? == and =<
                {
                    _dictionary.Active[key].IsDeleted = true;
                    _dictionary.Active[key].Dialogue.LastMessage.Id = idLastMessage;
                }
            }
            var deleteDialogTask = _hubContext.Clients.Group($"{sheetId}").SendAsync("DeleteDialog", sheetId, idInterlocutor, idLastMessage);
            var historyTask = _hubContext.Clients.Group($"{sheetId}").SendAsync("History");
            var newMessageTask = _hubContext.Clients.Group($"{sheetId}-{idInterlocutor}").SendAsync("NewMessage", sheetId, idInterlocutor, idNewMessage);
            await Task.WhenAll(deleteDialogTask, historyTask, newMessageTask);
        }

        public async Task ChangeSheetsStatusOnline(ConcurrentDictionary<int, bool> sheetsIsOnline)
        {
            var tasks = sheetsIsOnline.Select(kvp => _hubContext.Clients.Group($"{kvp.Key}").SendAsync("ChangeSheetIsOnline", kvp.Key, kvp.Value)).ToArray();
            await Task.WhenAll(tasks);
        }
    }
}