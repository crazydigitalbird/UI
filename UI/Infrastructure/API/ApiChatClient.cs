using Core.Models.Sheets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiChatClient : IChatClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiChatClient> _logger;

        public ApiChatClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiChatClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Messenger> GetMessangerAsync(Sheet sheet, string criteria = "", string cursor = "", int limit = 10, int operatorId = 0)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            string s = "";
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("limit", limit.ToString());
                httpClient.DefaultRequestHeaders.Add("criteria", criteria);
                httpClient.DefaultRequestHeaders.Add("cursor", cursor);

                if (operatorId > 0)
                {
                    httpClient.DefaultRequestHeaders.Add("operator", operatorId.ToString());
                }
                else
                {
                    httpClient.DefaultRequestHeaders.Add("operator", "");
                }

                var response = await httpClient.PostAsync("message_by_criteria", null);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif
                    var messenger = await response.Content.ReadFromJsonAsync<Messenger>();
                    return messenger;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    //Unauthorized
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

        public async Task<Messenger> GetMessangerPremiumAndTrashAsync(Sheet sheet, string criteria, string cursor = "", int limit = 20)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("idUser", sheet.Identity),
                    new KeyValuePair<string, string>("type", criteria),
                    new KeyValuePair<string, string>("cursor", cursor),
                    new KeyValuePair<string, string>("limit", $"{limit}")};
                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync($"get_premium", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var messenger = await response.Content.ReadFromJsonAsync<Messenger>();
                    return messenger;
                }
                else
                {
                    _logger.LogWarning("Error receiving dialogs is criteria {criteria}. HttpStatusCode: {httpStatusCode}", criteria, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error receiving dialogs is criteria {criteria}.", criteria);
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
#if DEBUGOFFLINE || DEBUG
                        var s = await response.Content.ReadAsStringAsync();
#endif
                        var sheetInfos = await response.Content.ReadFromJsonAsync<List<SheetInfo>>();
                        foreach (var sheetInfo in sheetInfos)
                        {
                            var chat = dialogues.FirstOrDefault(c => c.IdInterlocutor == sheetInfo.Id);
                            if (chat != null)
                            {
                                chat.Avatar = sheetInfo.Personal.AvatarSmall;
                                chat.UserName = sheetInfo.Personal.Name;
                                chat.Status = sheetInfo.IsOnline ? Status.Online : Status.Offline;
                                //chat.Age = sheetInfo.Personal.Age;
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

        public async Task<List<SheetInfo>> GetManProfiles(Sheet sheet, List<int> idDialogues)
        {
            if (idDialogues?.Count > 0)
            {
                HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
                try
                {
                    var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                    httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                    httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                    httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                    httpClient.DefaultRequestHeaders.Add("ids", string.Join(",", idDialogues));

                    var response = await httpClient.PostAsync($"man_profiles", null);
                    if (response.IsSuccessStatusCode)
                    {
#if DEBUGOFFLINE || DEBUG
                        var s = await response.Content.ReadAsStringAsync();
#endif
                        var sheetInfos = await response.Content.ReadFromJsonAsync<List<SheetInfo>>();
                        return sheetInfos;
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
            return null;
        }

        public async Task<SheetInfo> GetManProfile(Sheet sheet, int idUser)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("ids", idUser.ToString());

                var response = await httpClient.PostAsync($"man_profiles", null);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var sheetInfos = await response.Content.ReadFromJsonAsync<List<SheetInfo>>();
                    return sheetInfos.FirstOrDefault();
                }
                else
                {
                    _logger.LogWarning("Error getting information profile the sheet. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting information profile the sheet.");
            }
            return null;
        }

        public async Task<PhotosPerson> GetManPublicPhoto(Sheet sheet, int idUser)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("idUser", $"{idUser}") };
                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync($"man_profiles_photo", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var photosPersonData = await response.Content.ReadFromJsonAsync<PhotosPersonData>();
                    return photosPersonData.PhotosPerson;
                }
                else
                {
                    _logger.LogWarning("Error getting photos profile the sheet. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photos profile the sheet.");
            }
            return null;
        }

        public async Task<Messenger> GetMessagesChatAsync(Sheet sheet, int chatId, long idLastMessage)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var info = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
                var messanger = new Messenger
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
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var messages = (await response.Content.ReadFromJsonAsync<List<Message>>());
                    if (messages.Count == 0)
                    {
                        var dialog = await FindDialogueById(sheet, chatId);
                        dialog.Messages = new List<Message> { dialog.LastMessage };
                        messanger.Dialogs.Add(dialog);
                    }
                    else
                    {
                        messanger.Dialogs.Add(new Dialogue { Messages = messages, IdInterlocutor = chatId });
                        await GetManProfiles(sheet, messanger.Dialogs);
                    }
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
#if DEBUGOFFLINE
            return new MessagesAndMailsLeft { MailsLeft = 10, MessagesLeft = 20};
#endif
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
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var stickers = await response.Content.ReadFromJsonAsync<List<StickerGroup>>();
                    return stickers;
                }
                else
                {
                    _logger.LogWarning("Error loading stickers in sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stickers in sheet with id: {sheetId}.", sheet.Id);
            }
            return null;
        }

        #region Gifts
        public async Task<bool> CheckGiftsAsync(Sheet sheet, int idInterlocutor)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("idUserFrom", $"{sheet.Identity}"),
                    new KeyValuePair<string, string>("idUserTo", $"{idInterlocutor}")};
                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync($"get_gifts", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var limitGifts = await response.Content.ReadFromJsonAsync<LimitGifts>();
                    return limitGifts.Limit > 0 ? true : false;
                }
                else
                {
                    _logger.LogWarning("Error checked gifts userFrom: {userFrom}, userTo: {userTo}. HttpStatusCode: {httpStatusCode}", sheet.Identity, idInterlocutor, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checked gifts userFrom: {userFrom}, userTo: {userTo}.", sheet.Identity, idInterlocutor);
            }
            return false;
        }

        public async Task<GiftData> GetGiftsAsync(Sheet sheet)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("limit", "30"),
                    new KeyValuePair<string, string>("cursor", "") };

                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("list_gifts", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var giftData = await response.Content.ReadFromJsonAsync<GiftData>();
                    return giftData;
                }
                else
                {
                    _logger.LogWarning("Error loading gifts in sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading gifts in sheet with id: {sheetId}.", sheet.Id);
            }
            return null;
        }

        public async Task<long?> SendGiftAsync(Sheet sheet, int idUserTo, string idGift, string message, long idLastMessage)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("idUserTo", $"{idUserTo}"),
                    new KeyValuePair<string, string>("idGift", idGift),
                    new KeyValuePair<string, string>("message", message),
                    new KeyValuePair<string, string>("operator", GetUserId())};

                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("send_gifts", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var sendMessage = await response.Content.ReadFromJsonAsync<SendMessage>();
                    return sendMessage?.IdMessage;
                }
                else
                {
                    _logger.LogWarning("Error loading gifts in sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading gifts in sheet with id: {sheetId}.", sheet.Id);
            }
            return null;
        }
        #endregion

        #region Post
        public async Task<bool> CheckPostAsync(Sheet sheet, int idInterlocutor)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("idInterlocutor", $"{idInterlocutor}")};
                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync($"get_posts", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var limitPosts = await response.Content.ReadFromJsonAsync<LimitPost>();
                    return limitPosts.AllowSendPost;
                }
                else
                {
                    _logger.LogWarning("Error checked post userFrom: {userFrom}, userTo: {userTo}. HttpStatusCode: {httpStatusCode}", sheet.Identity, idInterlocutor, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checked post userFrom: {userFrom}, userTo: {userTo}.", sheet.Identity, idInterlocutor);
            }
            return false;
        }

        public async Task<List<Post>> ListPostAsync(Sheet sheet, int idRegularUser, long idLastMessage = 0, int limit = 20)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("idRegularUser", $"{idRegularUser}"),
                    new KeyValuePair<string, string>("idLastMessage", $"{idLastMessage}"),
                    new KeyValuePair<string, string>("limit", $"{limit}")};
                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync($"list_posts", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var posts = await response.Content.ReadFromJsonAsync<List<Post>>();
                    return posts;
                }
                else
                {
                    _logger.LogWarning("Error checked post userFrom: {userFrom}, userTo: {userTo}. HttpStatusCode: {httpStatusCode}", sheet.Identity, idRegularUser, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checked post userFrom: {userFrom}, userTo: {userTo}.", sheet.Identity, idRegularUser);
            }
            return null;
        }

        public async Task<long?> SendPostAsync(Sheet sheet, int idRegularUser, string text, List<long> videos, List<long> photos)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("idRegularUser", $"{idRegularUser}"),
                    new KeyValuePair<string, string>("text", text),
                    new KeyValuePair<string, string>("idsGalleryVideos", string.Join(",", videos)),
                    new KeyValuePair<string, string>("idsGalleryPhotos", string.Join(",", photos)),
                    new KeyValuePair<string, string>("operator", GetUserId())};
                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync($"send_superpost", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var sendMessage = await response.Content.ReadFromJsonAsync<SendMessage>();
                    return sendMessage?.IdMessage;
                }
                else
                {
                    _logger.LogWarning("Error send post userFrom: {userFrom}, userTo: {userTo}. HttpStatusCode: {httpStatusCode}", sheet.Identity, idRegularUser, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checked post userFrom: {userFrom}, userTo: {userTo}.", sheet.Identity, idRegularUser);
            }

#if DEBUGOFFLINE
            return (new Random()).Next();
#endif

            return null;
        }
        #endregion

        public async Task<Media> GetPhotosAsync(Sheet sheet, int idRegularUser, string statuses = "", string tags = "", string excludeTags = "", string cursor = "")
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("idRegularUser", $"{idRegularUser}");
                httpClient.DefaultRequestHeaders.Add("limit", "40");
                httpClient.DefaultRequestHeaders.Add("criteria", "");
                httpClient.DefaultRequestHeaders.Add("statuses", statuses);
                httpClient.DefaultRequestHeaders.Add("tags", tags);
                httpClient.DefaultRequestHeaders.Add("excludeTags", excludeTags);
                httpClient.DefaultRequestHeaders.Add("cursor", cursor);

                var response = await httpClient.PostAsync("media_photo", null);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
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

        public async Task<Media> GetVideosAsync(Sheet sheet, int idRegularUser, string statuses = "", string tags = "", string excludeTags = "", string cursor = "")
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("idRegularUser", $"{idRegularUser}");
                httpClient.DefaultRequestHeaders.Add("limit", "40");
                httpClient.DefaultRequestHeaders.Add("statuses", statuses);
                httpClient.DefaultRequestHeaders.Add("tags", tags);
                httpClient.DefaultRequestHeaders.Add("excludeTags", excludeTags);
                httpClient.DefaultRequestHeaders.Add("cursor", cursor);

                var response = await httpClient.PostAsync("media_video", null);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
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

        public async Task<long?> SendMessageAsync(Sheet sheet, int idRegularUser, MessageType messageType, string message, long idLastMessage)
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
                    new KeyValuePair<string, string>("idRegularUser", $"{idRegularUser}"),
                    new KeyValuePair<string, string>("idLastMessage", idLastMessage.ToString()),
                    new KeyValuePair<string, string>("operator", GetUserId())
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
                        //contentList.Add(new KeyValuePair<string, string>("idSticker", ""));
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

                    case MessageType.Photo:
                        contentList.Add(new KeyValuePair<string, string>("type", "gallery-photo"));
                        //contentList.Add(new KeyValuePair<string, string>("message", ""));
                        contentList.Add(new KeyValuePair<string, string>("idGalleryPhoto", message));
                        break;

                    case MessageType.Video:
                        contentList.Add(new KeyValuePair<string, string>("type", "gallery-video"));
                        //contentList.Add(new KeyValuePair<string, string>("message", ""));
                        contentList.Add(new KeyValuePair<string, string>("idGalleryVideo", message));
                        break;
                }

                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("send", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var sendMessage = await response.Content.ReadFromJsonAsync<SendMessage>();
                    return sendMessage?.IdMessage;
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

#if DEBUGOFFLINE
            return (new Random()).Next();
#endif
            return null;
        }

        public async Task<Dialogue> FindDialogueById(Sheet sheet, int idRegularUser)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var info = JsonConvert.DeserializeObject<SheetInfo>(sheet.Info);
                var messanger = new Messenger
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
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
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

        public async Task<bool> ChangePinAsync(Sheet sheet, int idRegularUser, bool addPin)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                httpClient.DefaultRequestHeaders.Add("site", sheet.Site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", credentials.Login);
                httpClient.DefaultRequestHeaders.Add("password", credentials.Password);
                httpClient.DefaultRequestHeaders.Add("idInterlocutor", $"{idRegularUser}");
                httpClient.DefaultRequestHeaders.Add("type", addPin ? "pin" : "unpin");

                var response = await httpClient.PostAsync("pinned", null);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    return true;
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
            return false;
        }

        public async Task<bool> ChangeBookmarkAsync(Sheet sheet, int idRegularUser, bool addBookmark)
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
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
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

        public async Task<bool> ChangePremiumAsync(Sheet sheet, int idInterlocutor, bool addPremium)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("type", addPremium ? "premium" : "unpremium"),
                    new KeyValuePair<string, string>("idUser", sheet.Identity),
                    new KeyValuePair<string, string>("idInterlocutor", $"{idInterlocutor}")};
                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync($"premium", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    return true;
                }
                else
                {
                    _logger.LogWarning("Error premium dialogue with id: {idRegularUser}.Sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", idInterlocutor, sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error premium dialogue with id: {idRegularUser}.Sheet with id: {sheetId}.", idInterlocutor, sheet.Id);
            }
            return false;
        }

        public async Task<bool> ChangeTrashAsync(Sheet sheet, int idInterlocutor, bool addTrash)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("type", addTrash ? "trash" : "untrash"),
                    new KeyValuePair<string, string>("idUser", sheet.Identity),
                    new KeyValuePair<string, string>("idInterlocutor", $"{idInterlocutor}")};
                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync($"premium", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    return true;
                }
                else
                {
                    _logger.LogWarning("Error trash dialogue with id: {idRegularUser}.Sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}.", idInterlocutor, sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trash dialogue with id: {idRegularUser}.Sheet with id: {sheetId}.", idInterlocutor, sheet.Id);
            }
            return false;
        }

        public async Task<Dictionary<long, MessageTimer>> Timers(IEnumerable<long?> idLastMessages)
        {
#if DEBUGOFFLINE || DEBUG
            string s;
#endif
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                string ids = string.Join(",", idLastMessages);
                httpClient.DefaultRequestHeaders.Add("id_message", ids);
                var response = await httpClient.PostAsync("timer", null);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif
                    var messagestimers = await response.Content.ReadFromJsonAsync<Dictionary<long, MessageTimer>>();
                    return messagestimers;
                }
                else
                {
                    _logger.LogWarning("Error getting timers for lastMessages. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timers for lastMessages");
            }
            return null;
        }

        public async Task<MessageTimer> SystemTimer(int idUserFrom, int idUserTo)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                httpClient.DefaultRequestHeaders.Add("idUserFrom", idUserFrom.ToString());
                httpClient.DefaultRequestHeaders.Add("idUserTo", idUserTo.ToString());
                var response = await httpClient.PostAsync("timer_system", null);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var timer = await response.Content.ReadFromJsonAsync<MessageTimer>();
                    return timer;
                }
                else
                {
                    _logger.LogWarning("Error getting timers for lastMessages. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timers for lastMessages");
            }
            return null;
        }

        private string GetUserId()
        {
            var user = _httpContextAccessor.HttpContext.User;
            var userId = user.FindFirst("Id")?.Value;
            return userId;
        }

    }

    public interface IChatClient
    {
        Task<Messenger> GetMessangerAsync(Sheet sheet, string criteria = "", string cursor = "", int limit = 10, int operatorId = 0);
        Task<Messenger> GetMessangerPremiumAndTrashAsync(Sheet sheet, string criteria, string cursor = "", int limit = 20);

        Task GetManProfiles(Sheet sheet, List<Dialogue> dialogues);
        Task<List<SheetInfo>> GetManProfiles(Sheet sheet, List<int> idDialogues);

        Task<SheetInfo> GetManProfile(Sheet sheet, int idUser);
        Task<PhotosPerson> GetManPublicPhoto(Sheet sheet, int idUser);

        Task<Messenger> GetMessagesChatAsync(Sheet sheet, int chatId, long idLastMessage);
        Task<MessagesAndMailsLeft> GetManMessagesMails(Sheet sheet, int idRegularUser);
        Task<List<StickerGroup>> GetStickersAsync(Sheet sheet);

        Task<bool> CheckGiftsAsync(Sheet sheet, int idInterlocutor);
        Task<GiftData> GetGiftsAsync(Sheet sheet);
        Task<long?> SendGiftAsync(Sheet sheet, int idUserTo, string idGift, string message, long idLastMessage);

        Task<bool> CheckPostAsync(Sheet sheet, int idInterlocutor);
        Task<List<Post>> ListPostAsync(Sheet sheet, int idRegularUser, long idLastMessage = 0, int limit = 20);
        Task<long?> SendPostAsync(Sheet sheet, int idRegularUser, string text, List<long> videos, List<long> photos);

        Task<Media> GetPhotosAsync(Sheet sheet, int idRegularUser, string statuses = "", string tags = "", string excludeTags = "", string cursor = "");
        Task<Media> GetVideosAsync(Sheet sheet, int idRegularUser, string statuses = "", string tags = "", string excludeTags = "", string cursor = "");
        Task<long?> SendMessageAsync(Sheet sheet, int idRegularUser, MessageType messageType, string message, long idLastMessage);
        Task<Dialogue> FindDialogueById(Sheet sheet, int idRegularUser);

        Task<bool> ChangePinAsync(Sheet sheet, int idRegularUser, bool addPin);
        Task<bool> ChangeBookmarkAsync(Sheet sheet, int idRegularUser, bool addBookmark);
        Task<bool> ChangePremiumAsync(Sheet sheet, int idInterlocutor, bool addPremium);
        Task<bool> ChangeTrashAsync(Sheet sheet, int idInterlocutor, bool addTrash);

        Task<Dictionary<long, MessageTimer>> Timers(IEnumerable<long?> idLastMessages);
        Task<MessageTimer> SystemTimer(int idUserFrom, int idUserTo);
    }
}
