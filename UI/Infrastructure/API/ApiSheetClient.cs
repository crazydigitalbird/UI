using Core.Models.Sheets;
using System.Security.Principal;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiSheetClient : ISheetClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiSheetClient> _logger;

        public ApiSheetClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiSheetClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

        public async Task GettingStatusAndMedia(List<SheetView> sheetsView)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                string ids = string.Join(",", sheetsView.Select(sw => sw.SheetId));
                httpClient.DefaultRequestHeaders.Add("ids", ids);
                var response = await httpClient.PostAsync("status_online", null);
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
                            sheetView.Status = sam.Value.Status == 1 ? Status.Online : Status.Offline;
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

        public async Task GetSheetAgencyOperatorSessionsCount(List<SheetView> sheetsView)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                Dictionary<int, int> taskIdToSheetId = new Dictionary<int, int>();
                var tasks = sheetsView.Select(s =>
                {
                    var task = httpClient.GetAsync($"Agencies/Operators/GetSheetAgencyOperatorSessionsCount?sheetId={s.Id}&sessionGuid={sessionGuid}");
                    taskIdToSheetId.Add(task.Id, s.Id);
                    return task;
                });
                await Task.WhenAll(tasks);
                foreach (var task in tasks)
                {
                    if (task.Result?.IsSuccessStatusCode ?? false)
                    {
                        var response = task.Result;
                        var sheetId = taskIdToSheetId[task.Id];
                        sheetsView.FirstOrDefault(s => s.Id == sheetId).Operators = await response.Content.ReadFromJsonAsync<int>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheets with id: {sheetId}.", string.Join(';', sheetsView));
            }
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

    public interface ISheetClient
    {
        Task<Sheet> GetSheetAsync(int sheetId);

        Task GettingStatusAndMedia(List<SheetView> sheetsView);

        Task GetSheetAgencyOperatorSessionsCount(List<SheetView> sheetsView);
    }
}
