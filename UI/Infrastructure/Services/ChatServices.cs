using Core.Models.Sheets;
using Core.Models.Users;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using UI.Infrastructure.API;
using UI.Infrastructure.Hubs;
using UI.Infrastructure.Repository;
using UI.Models;

namespace UI.Infrastructure.Services
{
    public class ChatServices : IHostedService, IDisposable
    {
        private Timer _timer;
        private static volatile bool _started;

        private readonly IServiceProvider _serviceProvider;
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
            ILogger<ChatServices> logger,
            IChatClient chatClient,
            IAuthenticationClient authenticationClient,
            IAdminClient adminClient,
            IDictionaryRepository<SheetDialogKey, NewMessage> dictionary)
        {
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
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
            stopwatch1.Start();
            using var scope = _serviceProvider.CreateScope();
            var _chatHub = scope.ServiceProvider.GetRequiredService<IChatHub>();

            var sessionGuid = await LogInAsync("admin", "admin");
            if (string.IsNullOrWhiteSpace(sessionGuid))
            {
                return;
            }

            List<SheetChat> sheets = await GetSheetsFast(sessionGuid);

            sheets.ForEach(sheet => sheet.Site = new SheetSite { Configuration = "https://talkytimes.com/" });

            DateTime dateThreshold = DateTime.Now - TimeSpan.FromDays(5);

            //var onlineSheetsDialogsTemp = new ConcurrentDictionary<SheetDialogKey, NewMessage>();
            var activeSheetsDialogsTemp = new ConcurrentDictionary<SheetDialogKey, NewMessage>();

            var sheetIdDialogsOnlineTasksIEnumerable = sheets.Select(sheet => LoadOnlineDialogues(sheet, activeSheetsDialogsTemp, 5));
            var sheetIdDialogsActiveTasksIEnumerable = sheets.Select(sheet => LoadActiveDialogues(sheet, activeSheetsDialogsTemp, dateThreshold, 15));

            var sheetIdDialogsOnlineTasks = sheetIdDialogsOnlineTasksIEnumerable.ToArray();
            var sheetIdDialogsActiveTasks = sheetIdDialogsActiveTasksIEnumerable.ToArray();

            await Task.WhenAll(sheetIdDialogsOnlineTasks.Concat(sheetIdDialogsActiveTasks));

            stopwatch2.Start();

            #region Processing online
            ////Исключаем
            //onlineSheetsDialogsTemp = new ConcurrentDictionary<SheetDialogKey, NewMessage>(onlineSheetsDialogsTemp.Except(activeSheetsDialogsTemp));

            //// Получаем все ключи первой последовательности, которых нет во второй последовательности. Эти диалоги необходимо удалить т.к. пользователи уже не в онлайне
            //var removeOnlineSheetDialogKeys = _dictionary.Online.Keys.Except(onlineSheetsDialogsTemp.Keys);

            //// Получаем все ключи первой последовательности, которых нет во второй последовательности. Эти диалоги необходимо добавить т.к. пользователи появились в онлайне
            //var newOnlineSheetDialogKeys = onlineSheetsDialogsTemp.Keys.Except(_dictionary.Online.Keys);

            ////Получаем все ключи общие для обоих коллекций, но имеющие разные Id LastMessage. Данные диалоги необходимо обновить.
            //var updateOnlineSheetDialogKeys = _dictionary.Online.Where(kvp => onlineSheetsDialogsTemp.ContainsKey(kvp.Key) && onlineSheetsDialogsTemp[kvp.Key].Dialogue.LastMessage.Id != kvp.Value.Dialogue.LastMessage.Id)
            //    .Select(kvp => kvp.Key)
            //    .ToList();

            //_dictionary.Online = onlineSheetsDialogsTemp;
            #endregion

            #region Processing active

            Dictionary<SheetDialogKey, long> removeActiveSheetDialogs = new Dictionary<SheetDialogKey, long>();
            Dictionary<SheetDialogKey, NewMessage> newActiveSheetDialogs = new Dictionary<SheetDialogKey, NewMessage>();
            Dictionary<SheetDialogKey, long> oldUpdateActiveSheetDialogs = new Dictionary<SheetDialogKey, long>();
            ImmutableDictionary<SheetDialogKey, NewMessage> newUpdateActiveSheetDialogsImmutable = null;

            lock (_dictionary)
            {
                // Получаем все ключи первой последовательности, которых нет во второй последовательности.
                // Из первой последовательности отбираются только диалоги из категории Онлайн, т.е. отправителем сообщения выступает девушка. 
                removeActiveSheetDialogs = _dictionary.Active
                    //.Where(kvp => kvp.Value.Dialogue.LastMessage.IdUserFrom != kvp.Key.IdInterlocutor)
                    .ExceptBy(activeSheetsDialogsTemp.Keys, kvp => kvp.Key)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Dialogue.LastMessage.Id.Value);

                foreach (var sheetDialog in removeActiveSheetDialogs)
                {
                    _dictionary.Active.Remove(sheetDialog.Key);
                }

                // Получаем все ключи первой последовательности, которых нет во второй последовательности. Эти диалоги необходимо добавить т.к. появились новые действия со стороны мужчины
                newActiveSheetDialogs = activeSheetsDialogsTemp.ExceptBy(_dictionary.Active.Keys, kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                foreach (var sheetDialog in newActiveSheetDialogs)
                {
                    var sheet = sheets.FirstOrDefault(s => s.Id == sheetDialog.Key.SheetId);
                    if (sheet != null)
                    {
                        sheetDialog.Value.SheetInfo = sheet.SheetInfo;
                    }
                }
            }

            await FillingInNewDialoguesProfiles(sheets.First(), newActiveSheetDialogs);

            ImmutableDictionary<SheetDialogKey, NewMessage> newActiveSheetDialogsImmutable = ImmutableDictionary.CreateRange(newActiveSheetDialogs);

            lock (_dictionary)
            {
                foreach (var sheetDialog in newActiveSheetDialogs)
                {
                    _dictionary.Active.TryAdd(sheetDialog.Key, sheetDialog.Value);
                }

                //Получаем все ключи общие для обоих коллекций, но имеющие разные Id LastMessage. Данные диалоги необходимо обновить.
                oldUpdateActiveSheetDialogs = _dictionary.Active
                    .Where(kvp => activeSheetsDialogsTemp.ContainsKey(kvp.Key) && activeSheetsDialogsTemp[kvp.Key].Dialogue.LastMessage.Id > kvp.Value.Dialogue.LastMessage.Id)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Dialogue.LastMessage.Id.Value);

                var newUpdateActiveSheetDialogs = activeSheetsDialogsTemp
                    .Where(kvp => _dictionary.Active.ContainsKey(kvp.Key) && _dictionary.Active[kvp.Key].Dialogue.LastMessage.Id < kvp.Value.Dialogue.LastMessage.Id)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                //foreach (var a in _dictionary.Active)
                //{
                //    if (activeSheetsDialogsTemp.ContainsKey(a.Key))
                //    {
                //        if (activeSheetsDialogsTemp[a.Key].Dialogue.LastMessage.Id != a.Value.Dialogue.LastMessage.Id)
                //        {

                //        }
                //    }
                //}


                foreach (var sheetDialog in newUpdateActiveSheetDialogs)
                {
                    sheetDialog.Value.SheetInfo = _dictionary.Active[sheetDialog.Key].SheetInfo;
                    sheetDialog.Value.Dialogue.Avatar = _dictionary.Active[sheetDialog.Key].Dialogue.Avatar;
                    sheetDialog.Value.Dialogue.Status = _dictionary.Active[sheetDialog.Key].Dialogue.Status;
                    sheetDialog.Value.Dialogue.UserName = _dictionary.Active[sheetDialog.Key].Dialogue.UserName;

                    _dictionary.Active[sheetDialog.Key].Dialogue.DateUpdated = sheetDialog.Value.Dialogue.DateUpdated;
                    _dictionary.Active[sheetDialog.Key].Dialogue.LastMessage = sheetDialog.Value.Dialogue.LastMessage;
                    _dictionary.Active[sheetDialog.Key].IsDeleted = false;
                }

                var softDeletedActiveSheetDialogs = _dictionary.Active.Where(sd => sd.Value.IsDeleted).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Dialogue.LastMessage.Id.Value);

