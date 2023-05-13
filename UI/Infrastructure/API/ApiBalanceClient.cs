﻿using Core.Models.Balances;
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
                var response = await httpClient.GetAsync($"Agencies/Balances/GetOperatorBalance?operatorId={operatorId}&dateBegin={begin:yyyy-MM-dd}&dateEnd={end:yyyy-MM-ddTHH:mm:ss.fff}&sessionGuid={sessionGuid}");
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
    }
}