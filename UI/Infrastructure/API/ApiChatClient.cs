using Core.Models.Sheets;
using Newtonsoft.Json;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiChatClient : IChatClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiChatClient> _logger;

        public ApiChatClient(IHttpClientFactory httpClientFactory, ILogger<ApiChatClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<Messanger> GetMessangerAsync(Sheet sheet, string criteria = "", string cursor = "", int limit = 10)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("limit", limit.ToString());
                httpClient.DefaultRequestHeaders.Add("criteria", criteria);
                httpClient.DefaultRequestHeaders.Add("cursor", cursor);

                var response = await httpClient.PostAsync("message_by_criteria", null);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    var messanger = await response.Content.ReadFromJsonAsync<Messanger>();
                    //await AddPinAsync(sheet, 81171332, true);
                    return messanger;
                }
                else
                {
                    _logger.LogWarning("Error receiving gialogs. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error receiving dialogs.");
            }
            return null;
        }

        public async Task GetManProfiles(Sheet sheet, List<Dialogue> dialogues)
        {
            if (dialogues?.Count > 0)
            {
                HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
                try
                {
                    var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                    httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                    httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                    httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                    httpClient.DefaultRequestHeaders.Add("ids", string.Join(",", dialogues.Select(c => c.IdInterlocutor)));

                    var response = await httpClient.PostAsync($"man_profiles", null);
                    if (response.IsSuccessStatusCode)
                    {
                        var s = await response.Content.ReadAsStringAsync();
                        var sheetInfos = await response.Content.ReadFromJsonAsync<List<SheetInfo>>();
                        foreach (var sheetInfo in sheetInfos)
                        {
                            var chat = dialogues.FirstOrDefault(c => c.IdInterlocutor == sheetInfo.Id);
                            if (chat != null)
                            {
                                chat.Avatar = sheetInfo.Personal.AvatarSmall;
                                chat.UserName = sheetInfo.Personal.Name;
                                chat.Status = sheetInfo.IsOnline ? Status.Online : Status.Offline;
                                chat.Age = sheetInfo.Personal.Age;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Error getting status and media all the sheets. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting status and media all the sheets.");
                }
            }
        }

        public async Task<Messanger> GetMessagesChatAsync(Sheet sheet, int chatId, long idLastMessage)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var info = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
                var messanger = new Messanger
                {
                    Sheet = info,
                    Dialogs = new List<Dialogue>()
                };

                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("idRegularUser", chatId.ToString());
                httpClient.DefaultRequestHeaders.Add("idLastMessage", idLastMessage.ToString());
                httpClient.DefaultRequestHeaders.Add("limit", "15");

                var response = await httpClient.PostAsync("message", null);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    var messages = (await response.Content.ReadFromJsonAsync<List<Message>>());

                    messanger.Dialogs.Add(new Dialogue { Messages = messages, IdInterlocutor = chatId });

                    await GetManProfiles(sheet, messanger.Dialogs);
                    return messanger;
                }
                else
                {
                    _logger.LogWarning("Error receiving messages in chat with id: {chatId}.Sheet with id: {sheetId}. Last message with id: {idLastMesssage}. HttpStatusCode: {httpStatusCode}.", chatId, sheet.Id, idLastMessage, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages in chat with id: {chatId}.Sheet with id: {sheetId}. Last message with id: {idLastMesssage}.", chatId, sheet.Id, idLastMessage);
            }
            return null;
        }

        public async Task<MessagesAndMailsLeft> GetManMessagesMails(Sheet sheet, int idRegularUser)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("idRegularUser", idRegularUser.ToString());

                var response = await httpClient.PostAsync("man_letters_mails", null);
                if (response.IsSuccessStatusCode)
                {
                    var messagesAndmailsLeft = await response.Content.ReadFromJsonAsync<MessagesAndMailsLeft>();
                    var info = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
                    return messagesAndmailsLeft;
                }
                else
                {
                    _logger.LogWarning("Error loading messages and mails left in sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading messages and mails left in sheet with id: {sheetId}.", sheet.Id);
            }
            return null;
        }

        public async Task<List<StickerGroup>> GetStickersAsync(Sheet sheet)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);

                var response = await httpClient.PostAsync("stikers", null);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    var stickers = await response.Content.ReadFromJsonAsync<List<StickerGroup>>();
                    return stickers;
                }
                else
                {
                    _logger.LogWarning("Error loading stikers in sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stikers in sheet with id: {sheetId}.", sheet.Id);
            }
            return null;
        }

        public async Task<Media> GetPhotosAsync(Sheet sheet, string cursor = "")
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("limit", "40");
                httpClient.DefaultRequestHeaders.Add("criteria", "");
                httpClient.DefaultRequestHeaders.Add("statuses", "");
                httpClient.DefaultRequestHeaders.Add("tags", "");
                httpClient.DefaultRequestHeaders.Add("excludeTags", "");
                httpClient.DefaultRequestHeaders.Add("cursor", cursor);

                var response = await httpClient.PostAsync("media_photo", null);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    var media = await response.Content.ReadFromJsonAsync<Media>();
                    return media;
                }
                else
                {
                    _logger.LogWarning("Error loading photos in sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading photos in sheet with id: {sheetId}.", sheet.Id);
            }
            return null;
        }

        public async Task<Media> GetVideosAsync(Sheet sheet, string cursor = "")
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("limit", "40");
                httpClient.DefaultRequestHeaders.Add("statuses", "");
                httpClient.DefaultRequestHeaders.Add("tags", "");
                httpClient.DefaultRequestHeaders.Add("excludeTags", "");
                httpClient.DefaultRequestHeaders.Add("cursor", cursor);

                var response = await httpClient.PostAsync("media_video", null);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    var media = await response.Content.ReadFromJsonAsync<Media>();
                    return media;
                }
                else
                {
                    _logger.LogWarning("Error loading videos in sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading videos in sheet with id: {sheetId}.", sheet.Id);
            }
            return null;
        }

        public async Task<SendMessage> SendMessageAsync(Sheet sheet, int idRegularUser, MessageType messageType, string message)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                //if (messageType == MessageType.Photo_batch)
                //{
                //    msg.Content = new Content() { Photos = new List<Photo> { new Photo { Url = message } } };
                //}

                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("idRegularUser", $"{idRegularUser}")
                };
                //httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                //httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                //httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                //httpClient.DefaultRequestHeaders.Add("idRegularUser", $"{idRegularUser}");

                switch (messageType)
                {
                    case MessageType.Message:
                        contentList.Add(new KeyValuePair<string, string>("type", "text"));
                        contentList.Add(new KeyValuePair<string, string>("message", message));
                        contentList.Add(new KeyValuePair<string, string>("idSticker", ""));
                        //httpClient.DefaultRequestHeaders.Add("type", "text");
                        //httpClient.DefaultRequestHeaders.Add("message", message);
                        //httpClient.DefaultRequestHeaders.Add("idSticker", "");
                        break;

                    case MessageType.Sticker:
                        contentList.Add(new KeyValuePair<string, string>("type", "sticker"));
                        contentList.Add(new KeyValuePair<string, string>("message", ""));
                        contentList.Add(new KeyValuePair<string, string>("idSticker", message));
                        //httpClient.DefaultRequestHeaders.Add("type", "sticker");
                        //httpClient.DefaultRequestHeaders.Add("message", "");
                        //httpClient.DefaultRequestHeaders.Add("idSticker", message);
                        break;
                }

                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("send", content);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    var sendMessage = await response.Content.ReadFromJsonAsync<SendMessage>();
                    return sendMessage;
                }
                else
                {
                    _logger.LogWarning("Error send message by user with id: {idRegularUser}. Message type: {messageType}. Sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", idRegularUser, messageType, sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error send message by user with id: {idRegularUser}. Message type: {messageType}. Sheet with id: {sheetId}.", idRegularUser, messageType, sheet.Id);
            }
            return null;
        }

        public async Task<Dialogue> FindDialogueById(Sheet sheet, int idRegularUser)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var info = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
                var messanger = new Messanger
                {
                    Sheet = info,
                    Dialogs = new List<Dialogue>()
                };

                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("idsRegularUser", $"{idRegularUser}");

                var response = await httpClient.PostAsync("find_by_id", null);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    var dialogues = await response.Content.ReadFromJsonAsync<List<Dialogue>>();
                    await GetManProfiles(sheet, dialogues);
                    return dialogues.FirstOrDefault();
                }
                else
                {
                    _logger.LogWarning("Error find dialogue with id: {idRegularUser}.Sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", idRegularUser, sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error find dialogue with id: {idRegularUser}.Sheet with id: {sheetId}.", idRegularUser, sheet.Id);
            }
            return null;
        }

        public async Task<object> AddPinAsync(Sheet sheet, int idRegularUser, bool addPin)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("idInterlocutor", $"{idRegularUser}");

                var response = await httpClient.PostAsync("pinned", null);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    _logger.LogWarning("Error pin dialogue with id: {idRegularUser}.Sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", idRegularUser, sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pin dialogue with id: {idRegularUser}.Sheet with id: {sheetId}.", idRegularUser, sheet.Id);
            }
            return null;
        }

        public async Task<bool> AddBookmarkAsync(Sheet sheet, int idRegularUser, bool addBookmark)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("idRegularUser", $"{idRegularUser}");
                httpClient.DefaultRequestHeaders.Add("bookmark", addBookmark ? "1" : "0");

                var response = await httpClient.PostAsync("bookmark", null);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    return true;
                }
                else
                {
                    _logger.LogWarning("Error bookmark dialogue with id: {idRegularUser}.Sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", idRegularUser, sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bookmark dialogue with id: {idRegularUser}.Sheet with id: {sheetId}.", idRegularUser, sheet.Id);
            }
            return false;
        }
    }

    public interface IChatClient
    {
        Task<Messanger> GetMessangerAsync(Sheet sheet, string criteria = "", string cursor = "", int limit = 10);
        Task GetManProfiles(Sheet sheet, List<Dialogue> dialogues);
        Task<Messanger> GetMessagesChatAsync(Sheet sheet, int chatId, long idLastMessage);
        Task<MessagesAndMailsLeft> GetManMessagesMails(Sheet sheet, int idRegularUser);
        Task<List<StickerGroup>> GetStickersAsync(Sheet sheet);
        Task<Media> GetPhotosAsync(Sheet sheet, string cursor = "");
        Task<Media> GetVideosAsync(Sheet sheet, string cursor = "");
        Task<SendMessage> SendMessageAsync(Sheet sheet, int idRegularUser, MessageType messageType, string message);
        Task<Dialogue> FindDialogueById(Sheet sheet, int idRegularUser);
    }
}
