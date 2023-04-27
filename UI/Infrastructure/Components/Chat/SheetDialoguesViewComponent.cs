﻿using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class SheetDialoguesViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;

        private readonly ILogger<SheetDialoguesViewComponent> _logger;

        public SheetDialoguesViewComponent(IChatClient chatClient, ILogger<SheetDialoguesViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet, string criteria = "active", bool online = false, string cursor = "", string filter = "")
        {
            int limit = 20;
#if DEBUGOFFLINE
            Messenger messenger = new Messenger
            {
                Dialogs = new List<Dialogue> { new Dialogue
                {
                    Status = Status.Offline,
                    Avatar = "/image/avatar1.webp",
                    DateUpdated = DateTime.Now - TimeSpan.FromDays(1),
                    UserName = "Anton",
                    IdInterlocutor = 73187865 ,
                    IsBookmarked = true,
                    IdUser = 11111,
                    LastMessage = new Message
                    {
                        DateCreated = DateTime.Now - TimeSpan.FromMinutes(5),
                        Type = MessageType.Message,
                        IdUserTo = 11111,
                        Content = new Content { Message = "Hi" }
                    }
                },
                new Dialogue
                {
                    Status = Status.Online,
                    Avatar = "/image/avatar1.webp",
                    DateUpdated = DateTime.Now - TimeSpan.FromDays(10),
                    UserName = "Anton Online",
                    IdInterlocutor = 22116547,
                    IsPinned = true,
                    IdUser = 22222,
                    LastMessage = new Message
                    {
                        DateCreated = DateTime.Now - TimeSpan.FromMinutes(3),
                        Type = MessageType.LikePhoto,
                        IdUserTo = 11111
                    }
                }}
            };
#else
            if (online)
            {
                criteria = $"{criteria},online";
            }
            var messenger = await _chatClient.GetMessangerAsync(sheet, criteria, cursor, limit) ?? new Messenger();
            if(filter == "premium")
            {
                messenger.Dialogs = messenger.Dialogs.Where(d => d.IsPinned).ToList();
            }
            if(messenger.Dialogs?.Count < limit)
            {
                messenger.Cursor = "";
            }
#endif
            messenger.SheetId = sheet.Id;
            messenger.Sheet = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
            messenger.Dialogs = messenger.Dialogs.Where(d => !d.IsBlocked).ToList();
            await _chatClient.GetManProfiles(sheet, messenger.Dialogs);
            return View(messenger);
        }
    }
}
