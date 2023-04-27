using Core.Models.Agencies;
using Core.Models.Agencies.Operators;
using Core.Models.Sheets;
using System.Security.Principal;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiOperatorClient : IOperatorClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiOperatorClient> _logger;

        public ApiOperatorClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiOperatorClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<SheetView>> GetSheetsViewAsync()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            var operatorId = await GetOperatorIdAsync();
            if (operatorId > 0)
            {
                try
                {
                    var response = await httpClient.GetAsync($"Agencies/Operators/GetAgencyOperatorSessions?operatorId={operatorId}&sessionGuid={sessionGuid}");
                    if (response.IsSuccessStatusCode)
                    {
                        var agencyOperatorSessions = await response.Content.ReadFromJsonAsync<IEnumerable<AgencyOperatorSession>>();

                        var individualSheets = agencyOperatorSessions.SelectMany(s => s.Session.Sheets).Select(ass => ass.Sheet);
                        var cabinetsSheets = agencyOperatorSessions.SelectMany(s => s.Session.Cabinets).SelectMany(c => c.Cabinet?.Sheets).Select(s => s.Sheet);
                        var sheets = individualSheets.Concat(cabinetsSheets);

                        var sheetsView = sheets.Where(s => s.IsActive).Select(s => (SheetView)s).ToList();
                        return sheetsView;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        SignOut();
                    }
                    else
                    {
                        _logger.LogWarning("Error getting all the sheets. Operator with id: {operatorId}. HttpStatusCode {httpStatusCode}", operatorId, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting all the sheets.");
                }
            }
            return null;
        }

        public async Task<List<Sheet>> GetSheetsAsync()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            var operatorId = await GetOperatorIdAsync();
            if (operatorId > 0)
            {
                try
                {
                    var response = await httpClient.GetAsync($"Agencies/Operators/GetAgencyOperatorSessions?operatorId={operatorId}&sessionGuid={sessionGuid}");
                    if (response.IsSuccessStatusCode)
                    {
                        var agencyOperatorSessions = await response.Content.ReadFromJsonAsync<IEnumerable<AgencyOperatorSession>>();

                        var individualSheets = agencyOperatorSessions.SelectMany(s => s.Session.Sheets).Select(ass => ass.Sheet);
                        var cabinetsSheets = agencyOperatorSessions.SelectMany(s => s.Session.Cabinets).SelectMany(c => c.Cabinet?.Sheets).Select(s => s.Sheet);
                        var sheets = individualSheets.Concat(cabinetsSheets).Where(s => s.IsActive).ToList();
                        return sheets;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        SignOut();
                    }
                    else
                    {
                        _logger.LogWarning("Error getting all the sheets. Operator with id: {operatorId}. HttpStatusCode {httpStatusCode}", operatorId, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting all the sheets.");
                }
            }
            return null;
        }

        #region Get operator Id

        public async Task<int> GetOperatorIdAsync()
        {
            var member = await GetAgencyMemberByUserAsync();
            if (member != null)
            {
                var agencyOperator = await GetAgencyOperatorAsync(member.Id);
                if (agencyOperator != null)
                {
                    return agencyOperator.Id;
                }
            }
            return 0;
        }

        private async Task<AgencyMember> GetAgencyMemberByUserAsync()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            var userId = GetUserId();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/GetAgencyMemberByUser?userId={userId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var agencyMember = await response.Content.ReadFromJsonAsync<AgencyMember>();
                    return agencyMember;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting agency member. User with id: {userId}. HttpStatusCode {httpStatusCode}", userId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency member. User with id: {userId}.", userId);
            }
            return null;
        }

        private async Task<AgencyOperator> GetAgencyOperatorAsync(int memberId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Operators/GetAgencyOperatorByMember?memberId={memberId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var agencyOperator = await response.Content.ReadFromJsonAsync<AgencyOperator>();
                    return agencyOperator;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting agency operator. Agency member with id: {memberId}. HttpStatusCode {httpStatusCode}", memberId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency operator. Agency member with id: {memberId}.", memberId);
            }
            return null;
        }

        private int GetUserId()
        {
            var user = _httpContextAccessor.HttpContext.User;
            var userId = int.Parse(user.FindFirst("Id")?.Value);
            return userId;
        }

        #endregion

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

    public interface IOperatorClient
    {
        Task<List<SheetView>> GetSheetsViewAsync();
        Task<List<Sheet>> GetSheetsAsync();
        Task<int> GetOperatorIdAsync();
    }
}
