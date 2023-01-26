using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiAdminClient : IAdminClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<ApiAdminClient> _logger;

        public object MediaTypeContentValue { get; private set; }

        public ApiAdminClient(IHttpClientFactory httpClientFactory, ILogger<ApiAdminClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<Agency>> GetAgencies()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<List<Agency>>("/Owner/Agencies");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the agencies.");
                return null;
            }
        }

        public async Task<Agency> GetAgencyById(int id)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                Agency agency = await httpClient.GetFromJsonAsync<Agency>($"/Owner/Agency/{id}");
                return agency;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency with Id: {id}.", id);
                return null;
            }
        }

        public async Task<HttpResponseMessage> AddAgency(Agency agency)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            HttpContent content = JsonContent.Create(agency);
            try
            {
                var response = await httpClient.PostAsync("/Owner/AddAgency", content);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding agency {name}.", agency.Name);
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

        public async Task<HttpResponseMessage> DeleteAgency(int id)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.DeleteAsync($"/Owner/DeleteAgency/{id}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting agency with Id: {id}.", id);
                return null;
            }
        }

        public async Task<List<ApplicationUser>> GetFreeUsers()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.GetFromJsonAsync<List<ApplicationUser>>("/FreeUsers");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting free users");
                return null;
            }
        }

        public async Task<List<ApplicationUser>> GetAdmins()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<List<ApplicationUser>>("/Admins");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admins");
                return null;
            }
        }

        public async Task<HttpResponseMessage> DeleteAdmin(int adminId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.DeleteAsync($"/Owner/DeleteAdmin/{adminId}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting admin with Id: {id}.", adminId);
                return null;
            }
        }

        public async Task<HttpResponseMessage> AddAdmin(int id)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            HttpContent content = JsonContent.Create(id);
            try
            {
                var response = await httpClient.PostAsync("/Owner/AddAdmin", content);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding agency {id}.", id);
                return null;
            }
        }
    }

    public interface IAdminClient
    {
        Task<List<Agency>> GetAgencies();
        Task<Agency> GetAgencyById(int id);
        Task<HttpResponseMessage> AddAgency(Agency agency);
        Task<HttpResponseMessage> UpdateAgency(Agency agency);
        Task<HttpResponseMessage> DeleteAgency(int id);
        Task<List<ApplicationUser>> GetFreeUsers();
        Task<List<ApplicationUser>> GetAdmins();
        Task<HttpResponseMessage> DeleteAdmin(int adminId);
        Task<HttpResponseMessage> AddAdmin(int id);
    }
}
