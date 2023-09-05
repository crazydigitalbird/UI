using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiStatisticClient : IStatisticClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAdminAgencyClient _adminAgencyClient;
        private readonly ISheetClient _sheetClient;
        private readonly ILogger<ApiStatisticClient> _logger;

        public ApiStatisticClient(IHttpClientFactory httpClientFactory, 
            IAdminAgencyClient adminAgencyClient, 
            ISheetClient sheetClient,
        ILogger<ApiStatisticClient> logger) 
        {
            _httpClientFactory = httpClientFactory;
            _adminAgencyClient = adminAgencyClient;
            _sheetClient = sheetClient;
            _logger = logger;
        }

        public async Task<AgencyMetrik> GetAgencyMetrikAsync(int agencyId)
        {
#if DEBUGOFFLINE || DEBUG
            string s;
#endif
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("idAdmin", $"{agencyId}")
                };

                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync("time_metrik", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    s = await response.Content.ReadAsStringAsync();
#endif
                    var averageResponseTime = await response.Content.ReadFromJsonAsync<AgencyMetrik>();
                    return averageResponseTime;
                }
                else
                {
                    _logger.LogWarning("Error getting time metric for agency with id: {id}. HttpStatusCode: {httpStatusCode}", agencyId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting time metric for agency with id: {id}. HttpStatusCode: {httpStatusCode}", agencyId);
            }
            return null;
        }

        public async Task<AgencySheetsStatistic> GetSheetsStatisticAsync(int agencyId)
        {
            var sheets = await _adminAgencyClient.GetSheets(agencyId);
            if (sheets != null)
            {
                await _sheetClient.GettingStatusAndMedia(sheets);
                var agencySheetsStatistic = new AgencySheetsStatistic(sheets);
                return agencySheetsStatistic;
            }
            return null;
        }
    }   

    public interface IStatisticClient
    {
        Task<AgencyMetrik> GetAgencyMetrikAsync(int agencyId);

        Task<AgencySheetsStatistic> GetSheetsStatisticAsync(int agencyId);
    }
}
