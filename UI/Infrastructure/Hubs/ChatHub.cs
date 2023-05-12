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

            var active = _dictionary.Active.Where(kvp => sheets.Any(sheet => sheet.Id == kvp.Key.SheetId));
            var online = _dictionary.Online.Where(kvp => sheets.Any(sheet => sheet.Id == kvp.Key.SheetId));

            var newMesssages = online.Union(active).Select(x => x.Value).ToList();

            Stopwatch s = new Stopwatch();
            s.Start();

            
            var body = await _renderer.RenderPartialToStringAsync("_AllNewMessagesFromAllMen", "");

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

