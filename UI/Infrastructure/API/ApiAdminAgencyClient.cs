using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiAdminAgencyClient : IAdminAgencyClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<ApiAdminAgencyClient> _logger;

        public ApiAdminAgencyClient(IHttpClientFactory httpClientFactory, ILogger<ApiAdminAgencyClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Profile>> GetProfiles()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<Profile>>("/Superadmin/Profiles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the profiles.");
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetGroups()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<string>>("/Superadmin/Groups");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the groups.");
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetShifts()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<string>>("/Superadmin/Shifts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the shifts.");
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetCabinets()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<string>>("/Superadmin/Cabinets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the cabinets.");
                return null;
            }
        }

        public async Task<HttpResponseMessage> DeleteProfiles(int[] profilesId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.PostAsJsonAsync<IEnumerable<int>>("/Superadmin/DeleteProfiles", profilesId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {profiles} profiles.", profilesId?.Length);
                return null;
            }
        }

        public async Task<HttpResponseMessage> AddGroup(string nameGroup)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.PostAsJsonAsync<string>("/Superadmin/AddGroup", nameGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the {nameGroup} group.", nameGroup);
                return null;
            }
        }

        public async Task<HttpResponseMessage> ChangeGroup(int profileId, int groupId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.PostAsJsonAsync("/Superadmin/ChangeGroup", new { profileId = profileId, groupId = groupId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "For profile id: {profileId}, the group {groupId} has not been changed.", profileId, groupId);
                return null;
            }
        }

        public async Task<HttpResponseMessage> ChangeShift(int profileId, int shiftId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.PostAsJsonAsync("/Superadmin/ChangeShift", new { profileId = profileId, shiftId = shiftId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "For profile id: {profileId}, the shift {shiftId} has not been changed.", profileId, shiftId);
                return null;
            }
        }

        public async Task<HttpResponseMessage> ChangeCabinet(int profileId, int cabinetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.PostAsJsonAsync("/Superadmin/ChangeCabinet", new { profileId = profileId, cabinetId = cabinetId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "For profile id: {profileId}, the cabinet {cabinetId} has not been changed.", profileId, cabinetId);
                return null;
            }
        }

        public async Task<IEnumerable<Operator>> GetOperatorsProfile(int profileId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<Operator>>($"/Superadmin/GetOperatorsProfile?profileId={profileId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile: {profileId} operators .", profileId);
                return null;
            }
        }

        public async Task<HttpResponseMessage> DeleteOperatorFromProfile(int operatorId, int profileId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.PostAsJsonAsync<int>("/Superadmin/DeleteOperatorFromProfile", profileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "For profile id: {profileId}, the operator id: {operatorId} has not been deleted.", profileId, operatorId);
                return null;
            }
        }

        public async Task<HttpResponseMessage> AddOperatorFromProfile(int operatorId, int profileId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.PostAsJsonAsync("/Superadmin/AddOperatorFromProfile", new { operatorId = operatorId, profileId = profileId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return null;
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetAgencyUsers(int agencyId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<ApplicationUser>>($"/FreeUsers?agencyId={agencyId}");
            }
            catch (Exception ex)
            {
                _logger.LogError( ex, $"Error get users agency by Id: {agencyId}", agencyId);
                return null;
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetFreeUsers()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<ApplicationUser>>("/FreeUsers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error get free users");
                return null;
            }
        }

        public async Task<HttpResponseMessage> UpdateAgency(Agency agency)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            HttpContent content = JsonContent.Create(agency);
            try
            {
                var response = await httpClient.PatchAsync("/Owner/UpdateAgency", content);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agency {name}. Id: {id}.", agency.Name, agency.Id);
                return null;
            }
        }

        public async Task<Agency> GetAgency()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                Agency agency = await httpClient.GetFromJsonAsync<Agency>($"/Owner/Agency/1");
                return agency;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency .");
                return null;
            }
        }
    }

    public interface IAdminAgencyClient
    {
        Task<IEnumerable<Profile>> GetProfiles();
        Task<IEnumerable<string>> GetGroups();
        Task<IEnumerable<string>> GetShifts();
        Task<IEnumerable<string>> GetCabinets();
        Task<HttpResponseMessage> DeleteProfiles(int[] profilesId);
        Task<HttpResponseMessage> AddGroup(string nameGroup);
        Task<HttpResponseMessage> ChangeGroup(int profileId, int groupId);
        Task<HttpResponseMessage> ChangeShift(int profileId, int shiftId);
        Task<HttpResponseMessage> ChangeCabinet(int profileId, int cabinetId);
        Task<IEnumerable<Operator>> GetOperatorsProfile(int profileId);
        Task<HttpResponseMessage> DeleteOperatorFromProfile(int operatorId, int profileId);
        Task<HttpResponseMessage> AddOperatorFromProfile(int operatorId, int profileId);
        Task<IEnumerable<ApplicationUser>> GetAgencyUsers(int agensyId);
        Task<IEnumerable<ApplicationUser>> GetFreeUsers();
        Task<HttpResponseMessage> UpdateAgency(Agency agency);
        Task<Agency> GetAgency();
    }
}
