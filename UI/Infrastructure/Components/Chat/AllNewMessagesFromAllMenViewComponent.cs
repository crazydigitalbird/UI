using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UI.Infrastructure.API;
using UI.Models;

namespace UI.Infrastructure.Components
{
    public class AllNewMessagesFromAllMenViewComponent : ViewComponent
    {
        private readonly IChatClient _chatClient;

        private readonly ILogger<AllNewMessagesFromAllMenViewComponent> _logger;

        public AllNewMessagesFromAllMenViewComponent(IChatClient chatClient, ILogger<AllNewMessagesFromAllMenViewComponent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(List<Sheet> sheets)
        {
            List<(Dialogue Dialogue, SheetInfo SheetInfo)> dialoguesBindingToSheets = new List<(Dialogue Dialogue, SheetInfo SheetInfo)>();
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
#else
                //Все активные диалоги(диалоги с возможностью отправки сообщений) за указанное количеств дней (10 дней).
                //Производится выборка диалогов, в которых последнее сообщение отправил мужчина.
                var dialoguesActive = (await LoadDialogues(sheet, "active", 10)).Where(d => d.LastMessage.IdUserFrom != d.IdUser);

                //Все активные диалоги(диалоги с возможностью отправки сообщений) и находящиеся онлайн за указанное количеств дней (365 дней).
                //Отправитель сообщений не иммет значения т.к. отображаются все последние сообщения для диологов онлайн.
                var dialoguesActiveOnline = await LoadDialogues(sheet, "active,online", null);

                //Объединяем диалоги active и active,online
                var dialogues = dialoguesActive.Union(dialoguesActiveOnline).ToList();

                //Заполняем диалоги данными о мужчине (имя, аватар, возраст, статус онлайн/оффлайн).
                await _chatClient.GetManProfiles(sheet, dialogues);
#endif

                //Извлекаем данные о владельце анкеты, необходимые для представления
                var sheetInfo = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
                sheetInfo.SheetId = sheet.Id;

                dialoguesBindingToSheets.AddRange(dialogues.Select(d => (d, sheetInfo)));
            }
            return View(dialoguesBindingToSheets.OrderBy(ds => ds.Dialogue.LastMessage.DateCreated));
        }

        private async Task<IEnumerable<Dialogue>> LoadDialogues(Sheet sheet, string criteria, int? days)
        {
            var limit = 20;

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
                messanger = await _chatClient.GetMessangerAsync(sheet, criteria, messanger?.Cursor, limit);
                if (messanger == null || messanger.Dialogs == null || messanger.Dialogs.Count == 0)
                {
                    break;
                }
                dialogues = dialogues.Union(messanger.Dialogs).ToList();
            } while (messanger.Dialogs.Count == limit && IsLoadDialogues(messanger, dateThreshold));


            return dialogues.Where(d => d.DateUpdated > dateThreshold);
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
