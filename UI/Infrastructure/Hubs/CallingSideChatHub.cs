using Microsoft.AspNetCore.SignalR;
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
            foreach (var sheetDialogue in sheetsDialogues)
            {
                await _hubContext.Clients.Group($"{sheetDialogue.Key.SheetId}").SendAsync("DeleteDialog", sheetDialogue.Key.SheetId, sheetDialogue.Key.IdInterlocutor, sheetDialogue.Value);
            }
        }

        public async Task AddDialogs(IEnumerable<KeyValuePair<SheetDialogKey, NewMessage>> sheetsDialogues)
        {
            foreach (var sheetDialogue in sheetsDialogues.OrderBy(kvp => kvp.Value.Dialogue.LastMessage.DateCreated))
            {
                var element = await _renderer.RenderPartialToStringAsync("_AllNewMessagesFromAllMen", new List<NewMessage> { sheetDialogue.Value });
                await _hubContext.Clients.Group($"{sheetDialogue.Key.SheetId}").SendAsync("AddDialog", element);

                await _hubContext.Clients
                    .Group($"{sheetDialogue.Key.SheetId}-{sheetDialogue.Key.IdInterlocutor}")
                    .SendAsync("NewMessage", sheetDialogue.Key.SheetId, sheetDialogue.Key.IdInterlocutor, sheetDialogue.Value.Dialogue.LastMessage.Id);
            }
        }

        public async Task UpdateDialogs(IEnumerable<KeyValuePair<SheetDialogKey, long>> oldSheetsDialogues, IEnumerable<KeyValuePair<SheetDialogKey, NewMessage>> newSheetsDialogues)
        {
            await DeleteDialogs(oldSheetsDialogues);
            await AddDialogs(newSheetsDialogues);
        }

        public async Task ReplyToNewMessage(int sheetId, int idInterlocutor, long idLastMessage, long idNewMessage)
        {
            var key = new SheetDialogKey(sheetId, idInterlocutor);
            if (_dictionary.Active.ContainsKey(key) && _dictionary.Active[key].Dialogue?.LastMessage?.Id.Value == idLastMessage)
            {
                _dictionary.Active.TryRemove(key, out var newMessage);
            }
            await _hubContext.Clients.Group($"{sheetId}").SendAsync("DeleteDialog", sheetId, idInterlocutor, idLastMessage);

            await _hubContext.Clients.Group($"{sheetId}-{idInterlocutor}").SendAsync("NewMessage", sheetId, idInterlocutor, idNewMessage);
        }
    }
}