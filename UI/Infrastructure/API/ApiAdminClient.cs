using Core.Models.Agencies;
using Core.Models.Users;
using System.Net;
using System.Security.Principal;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiAdminClient : IAdminClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAdminAgencyClient _adminAgencyClient;
        private readonly ILogger<ApiAdminClient> _logger;

        public ApiAdminClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IAdminAgencyClient adminAgencyClient, ILogger<ApiAdminClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _adminAgencyClient = adminAgencyClient;
            _logger = logger;
        }

        public async Task<List<Agency>> GetAgencies()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();
                var response = await httpClient.GetAsync($"Agencies/GetAgencies?sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var agencys = (await response.Content.ReadFromJsonAsync<List<Agency>>()).Where(a => a.IsActive).ToList();
                    return agencys;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting all the agencies. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the agencies.");
            }
            return null;
        }

        public async Task<bool> AddAgency(AgencyView agency)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();
                var response = await httpClient.PutAsync($"Agencies/AddAgency?agencyName={agency.Name}&agencyDescription={agency.Description}&sessionGuid={sessionGuid}", null);
                if (response.IsSuccessStatusCode)
                {
                    var addedAgency = await response.Content.ReadFromJsonAsync<Agency>();
                    if (agency.Users != null)
                    {
                        foreach (var user in agency.Users)
                        {
                            await _adminAgencyClient.AddUserAgency(httpClient, sessionGuid, addedAgency.Id, user);
                        }
                    }
                    return true;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error adding agency '{agencyName}'. HttpStatsCode: {httpStatusCode}", agency.Name, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding agency '{agencyName}'.", agency.Name);
            }
            return false;
        }

        public async Task<bool> DeleteAgency(int agencyId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();
                var response = await httpClient.DeleteAsync($"Agencies/DeleteAgency?agencyId={agencyId}&sessionGuid={sessionGuid}");
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
                    _logger.LogWarning("Error deleting agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", agencyId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting agency with Id: {agencyId}.", agencyId);
            }
            return false;
        }

        public async Task<List<UserAdmin>> GetAdmins()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Users/GetAdmins?sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var admins = await response.Content.ReadFromJsonAsync<List<UserAdmin>>();
                    return admins;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting admins. HttpStatusCode {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admins.");
            }
            return null;
        }

        public async Task<List<User>> GetUsers()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Users/GetUsers?sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<User>>();
                    return users;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting users. HttpStatusCode {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users.");
            }
            return null;
        }

        public async Task<bool> DeleteAdmin(int adminId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.DeleteAsync($"Users/DeleteAdmin?userAdminId={adminId}&sessionGuid={sessionGuid}");
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
                    _logger.LogWarning("Error deleting admin with Id: {id}. HttpStatusCode: {httpStatusCode}", adminId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting admin with Id: {id}.", adminId);
            }
            return false;
        }

        public async Task<bool> AddAdmin(int userId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.PostAsync($"Users/AddAdmin?userId={userId}&sessionGuid={sessionGuid}", null);
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
                    _logger.LogWarning("Error adding agency {id}. HttpStatusCode: {httpStatusCode}", userId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding agency {id}.", userId);
            }
            return false;
        }

        private string GetSessionGuid()
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

    public interface IAdminClient
    {
        Task<List<Agency>> GetAgencies();
        Task<bool> AddAgency(AgencyView agency);
        Task<bool> DeleteAgency(int agencyId);

        Task<List<UserAdmin>> GetAdmins();
        Task<List<User>> GetUsers();
        Task<bool> DeleteAdmin(int adminId);
        Task<bool> AddAdmin(int agencyId);
    }
}
