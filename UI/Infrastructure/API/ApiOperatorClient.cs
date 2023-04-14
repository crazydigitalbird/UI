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

        public async Task<IEnumerable<SheetView>> GetSheetsAsync()
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
                        await GettingStatusAndMedia(sheetsView);
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

        public async Task<Sheet> GetSheetAsync(int sheetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Sheets/GetSheet?sheedId={sheetId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var sheet = await response.Content.ReadFromJsonAsync<Sheet>();
                    return sheet;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting sheet with id: {sheetId}. HttpStatusCode {httpStatusCode}", sheetId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheet with id: {sheetId}.", sheetId);
            }
            return null;
        }

        private async Task GettingStatusAndMedia(List<SheetView> sheetsView)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                string ids = string.Join(",", sheetsView.Select(sw => sw.SheetId));
                httpClient.DefaultRequestHeaders.Add("id", ids);
                var response = await httpClient.PostAsync($"status", null);
                if (response.IsSuccessStatusCode)
                {
                    var dictionary = await response.Content.ReadFromJsonAsync<Dictionary<int, SheetStatusAndMedia>>();
                    foreach (var sam in dictionary)
                    {
                        var sheetView = sheetsView.FirstOrDefault(sw => sw.SheetId == sam.Key);
                        if (sheetView != null)
                        {
                            sheetView.Photo = sam.Value.Photo;
                            sheetView.PrivatePhoto = sam.Value.PrivatePhoto;
                            sheetView.Video = sam.Value.Video;
                            sheetView.Status = sam.Value.Status ? Status.Online : Status.Offline;
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Error getting status and media all the sheets. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status and media all the sheets.");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance");
                return null;
            }
        }

        #region Get operator Id

        private async Task<int> GetOperatorIdAsync()
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
        Task<IEnumerable<SheetView>> GetSheetsAsync();
        Task<Sheet> GetSheetAsync(int sheetId);
        Task<Dictionary<int, int>> GetBalanceAsync(string name, Interval interval);
    }
}
