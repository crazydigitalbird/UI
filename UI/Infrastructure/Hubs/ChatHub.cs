using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace UI.Infrastructure.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public ChatHub()
        {
        }
    }
}

