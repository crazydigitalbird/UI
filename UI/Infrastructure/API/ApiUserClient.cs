using Core.Models.Sheets;
using Newtonsoft.Json;
using System.Net;
using System.Security.Policy;
using System.Security.Principal;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiUserClient : IUserClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILogger<IUserClient> _logger;

        public ApiUserClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<IUserClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IEnumerable<Sheet>> GetSheets()
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

        public async Task<Sheet> AddSheet(int siteId, string login, string password)
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
                    if (sheetInfo != null)
                    {
                        var credentials = WebUtility.UrlEncode(JsonConvert.SerializeObject(new { login = login, password = password }, Formatting.Indented));

                        var info = WebUtility.UrlEncode(JsonConvert.SerializeObject(sheetInfo, Formatting.Indented));

                        var response = await httpClient.PutAsync($"Sheets/AddSheet?siteId={siteId}&credentials={credentials}&info={info}&sessionGuid={sessionGuid}", null);
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
                    var s = await response.Content.ReadAsStringAsync();
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

        public async Task<SheetSite> UpdateSheet(int sheetId, string password)
        {
            HttpClient httpClien = _httpClientFactory.CreateClient("api");
            HttpClient httpClientBot = _httpClientFactory.CreateClient("apiBot");
            var sesstionGuid = GetSessionGuid();
            try
            {
                var sheets = await GetSheets();
                var sheet = sheets?.Where(s => s.Id == sheetId).FirstOrDefault();
                if (sheet != null)
                {
                    var credentialsObject = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);
                    credentialsObject.Password = password;
                    var credentials = WebUtility.UrlEncode(JsonConvert.SerializeObject(credentialsObject, Formatting.Indented));

                    var sheetInfo = await Registrate(httpClientBot, sheet.Site, credentialsObject.Login, credentialsObject.Password);
                    if (sheetInfo != null)
                    {
                        var info = WebUtility.UrlEncode(JsonConvert.SerializeObject(sheetInfo, Formatting.Indented));

                        var response = await httpClien.PostAsync($"Sheets/UpdateSheet?sheedId={sheetId}&credentials={credentials}&info={info}&sessionGuid={sesstionGuid}", null);
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

        public async Task<bool> DeleteSheet(int sheetId)
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

        public async Task<List<SheetSite>> GetSites()
        {
            var sessionGuid = GetSessionGuid();
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.GetAsync($"Sheets/GetSites?sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<SheetSite>>();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting all the sites. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the sites.");
            }
            return null;
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

        public void SignOut()
        {
            _httpContextAccessor.HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
        }

        public Task<bool> ChangePaassword(int sheetId, string password)
        {
            throw new NotImplementedException();
        }
    }

    public interface IUserClient
    {
        Task<Sheet> AddSheet(int siteId, string login, string password);
        Task<SheetSite> UpdateSheet(int sheetId, string password);
        Task<bool> ChangePaassword(int sheetId, string password);
        Task<bool> DeleteSheet(int sheetId);
        Task<IEnumerable<Sheet>> GetSheets();
        Task<List<SheetSite>> GetSites();
    }
}
