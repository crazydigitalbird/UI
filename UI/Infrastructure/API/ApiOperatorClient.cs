using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiOperatorClient : IOperatorClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiOperatorClient> _logger;

        public ApiOperatorClient(IHttpClientFactory httpClientFactory, ILogger<ApiOperatorClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Profile>> GetProfilesAsync()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<Profile>>("/Operator/Profiles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the profiles.");
                return null;
            }
        }

        public async Task<Dictionary<int, int>> GetBalanceAsync(string name, Interval interval)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var url = $"/Operator/Balance?operator={name}&interval={interval}";
                return await httpClient.GetFromJsonAsync<Dictionary<int, int>>(url);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error getting balance");
                return null;
            }
        }

        public async Task<List<Note>> Notes(string name, int profileId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<List<Note>>($"/Operator/Notes?operator={name}&&profileId={profileId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving notes for profile with id: {profileId}. Operator {operatro}", profileId, name);
                return null;
            }
        }

        public async Task<HttpResponseMessage> CreateNoteAsync(string name, int profileId, string text)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.PostAsJsonAsync("/Operator/CreateNote", new { profileId = profileId, text = text});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note for profile with id: {profileId}", profileId);
                return null;
            }
        }
    }

    public interface IOperatorClient
    {
        Task<Dictionary<int, int>> GetBalanceAsync(string name, Interval interval);
        Task<List<Note>> Notes(string name, int profileId);
        Task<IEnumerable<Profile>> GetProfilesAsync();
        Task<HttpResponseMessage> CreateNoteAsync(string name, int profileId, string text);
    }
}
