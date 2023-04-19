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

        public async Task<IViewComponentResult> InvokeAsync(Sheet sheet)
        {
            //Все активные диалоги(диалоги с возможностью отправки сообщений) за указанное количеств дней (10 дней).
            //Производится выборка диалогов, в которых последнее сообщение отправил мужчина.
            var dialoguesActive = (await LoadDialogues(sheet, "active", 10)).Where(d => d.LastMessage.IdUserFrom != d.IdUser);

            //Все активные диалоги(диалоги с возможностью отправки сообщений) и находящиеся онлайн за указанное количеств дней (365 дней).
            //Отправитель сообщений не иммет значения т.к. отображаются все последние сообщения для диологов онлайн.
            var dialoguesActiveOnline = await LoadDialogues(sheet, "active,online", 365);

            //Объединяем диалоги active и active,online
            var dialogues = dialoguesActive.Union(dialoguesActiveOnline).ToList();

            //Заполняем диалоги данными о мужчине (имя, аватар, возраст, статус онлайн/оффлайн).
            await _chatClient.GetManProfiles(sheet, dialogues);

            //Извлекаем данные о владельце анкеты, необходимые для представления
            var sheetInfo = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);

            Messanger messanger = new Messanger
            {
                Sheet = sheetInfo,
                Dialogs = dialogues,
            };

            return View(messanger);
        }

        private async Task<IEnumerable<Dialogue>> LoadDialogues(Sheet sheet, string criteria, int days)
        {
            var limit = 20;

            //Вычисляем пороговое значение даты. Это дата до которой будут загружаться диалоги.
            //Все диалоги у которых значение поля DateUpdate страше dateThreshodl, будут отбрасывтаься. 
            var dateThreshold = DateTime.Now - TimeSpan.FromDays(days);
            var dialogues = new List<Dialogue>();
            Messanger messanger = null;

            do
            {
                messanger = await _chatClient.GetMessangerAsync(sheet, criteria, messanger?.Cursor, limit);
                if(messanger?.Dialogs?.Count == 0)
                {
                    break;
                }
                dialogues = dialogues.Union(messanger.Dialogs).ToList();
            } while (messanger.Dialogs.Count == limit && IsLoadDialogues(messanger, dateThreshold));


            return dialogues.Where(d => d.DateUpdated > dateThreshold);
        }

        //Возвращаем true если у всех диалогов значение поля DateUpdate младше dateThreshodl
        //Возвращаем fales если у хотбы у одного зи диалогов значение поля DateUpdate старше dateThreshodl
        private bool IsLoadDialogues(Messanger messanger, DateTime dateThreshold)
        {
            return messanger.Dialogs.All(d => d.DateUpdated > dateThreshold);
        }
    }
}