                if (softDeletedActiveSheetDialogs.Any())
                {
                    foreach (var sheetDialog in softDeletedActiveSheetDialogs)
                    {
                        _dictionary.Active.Remove(sheetDialog.Key);
                        removeActiveSheetDialogs.Add(sheetDialog.Key, sheetDialog.Value);
                    }
                }

                newUpdateActiveSheetDialogsImmutable = ImmutableDictionary.CreateRange(newUpdateActiveSheetDialogs);
            }

            #endregion

            var deleteTask = _chatHub.DeleteDialogs(removeActiveSheetDialogs);
            var addTask = _chatHub.AddDialogs(newActiveSheetDialogsImmutable);
            var updateTask = _chatHub.UpdateDialogs(oldUpdateActiveSheetDialogs, newUpdateActiveSheetDialogsImmutable);
            var changeNumberOfUsersOnlineTask = _chatHub.ChangeNumberOfUsersOnline(_dictionary.Online);

            await Task.WhenAll(deleteTask, addTask, updateTask, changeNumberOfUsersOnlineTask);

            //Task.Run(() => deleteTask);
            //Task.Run(() => addTask);
            //Task.Run(() => updateTask);

            stopwatch2.Stop();
            stopwatch1.Stop();
            count++;
            var rez = TimeSpan.FromMilliseconds(stopwatch1.ElapsedMilliseconds).TotalSeconds / count;
            var rezCollection = TimeSpan.FromMilliseconds(stopwatch2.ElapsedMilliseconds).TotalSeconds / count;
        }

        private async Task FillingInNewDialoguesProfiles(Sheet sheet, IEnumerable<KeyValuePair<SheetDialogKey, NewMessage>> dictionary)
        {
            if (dictionary.Count() == 0)
            {
                return;
            }

            List<SheetInfo> sheetInfos = null;
            var allNewDialogs = dictionary.Select(key => key.Value.Dialogue.IdInterlocutor).Distinct().ToList();

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
            if (sheetInfos != null)
            {
                foreach (var sheetInfo in sheetInfos)
                {
                    var findActive = dictionary.Where(kvp => kvp.Key.IdInterlocutor == sheetInfo.Id);
                    foreach (var active in findActive)
                    {
                        active.Value.Dialogue.Avatar = sheetInfo.Personal.AvatarSmall;
                        active.Value.Dialogue.UserName = sheetInfo.Personal.Name;
                        if (active.Value.Dialogue.Status != Status.Online)
                        {
                            active.Value.Dialogue.Status = sheetInfo.IsOnline ? Status.Online : Status.Offline;
                        }
                    }
                    //var findOnline = _dictionary.Online.Where(kvp => kvp.Key.IdInterlocutor == sheetInfo.Id);
                    //foreach (var online in findOnline)
                    //{
                    //    online.Value.Dialogue.Avatar = sheetInfo.Personal.AvatarSmall;
                    //    online.Value.Dialogue.UserName = sheetInfo.Personal.Name;
                    //    online.Value.Dialogue.Status = Status.Online;
                    //}
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

        private async Task LoadActiveDialogues(Sheet sheet, ConcurrentDictionary<SheetDialogKey, NewMessage> dictionary, DateTime dateThreshold, int limit)
        {
            //dateThreshold это дата до которой будут загружаться диалоги.
            //Все диалоги у которых значение поля DateUpdate страше dateThreshodl, будут отбрасывтаься.
            var dialogues = new List<Dialogue>();
            Messenger messanger = null;
            do
            {
                messanger = await _chatClient.GetMessangerAsync(sheet, "active", messanger?.Cursor, limit);
                if (messanger == null || messanger.Dialogs == null || messanger.Dialogs.Count == 0)
                {
                    break;
                }
                dialogues = dialogues.Union(messanger.Dialogs).ToList();
            } while (messanger.Dialogs.Count == limit && IsLoadDialogues(messanger, dateThreshold));
            dialogues.ForEach(dialogue =>
            {
                if (!dialogue.IsBlocked && dialogue.DateUpdated >= dateThreshold && (dialogue.IdInterlocutor == dialogue.LastMessage?.IdUserFrom || dialogue.LastMessage?.Type == MessageType.System))
                {
                    CheckAndUpdateDateCreatedSystemLastMessage(dialogue);
                    dictionary.TryAdd(new SheetDialogKey(sheet.Id, dialogue.IdInterlocutor), new NewMessage { Dialogue = dialogue });
                }
            });
        }

        private async Task LoadOnlineDialogues(Sheet sheet, ConcurrentDictionary<SheetDialogKey, NewMessage> dictionary, int limit)
        {
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
            int countOnline = dialogues.Count(d => !d.IsBlocked);

            _dictionary.Online.AddOrUpdate(sheet.Id, countOnline, (key, oldValue) => countOnline);

            dialogues.ForEach(dialogue =>
            {
                if (!dialogue.IsBlocked)
                {
                    
                    dialogue.Status = Status.Online;
                    CheckAndUpdateDateCreatedSystemLastMessage(dialogue);
                    dictionary.TryAdd(new SheetDialogKey(sheet.Id, dialogue.IdInterlocutor), new NewMessage { Dialogue = dialogue });
                }
            });
        }

        //Возвращаем true если у последнего диалога значение поля DateUpdate младше dateThreshodl
        private bool IsLoadDialogues(Messenger messanger, DateTime dateThreshold)
        {
            return messanger.Dialogs.Last().DateUpdated > dateThreshold;
        }

        private void CheckAndUpdateDateCreatedSystemLastMessage(Dialogue dialogue)
        {
            if (dialogue.LastMessage.Type == MessageType.System)
            {
                dialogue.LastMessage.DateCreated = dialogue.DateUpdated;
            }
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

        private async Task<List<SheetChat>> GetSheetsFast(string sessionGuid)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.GetAsync($"Sheets/GetSheetsFast?sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var sheets = (await response.Content.ReadFromJsonAsync<List<SheetChat>>()).Where(a => a.IsActive).ToList();
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
