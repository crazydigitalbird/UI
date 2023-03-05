using Core.Models.Agencies;
using Core.Models.Sheets;
using System.Net;
using System.Security.Principal;

namespace UI.Infrastructure.API
{
    public class ApiSiteClient : ISiteClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILogger<ApiSiteClient> _logger;

        public ApiSiteClient(IHttpClientFactory httpClientFactory,IHttpContextAccessor httpContextAccessor, ILogger<ApiSiteClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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
                else if(response.StatusCode == HttpStatusCode.Unauthorized)
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

        public async Task<SheetSite> GetSiteById(int siteId)
        {
            var sessionGuid = GetSessionGuid();
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.GetAsync($"Sheets/GetSite?siteId={siteId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<SheetSite>();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting site with Id: {siteId}. HttpStatsCode: {httpStatusCode}", siteId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting site with Id: {siteId}.", siteId);
            }
            return null;
        }

        public async Task<bool> Add(SheetSite site)
        {
            var sessionGuid = GetSessionGuid();
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.PutAsync($"Sheets/AddSite?siteName={site.Name}&siteDescription={site.Description}&siteConfiguration={site.Configuration}&sessionGuid={sessionGuid}", null);
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
                    _logger.LogWarning("Error adding site '{siteName}'. HttpStatsCode: {httpStatusCode}", site.Name, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding site.");
            }
            return false;
        }

        public async Task<bool> Update(SheetSite site)
        {
            var sessionGuid = GetSessionGuid();
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.PostAsync($"Sheets/UpdateSite?id={site.Id}&siteName={site.Name}&siteDescription={site.Description}&siteConfiguration={site.Configuration}&sessionGuid={sessionGuid}", null);
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
                    _logger.LogWarning("Error updating site '{siteName}'. Id: {siteId}. HttpStatsCode: {httpStatusCode}", site.Name, site.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating site '{siteName}'. Id: {siteId}.", site.Name, site.Id);
            }
            return false;
        }

        public async Task<bool> Delete(int siteId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();
                var response = await httpClient.DeleteAsync($"Sheets/DeleteSite?siteId={siteId}&sessionGuid={sessionGuid}");
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
                    _logger.LogWarning("Error deleting site with Id: {siteId}. HttpStatsCode: {httpStatusCode}", siteId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting site with Id: {siteId}.", siteId);
            }
            return false;
        }

        private object GetSessionGuid()
        {
            var user = _httpContextAccessor.HttpContext.User;
            var sessionGuid = user.FindFirst("SessionGuid")?.Value;
            return sessionGuid;
        }

        public void SignOut()
        {
            _httpContextAccessor.HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
        }
    }

    public interface ISiteClient
    {
        Task<List<SheetSite>> GetSites();
        Task<bool> Add(SheetSite site);
        Task<bool> Update(SheetSite site);
        Task<bool> Delete(int siteId);
        Task<SheetSite> GetSiteById(int siteId);
    }
}
