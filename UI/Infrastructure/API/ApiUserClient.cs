using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiUserClient : IUserClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<IUserClient> _logger;

        public ApiUserClient(IHttpClientFactory httpClientFactory, ILogger<IUserClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Profile>> GetProfiles()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                return await httpClient.GetFromJsonAsync<IEnumerable<Profile>>("/SuperAdmin/Profiles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the profiles.");
                return null;
            }
        }

        public async Task<HttpResponseMessage> AddProfile(string email, string password)
        {
            HttpClient httpClien = _httpClientFactory.CreateClient("api");
            try
            {
               return await httpClien.PostAsJsonAsync("/User/AddProfile", new { email = email, password = password });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding a {email} profile.");
                return null;
            }
        }
    }

    public interface IUserClient
    {
        Task<HttpResponseMessage> AddProfile(string email, string password);
        Task<IEnumerable<Profile>> GetProfiles();
    }
}
