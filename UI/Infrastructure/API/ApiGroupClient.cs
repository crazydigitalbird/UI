using System.Net.Http;
using System.Net;
using System.Security.Principal;
using Core.Models.Agencies.Cabinets;
using UI.Models;
using Core.Models.Agencies.Groups;

namespace UI.Infrastructure.API
{
    public class ApiGroupClient : IGroupClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILogger<ApiAdminAgencyClient> _logger;

        public ApiGroupClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiAdminAgencyClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IEnumerable<AgencyGroup>> GetGroupsAsync(int agencyId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Groups/GetAgencyGroups?agencyId={agencyId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var groups = await response.Content.ReadFromJsonAsync<IEnumerable<AgencyGroup>>();
                    return groups;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting all the groups. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the groups.");
            }
            return null;
        }

        public async Task<AgencyGroup> AddGroupAsync(int agencyId, string nameGroup)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.PutAsync($"Agencies/Groups/AddAgencyGroup?agencyId={agencyId}&description={nameGroup}&sessionGuid={sessionGuid}", null);
                if (response.IsSuccessStatusCode)
                {
                    var group = await response.Content.ReadFromJsonAsync<AgencyGroup>();
                    return group;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error adding the {nameGroup} group. HttpStatusCode: {httpStatusCode}", nameGroup, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the {nameGroup} group.", nameGroup);
            }
            return null;
        }

        public async Task<bool> ChangeGroupAsync(int sheetId, int groupId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var responseAgencyGroupSheets = await httpClient.GetAsync($"Agencies/Groups/GetAgencyGroupSheetsBySheet?sheetId={sheetId}&sessionGuid={sessionGuid}");
                if (responseAgencyGroupSheets.IsSuccessStatusCode)
                {
                    var agencyGroupSheets = await responseAgencyGroupSheets.Content.ReadFromJsonAsync<IEnumerable<AgencyGroupSheet>>();
                    if (agencyGroupSheets?.Count() > 0)
                    {
                        foreach (var ags in agencyGroupSheets)
                        {
                            var groupSheetId = ags.Id;
                            var responseDelete = await httpClient.DeleteAsync($"Agencies/Groups/DeleteAgencyGroupSheet?groupSheetId={groupSheetId}&sessionGuid={sessionGuid}");
                            if (responseDelete.IsSuccessStatusCode)
                            {

                            }
                            else if (responseDelete.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                SignOut();
                            }
                            else
                            {
                                _logger.LogWarning("For sheet id: {sheetId}, the group {groupId} has not been changed. HttpStatusCode: {httpStatusCode}", sheetId, groupId, responseDelete.StatusCode);
                            }
                        }
                    }
                    if (groupId != 0)
                    {
                        var response = await httpClient.PostAsync($"Agencies/Groups/AddAgencyGroupSheet?groupId={groupId}&sheetId={sheetId}&sessionGuid={sessionGuid}", null);
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            SignOut();
                        }
                        else
                        {
                            _logger.LogWarning("For sheet id: {sheetId}, the group {groupId} has not been changed. HttpStatusCode: {httpStatusCode}", sheetId, groupId, response.StatusCode);
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else if (responseAgencyGroupSheets.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("For sheet id: {sheetId}, the group {groupId} has not been changed. HttpStatusCode: {httpStatusCode}", sheetId, groupId, responseAgencyGroupSheets.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "For sheet id: {sheetId}, the group {groupId} has not been changed.", sheetId, groupId);
            }
            return false;
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

    public interface IGroupClient
    {
        Task<IEnumerable<AgencyGroup>> GetGroupsAsync(int agencyId);
        Task<AgencyGroup> AddGroupAsync(int agencyId, string nameGroup);
        Task<bool> ChangeGroupAsync(int sheetId, int groupId);
    }
}
