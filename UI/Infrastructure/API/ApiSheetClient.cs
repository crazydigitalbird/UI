using Core.Models.Sheets;
using Newtonsoft.Json;
using System.Net;
using System.Security.Principal;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiSheetClient : ISheetClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiSheetClient> _logger;

        public ApiSheetClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiSheetClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Sheet> GetSheetAsync(int sheetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Sheets/GetSheet?sheedId={sheetId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var sheet = await response.Content.ReadFromJsonAsync<Sheet>();
                    return sheet;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting sheet with id: {sheetId}. HttpStatusCode {httpStatusCode}", sheetId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheet with id: {sheetId}.", sheetId);
            }
            return null;
        }

        //Only Role.User
        public async Task<IEnumerable<Sheet>> GetSheetsAsync()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();
                var userId = GetUserId();
                var response = await httpClient.GetAsync($"Sheets/GetSheets?userId={userId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var sheets = (await response.Content.ReadFromJsonAsync<IEnumerable<Sheet>>()).Where(s => s.IsActive);
                    return sheets;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
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

        public async Task GettingStatusAndMedia(List<SheetView> sheetsView)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                string ids = string.Join(",", sheetsView.Select(sw => sw.SheetId));
                httpClient.DefaultRequestHeaders.Add("ids", ids);
                var response = await httpClient.PostAsync("status_online", null);
                if (response.IsSuccessStatusCode)
                {
                    var dictionary = await response.Content.ReadFromJsonAsync<Dictionary<int, SheetStatusAndMedia>>();
                    foreach (var sam in dictionary)
                    {
                        var sheetView = sheetsView.FirstOrDefault(sw => sw.SheetId == sam.Key);
                        if (sheetView != null)
                        {
                            sheetView.Photo = sam.Value.Photo;
                            sheetView.PrivatePhoto = sam.Value.PrivatePhoto;
                            sheetView.Video = sam.Value.Video;
                            sheetView.Status = sam.Value.Status == 1 ? Status.Online : Status.Offline;
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

        public async Task GetSheetAgencyOperatorSessionsCount(List<SheetView> sheetsView)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                Dictionary<int, int> taskIdToSheetId = new();
                var tasks = sheetsView.Select(s =>
                {
                    var task = httpClient.GetAsync($"Agencies/Operators/GetSheetAgencyOperatorSessionsCount?sheetId={s.Id}&sessionGuid={sessionGuid}");
                    taskIdToSheetId.Add(task.Id, s.Id);
                    return task;
                });
                await Task.WhenAll(tasks);
                foreach (var task in tasks)
                {
                    if (task.Result?.IsSuccessStatusCode ?? false)
                    {
                        var response = task.Result;
                        var sheetId = taskIdToSheetId[task.Id];
                        sheetsView.FirstOrDefault(s => s.Id == sheetId).Operators = await response.Content.ReadFromJsonAsync<int>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheets with id: {sheetId}.", string.Join(';', sheetsView));
            }
        }

        public async Task GetSheetsAgencyOperatorSessionsCount(List<SheetView> sheetsView)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var ids = sheetsView.Select(s => s.Id).ToArray();
                var content = JsonContent.Create(ids);
                var response = await httpClient.PostAsync($"Agencies/Operators/GetSheetsAgencyOperatorSessionsCount?sessionGuid={sessionGuid}", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUG || DEBUGOFFLINE
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var sheetsOperatorSessions = await response.Content.ReadFromJsonAsync<List<SheetOperatorSessions>>();
                    sheetsView.ForEach(s =>
                    {
                        var sheetOperatorSessions = sheetsOperatorSessions.FirstOrDefault(sos => sos.SheetId == s.Id);
                        s.Operators = sheetOperatorSessions?.Count ?? 0;
                    });
                }
                else
                {
                    _logger.LogWarning("Error getting sheets agency operator sessions count. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheets agency operator sessions count.");
            }
        }

        public async Task<Sheet> AddAsync(int siteId, string login, string password)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            HttpClient httpClientBot = _httpClientFactory.CreateClient("apiBot");
            var sessionGuid = GetSessionGuid();
            try
            {
                var site = await GetSite(siteId, httpClient, sessionGuid);
                if (site != null)
                {
                    var sheetInfo = await Registrate(httpClientBot, site, login, password);
                    //sheetInfo = new SheetInfo { Id = 1111111, Personal = new Personal { Avatar = "/image/avatar.webp", AvatarSmall = "/image/avatar.webp", Name = "Diana" } };
                    if (sheetInfo != null)
                    {
                        var credentials = WebUtility.UrlEncode(JsonConvert.SerializeObject(new { login, password }, Formatting.Indented));

                        var info = WebUtility.UrlEncode(JsonConvert.SerializeObject(sheetInfo, Formatting.Indented));

                        var response = await httpClient.PutAsync($"Sheets/AddSheet?siteId={siteId}&credentials={credentials}&identity={sheetInfo.Id}&info={info}&sessionGuid={sessionGuid}", null);
                        if (response.IsSuccessStatusCode)
                        {
                            var sheet = await response.Content.ReadFromJsonAsync<Sheet>();
                            return sheet;
                        }
                        else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            SignOut();
                        }
                        else
                        {
                            _logger.LogWarning("Error adding sheet login {login}, site with Id '{siteId}'. HttpStatsCode: {httpStatusCode}", login, siteId, response.StatusCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding sheet login {login}, site with Id '{siteId}'.", login, siteId);
            }
            return null;
        }

        public async Task<SheetSite> UpdateAsync(int sheetId, string password)
        {
            HttpClient httpClien = _httpClientFactory.CreateClient("api");
            HttpClient httpClientBot = _httpClientFactory.CreateClient("apiBot");
            var sesstionGuid = GetSessionGuid();
            try
            {
                var sheet = await GetSheetAsync(sheetId);
                if (sheet != null)
                {
                    var credentialsObject = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                    credentialsObject.Password = password;
                    var credentials = WebUtility.UrlEncode(JsonConvert.SerializeObject(credentialsObject, Formatting.Indented));

                    var sheetInfo = await Registrate(httpClientBot, sheet.Site, credentialsObject.Login, credentialsObject.Password);
                    if (sheetInfo != null)
                    {
                        var info = WebUtility.UrlEncode(JsonConvert.SerializeObject(sheetInfo, Formatting.Indented));

                        var response = await httpClien.PostAsync($"Sheets/UpdateSheet?sheedId={sheetId}&credentials={credentials}&identity={sheetInfo.Id}&info={info}&isActive=true&sessionGuid={sesstionGuid}", null);
                        if (response.IsSuccessStatusCode)
                        {
                            var sheetUpdated = await response.Content.ReadFromJsonAsync<SheetSite>();
                            return sheetUpdated;
                        }
                        else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            SignOut();
                        }
                        else
                        {
                            _logger.LogWarning("Error updating sheet with Id '{sheetId}'. HttpStatsCode: {httpStatusCode}", sheetId, response.StatusCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding sheet with Id '{sheetId}'.", sheetId);
            }
            return null;
        }

        public async Task<bool> DeleteAsync(int sheetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.DeleteAsync($"Sheets/DeleteSheet?sheedId={sheetId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error deleting sheet with Id: {sheetId}. HttpStatsCode: {httpStatusCode}", sheetId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sheet with Id: {sheetId}.", sheetId);
            }
            return false;
        }

        public Task<bool> ChangePaassword(int sheetId, string password)
        {
            throw new NotImplementedException();
        }

        private string GetSessionGuid()
        {
            var user = _httpContextAccessor.HttpContext.User;
            var sessionGuid = user.FindFirst("SessionGuid")?.Value;
            return sessionGuid;
        }

        private int GetUserId()
        {
            var user = _httpContextAccessor.HttpContext.User;
            var userId = int.Parse(user.FindFirst("Id")?.Value);
            return userId;
        }

        private async Task<SheetSite> GetSite(int siteId, HttpClient httpClient, string sessionGuid)
        {
            try
            {
                var response = await httpClient.GetAsync($"Sheets/GetSite?siteId={siteId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var site = await response.Content.ReadFromJsonAsync<SheetSite>();
                    return site;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error get site with Id '{siteId}'. HttpStatsCode: {httpStatusCode}", siteId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get site with Id '{siteId}'.", siteId);
            }
            return null;
        }

        private async Task<SheetInfo> Registrate(HttpClient httpClient, SheetSite site, string email, string password)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Add("site", site.Configuration);
                httpClient.DefaultRequestHeaders.Add("email", email);
                httpClient.DefaultRequestHeaders.Add("password", password);

                var response = await httpClient.PostAsync($"registrate", null);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var sheetInfo = await response.Content.ReadFromJsonAsync<SheetInfo>();
                    return sheetInfo;
                }
                _logger.LogWarning("Error registrate sheet email {email} password {password}, site with Id '{siteId}'. HttpStatusCode: {httpStatusCode}", email, password, site.Id, response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrate sheet email {email} password {password}, site with Id '{siteId}'.", email, password, site.Id);
            }
            return null;
        }

        public void SignOut()
        {
            _httpContextAccessor.HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
        }
    }

    public interface ISheetClient
    {
        Task<Sheet> GetSheetAsync(int sheetId);

        Task<IEnumerable<Sheet>> GetSheetsAsync();

        Task GettingStatusAndMedia(List<SheetView> sheetsView);

        Task GetSheetAgencyOperatorSessionsCount(List<SheetView> sheetsView);

        Task GetSheetsAgencyOperatorSessionsCount(List<SheetView> sheetsView);

        Task<Sheet> AddAsync(int siteId, string login, string password);

        Task<SheetSite> UpdateAsync(int sheetId, string password);

        Task<bool> DeleteAsync(int sheetId);

        Task<bool> ChangePaassword(int sheetId, string password);
    }
}
