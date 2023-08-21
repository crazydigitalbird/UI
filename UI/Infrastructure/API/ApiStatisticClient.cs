using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiStatisticClient : IStatisticClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiStatisticClient> _logger;

        public ApiStatisticClient(IHttpClientFactory httpClientFactory, ILogger<ApiStatisticClient> logger) 
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<AverageResponseTime> GetAgencyAverageResponseTimeAsync(int agencyId)
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
                    var averageResponseTime = await response.Content.ReadFromJsonAsync<AverageResponseTime>();
                    return averageResponseTime;
                }
                else
                {
                    _logger.LogWarning("Error getting time metric for agency with id: {id}. HttpStatusCode: {httpStatusCode}", agencyId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting time metric for agency with id: {id}. HttpStatusCode: {httpStatusCode}", agencyId);
            }
            return null;
        }
    }

    public interface IStatisticClient
    {
        Task<AverageResponseTime> GetAgencyAverageResponseTimeAsync(int agencyId);
    }
}
