using Core.Models.Agencies;
using Core.Models.Agencies.Operators;
using Core.Models.Balances;
using System.Net;
using System.Security.Principal;
using UI.Models;

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

        public async Task<BalanceStatisticAgency> GetBalanceStatisticAgencyAsync(int agencyId)
        {
            var endDateTime = DateTime.Now;
            var beginDatetime = endDateTime.AddMonths(-1);
            beginDatetime = new DateTime(beginDatetime.Year, beginDatetime.Month, 1);            

            var balanceStatisticAgencyTask = GetBalanceStatisticAgencyAsync(agencyId, beginDatetime, endDateTime);
            var balanceStatisticAgencyOperatorsTask = GetBalanceStatisticAgencyOperators(agencyId, beginDatetime, endDateTime);

            await Task.WhenAll(balanceStatisticAgencyTask, balanceStatisticAgencyOperatorsTask);

            var balanceStatisticAgency = balanceStatisticAgencyTask.Result ?? new BalanceStatisticAgency();
            balanceStatisticAgency.Operators = balanceStatisticAgencyOperatorsTask.Result;
            return balanceStatisticAgency;
        }

        public async Task<BalanceStatisticAgency> GetBalanceStatisticAgencyAsync(int agencyId, DateTime begin, DateTime end)
        {
            var balances = await GetAgencyBalance(agencyId, begin, end);
            if (balances != null)
            {
                var lastPeriod = new DateTime(end.Year, end.Month, 1);
                var statistics = new BalanceStatisticAgency();
                statistics.BalancesLastMonth = GetBalancesMonth(DateTime.DaysInMonth(begin.Year, begin.Month), begin.Month, balances);
                statistics.BalancesCurrentMonth = GetBalancesMonth(end.Day, end.Month, balances);
                BalancesType(end.Year, end.Month, begin.Year, begin.Month, balances, statistics.BalancesType);
                statistics.Balance = balances.Where(b => b.Date.Year == end.Year && b.Date.Month == end.Month).Sum(b => b.Cash);
                statistics.BalanceIncrement = GetBalanceIncrement(balances, statistics.Balance, lastPeriod);
                statistics.BalanceToday = balances.Where(b => b.Date.Year == end.Year && b.Date.Month == end.Month && b.Date.Day == end.Day).Sum(b => b.Cash);
                statistics.BalanceTodayIncrement = GetBalanceTodayIncrement(balances, statistics.BalanceToday, end);
                return statistics;
            }

            return null;
        }

        public async Task<List<AgencyBalanceStatistic>> GetBalanceStatisticAgencyOperators(int agencyId, DateTime begin, DateTime end)
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
                    List<Core.Models.Balances.AgencyBalanceStatistic> agencyBalanceStatistics = await response.Content.ReadFromJsonAsync<List<Core.Models.Balances.AgencyBalanceStatistic>>();
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

        private decimal GetBalanceTodayIncrement(List<AgencyBalance> balances, decimal balanceToday, DateTime endDateTime)
        {
            var lasDayDate = endDateTime.AddDays(-1);
            decimal balanceLastDay = balances.Where(b => b.Date.Year == lasDayDate.Year && b.Date.Month == lasDayDate.Month && b.Date.Day == lasDayDate.Day).Sum(b => b.Cash);
            if (balanceLastDay > 0)
            {
                return balanceToday * 100 / balanceLastDay - 100;
            }
            return 0;
        }

        private decimal GetBalanceIncrement(List<AgencyBalance> balances, decimal balance, DateTime lastPeriod)
        {
            var balanceLastPeriod = balances.Where(b => b.Date < lastPeriod).Sum(b => b.Cash);
            if (balanceLastPeriod != 0)
            {
                return balance * 100 / balanceLastPeriod - 100;
            }
            return 0;
        }

        private decimal[] GetBalancesMonth(int days, int month, List<AgencyBalance> balances)
        {
            var balancesMonth = new decimal[days];
            for (int i = 0; i < balancesMonth.Length; i++)
            {
                balancesMonth[i] = balances.Where(b => b.Date.Month == month && b.Date.Day == i + 1).Sum(b => b.Cash); ;
            }
            return balancesMonth;
        }

        private void BalancesType(int currentYear, int currentMonth, int lastYear, int lastMonth, List<AgencyBalance> balances, Dictionary<string, BalanceType> balancesType)
        {
            foreach (var bt in balancesType)
            {
                var balanceCurrentMonthType = BalancesMonthType(currentYear, currentMonth, balances, bt.Key);
                var balanceLastMonthType = BalancesMonthType(lastYear, lastMonth, balances, bt.Key);
                if (balanceLastMonthType > 0)
                {
                    bt.Value.ChangePercent = (int)(balanceCurrentMonthType * 100 / balanceLastMonthType - 100);
                }
                else
                {
                    bt.Value.ChangePercent = 0;
                }
            }
        }

        private decimal BalancesMonthType(int year, int month, List<AgencyBalance> balances, string type)
        {
            return balances.Where(b => b.Type == type && b.Date.Year == year && b.Date.Month == month).Sum(b => b.Cash);
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

        Task<BalanceStatisticAgency> GetBalanceStatisticAgencyAsync(int agencyId);

        Task<BalanceStatisticAgency> GetBalanceStatisticAgencyAsync(int agencyId, DateTime begin, DateTime end);

        Task<List<AgencyBalanceStatistic>> GetBalanceStatisticAgencyOperators(int agencyId, DateTime begin, DateTime end);
    }
}
