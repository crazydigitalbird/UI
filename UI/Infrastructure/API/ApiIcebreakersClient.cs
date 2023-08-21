using Core.Models.Sheets;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text.Json;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiIcebreakersClient : IIcebreakersClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiIcebreakersClient> _logger;

        public ApiIcebreakersClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiIcebreakersClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<AprovIce> Approved(Sheet sheet, int idLast = 0)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            string s = "";
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                //var contentList = new List<KeyValuePair<string, string>> {
                //    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                //    new KeyValuePair<string, string>("email", credentials.Login),
                //    new KeyValuePair<string, string>("password", credentials.Password),
                //    new KeyValuePair<string, string>("types", "message,mail"),
                //    new KeyValuePair<string, string>("limit", "100"),
                //    new KeyValuePair<string, string>("idLast", $"{idLast}")
                //};

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("cursor", ""),
                    new KeyValuePair<string, string>("limit", "200"),
                    new KeyValuePair<string, string>("type", "message,mail"),
                    new KeyValuePair<string, string>("idUser", sheet.Identity)
                };

                var contents = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("get_ice", contents); //icebreakers_approved
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif

                    var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                    jsonOptions.Converters.Add(new DateTimeConverter());
                    var aprovIce = await response.Content.ReadFromJsonAsync<AprovIce>(jsonOptions);
                    return aprovIce;
                }
                else
                {
                    _logger.LogWarning("Error get icebreakers approved. Sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}", sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get icebreakers approved. Sheet with id: {sheetId}.", sheet.Id);
            }
            return null;
        }

        public async Task<AprovIceProgress> Progress(Sheet sheet, int idLast = 0)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            string s = "";
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("types", "message,mail"),
                    new KeyValuePair<string, string>("limit", "40"),
                    new KeyValuePair<string, string>("idLast", $"{idLast}")
                };
                var contents = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("icebreakers_progress", contents);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif

                    var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                    jsonOptions.Converters.Add(new DateTimeConverter());
                    var aprovIce = await response.Content.ReadFromJsonAsync<AprovIceProgress>(jsonOptions);
                    return aprovIce;
                }
                else
                {
                    _logger.LogWarning("Error get icebreakers progress. Sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}", sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get icebreakers progress. Sheet with id: {sheetId}.", sheet.Id);
            }
            return null;
        }

        public async Task<AprovIce> Trash(Sheet sheet)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            string s = "";
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("cursor", ""),
                    new KeyValuePair<string, string>("limit", "200"),
                    new KeyValuePair<string, string>("type", "trash"),
                    new KeyValuePair<string, string>("idUser", sheet.Identity)
                };

                var contents = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("get_icetrash", contents);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif

                    var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                    jsonOptions.Converters.Add(new DateTimeConverter());
                    var aprovIce = await response.Content.ReadFromJsonAsync<AprovIce>(jsonOptions);
                    return aprovIce;
                }
                else
                {
                    _logger.LogWarning("Error get icebreakers trash. Sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}", sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get icebreakers trash. Sheet with id: {sheetId}.", sheet.Id);
            }
            return null;
        }

        public async Task<long> Create(Sheet sheet, IceType iceType, string content)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            string s = "";
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("type", $"{iceType}".ToLower()),
                    new KeyValuePair<string, string>("content", content)
                };
                var contents = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("icebreakers_create", contents);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif
                    var sendIce = await response.Content.ReadFromJsonAsync<SendIce>();
                    return sendIce.Id;
                }
                else
                {
                    _logger.LogWarning("Error create icebreakers. Sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}", sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error create icebreakers. Sheet with id: {sheetId}.", sheet.Id);
            }
            return 0;
        }

        public async Task<bool> Switch(Sheet sheet, long iceId, string status)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            string s = "";
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("id", $"{iceId}"),
                    new KeyValuePair<string, string>("status", status)
                };
                var contents = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("ice_switch", contents);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif
                    //await response.Content.ReadFromJsonAsync<bool>();
                    return true;
                }
                else
                {
                    _logger.LogWarning("Error switch ice with id: {iceId}, status: {status}. Sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}", iceId, status, sheet.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switch ice with id: {iceId}, status: {status}. Sheet with id: {sheetId}.", iceId, status, sheet.Id);
            }
            return false;
        }

        public async Task<bool> Delete(long iceId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            string s = "";
            try
            {
                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("type", "trash"),
                    new KeyValuePair<string, string>("idIcebreakers", $"{iceId}")
                };
                var contents = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("make_icetrash", contents);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif
                    var makeIceTrash = await response.Content.ReadFromJsonAsync<MakeIceTrash>();

                    return makeIceTrash.Response;
                }
                else
                {
                    _logger.LogWarning("Error delete ice with id: {iceId}. HttpStatusCode: {httpStatusCode}", iceId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delete ice with id: {iceId}.", iceId);
            }
            return false;
        }

        public async Task<bool> Reply(long iceId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            string s = "";
            try
            {
                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("type", "untrash"),
                    new KeyValuePair<string, string>("idIcebreakers", $"{iceId}")
                };
                var contents = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("make_icetrash", contents);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif
                    var makeIceTrash = await response.Content.ReadFromJsonAsync<MakeIceTrash>();

                    return makeIceTrash.Response;
                }
                else
                {
                    _logger.LogWarning("Error reply ice with id: {iceId}. HttpStatusCode: {httpStatusCode}", iceId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reply ice with id: {iceId}.", iceId);
            }
            return false;
        }

        public async Task<List<IceInfo>> IcesInfo(List<Sheet> sheets)
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

                    var response = await httpClient.PostAsync("get_icecount", contents);
                    if (response.IsSuccessStatusCode)
                    {
#if DEBUGOFFLINE || DEBUG
                        s = await response.Content.ReadAsStringAsync();
#endif
                        var icesInfoData = await response.Content.ReadFromJsonAsync<IceInfoBody>();
                        return icesInfoData?.Data?.IcesInfo;
                    }
                    else
                    {
                        _logger.LogWarning("Error get ices info with ids: {iceIds}. HttpStatusCode: {httpStatusCode}", string.Join(",", sheets.Select(s => s.Identity)), response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reply ice with ids: {iceIds}.", string.Join(",", sheets.Select(s => s.Identity)));
                }
            }
            return null;
        }
    }

    public interface IIcebreakersClient
    {
        Task<AprovIce> Approved(Sheet sheet, int idLast = 0);

        Task<AprovIceProgress> Progress(Sheet sheet, int idLast = 0);

        Task<AprovIce> Trash(Sheet sheet);

        Task<long> Create(Sheet sheet, IceType iceType, string content);

        Task<bool> Switch(Sheet sheet, long iceId, string status);

        Task<bool> Delete(long iceId);

        Task<bool> Reply(long iceId);

        Task<List<IceInfo>> IcesInfo(List<Sheet> sheets);
    }
}
