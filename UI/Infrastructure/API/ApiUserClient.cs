using Core.Models.Sheets;
using System.Net;
using System.Security.Principal;
using UI.Models;
using Newtonsoft.Json;

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
                var sesstionGuid = GetSessionGuid();
                var userId = GetUserId();
                var response = await httpClient.GetAsync($"Sheets/GetSheets?userId={userId}&sessionGuid={sesstionGuid}");
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
            HttpClient httpClien = _httpClientFactory.CreateClient("api");
            var sesstionGuid = GetSessionGuid();
            try
            {
                var credentials = JsonConvert.SerializeObject(new { login = login, password = password }, Formatting.Indented);

                var info = JsonConvert.SerializeObject(new { name = "Anna", lastName = "Sokolova", avatar= "~/image/avatar.webp" }, Formatting.Indented);

                var response = await httpClien.PutAsync($"Sheets/AddSheet?siteId={siteId}&credentials={credentials}&info={info}&sessionGuid={sesstionGuid}", null);
                if (response.IsSuccessStatusCode)
                {
                    var sheet = await response.Content.ReadFromJsonAsync<Sheet>();
                    return sheet;
                }
                else if(response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error adding sheet login {login}, site with Id '{siteId}'. HttpStatsCode: {httpStatusCode}", login, siteId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding sheet login {login}, site with Id '{siteId}'.", login, siteId);
            }
            return null;
        }

        public async Task<Sheet> UpdateSheet(int sheetId, string login, string password)
        {
            HttpClient httpClien = _httpClientFactory.CreateClient("api");
            var sesstionGuid = GetSessionGuid();
            try
            {
                var credentials = JsonConvert.SerializeObject(new { login = login, password = password }, Formatting.Indented);

                var info = JsonConvert.SerializeObject(new { name = "Anna", lastName = "Sokolova", avatar = "~/image/avatar.webp" }, Formatting.Indented);

                var response = await httpClien.PostAsync($"Sheets/UpdateSheet?sheedId={sheetId}&credentials={credentials}&info={info}&sessionGuid={sesstionGuid}", null);
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
                    _logger.LogWarning("Error updating sheet with Id '{sheetId}', login {login}. HttpStatsCode: {httpStatusCode}", sheetId, login, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding sheet with Id '{sheetId}', login {login}.", sheetId, login);
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

    }

    public interface IUserClient
    {
        Task<Sheet> AddSheet(int siteId, string login, string password);
        Task<bool> DeleteSheet(int sheetId);
        Task<IEnumerable<Sheet>> GetSheets();
        Task<List<SheetSite>> GetSites();
    }
}
