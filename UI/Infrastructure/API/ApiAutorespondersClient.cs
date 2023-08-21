using Core.Models.Sheets;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiAutorespondersClient : IAutorespondersClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiAutorespondersClient> _logger;

        public ApiAutorespondersClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiAutorespondersClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Dictionary<string, AutoresponderInfo>> GetAutorespondersInfoAsync(List<Sheet> sheets)
        {
            if (sheets?.Count > 0)
            {
                HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
                string s = "";
                try
                {
                    var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("idUser", string.Join(",", sheets.Select(s => s.Identity)))
                };
                    var contents = new FormUrlEncodedContent(contentList);

                    var response = await httpClient.PostAsync("autoanswer_status", contents);
                    if (response.IsSuccessStatusCode)
                    {
#if DEBUGOFFLINE || DEBUG
                        s = await response.Content.ReadAsStringAsync();
#endif
                        var AutoresponderInfoBody = await response.Content.ReadFromJsonAsync<AutoresponderInfoBody>();
                        return AutoresponderInfoBody.Data.Select(d => d.First()).DistinctBy(d => d.Key).ToDictionary(d => d.Key, d => d.Value);
                    }
                    else
                    {
                        _logger.LogWarning("Error get autoresponders info with ids: {sheetsId}. HttpStatusCode: {httpStatusCode}", string.Join(",", sheets.Select(s => s.Identity)), response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error get autoresponders info with ids: {sheetsId}.", string.Join(",", sheets.Select(s => s.Identity)));
                }
            }
            return new Dictionary<string, AutoresponderInfo>();
        }

        public async Task<Autoresponder> GetAutorespondersAsync(Sheet sheet)
        {
            if (sheet != null)
            {
                HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
                string s = "";
                try
                {
                    var contentList = new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("user_id", sheet.Identity)
                    };

                    var contents = new FormUrlEncodedContent(contentList);

                    var response = await httpClient.PostAsync("get_autoanswer", contents);
                    if (response.IsSuccessStatusCode)
                    {
#if DEBUGOFFLINE || DEBUG
                        s = await response.Content.ReadAsStringAsync();
#endif
                        var autoresponder = await response.Content.ReadFromJsonAsync<Autoresponder>();
                        return autoresponder;
                    }
                    else
                    {
                        _logger.LogWarning("Error get autoresponders with id: {sheetId}. HttpStatusCode: {httpStatusCode}", sheet.Id, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error get autoresponders with id: {sheetId}.", sheet.Id);
                }
            }
            return null;
        }

        public async Task<bool> ClearAutoresponderAsync(Sheet sheet, string stackType, AutoresponderMessages autoresponderMessages)
        {
            if (sheet != null)
            {
                HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
                string s = "";
                try
                {
                    var contentList = new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("user_id", sheet.Identity),
                        new KeyValuePair<string, string>("stack_type", stackType),
                        new KeyValuePair<string, string>("messages_to_clear", autoresponderMessages.ToString())
                    };

                    var contents = new FormUrlEncodedContent(contentList);

                    var response = await httpClient.PostAsync("clear_autoanswer", contents);
                    if (response.IsSuccessStatusCode)
                    {
#if DEBUGOFFLINE || DEBUG
                        s = await response.Content.ReadAsStringAsync();
#endif
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Error clear autoresponder sheet with id: {sheetId}. Autoresponder stack type: {stackType} Autoresponder message: {autoresponderMessages}. HttpStatusCode: {httpStatusCode}", sheet.Id, stackType, autoresponderMessages, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clear autoresponder sheet with id: {sheetId}. Autoresponder stack type: {stackType} Autoresponder message: {autoresponderMessages}.", sheet.Id, stackType, autoresponderMessages);
                }
            }
            return false;
        }

        public async Task<bool> UpdateAsync(Sheet sheet, string stackType, AutoresponderMessages autoresponderMessages, string message, int intervalStart, int intervalFinish, int limitMessage)
        {
            if (sheet != null)
            {
                HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
                string s = "";
                try
                {
                    var contentList = new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("user_id", sheet.Identity),
                        new KeyValuePair<string, string>("stack_type", stackType)                        
                    };

                    switch (autoresponderMessages)
                    {
                        case AutoresponderMessages.first_message:
                            contentList.Add(new KeyValuePair<string, string>("first_message", message));
                            contentList.Add(new KeyValuePair<string, string>("first_limit_count", $"{limitMessage}"));
                            break;

                        case AutoresponderMessages.second_message:
                            contentList.Add(new KeyValuePair<string, string>("second_message", message));
                            contentList.Add(new KeyValuePair<string, string>("first_interval", $"{intervalStart}"));
                            contentList.Add(new KeyValuePair<string, string>("first_interval_finish", $"{intervalFinish}"));
                            contentList.Add(new KeyValuePair<string, string>("second_limit_count", $"{limitMessage}"));
                            break;

                        case AutoresponderMessages.last_message:
                            contentList.Add(new KeyValuePair<string, string>("last_message", message));
                            contentList.Add(new KeyValuePair<string, string>("second_interval", $"{intervalStart}"));
                            contentList.Add(new KeyValuePair<string, string>("second_interval_finish", $"{intervalFinish}"));
                            contentList.Add(new KeyValuePair<string, string>("last_limit_count", $"{limitMessage}"));
                            break;
                    }

                    var contents = new FormUrlEncodedContent(contentList);

                    var response = await httpClient.PostAsync("update_autoanswer", contents);
                    if (response.IsSuccessStatusCode)
                    {
#if DEBUGOFFLINE || DEBUG
                        s = await response.Content.ReadAsStringAsync();
#endif
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Error update autoresponder sheet with id: {sheetId}. Autoresponder stack type: {stackType} Autoresponder message: {autoresponderMessages}. HttpStatusCode: {httpStatusCode}", sheet.Id, stackType, autoresponderMessages, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error update autoresponder sheet with id: {sheetId}. Autoresponder stack type: {stackType} Autoresponder message: {autoresponderMessages}.", sheet.Id, stackType, autoresponderMessages);
                }
            }
            return false;
        }
    }
}

public interface IAutorespondersClient
{
    Task<Dictionary<string, AutoresponderInfo>> GetAutorespondersInfoAsync(List<Sheet> sheets);

    Task<Autoresponder> GetAutorespondersAsync(Sheet sheet);

    Task<bool> ClearAutoresponderAsync(Sheet sheet, string stackType, AutoresponderMessages autoresponderMessages);

    Task<bool> UpdateAsync(Sheet sheet, string stackType, AutoresponderMessages autoresponderMessages, string message, int intervalStart, int intervalFinish, int limitMessage);
}
