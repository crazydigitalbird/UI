using Core.Models.Sheets;
using Newtonsoft.Json;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiMailClient : IMailClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiMailClient> _logger;

        public ApiMailClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiMailClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Mailbox> GetMailByUserAsync(Sheet sheet, string type, int offset = 0, int limit = 15)
        {
#if DEBUGOFFLINE || DEBUG
            string s;
#endif
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("femaleIds", sheet.Identity),
                    new KeyValuePair<string, string>("type", type),
                    new KeyValuePair<string, string>("offset", $"{offset}"),
                    new KeyValuePair<string, string>("limit",  $"{limit}")};

                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("mail", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif
                    var mailBox = await response.Content.ReadFromJsonAsync<Mailbox>();
                    return mailBox;
                }
                else
                {
                    _logger.LogWarning("Error getting mail for user with id: {}. HttpStatusCode: {httpStatusCode}", sheet.Identity, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mail for user with id: {}", sheet.Identity);
            }
            return null;
        }

        public async Task<MailHistoryData> HistoryAsync(Sheet sheet, int idRegularUser, long idCorrespondence = 0, int page = 1, int limit = 40)
        {
#if DEBUGOFFLINE || DEBUG
            string s;
#endif
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("idRegularUser", $"{idRegularUser}"),
                    new KeyValuePair<string, string>("id_correspondence", $"{idCorrespondence}"),
                    new KeyValuePair<string, string>("page", $"{page}"),
                    new KeyValuePair<string, string>("limit",  $"{limit}")};

                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("emails_history", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif
                    var historyData = await response.Content.ReadFromJsonAsync<MailHistoryData>();
                    return historyData;
                }
                else
                {
                    _logger.LogWarning("Error getting history mail for user with id: {id} to regular user with id: {idRegularUser}. HttpStatusCode: {httpStatusCode}", sheet.Identity, idRegularUser, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history mail for user with id: {id} to regular user with id: {idRegularUser}", sheet.Identity, idRegularUser);
            }
            return null;
        }

        public async Task<long?> SendAsync(Sheet sheet, int idRegularUser, string text, List<long> videos, List<long> photos)
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

                var response = await httpClient.PostAsync($"send_mail", content);
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
                    _logger.LogWarning("Error send mail userFrom: {userFrom}, userTo: {userTo}. HttpStatusCode: {httpStatusCode}", sheet.Identity, idRegularUser, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error send mail userFrom: {userFrom}, userTo: {userTo}.", sheet.Identity, idRegularUser);
            }

#if DEBUGOFFLINE
            return (new Random()).Next();
#endif

            return null;
        }

        private string GetUserId()
        {
            var user = _httpContextAccessor.HttpContext.User;
            var userId = user.FindFirst("Id")?.Value;
            return userId;
        }
    }

    public interface IMailClient
    {
        Task<Mailbox> GetMailByUserAsync(Sheet sheet, string type, int offset = 0, int limit = 15);

        Task<MailHistoryData> HistoryAsync(Sheet sheet, int idRegularUser, long idCorrespondence = 0, int page = 1, int limit = 40);

        Task<long?> SendAsync(Sheet sheet, int idRegularUser, string text, List<long> videos, List<long> photos);
    }
}
