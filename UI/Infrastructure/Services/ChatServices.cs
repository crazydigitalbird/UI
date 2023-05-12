using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UI.Infrastructure.API;
using UI.Infrastructure.Components;
using UI.Infrastructure.Hubs;
using UI.Infrastructure.Repository;
using UI.Models;
using System.Diagnostics;
using System.Net.Http;
using Core.Models.Users;
using System.Net;

namespace UI.Infrastructure.Services
{
    public class ChatServices : IHostedService, IDisposable
    {
        private Timer _timer;
        private static volatile bool _started;

        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChatServices> _logger;

        private readonly IChatClient _chatClient;
        private readonly IAuthenticationClient _authenticationClient;
        private readonly IAdminClient _adminClient;
        private readonly IDictionaryRepository<SheetDialogKey, NewMessage> _dictionary;

        Stopwatch stopwatch1 = new Stopwatch();
        Stopwatch stopwatch2 = new Stopwatch();
        int count = 0;

        public ChatServices(IServiceProvider serviceProvider,
            IHttpClientFactory httpClientFactory,
            IHubContext<ChatHub> hubContext,
            ILogger<ChatServices> logger,
            IChatClient chatClient,
            IAuthenticationClient authenticationClient,
            IAdminClient adminClient,
            IDictionaryRepository<SheetDialogKey, NewMessage> dictionary)
        {
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
            _logger = logger;
            _chatClient = chatClient;
            _authenticationClient = authenticationClient;
            _adminClient = adminClient;
            _dictionary = dictionary;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(UpdateOnlineStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public async void UpdateOnlineStatus(object state)
        {
            if (_started)
            {
                return;
            }
            _started = true;
            try
            {
                await GetNewMessage();
                //await _hubContext.Clients.All.SendAsync("Recive", "Hellow");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "{ExceptionMessage}", ex.Message);
            }
            finally
            {
                _started = false;
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task GetNewMessage()
        {
            var sessionGuid = await LogInAsync("admin", "admin");
            if (string.IsNullOrWhiteSpace(sessionGuid))
            {
                return;
            }

            var sheets = await GetSheetsFast(sessionGuid);

            sheets.ForEach(sheet => sheet.Site = new SheetSite { Configuration = "https://talkytimes.com/" });



            count++;
            stopwatch2.Start();
            //---------------------------------2------------------------------

            var onlineSheetsDialogsTempNew = new ConcurrentDictionary<SheetDialogKey, NewMessage>();
            var sheetIdDialogsOnlineTasksIEnumerableNew = sheets.Select(sheet => LoadOnlineDialogues(sheet, onlineSheetsDialogsTempNew, 5));
            var sheetIdDialogsOnlineTasksNew = sheetIdDialogsOnlineTasksIEnumerableNew.ToArray();
            await Task.WhenAll(sheetIdDialogsOnlineTasksNew);
            //---------------------------------2------------------------------
            stopwatch2.Stop();


            stopwatch1.Start();
            //---------------------------------1------------------------------

            //var sheetIdDialogsActiveTasksIEnumerable = sheets.Select(sheet => LoadDialogues(sheet, "active", 5, 0, 15));
            var sheetIdDialogsOnlineTasksIEnumerable = sheets.Select(sheet => LoadDialogues(sheet, "active,online", null, 0, 5));

            //var sheetIdDialogsActiveTasks = sheetIdDialogsActiveTasksIEnumerable.ToArray();
            var sheetIdDialogsOnlineTasks = sheetIdDialogsOnlineTasksIEnumerable.ToArray();

            //var dialogsActiveKeyValuePair = await Task.WhenAll(sheetIdDialogsActiveTasks);
            var dialogsOnlineKeyValuePair = await Task.WhenAll(sheetIdDialogsOnlineTasks);


            //var newSheetsDialogs = new List<SheetDialogKey>();
            //var updateSheetsDialogs = new List<SheetDialogKey>();

            var onlineSheetsDialogsTemp = new ConcurrentDictionary<SheetDialogKey, NewMessage>();

            foreach (var sheet in sheets)
            {
                //Все активные диалоги(диалоги с возможностью отправки сообщений) за указанное количеств дней (7 дней).
                //Производится выборка диалогов, в которых последнее сообщение отправил мужчина.
                //var dialoguesActive = dialogsActiveKeyValuePair.FirstOrDefault(kvp => kvp.Key == sheet.Id).Value?.Where(d => d.HasNewMessage || d.LastMessage?.Type == MessageType.System);
                //foreach (var dialog in dialoguesActive)
                //{
                //    var key = new SheetDialogKey(sheet.Id, dialog.IdInterlocutor);
                //    _dictionary.Active.AddOrUpdate(key, (key) =>
                //    {
                //        newSheetsDialogs.Add(key);
                //        return new NewMessage { Dialogue = dialog };
                //    }, (key, oldValue) =>
                //    {
                //        if (oldValue.Dialogue.LastMessage.Id != dialog.LastMessage.Id)
                //        {
                //            updateSheetsDialogs.Add(key);
                //            return new NewMessage { Dialogue = dialog };
                //        }
                //        else
                //        {
                //            return oldValue;
                //        }
                //    });
                //}

                //Все активные диалоги(диалоги с возможностью отправки сообщений) и находящиеся онлайн за указанное количеств дней (365 дней).
                //Отправитель сообщений не иммет значения т.к. отображаются все последние сообщения для диологов онлайн.
                var dialoguesOnline = dialogsOnlineKeyValuePair.FirstOrDefault(kvp => kvp.Key == sheet.Id).Value;
                foreach (var dialog in dialoguesOnline)
                {
                    var key = new SheetDialogKey(sheet.Id, dialog.IdInterlocutor);
                    dialog.Status = Status.Online;
                    onlineSheetsDialogsTemp.TryAdd(key, new NewMessage { Dialogue = dialog });
                }
            }

            //---------------------------------1------------------------------
            stopwatch1.Stop();



            var res1 = TimeSpan.FromMilliseconds(stopwatch1.ElapsedMilliseconds).TotalSeconds / count;
            var res2 = TimeSpan.FromMilliseconds(stopwatch2.ElapsedMilliseconds).TotalSeconds / count;

            // Получаем все ключи первой последовательности, которых нет во второй последовательности. Эти диалоги необходимо удалить т.к. пользователи уже не в онлайне
            var removeOnlineSheetDialogKeys = _dictionary.Online.Keys.Except(onlineSheetsDialogsTemp.Keys);

            // Получаем все ключи первой последовательности, которых нет во второй последовательности. Эти диалоги необходимо добавить т.к. пользователи появились в онлайне
            var newOnlineSheetDialogKeys = onlineSheetsDialogsTemp.Keys.Except(_dictionary.Online.Keys);

            //Получаем все ключи общие для обоих коллекций, но имеющие разные Id LastMessage. Данные диалоги необходимо обновить.
            var updateOnlineSheetDialogKeys = _dictionary.Online.Where(kvp => onlineSheetsDialogsTemp.ContainsKey(kvp.Key) && onlineSheetsDialogsTemp[kvp.Key].Dialogue.LastMessage.Id != kvp.Value.Dialogue.LastMessage.Id)
                .Select(kvp => kvp.Key)
                .ToList();

            _dictionary.Online = onlineSheetsDialogsTemp;

            //var allNewDialogs = newOnlineSheetDialogKeys.UnionBy(newSheetsDialogs, key => key.IdInterlocutor).Select(key => key.IdInterlocutor).ToList();

            //await FillingInNewDialoguesProfiles(sheets.First(), allNewDialogs);
        }

        private async Task FillingInNewDialoguesProfiles(Sheet sheet, List<int> allNewDialogs)
        {
            List<SheetInfo> sheetInfos = null;

            if (allNewDialogs.Count > 50)
            {
                sheetInfos = new List<SheetInfo>(allNewDialogs.Count);
                for (int i = 0; i < allNewDialogs.Count; i += 50)
                {
                    int count = 50;
                    if ((i + count) > allNewDialogs.Count)
                    {
                        count = allNewDialogs.Count - i;
                    }
                    var rangeAllDialogues = allNewDialogs.GetRange(i, count);
                    sheetInfos.AddRange(await _chatClient.GetManProfiles(sheet, rangeAllDialogues));
                }
            }
            else
            {
                sheetInfos = await _chatClient.GetManProfiles(sheet, allNewDialogs);
            }
            foreach (var sheetInfo in sheetInfos)
            {
                var findActive = _dictionary.Active.Where(kvp => kvp.Key.IdInterlocutor == sheetInfo.Id);
                foreach (var active in findActive)
                {
                    active.Value.Dialogue.Avatar = sheetInfo.Personal.AvatarSmall;
                    active.Value.Dialogue.UserName = sheetInfo.Personal.Name;
                    active.Value.Dialogue.Status = sheetInfo.IsOnline ? Status.Online : Status.Offline;
                }
                var findOnline = _dictionary.Online.Where(kvp => kvp.Key.IdInterlocutor == sheetInfo.Id);
                foreach (var online in findOnline)
                {
                    online.Value.Dialogue.Avatar = sheetInfo.Personal.AvatarSmall;
                    online.Value.Dialogue.UserName = sheetInfo.Personal.Name;
                    online.Value.Dialogue.Status = Status.Online;
                }
            }
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
            } while (messanger.Dialogs.Count == limit); //&& IsLoadDialogues(messanger, dateThreshold)

            return new KeyValuePair<int, List<Dialogue>>(sheet.Id, dialogues.Where(d => !d.IsBlocked).ToList());
        }

        private async Task LoadOnlineDialogues(Sheet sheet, ConcurrentDictionary<SheetDialogKey, NewMessage> dictionary, int limit)
        {
            //Вычисляем пороговое значение даты. Это дата до которой будут загружаться диалоги.
            //Все диалоги у которых значение поля DateUpdate страше dateThreshodl, будут отбрасывтаься.
            //Если days равно null, то критерий прогового значения даты не учитывается и загружаются все имеющиеся диалоги.

            //Messenger messanger = null;
            //do
            //{
            //    messanger = await _chatClient.GetMessangerAsync(sheet, "active,online", messanger?.Cursor, limit);
            //    if (messanger == null || messanger.Dialogs == null || messanger.Dialogs.Count == 0)
            //    {
            //        break;
            //    }
            //    messanger.Dialogs
            //        .Where(d => !d.IsBlocked)
            //        .ToList()
            //        .ForEach(dialogue =>
            //        {
            //            dialogue.Status = Status.Online;
            //            dictionary.TryAdd(new SheetDialogKey(sheet.Id, dialogue.IdInterlocutor), new NewMessage { Dialogue = dialogue });
            //        });
            //} while (true);

            var dialogues = new List<Dialogue>();
            Messenger messanger = null;
            do
            {
                messanger = await _chatClient.GetMessangerAsync(sheet, "active,online", messanger?.Cursor, limit);
                if (messanger == null || messanger.Dialogs == null || messanger.Dialogs.Count == 0)
                {
                    break;
                }
                dialogues = dialogues.Union(messanger.Dialogs).ToList();
            } while (messanger.Dialogs.Count == limit);
            dialogues.ForEach(dialogue =>
            {
                if (!dialogue.IsBlocked)
                {
                    dialogue.Status = Status.Online;
                    dictionary.TryAdd(new SheetDialogKey(sheet.Id, dialogue.IdInterlocutor), new NewMessage { Dialogue = dialogue });
                }
            });
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

        private async Task<string> LogInAsync(string login, string passowrd)
        {
            var client = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await client.PutAsync($"Users/AddSession?userLogin={login}&userPassword={passowrd}&sessionLength=8766", null);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var userSession = await response.Content.ReadFromJsonAsync<UserSession>();
                    return userSession.Guid;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An error occurred while logging in: {ErrorMessage}. Login: {login}.", ex.Message, login);
            }
            return null;
        }

        private async Task<List<Sheet>> GetSheetsFast(string sessionGuid)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.GetAsync($"Sheets/GetSheetsFast?sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var sheets = (await response.Content.ReadFromJsonAsync<List<Sheet>>()).Where(a => a.IsActive).ToList();
                    return sheets;
                }
                else
                {
                    _logger.LogWarning("Error getting all the sheets. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the sheets.");
            }
            return null;
        }
    }
}
