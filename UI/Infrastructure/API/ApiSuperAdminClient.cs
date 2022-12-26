using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiSuperAdminClient : ISuperAdminClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<ApiSuperAdminClient> _logger;

        public ApiSuperAdminClient(IHttpClientFactory httpClientFactory, ILogger<ApiSuperAdminClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Profile>> GetProfiles()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("superadminapi");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<Profile>>("Profiles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the profiles.");
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetGroups()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("superadminapi");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<string>>("Groups");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the groups.");
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetShifts()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("superadminapi");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<string>>("Shifts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the shifts.");
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetCabinets()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("superadminapi");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<string>>("Cabinets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the cabinets.");
                return null;
            }
        }

        public async Task<HttpResponseMessage> DeleteProfiles(int[] profilesId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("superadminapi");
            try
            {
                return await httpClient.PostAsJsonAsync<IEnumerable<int>>("DeleteProfiles", profilesId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {profiles} profiles.", profilesId?.Length);
                return null;
            }
        }

        public async Task<HttpResponseMessage> AddGroup(string nameGroup)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("superadminapi");
            try
            {
                return await httpClient.PostAsJsonAsync<string>("AddGroup", nameGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the {nameGroup} group.", nameGroup);
                return null;
            }
        }

        public async Task<HttpResponseMessage> ChangeGroup(int profileId, int groupId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("superadminapi");
            try
            {
                return await httpClient.PostAsJsonAsync("ChangeGroup", new { profileId = profileId, groupId = groupId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "For profile id: {profileId}, the group {groupId} has not been changed.", profileId, groupId);
                return null;
            }
        }

        public async Task<HttpResponseMessage> ChangeShift(int profileId, int shiftId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("superadminapi");
            try
            {
                return await httpClient.PostAsJsonAsync("ChangeShift", new { profileId = profileId, shiftId = shiftId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "For profile id: {profileId}, the shift {shiftId} has not been changed.", profileId, shiftId);
                return null;
            }
        }

        public async Task<HttpResponseMessage> ChangeCabinet(int profileId, int cabinetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("superadminapi");
            try
            {
                return await httpClient.PostAsJsonAsync("ChangeCabinet", new { profileId = profileId, cabinetId = cabinetId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "For profile id: {profileId}, the cabinet {cabinetId} has not been changed.", profileId, cabinetId);
                return null;
            }
        }
    }

    public interface ISuperAdminClient
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
    }
}
