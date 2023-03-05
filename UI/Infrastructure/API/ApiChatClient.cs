using System.Net;
using System.Security.Principal;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiChatClient : IChatClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiChatClient> _logger;

        public ApiChatClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiChatClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Messanger> GetMessangerAsync()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.GetAsync("/Chats");
                if (response.IsSuccessStatusCode)
                {
                    var messanger = await response.Content.ReadFromJsonAsync<Messanger>();
                    return messanger;
                }
                else if(response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error receiving chats. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error receiving chats.");
            }
            return null;
        }

        public void SignOut()
        {
            _httpContextAccessor.HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
        }
    }

    public interface IChatClient
    {
        Task<Messanger> GetMessangerAsync();
    }
}
