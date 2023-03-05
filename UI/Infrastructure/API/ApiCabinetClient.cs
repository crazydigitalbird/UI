using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiCabinetClient : ICabinetClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ICabinetClient> _logger;

        public ApiCabinetClient(IHttpClientFactory httpClientFactory, ILogger<ICabinetClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Cabinet>> GetCabinetsAsync()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<Cabinet>>("/Cabinets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the cabinets.");
                return null;
            }
        }

        public async Task<HttpResponseMessage> AddAsync(string name)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.PostAsJsonAsync("/AddCabinet", name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the {nameCabinet} cabinet.", name);
                return null;
            }
        }

        public async Task<HttpResponseMessage> BindCabinetToUserAsync(List<int> cabinets, int userId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.PutAsJsonAsync("/BindCabinet", new { cabinets = cabinets, userId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error binding/unbinding the cabinets to the user with id {userId}.", userId);
                return null;
            }
        }

        public async Task<HttpResponseMessage> UnbindCabinetToUserAsync(int cabinetId, int userId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.PutAsJsonAsync("/UnbindCabinet", new { cabinetId = cabinetId, userId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbinding the cabinet with id {cabinetId} to the user with id {userId}.", cabinetId, userId);
                return null;
            }
        }
    }

    public interface ICabinetClient
    {
        Task<IEnumerable<Cabinet>> GetCabinetsAsync();

        Task<HttpResponseMessage> AddAsync(string name);

        Task<HttpResponseMessage> BindCabinetToUserAsync(List<int> cabinets, int userId);

        Task<HttpResponseMessage> UnbindCabinetToUserAsync(int cabinetId, int userId);
    }
}
