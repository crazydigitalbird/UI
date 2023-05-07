using Core.Models.Agencies.Cabinets;
using Core.Models.Agencies.Operators;
using Core.Models.Agencies.Sessions;
using System.Net;
using System.Security.Principal;
using UI.Models;

namespace UI.Infrastructure.API
{
    public class ApiCabinetClient : ICabinetClient, ISignOut
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ICabinetClient> _logger;

        public ApiCabinetClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<ICabinetClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IEnumerable<AgencyCabinet>> GetCabinetsAsync(int agencyId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Cabinets/GetAgencyCabinets?agencyId={agencyId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var agencyCabinets = await response.Content.ReadFromJsonAsync<IEnumerable<AgencyCabinet>>();
                    return agencyCabinets;
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

        private async Task<ICollection<ApplicationUser>> GetCabinetAgencyOperatorSessions(int cabinetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Operators/GetCabinetAgencyOperatorSessions?cabinetId={cabinetId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var agencyOperatorSession = await response.Content.ReadFromJsonAsync<IEnumerable<AgencyOperatorSession>>();
                    var operators = agencyOperatorSession.Select(aos => (ApplicationUser)aos.Operator).ToList();
                    return operators;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting all the operators in cabinet with id: {cabinetId}. HttpStatusCode: {httpStatusCode}", cabinetId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the operators in cabinet with id: {cabinetId}.", cabinetId);
            }
            return null;
        }

        private async Task<List<SheetView>> GetAgencyCabinetSheets(int cabinetId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.GetAsync($"Agencies/Cabinets/GetAgencyCabinetSheets?cabinetId={cabinetId}&sessionGuid={sessionGuid}");
                if (response.IsSuccessStatusCode)
                {
                    var agencyCabinetSheets = await response.Content.ReadFromJsonAsync<IEnumerable<AgencyCabinetSheet>>();
                    var sheetsView = agencyCabinetSheets.Select(acs => (SheetView)acs.Sheet).ToList();
                    return sheetsView;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error getting all the sheets in cabinet with id: {cabinetId}. HttpStatusCode: {httpStatusCode}", cabinetId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all the sheets in cabinet with id: {cabinetId}.", cabinetId);
            }
            return null;
        }

        public async Task<Cabinet> AddAsync(int agencyId, string name)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var response = await httpClient.PutAsync($"Agencies/Cabinets/AddAgencyCabinet?agencyId={agencyId}&name={name}&sessionGuid={sessionGuid}", null);
                if (response.IsSuccessStatusCode)
                {
                    var agencyCabinet = await response.Content.ReadFromJsonAsync<AgencyCabinet>();
                    return (Cabinet)agencyCabinet;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error adding the {nameCabinet} cabinet. HttpStatusCode: {httpStatusCode}", name, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the {nameCabinet} cabinet.", name);
            }
            return null;
        }

        #region Bind cabinet to operator

        public async Task<bool> BindCabinetToOperatorAsync(int agencyId, int cabinetId, int operatorId)
        {
            var sessionGuid = GetSessionGuid();
            try
            {
                var agencySession = await AddAgencySession(agencyId, sessionGuid);
                if (agencySession != null)
                {
                    var agencySessionSheet = await AddAgencySessionCabinet(cabinetId, agencySession.Id, sessionGuid);
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
                _logger.LogError(ex, "Error adding the operator with Id: {operatorId} in cabinet with Id: {cabinetId}.", operatorId, cabinetId);
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

        private async Task<AgencySessionCabinet> AddAgencySessionCabinet(int cabinetId, int agencySessionId, string sessionGuid)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await httpClient.PostAsync($"Agencies/Sessions/AddAgencySessionCabinet?agencySessionId={agencySessionId}&cabinetId={cabinetId}&sessionGuid={sessionGuid}", null);
                if (response.IsSuccessStatusCode)
                {
                    var agencySessionCabinet = await response.Content.ReadFromJsonAsync<AgencySessionCabinet>();
                    return agencySessionCabinet;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SignOut();
                }
                else
                {
                    _logger.LogWarning("Error adding the cabinet with Id: {cabinetId} in session with Id: {sessionId}. HttpStatusCode: {httpStatusCode}", cabinetId, agencySessionId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding the cabinet with Id: {cabinetId} in session with Id: {sessionId}.", cabinetId, agencySessionId);
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

        public async Task<bool> UnbindCabinetToUserAsync(int cabinetId, int operatorId)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("api");
            var sessionGuid = GetSessionGuid();
            try
            {
                var responseAgencyOperatorSessions = await httpClient.GetAsync($"Agencies/Operators/GetCabinetAgencyOperatorSessions?cabinetId={cabinetId}&sessionGuid={sessionGuid}");
                if (responseAgencyOperatorSessions.IsSuccessStatusCode)
                {
                    var agencyOperatorSessions = (await responseAgencyOperatorSessions.Content.ReadFromJsonAsync<IEnumerable<AgencyOperatorSession>>());
                    var agencyCurrentOperatorSessions = agencyOperatorSessions.Where(s => s.Operator.Id == operatorId && s.Session.Cabinets.Count > 0);
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
                                _logger.LogWarning("For cabinet id: {cabinetId}, the operator id: {operatorId} has not been deleted. HttpStatusCode: {httpStatusCode}",cabinetId, operatorId, responseAgencyOperatorSessions.StatusCode);
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
                    _logger.LogWarning("Error unbinding the cabinet with id {cabinetId} to the user with id {operatorId}. HttpStatusCode: {httpStatusCode}", cabinetId, operatorId, responseAgencyOperatorSessions.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbinding the cabinet with id {cabinetId} to the user with id {operatorId}.", cabinetId, operatorId);
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

    public interface ICabinetClient
    {
        Task<IEnumerable<AgencyCabinet>> GetCabinetsAsync(int agencyId);
        Task<Cabinet> AddAsync(int agencyId, string name);
        Task<bool> BindCabinetToOperatorAsync(int agencyId, int cabinetId, int operatorId);
        Task<bool> UnbindCabinetToUserAsync(int cabinetId, int userId);
    }
}
