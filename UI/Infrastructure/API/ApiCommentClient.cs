using Core.Models.Agencies.Comments;
using System.Security.Principal;

namespace UI.Infrastructure.API
{
    public class ApiCommentClient : ICommentClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiCommentClient> _logger;

        public ApiCommentClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiCommentClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<int> GetNewSheetCommentsCountAsync(int sheetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Comments/GetNewSheetCommentsCount?sheetId={sheetId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var count = await response.Content.ReadFromJsonAsync<int>();
                    return count;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error geting new sheet comments for sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}", sheetId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error geting new sheet comments for sheet with id: {sheetId}.", sheetId);
            }
            return 0;
        }

        public async Task<IEnumerable<SheetComment>> GetCommentsAsync(int sheetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Comments/GetSheetComments?sheetId={sheetId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var comments = await response.Content.ReadFromJsonAsync<IEnumerable<SheetComment>>();
                    return comments;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error geting comments for sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}", sheetId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error geting comments for sheet with id: {sheetId}.", sheetId);
            }
            return null;
        }

        public async Task<SheetComment> AddCommentAsync(int sheetId, string text)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.PutAsync($"Agencies/Comments/AddSheetComment?sheetId={sheetId}&content={text}&sessionGuid={sessionGuid}", null);
                if (response.IsSuccessStatusCode)
                {
                    var comment = await response.Content.ReadFromJsonAsync<SheetComment>();
                    return comment;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error creating comment for sheet with id: {sheetId}", sheetId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for sheet with id: {sheetId}", sheetId);
            }
            return null;
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

    public interface ICommentClient
    {
        Task<int> GetNewSheetCommentsCountAsync(int sheetId);
        Task<IEnumerable<SheetComment>> GetCommentsAsync(int sheetId);
        Task<SheetComment> AddCommentAsync(int sheetId, string text);
    }
}
