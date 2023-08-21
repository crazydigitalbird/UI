using Core.Models.Sheets;
using Newtonsoft.Json;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiMediaClient : IMediaClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiMediaClient> _logger;

        public ApiMediaClient(IHttpClientFactory httpClientFactory, ILogger<ApiMediaClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> GetOriginalUrlPhoto(Sheet sheet, string urlPreview)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("photo", urlPreview) };
                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync($"big_photo", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var media = await response.Content.ReadFromJsonAsync<OriginalUrlPhoto>();
                    return media.Data.Url;
                }
                else
                {
                    _logger.LogWarning("Error getting original url media the sheet with id: {sheetId}. UrlPreview: {urlPreview}. HttpStatusCode: {httpStatusCode}", sheet.Id, urlPreview, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting original url media the sheet with id: {sheetId}. UrlPreview: {urlPreview}.", sheet.Id, urlPreview);
            }
            return null;
        }

        public async Task<string> GetOriginalUrlVideo(Sheet sheet, string idVideo)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("apiBot");
            try
            {
                var credentials = JsonConvert.DeserializeObject<Credentials>(sheet.Credentials);

                var contentList = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("site", sheet.Site.Configuration),
                    new KeyValuePair<string, string>("email", credentials.Login),
                    new KeyValuePair<string, string>("password", credentials.Password),
                    new KeyValuePair<string, string>("video", idVideo) };
                var content = new FormUrlEncodedContent(contentList);

                var response = await httpClient.PostAsync($"big_video", content);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var media = await response.Content.ReadFromJsonAsync<OriginalUrlVideo>();
                    return media.Data.FirstOrDefault(v => v.Label.ToLower() == "hd")?.Src;
                }
                else
                {
                    _logger.LogWarning("Error getting original url media the sheet with id: {sheetId}. Video with id: {idVideo}. HttpStatusCode: {httpStatusCode}", sheet.Id, idVideo, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting original url media the sheet with id: {sheetId}. Video with id: {idVideo}.", sheet.Id, idVideo);
            }
            return null;
        }
    }

    public interface IMediaClient
    {
        Task<string> GetOriginalUrlPhoto(Sheet sheet, string urlPreview);

        Task<string> GetOriginalUrlVideo(Sheet sheet, string idVideo);
    }
}
