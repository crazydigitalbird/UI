using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UI.Infrastructure.API;
using UI.Infrastructure.Repository;
using System.Diagnostics;
using UI.Models;

namespace UI.Infrastructure.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IDictionaryRepository<SheetDialogKey, NewMessage> _dictionary;
        private readonly IOperatorClient _operatorClient;
        private readonly IRazorPartialToStringRenderer _renderer;

        public ChatHub(IDictionaryRepository<SheetDialogKey, NewMessage> dictionary, IOperatorClient operatorClient, IRazorPartialToStringRenderer renderer)
        {
            _dictionary = dictionary;
            _operatorClient = operatorClient;
            _renderer = renderer;
        }

        public async Task SendInitialAllNewMessagesFromAllMen()
        {
            var sheets = await _operatorClient.GetSheetsAsync();
            if(sheets == null)
            {
                return;
            }

            var allMessages = _dictionary.Active.Where(kvp => sheets.Any(sheet => sheet.Id == kvp.Key.SheetId)).Select(kvp => {
                if(kvp.Value.Dialogue.LastMessage.Type == MessageType.System)
                {
                    kvp.Value.Dialogue.LastMessage.DateCreated = kvp.Value.Dialogue.DateUpdated;
                }
                return kvp.Value;
            }).OrderByDescending(m => m.Dialogue.LastMessage.DateCreated);

            Stopwatch s = new Stopwatch();
            s.Start();
            
            var body = await _renderer.RenderPartialToStringAsync("_AllNewMessagesFromAllMen", allMessages);

            s.Stop();

            var time = s.ElapsedMilliseconds;

            await Clients.Caller.SendAsync("ReceiveInitialAllNewMessagesFromAllMen", "Ok");
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}

