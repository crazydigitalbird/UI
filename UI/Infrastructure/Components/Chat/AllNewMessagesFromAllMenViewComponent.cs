using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class AllNewMessagesFromAllMenViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;

        private readonly IOperatorClient _operatorClient;

        private readonly ILogger<AllNewMessagesFromAllMenViewComponent> _logger;

        public AllNewMessagesFromAllMenViewComponent(IChatClient chatClient, IOperatorClient operatorClient, ILogger<AllNewMessagesFromAllMenViewComponent> logger)
        {
            _chatClient = chatClient;
            _operatorClient = operatorClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(List<Sheet> sheets)
        {
            var operatorId = 0;/*await _operatorClient.GetOperatorIdAsync();*/
            List<(Dialogue Dialogue, SheetInfo SheetInfo)> dialoguesBindingToSheets = new List<(Dialogue Dialogue, SheetInfo SheetInfo)>();

            var sheetIdDialogsActiveTasksIEnumerable = sheets.Select(sheet => LoadDialogues(sheet, "active", 5, operatorId, 15));
            var sheetIdDialogsOnlineTasksIEnumerable = sheets.Select(sheet => LoadDialogues(sheet, "active,online", null, operatorId, 5));

            var sheetIdDialogsActiveTasks = sheetIdDialogsActiveTasksIEnumerable.ToArray();
            var sheetIdDialogsOnlineTasks = sheetIdDialogsOnlineTasksIEnumerable.ToArray();

            var dialogsActiveKeyValuePair = await Task.WhenAll(sheetIdDialogsActiveTasks);
            var dialogsOnlineKeyValuePair = await Task.WhenAll(sheetIdDialogsOnlineTasks);

            foreach (var sheet in sheets)
            {
#if DEBUGOFFLINE
                var test = new List<Dialogue> { new Dialogue
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
                }};

                var dialogues = test.AsEnumerable();

                //Извлекаем данные о владельце анкеты, необходимые для представления
                var sheetInfo = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
                sheetInfo.SheetId = sheet.Id;
#else
                ////Все активные диалоги(диалоги с возможностью отправки сообщений) за указанное количеств дней (10 дней).
                ////Производится выборка диалогов, в которых последнее сообщение отправил мужчина.
                //var dialoguesActive = (await LoadDialogues(sheet, "active", 7, operatorId)).Where(d => d.HasNewMessage);

                ////Все активные диалоги(диалоги с возможностью отправки сообщений) и находящиеся онлайн за указанное количеств дней (365 дней).
                ////Отправитель сообщений не иммет значения т.к. отображаются все последние сообщения для диологов онлайн.
                //var dialoguesActiveOnline = await LoadDialogues(sheet, "active,online", null, operatorId);
                //dialoguesActiveOnline.ForEach(d => d.Status = Status.Online);

                ////Объединяем диалоги active и active,online
                //var dialogues = dialoguesActiveOnline.Union(dialoguesActive).ToList();

                ////Извлекаем данные о владельце анкеты, необходимые для представления
                //var sheetInfo = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
                //sheetInfo.SheetId = sheet.Id;

                ////Для LastMessage запрашиваем таймер
                //await SetTimerToLastMessages(dialogues);


                //New


                //Все активные диалоги(диалоги с возможностью отправки сообщений) за указанное количеств дней (7 дней).
                //Производится выборка диалогов, в которых последнее сообщение отправил мужчина.
                var dialoguesActive = dialogsActiveKeyValuePair.FirstOrDefault(kvp => kvp.Key == sheet.Id).Value?.Where(d => d.IdInterlocutor == d.LastMessage.IdUserFrom || d.LastMessage?.Type == MessageType.System);

                //Все активные диалоги(диалоги с возможностью отправки сообщений) и находящиеся онлайн за указанное количеств дней (365 дней).
                //Отправитель сообщений не иммет значения т.к. отображаются все последние сообщения для диологов онлайн.
                var dialoguesOnline = dialogsOnlineKeyValuePair.FirstOrDefault(kvp => kvp.Key == sheet.Id).Value;
                dialoguesOnline.ForEach(d => d.Status = Status.Online);

                //Объединяем диалоги active,online и active
                var dialogues = dialoguesOnline.UnionBy(dialoguesActive, d => d.IdInterlocutor).Where(d => !d.IsBlocked).ToList();


                //Извлекаем данные о владельце анкеты, необходимые для представления
                var sheetInfo = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
                sheetInfo.SheetId = sheet.Id;
#endif
                dialoguesBindingToSheets.AddRange(dialogues.Select(d => (d, sheetInfo)));
            }

#if !DEBUGOFFLINE
            //Для LastMessage запрашиваем таймер
            await SetTimerToLastMessages(dialoguesBindingToSheets);
#endif

            //Заполняем диалоги данными о мужчине (имя, аватар, возраст, статус онлайн/оффлайн).
            var allDialogues = dialoguesBindingToSheets.Select(ds => ds.Dialogue).DistinctBy(d => d.IdInterlocutor).ToList();
            if (allDialogues.Count > 50)
            {
                for (int i = 0; i < allDialogues.Count; i += 50)
                {
                    int count = 50;
                    if ((i + count) > allDialogues.Count)
                    {
                        count = allDialogues.Count - i;
                    }
                    var rangeAllDialogues = allDialogues.GetRange(i, count);
                    await _chatClient.GetManProfiles(sheets.First(), rangeAllDialogues);
                }
            }
            else
            {
                await _chatClient.GetManProfiles(sheets.First(), allDialogues);
            }
            foreach (var ds in dialoguesBindingToSheets)
            {
                var dialogue = allDialogues.FirstOrDefault(d => d.IdInterlocutor == ds.Dialogue.IdInterlocutor);
                if (dialogue != null)
                {
                    ds.Dialogue.Avatar = dialogue.Avatar;
                    ds.Dialogue.UserName = dialogue.UserName;
                    if (ds.Dialogue.Status == Status.Offline && dialogue.Status == Status.Online)
                    {
                        ds.Dialogue.Status = dialogue.Status;
                    }
                }
            }

            if(dialoguesBindingToSheets.Any(ds => !ds.Dialogue.IsActive))
            {
                var d = dialoguesBindingToSheets.Where(ds => !ds.Dialogue.IsActive).ToList();
            }

            return View(dialoguesBindingToSheets.OrderByDescending(ds => ds.Dialogue.LastMessage.DateCreated));
        }

        private async Task SetTimerToLastMessages(List<Dialogue> dialogues)
        {
            var idLastMessages = dialogues.Where(d => d.LastMessage.Type != MessageType.System).Select(d => d.LastMessage.Id);
            if (idLastMessages.Count() > 0)
            {
                Dictionary<long, MessageTimer> timers = await _chatClient.Timers(idLastMessages);

                foreach (var t in timers)
                {
                    var dialogue = dialogues.FirstOrDefault(d => d.LastMessage.Id == t.Key);
                    if (dialogue != null)
                    {
                        t.Value.MessageType = dialogue.LastMessage.Type;
                        dialogue.LastMessage.Timer = t.Value;
                    }
                }
            }

            //var dialogueesContainsSystemLastMessages = dialogues.Where(d => d.LastMessage.Type == MessageType.System);
            //foreach (var dialogue in dialogueesContainsSystemLastMessages)
            //{
            //    MessageTimer timer = await _chatClient.SystemTimer(dialogue.LastMessage.IdUserFrom, dialogue.LastMessage.IdUserTo);
            //    if (timer != null)
            //    {
            //        timer.MessageType = MessageType.System;
            //        dialogue.LastMessage.Timer = timer;

            //        dialogues.FirstOrDefault(d => d.LastMessage.Id == dialogue.LastMessage.Id).LastMessage.Timer = timer;
            //    }
            //}
        }

        private async Task SetTimerToLastMessages(List<(Dialogue Dialogue, SheetInfo SheetInfo)> dialoguesSheetInfo)
        {
            var idLastMessages = dialoguesSheetInfo.Select(ds=> ds.Dialogue).Where(d => d.LastMessage.Type != MessageType.System).Select(d => d.LastMessage.Id);
            if (idLastMessages.Count() > 0)
            {
                Dictionary<long, MessageTimer> timers = await _chatClient.Timers(idLastMessages);

                foreach (var timer in timers)
                {
                    var dialogue = dialoguesSheetInfo.FirstOrDefault(ds => ds.Dialogue.LastMessage.Id == timer.Key).Dialogue;
                    if (dialogue != null)
                    {
                        timer.Value.MessageType = dialogue.LastMessage.Type;
                        dialogue.LastMessage.Timer = timer.Value;
                    }
                }
            }

            //var dialogueesContainsSystemLastMessages = dialoguesSheetInfo.Select(ds => ds.Dialogue).Where(d => d.LastMessage.Type == MessageType.System);
            //foreach (var dialogue in dialogueesContainsSystemLastMessages)
            //{
            //    MessageTimer timer = await _chatClient.SystemTimer(dialogue.LastMessage.IdUserFrom, dialogue.LastMessage.IdUserTo);
            //    if (timer != null)
            //    {
            //        timer.MessageType = MessageType.System;
            //        dialogue.LastMessage.Timer = timer;

            //        dialoguesSheetInfo.FirstOrDefault(d => d.Dialogue.LastMessage.Id == dialogue.LastMessage.Id).Dialogue.LastMessage.Timer = timer;
            //    }
            //}
        }

        private async Task<KeyValuePair<int, List<Dialogue>>> LoadDialogues(Sheet sheet, string criteria, int? days, int operatorId, int limit)
        {
            //Вычисляем пороговое значение даты. Это дата до которой будут загружаться диалоги.
            //Все диалоги у которых значение поля DateUpdate страше dateThreshodl, будут отбрасывтаься.
            //Если days равно null, то критерий прогового значения даты не учитывается и загружаются все имеющиеся диалоги.
            DateTime? dateThreshold = null;
            if (days.HasValue)
            {
                dateThreshold = DateTime.Now - TimeSpan.FromDays(days.Value);
            }
            var dialogues = new List<Dialogue>();
            Messenger messanger = null;

            do
            {
                messanger = await _chatClient.GetMessangerAsync(sheet, criteria, messanger?.Cursor, limit, operatorId);
                if (messanger == null || messanger.Dialogs == null || messanger.Dialogs.Count == 0)
                {
                    break;
                }
                dialogues = dialogues.Union(messanger.Dialogs).ToList();
            } while (messanger.Dialogs.Count == limit && IsLoadDialogues(messanger, dateThreshold));

            return new KeyValuePair<int, List<Dialogue>>(sheet.Id, dialogues);
        }

        //Возвращаем true если у всех диалогов значение поля DateUpdate младше dateThreshodl
        //Возвращаем fales если у хотбы у одного зи диалогов значение поля DateUpdate старше dateThreshodl
        private bool IsLoadDialogues(Messenger messanger, DateTime? dateThreshold)
        {
            if (dateThreshold.HasValue)
            {
                return messanger.Dialogs.All(d => d.DateUpdated > dateThreshold.Value);
            }
            return true;
        }
    }
}
