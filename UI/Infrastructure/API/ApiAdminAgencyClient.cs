using Core.Models.Agencies;
using Core.Models.Agencies.Cabinets;
using Core.Models.Agencies.Operators;
using Core.Models.Agencies.Sessions;
using Core.Models.Sheets;
using Core.Models.Users;
using System.Net;
using System.Security.Principal;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiAdminAgencyClient : IAdminAgencyClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiAdminAgencyClient> _logger;

        public ApiAdminAgencyClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ApiAdminAgencyClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<SheetView>> GetSheets(int agencyId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/GetAgencySheets?agencyId={agencyId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var sheets = (await response.Content.ReadFromJsonAsync<IEnumerable<Sheet>>()).Where(s => s.IsActive);
                    var sheetsView = sheets.Select(s => (SheetView)s).ToList();
                    return sheetsView;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting all the sheets. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the sheets.");
            }
            return null;
        }

        public async Task<int> GetAgencyId()
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
                    return agencyMember.Agency.Id;

                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting agency Id. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency Id.");
            }
            return 0;
        }

        public async Task<bool> DeleteSheet(int sheetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.DeleteAsync($"Sheets/DeleteSheet?sheedId={sheetId}&sessionGuid={sessionGuid}");
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
                    _logger.LogWarning("Error deleting sheet whith id {sheetId}.HttpStatusCode: {httpStatusCode}", sheetId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sheet whith id {sheetId}.", sheetId);
            }
            return false;
        }

        public async Task<IEnumerable<AgencyCabinet>> GetCabinets(int agencyId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();
                var response = await httpClient.GetAsync($"Agencies/Cabinets/GetAgencyCabinets?agencyId={agencyId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var cabinets = await response.Content.ReadFromJsonAsync<IEnumerable<AgencyCabinet>>();
                    return cabinets;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting all the cabinets. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the cabinets.");
            }
            return null;
        }

        public async Task<bool> ChangeCabinet(int sheetId, int cabinetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var responseAgencyCabinetSheets = await httpClient.GetAsync($"Agencies/Cabinets/GetAgencyCabinetSheetsBySheet?sheetId={sheetId}&sessionGuid={sessionGuid}");
                if (responseAgencyCabinetSheets.IsSuccessStatusCode)
                {
                    var agencyCabinetSheets = await responseAgencyCabinetSheets.Content.ReadFromJsonAsync<IEnumerable<AgencyCabinetSheet>>();
                    if (agencyCabinetSheets?.Count() > 0)
                    {
                        var cabinetSheetId = agencyCabinetSheets.FirstOrDefault().Id;
                        var responseDelete = await httpClient.DeleteAsync($"Agencies/Cabinets/DeleteAgencyCabinetSheet?cabinetSheetId={cabinetSheetId}&sessionGuid={sessionGuid}");
                        if (responseDelete.IsSuccessStatusCode)
                        {

                        }
                        else if (responseDelete.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            SignOut();
                        }
                        else
                        {
                            _logger.LogWarning("For sheet id: {sheetId}, the cabinet {cabinetId} has not been changed. HttpStatusCode: {httpStatusCode}", sheetId, cabinetId, responseDelete.StatusCode);
                        }
                    }
                    if (cabinetId != 0)
                    {
                        var response = await httpClient.PostAsync($"Agencies/Cabinets/AddAgencyCabinetSheet?cabinetId={cabinetId}&sheetId={sheetId}&sessionGuid={sessionGuid}", null);
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
                            _logger.LogWarning("For sheet id: {sheetId}, the cabinet {cabinetId} has not been changed. HttpStatusCode: {httpStatusCode}", sheetId, cabinetId, response.StatusCode);
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else if (responseAgencyCabinetSheets.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("For sheet id: {sheetId}, the cabinet {cabinetId} has not been changed. HttpStatusCode: {httpStatusCode}", sheetId, cabinetId, responseAgencyCabinetSheets.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "For sheet id: {sheetId}, the cabinet {cabinetId} has not been changed.", sheetId, cabinetId);
            }
            return false;
        }

        public async Task<IEnumerable<OperatorSessionsView>> GetAgencyOperators(int agencyId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Operators/GetAgencyOperators?agencyId={agencyId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var agencyOperators = await response.Content.ReadFromJsonAsync<IEnumerable<AgencyOperator>>();

                    var operatorSessionsView = new List<OperatorSessionsView>();
                    foreach (var ao in agencyOperators)
                    {
                        var osv = new OperatorSessionsView
                        {
                            Operator = ao,
                            Sessions = await GetAgencyOperatorSessions(ao.Id, sessionGuid)
                        };
                        operatorSessionsView.Add(osv);
                    }

                    return operatorSessionsView;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting agency: {agencyId} operators. HttpStatusCode: {httpStatusCode}", agencyId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency: {agencyId} operators.", agencyId);
            }
            return null;
        }

        private async Task<IEnumerable<AgencyOperatorSession>> GetAgencyOperatorSessions(int operatorId, string sessionGuid)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Operators/GetAgencyOperatorSessions?operatorId={operatorId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var agencyOperatorSessions = (await response.Content.ReadFromJsonAsync<IEnumerable<AgencyOperatorSession>>());
                    return agencyOperatorSessions;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting agency operator sessions whith operatorId: {operatorId}. HttpStatusCode: {httpStatusCode}", operatorId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency operator sessions whith sheetId: {sheetId}.", operatorId);
            }
            return null;
        }

        public async Task<List<SheetView>> GetAgencyOperatorSessions(int operatorId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid(); ;
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Operators/GetAgencyOperatorSessions?operatorId={operatorId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var agencyOperatorSessions = (await response.Content.ReadFromJsonAsync<IEnumerable<AgencyOperatorSession>>());
                    var individualSheets = agencyOperatorSessions.SelectMany(aos => aos.Session.Sheets).Select(s => (SheetView)s.Sheet);
                    var cabinetSheets = agencyOperatorSessions.SelectMany(aos => aos.Session.Cabinets).Select(c => (SheetView)c.Cabinet.Sheets);
                    var sheets = individualSheets.Concat(cabinetSheets).ToList(); // Union
                    return sheets;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting agency operator sessions whith operatorId: {operatorId}. HttpStatusCode: {httpStatusCode}", operatorId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency operator sessions whith sheetId: {sheetId}.", operatorId);
            }
            return null;
        }

        public async Task<IEnumerable<AgencyOperatorSession>> GetSheetAgencyOperators(int sheetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Operators/GetSheetAgencyOperatorSessions?sheetId={sheetId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var agencyOperatorSessions = (await response.Content.ReadFromJsonAsync<IEnumerable<AgencyOperatorSession>>());
                    return agencyOperatorSessions;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting agency operator sessions whith sheetId: {sheetId}. HttpStatusCode: {httpStatusCode}", sheetId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency operator sessions whith sheetId: {sheetId}.", sheetId);
            }
            return null;
        }

        #region Delete operator from sheet

        public async Task<bool> DeleteOperatorFromSheet(int operatorId, int sheetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var responseAgencyOperatorSessions = await httpClient.GetAsync($"Agencies/Operators/GetSheetAgencyOperatorSessions?sheetId={sheetId}&sessionGuid={sessionGuid}");
                if (responseAgencyOperatorSessions.IsSuccessStatusCode)
                {
                    var agencyOperatorSessions = (await responseAgencyOperatorSessions.Content.ReadFromJsonAsync<IEnumerable<AgencyOperatorSession>>());
                    var agencyCurrentOperatorSessions = agencyOperatorSessions.Where(s => s.Operator.Id == operatorId && s.Session.Sheets.Count > 0);
                    if (agencyCurrentOperatorSessions.Any())
                    {
                        foreach (var aos in agencyCurrentOperatorSessions)
                        {
                            var response = await httpClient.DeleteAsync($"Agencies/Operators/DeleteAgencyOperatorSession?operatorSessionId={aos.Id}&sessionGuid={sessionGuid}");
                            if (response.IsSuccessStatusCode)
                            {
                                continue;
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                SignOut();
                                throw new InvalidOperationException($"HttpStatusCode: {response.StatusCode}");
                            }
                            else
                            {
                                _logger.LogWarning("For sheet id: {sheetId}, the operator id: {operatorId} has not been deleted. HttpStatusCode: {httpStatusCode}", sheetId, operatorId, responseAgencyOperatorSessions.StatusCode);
                                throw new InvalidOperationException($"HttpStatusCode: {response.StatusCode}");
                            }
                        }
                        return true;
                    }
                }
                else if (responseAgencyOperatorSessions.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("For sheet id: {sheetId}, the operator id: {operatorId} has not been deleted. HttpStatusCode: {httpStatusCode}", sheetId, operatorId, responseAgencyOperatorSessions.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "For sheet id: {sheetId}, the operator id: {operatorId} has not been deleted.", sheetId, operatorId);
            }
            return false;
        }

        #endregion

        #region Add operator from sheet

        public async Task<bool> AddOperatorFromSheet(int agencyId, int sheetId, int operatorId)
        {
            var sessionGuid = GetSessionGuid();
            try
            {
                var agencySession = await AddAgencySession(agencyId, sessionGuid);
                if (agencySession != null)
                {
                    var agencySessionSheet = await AddAgencySessionSheet(sheetId, agencySession.Id, sessionGuid);
                    if (agencySessionSheet != null)
                    {
                        var agencyOperatorSession = await AddAgencyOperatorSession(operatorId, agencySession.Id, sessionGuid);
                        if (agencyOperatorSession != null)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the operator with Id: {operatorId} in sheet with Id: {sheetId}.", operatorId, sheetId);
            }
            return false;
        }

        private async Task<AgencySession> AddAgencySession(int agencyId, string sessionGuid)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.PutAsync($"Agencies/Sessions/AddAgencySession?agencyId={agencyId}&sessionGuid={sessionGuid}", null);
                if (response.IsSuccessStatusCode)
                {
#if DEBUGOFFLINE || DEBUG
                    var s = await response.Content.ReadAsStringAsync();
#endif
                    var agencySession = await response.Content.ReadFromJsonAsync<AgencySession>();
                    return agencySession;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error adding the agency session. HttpStatusCode: {httpStatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the agency session.");
            }
            return null;
        }

        private async Task<AgencySessionSheet> AddAgencySessionSheet(int sheetId, int agencySessionId, string sessionGuid)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.PostAsync($"Agencies/Sessions/AddAgencySessionSheet?agencySessionId={agencySessionId}&sheetId={sheetId}&sessionGuid={sessionGuid}", null);
                if (response.IsSuccessStatusCode)
                {
                    var agencySessionSheet = await response.Content.ReadFromJsonAsync<AgencySessionSheet>();
                    return agencySessionSheet;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error adding the sheet with Id: {sheetId} in session with Id: {sessionId}. HttpStatusCode: {httpStatusCode}", sheetId, agencySessionId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the sheet with Id: {sheetId} in session with Id: {sessionId}.", sheetId, agencySessionId);
            }
            return null;
        }

        private async Task<AgencyOperatorSession> AddAgencyOperatorSession(int operatorId, int agencySessionId, string sessionGuid)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.PutAsync($"Agencies/Operators/AddAgencyOperatorSession?operatorId={operatorId}&agencySessionId={agencySessionId}&sessionGuid={sessionGuid}", null);
                if (response.IsSuccessStatusCode)
                {
                    var agencyOperatorSession = await response.Content.ReadFromJsonAsync<AgencyOperatorSession>();
                    return agencyOperatorSession;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error adding agency operator session the operator with Id: {operatorId} in session with Id: {sessionId}. HttpStatusCode: {httpStatusCode}", operatorId, agencySessionId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding agency operator session the operator with Id: {operatorId} in session with Id: {sessionId}.", operatorId, agencySessionId);
            }
            return null;
        }

        #endregion

        public async Task<AgencyView> GetAgencyById(int agencyId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();

                var response = await httpClient.GetAsync($"Agencies/GetAgency?agencyId={agencyId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    AgencyView agency = await response.Content.ReadFromJsonAsync<AgencyView>();

                    var responseMembers = await httpClient.GetAsync($"Agencies/GetAgencyMembers?agencyId={agencyId}&sessionGuid={sessionGuid}");
                    if (responseMembers.IsSuccessStatusCode)
                    {
                        var agencyMembers = await responseMembers.Content.ReadFromJsonAsync<List<AgencyMember>>();
                        agency.Users = agencyMembers.Select(am => (ApplicationUser)am).ToList();

                        return agency;
                    }
                    else if (responseMembers.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        SignOut();
                    }
                    else
                    {
                        _logger.LogWarning("Error getting agencymembers with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", agencyId, responseMembers.StatusCode);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", agencyId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agency with Id: {agencyId}.", agencyId);
            }
            return null;
        }

        public async Task<bool> UpdateAgency(AgencyView agency, List<ApplicationUser> originalUsers)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();
                var response = await httpClient.PostAsync($"Agencies/UpdateAgency?agencyId={agency.Id}&agencyName={agency.Name}&agencyDescription={agency.Description}&isActive=true&sessionGuid={sessionGuid}", null);
                if (response.IsSuccessStatusCode)
                {
                    var deleteUsers = originalUsers.ExceptBy(agency.Users.Select(u => u.Id), ou => ou.Id).ToList();
                    foreach (var user in deleteUsers)
                    {
                        await DeleteUserFromAgency(httpClient, sessionGuid, agency.Id, user);
                    }

                    var addUsers = agency.Users.ExceptBy(originalUsers.Select(ou => ou.Id), u => u.Id).ToList();
                    foreach (var user in addUsers)
                    {
                        await AddUserAgency(httpClient, sessionGuid, agency.Id, user);
                    }

                    var updateUser = agency.Users.Intersect(originalUsers, new MyApplicationUserIsUpdateRoleComparer()).ToList();
                    foreach (var user in updateUser)
                    {
                        await UpdateUserAgency(httpClient, sessionGuid, agency.Id, user);
                    }

                    return true;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error updating agency '{agencyName}'. Id: {agencyId}. HttpStatsCode: {httpStatusCode}", agency.Name, agency.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agency '{agencyName}'. Id: {agencyId}.", agency.Name, agency.Id);
            }
            return false;
        }

        public async Task<List<ApplicationUser>> GetAgencyUsers(int agencyId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();

                var response = await httpClient.GetAsync($"Agencies/GetAgencyMembers?agencyId={agencyId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var agencyMembers = await response.Content.ReadFromJsonAsync<List<AgencyMember>>();
                    var users = agencyMembers.Select(am => (ApplicationUser)am).ToList();

                    return users;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting users agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", agencyId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users agency with Id: {agencyId}.", agencyId);
            }
            return null;
        }

        public async Task<List<ApplicationUser>> GetNonAgencyUsers()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();
                var response = await httpClient.PostAsync($"Agencies/GetNonAgencyUsers?sessionGuid={sessionGuid}", null);

                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<User>>();
                    var appUsers = users.Select(u => (ApplicationUser)u).ToList();
                    return appUsers;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting non agency users");
            }
            return null;
        }

        public async Task AddUserAgency(HttpClient httpClient, string sessionGuid, int agencyId, ApplicationUser user)
        {
            var responseMember = await httpClient.PostAsync($"Agencies/AddAgencyMember?agencyId={agencyId}&userId={user.Id}&sessionGuid={sessionGuid}", null);
            if (responseMember.IsSuccessStatusCode)
            {
                if (user.Role == Role.User)
                {
                    return;
                }

                var newMember = await responseMember.Content.ReadFromJsonAsync<AgencyMember>();

                if (user.Role == Role.AdminAgency)
                {
                    await SetAdminAgency(httpClient, sessionGuid, agencyId, newMember.Id);
                }
                else if (user.Role == Role.Operator)
                {
                    await SetOperatorAgency(httpClient, sessionGuid, agencyId, newMember.Id);
                }
            }
            else if (responseMember.StatusCode == HttpStatusCode.Unauthorized)
            {
                SignOut();
            }
            else
            {
                _logger.LogWarning("Error adding user {email} in agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", user.Email, agencyId, responseMember.StatusCode);
            }
        }

        public async Task<bool> AddUserAgency(int agencyId, ApplicationUser user)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var sessionGuid = GetSessionGuid();
                var responseMember = await httpClient.PostAsync($"Agencies/AddAgencyMember?agencyId={agencyId}&userId={user.Id}&sessionGuid={sessionGuid}", null);
                if (responseMember.IsSuccessStatusCode)
                {
                    var newMember = await responseMember.Content.ReadFromJsonAsync<AgencyMember>();

                    if (user.Role == Role.AdminAgency)
                    {
                        await SetAdminAgency(httpClient, sessionGuid, agencyId, newMember.Id);
                    }
                    else if (user.Role == Role.Operator)
                    {
                        await SetOperatorAgency(httpClient, sessionGuid, agencyId, newMember.Id);
                    }
                }
                else if (responseMember.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error adding user {email} in agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", user.Email, agencyId, responseMember.StatusCode);
                }
            }
            catch
            {

            }
            return false;
        }

        private async Task SetAdminAgency(HttpClient httpClient, string sessionGuid, int agencyId, int memberId)
        {
            var response = await httpClient.PostAsync($"Agencies/AddAgencyAdmin?agencyId={agencyId}&memberId={memberId}&sessionGuid={sessionGuid}", null);
            if (response.IsSuccessStatusCode)
            {
                return;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                SignOut();
            }
            else
            {
                _logger.LogWarning("Error installing the admin role, for a user with memberId: {memberId}, in agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", memberId, agencyId, response.StatusCode);
            }
        }

        private async Task SetOperatorAgency(HttpClient httpClient, string sessionGuid, int agencyId, int memberId)
        {
            var response = await httpClient.PutAsync($"Agencies/Operators/AddAgencyOperator?agencyId={agencyId}&memberId={memberId}&sessionGuid={sessionGuid}", null);
            if (response.IsSuccessStatusCode)
            {
                return;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                SignOut();
            }
            else
            {
                _logger.LogWarning("Error installing the operator role, for a user with memberId: {memberId}, in agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", memberId, agencyId, response.StatusCode);
            }
        }

        private async Task UpdateUserAgency(HttpClient httpClient, string sessionGuid, int agencyId, ApplicationUser user)
        {
            await DeleteUserFromAgency(httpClient, sessionGuid, agencyId, user);
            await AddUserAgency(httpClient, sessionGuid, agencyId, user);
        }

        private async Task DeleteUserFromAgency(HttpClient httpClient, string sessionGuid, int agencyId, ApplicationUser user)
        {
            var responseMember = await httpClient.GetAsync($"Agencies/GetAgencyMemberByUser?userId={user.Id}&sessionGuid={sessionGuid}");
            if (responseMember.IsSuccessStatusCode)
            {
                var member = await responseMember.Content.ReadFromJsonAsync<AgencyMember>();
                var response = await httpClient.DeleteAsync($"Agencies/DeleteAgencyMember?memberId={member.Id}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error deleting a user {email} from the agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", user.Email, agencyId, responseMember.StatusCode);
                }
            }
            else if (responseMember.StatusCode == HttpStatusCode.Unauthorized)
            {
                SignOut();
            }
            else
            {
                _logger.LogWarning("Error getting the memberId for the user {email} in agency with Id: {agencyId}. HttpStatsCode: {httpStatusCode}", user.Email, agencyId, responseMember.StatusCode);
            }
        }

        private int GetUserId()
        {
            var user = _httpContextAccessor.HttpContext.User;
            var userId = int.Parse(user.FindFirst("Id")?.Value);
            return userId;
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

    public interface IAdminAgencyClient
    {
        Task<List<SheetView>> GetSheets(int agencyId);
        Task<bool> DeleteSheet(int sheetId);
        Task<IEnumerable<AgencyCabinet>> GetCabinets(int agencyId);
        Task<bool> ChangeCabinet(int sheetId, int cabinetId);

        Task<IEnumerable<OperatorSessionsView>> GetAgencyOperators(int agencyId);
        Task<IEnumerable<AgencyOperatorSession>> GetSheetAgencyOperators(int sheetId);
        Task<List<SheetView>> GetAgencyOperatorSessions(int sheetId);
        Task<bool> AddOperatorFromSheet(int agencyId, int sheetId, int operatorId);
        Task<bool> DeleteOperatorFromSheet(int operatorId, int sheetId);

        Task<int> GetAgencyId();
        Task<AgencyView> GetAgencyById(int agencyId);
        Task<bool> UpdateAgency(AgencyView agency, List<ApplicationUser> originalUsers);
        Task<List<ApplicationUser>> GetNonAgencyUsers();
        Task AddUserAgency(HttpClient httpClient, string sessionGuid, int agencyId, ApplicationUser user);
        Task<bool> AddUserAgency(int agencyId, ApplicationUser user);
    }
}
