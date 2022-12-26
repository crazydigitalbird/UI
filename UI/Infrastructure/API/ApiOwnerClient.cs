using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiOwnerClient : IOwnerClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<ApiOwnerClient> _logger;

        public object MediaTypeContentValue { get; private set; }

        public ApiOwnerClient(IHttpClientFactory httpClientFactory, ILogger<ApiOwnerClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<Agency>> GetAgencies()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("ownerapi");
            try
            {
                return await httpClient.GetFromJsonAsync<List<Agency>>("Agencies");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the agencies.");
                return null;
            }
        }

        public async Task<Agency> GetAgencyById(int id)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("ownerapi");
            try
            {
                Agency agency = await httpClient.GetFromJsonAsync<Agency>($"Agency/{id}");
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
            HttpClient httpClient = _httpClientFactory.CreateClient("ownerapi");
            HttpContent content = JsonContent.Create(agency);
            try
            {
                var response = await httpClient.PostAsync("AddAgency", content);
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
            HttpClient httpClient = _httpClientFactory.CreateClient("ownerapi");
            HttpContent content = JsonContent.Create(agency);
            try
            {
                var response = await httpClient.PatchAsync("UpdateAgency", content);
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
            HttpClient httpClient = _httpClientFactory.CreateClient("ownerapi");
            try
            {
                var response = await httpClient.DeleteAsync($"DeleteAgency/{id}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting agency with Id: {id}.", id);
                return null;
            }
        }
    }

    public interface IOwnerClient
    {
        Task<List<Agency>> GetAgencies();
        Task<Agency> GetAgencyById(int id);
        Task<HttpResponseMessage> AddAgency(Agency agency);
        Task<HttpResponseMessage> UpdateAgency(Agency agency);
        Task<HttpResponseMessage> DeleteAgency(int id);
    }
}
