using Core.Models.Agencies;
using Core.Models.Agencies.Operators;
using Core.Models.Balances;
using System.Net;
using System.Security.Principal;

namespace UI.Infrastructure.API
{
    public class ApiBalanceClient : IBalanceClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiBalanceClient> _logger;

        public ApiBalanceClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiBalanceClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<OperatorBalance>> GetOperatorBalance(int operatorId, DateTime begin, DateTime end)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Balances/GetOperatorBalance?operatorId={operatorId}&dateBegin={begin:yyyy-MM-dd}&dateEnd={end:yyyy-MM-dd}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    List<OperatorBalance> operatorBalances = await response.Content.ReadFromJsonAsync<List<OperatorBalance>>();
                    return operatorBalances;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting balance the operator with id: {operatorId}. HttpStatusCode: {httpStatusCode}", operatorId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance the operator with id: {operatorId}.", operatorId);
            }
            return null;
        }

        public async Task<List<AgencyOperatorBalance>> GetOperatorBalances(int operatorId, DateTime begin, DateTime end)
        {
#if DEBUGOFFLINE
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
#else
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
#endif
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Balances/GetOperatorBalances?operatorId={operatorId}&dateBegin={begin:yyyy-MM-dd}&dateEnd={end:yyyy-MM-ddT23:59:59}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    List<AgencyOperatorBalance> operatorBalances = await response.Content.ReadFromJsonAsync<List<AgencyOperatorBalance>>();
                    return operatorBalances;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting balances the operator with id: {operatorId}. HttpStatusCode: {httpStatusCode}", operatorId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balances the operator with id: {operatorId}.", operatorId);
            }
            return null;
        }

        public async Task<List<SheetBalance>> GetSheetBalance(int sheetId, DateTime begin, DateTime end)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Balances/GetSheetBalance?sheetId={sheetId}&dateBegin={begin:yyyy-MM-dd}&dateEnd={end:yyyy-MM-ddTHH:mm:ss.fff}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    List<SheetBalance> sheetBalances = await response.Content.ReadFromJsonAsync<List<SheetBalance>>();
                    return sheetBalances;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting balance the sheet with id: {sheetId}. HttpStatusCode: {httpStatusCode}", sheetId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance the sheet with id: {sheetId}.", sheetId);
            }
            return null;
        }

        public async Task<List<AgencyBalance>> GetAgencyBalance(int agencyId, DateTime begin, DateTime end)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Balances/GetAgencyBalance?agencyId={agencyId}&dateBegin={begin:yyyy-MM-dd}&dateEnd={end:yyyy-MM-ddTHH:mm:ss.fff}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    List<AgencyBalance> agencyBalances = await response.Content.ReadFromJsonAsync<List<AgencyBalance>>();
                    return agencyBalances;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting balance the agency with id: {agencyId}. HttpStatusCode: {httpStatusCode}", agencyId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance the agency with id: {agencyId}.", agencyId);
            }
            return null;
        }

        public async Task<List<AgencyBalanceStatistic>> GetAgencyBalanceStatistic(int agencyId, DateTime begin, DateTime end)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Balances/GetAgencyBalanceStatistic?agencyId={agencyId}&dateBegin={begin:yyyy-MM-dd}&dateEnd={end:yyyy-MM-ddTHH:mm:ss.fff}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    List<AgencyBalanceStatistic> agencyBalanceStatistics = await response.Content.ReadFromJsonAsync<List<AgencyBalanceStatistic>>();
                    return agencyBalanceStatistics;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting agency balance statistic the agency with id: {agencyId}. HttpStatusCode: {httpStatusCode}", agencyId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency balance statistic the agency with id: {agencyId}.", agencyId);
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

    public interface IBalanceClient
    {
        Task<List<SheetBalance>> GetSheetBalance(int sheetId, DateTime begin, DateTime end);

        Task<List<OperatorBalance>> GetOperatorBalance(int operatorId, DateTime begin, DateTime end);

        Task<List<AgencyOperatorBalance>> GetOperatorBalances(int operatorId, DateTime begin, DateTime end);

        Task<List<AgencyBalance>> GetAgencyBalance(int agencyId, DateTime begin, DateTime end);

        Task<List<AgencyBalanceStatistic>> GetAgencyBalanceStatistic(int agencyId, DateTime begin, DateTime end);
    }
}
