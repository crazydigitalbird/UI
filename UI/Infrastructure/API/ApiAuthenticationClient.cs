using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiAuthenticationClient : IAuthenticationClient
    {
        IHttpClientFactory _httpClientFactory;
        ILogger<ApiAuthenticationClient> _logger;

        public ApiAuthenticationClient(IHttpClientFactory httpClientFactory, ILogger<ApiAuthenticationClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ApplicationUser> LogIn(string login, string hashPassowrd)
        {
            var client = _httpClientFactory.CreateClient("api");
            try
            {
                return await client.GetFromJsonAsync<ApplicationUser>($"/Authentication/?login={login}&hash={hashPassowrd}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "There was an error getting out forecast: {ErrorMessage}. Login: {Login}", ex.Message, login);
                return null;
            }
        }
    }

    public interface IAuthenticationClient
    {
        Task<ApplicationUser> LogIn(string login, string hashPassword);
    }
}
