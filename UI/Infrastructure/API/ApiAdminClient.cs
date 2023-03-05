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

        private readonly ILogger<ApiAdminClient> _logger;

        public ApiAdminClient(IHttpClientFactory httpClientFactory, ILogger<ApiAdminClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<AgencyView> GetAgencyById(int agencyId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();

                var response = await httpClient.GetAsync($"Agencies/GetAgency?agencyId={agencyId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    AgencyView agency = await response.Content.ReadFromJsonAsync<AgencyView>();

                    var responseMembers = await httpClient.GetAsync($"Agencies/GetAgencyMembers?agencyId={agencyId}&sessionGuid={sessionGuid}");
                    if (responseMembers.IsSuccessStatusCode)
                    {
                        var agencyMembers = await responseMembers.Content.ReadFromJsonAsync<List<AgencyMember>>();
                        agency.Users = agencyMembers.Select(am => (ApplicationUser)am.User).ToList();

                        return agency;
                    }
                    else if (responseMembers.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        SignOut();
                    }
                    else
                    {
                        _logger.LogWarning("Error getting agencymembers with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", agencyId, responseMembers.StatusCode);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", agencyId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency with Id: {agencyId}.", agencyId);
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
                            await AddUserAgency(httpClient, sessionGuid, addedAgency.Id, user);
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

        private async Task AddUserAgency(HttpClient httpClient, string sessionGuid, int agencyId, ApplicationUser user)
        {
            var responseMember = await httpClient.PostAsync($"Agencies/AddAgencyMember?agencyId={agencyId}&userId={user.Id}&sessionGuid={sessionGuid}", null);
            if (responseMember.IsSuccessStatusCode)
            {
                if (user.Role == Role.User)
                {
                    return;
                }

                var newMember = await responseMember.Content.ReadFromJsonAsync<AgencyMember>();

                if (user.Role == Role.AdminAgency)
                {
                    await SetAdminAgency(httpClient, sessionGuid, agencyId, newMember.Id);
                }
                else if (user.Role == Role.Operator)
                {
                    await SetOperatorAgency(httpClient, sessionGuid, agencyId, newMember.Id);
                }
            }
            else if (responseMember.StatusCode == HttpStatusCode.Unauthorized)
            {
                SignOut();
            }
            else
            {
                _logger.LogWarning("Error adding user {email} in agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", user.Email, agencyId, responseMember.StatusCode);
            }
        }

        private async Task SetAdminAgency(HttpClient httpClient, string sessionGuid, int agencyId, int memberId)
        {
            var response = await httpClient.PostAsync($"Agencies/AddAgencyAdmin?agencyId={agencyId}&memberId={memberId}&sessionGuid={sessionGuid}", null);
            if (response.IsSuccessStatusCode)
            {
                return;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                SignOut();
            }
            else
            {
                _logger.LogWarning("Error installing the admin role, for a user with memberId: {memberId}, in agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", memberId, agencyId, response.StatusCode);
            }
        }

        private async Task SetOperatorAgency(HttpClient httpClient, string sessionGuid, int agencyId, int memberId)
        {
            var response = await httpClient.PutAsync($"Agencies/Operators/AddAgencyOperator?agencyId={agencyId}&memberId={memberId}&sessionGuid={sessionGuid}", null);
            if (response.IsSuccessStatusCode)
            {
                return;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                SignOut();
            }
            else
            {
                _logger.LogWarning("Error installing the operator role, for a user with memberId: {memberId}, in agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", memberId, agencyId, response.StatusCode);
            }
        }

        public async Task<bool> UpdateAgency(AgencyView agency, List<ApplicationUser> originalUsers)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();
                var response = await httpClient.PostAsync($"Agencies/UpdateAgency?agencyId={agency.Id}&agencyName={agency.Name}&agencyDescription={agency.Description}&isActive=true&sessionGuid={sessionGuid}", null);
                if (response.IsSuccessStatusCode)
                {
                    var deleteUsers = originalUsers.ExceptBy(agency.Users.Select(u => u.Id), ou => ou.Id).ToList();
                    foreach (var user in deleteUsers)
                    {
                        await DeleteUserFromAgency(httpClient, sessionGuid, agency.Id, user);
                    }

                    var addUsers = agency.Users.ExceptBy(originalUsers.Select(ou => ou.Id), u => u.Id).ToList();
                    foreach (var user in addUsers)
                    {
                        await AddUserAgency(httpClient, sessionGuid, agency.Id, user);
                    }

                    var updateUser = agency.Users.Intersect(originalUsers, new MyApplicationUserIsUpdateRoleComparer()).ToList();
                    foreach (var user in updateUser)
                    {
                        await UpdateUserAgency(httpClient, sessionGuid, agency.Id, user);
                    }

                    return true;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error updating agency '{agencyName}'. Id: {agencyId}. HttpStatsCode: {httpStatusCode}", agency.Name, agency.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agency '{agencyName}'. Id: {agencyId}.", agency.Name, agency.Id);
            }
            return false;
        }

        private async Task UpdateUserAgency(HttpClient httpClient, string sessionGuid, int agencyId, ApplicationUser user)
        {
            await DeleteUserFromAgency(httpClient, sessionGuid, agencyId, user);
            await AddUserAgency(httpClient, sessionGuid, agencyId, user);
        }

        private async Task DeleteUserFromAgency(HttpClient httpClient, string sessionGuid, int agencyId, ApplicationUser user)
        {
            var responseMember = await httpClient.GetAsync($"Agencies/GetAgencyMemberByUser?userId={user.Id}&sessionGuid={sessionGuid}");
            if (responseMember.IsSuccessStatusCode)
            {
                var member = await responseMember.Content.ReadFromJsonAsync<AgencyMember>();
                var response = await httpClient.DeleteAsync($"Agencies/DeleteAgencyMember?agencyId={agencyId}&memberId={member.Id}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error deleting a user {email} from the agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", user.Email, agencyId, responseMember.StatusCode);
                }
            }
            else if (responseMember.StatusCode == HttpStatusCode.Unauthorized)
            {
                SignOut();
            }
            else
            {
                _logger.LogWarning("Error getting the memberId for the user {email} in agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", user.Email, agencyId, responseMember.StatusCode);
            }
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

        public async Task<List<ApplicationUser>> GetNonAgencyUsers()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();
                var response = await httpClient.PostAsync($"Agencies/GetNonAgencyUsers?sessionGuid={sessionGuid}", null);

                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<User>>();
                    var appUsers = users.Select(u => (ApplicationUser)u).ToList();
                    return appUsers;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting non agency users");
            }
            return null;
        }

        public async Task<List<ApplicationUser>> GetAdmins()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.GetAsync("/Admins");
                if (response.IsSuccessStatusCode)
                {
                    var admins = await response.Content.ReadFromJsonAsync<List<ApplicationUser>>();
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
                _logger.LogError(ex, "Error getting admins");
            }
            return null;
        }

        public async Task<bool> DeleteAdmin(int adminId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.DeleteAsync($"/Owner/DeleteAdmin/{adminId}");
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

        public async Task<bool> AddAdmin(int adminId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.PostAsync("/Owner/AddAdmin", null);
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
                    _logger.LogWarning("Error adding agency {id}. HttpStatusCode: {httpStatusCode}", adminId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding agency {id}.", adminId);
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
        Task<AgencyView> GetAgencyById(int agencyId);
        Task<bool> AddAgency(AgencyView agency);
        Task<bool> UpdateAgency(AgencyView agency, List<ApplicationUser> originalUsers);
        Task<bool> DeleteAgency(int agencyId);
        Task<List<ApplicationUser>> GetNonAgencyUsers();

        Task<List<ApplicationUser>> GetAdmins();
        Task<bool> DeleteAdmin(int adminId);
        Task<bool> AddAdmin(int agencyId);
    }
}
