using UI.Models;
using Newtonsoft.Json;
using System.Net;
using Core.Models.Users;

namespace UI.Infrastructure.API
{
    public class ApiAuthenticationClient : IAuthenticationClient
    {
        IHttpClientFactory _httpClientFactory;
        ILogger<ApiAuthenticationClient> _logger;

        public ApiAuthenticationClient(IHttpClientFactory httpClientFactory, ILogger<ApiAuthenticationClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ApplicationUser> LogInAsync(string login, string passowrd)
        {
            var client = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await client.PutAsync($"Users/AddSession?userLogin={login}&userPassword={passowrd}&sessionLength=15", null);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var userSession = await response.Content.ReadFromJsonAsync<UserSession>();

                    var user = (ApplicationUser)userSession.User;
                    user.SesstionGuid = userSession.Guid;

                    //await SetRoleAsync(user);
                    return user;
                }
                //return await client.GetFromJsonAsync<ApplicationUser>($"/Authentication/?login={login}&hash={hashPassowrd}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An error occurred while logging in: {ErrorMessage}. Login: {login}", ex.Message, login);
            }
            return null;
        }

        public async Task SetRoleAsync(ApplicationUser user)
        {
            var client = _httpClientFactory.CreateClient("api");

            if (await IsAdmin(client, user))
            {
                user.Role = Role.Admin;
                return;
            }

            await SetAgencyMemberIdByUser(client, user);
            if (user.MemeberId == 0)
            {
                user.Role = Role.User;
                return;
            }

            if (await IsAdminAgency(client, user))
            {
                user.Role = Role.AdminAgency;
                return;
            }

            if (await IsOperator(client, user))
            {
                user.Role = Role.Operator;
                return;
            }

            user.Role = Role.User;
        }

        private async Task<bool> IsAdmin(HttpClient client, ApplicationUser user)
        {
            var response = await client.GetAsync($"Users/GetAdminByUser?userId={user.Id}&sessionGuid={user.SesstionGuid}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        private async Task SetAgencyMemberIdByUser(HttpClient client, ApplicationUser user)
        {
            var response = await client.GetAsync($"Agencies/GetAgencyMemberByUser?userId={user.Id}&sessionGuid={user.SesstionGuid}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                user.MemeberId = responseObject.id;
            }
        }

        private async Task<bool> IsAdminAgency(HttpClient client, ApplicationUser user)
        {
            var response = await client.GetAsync($"Agencies/GetAgencyAdmin?memberId={user.MemeberId}&sessionGuid={user.SesstionGuid}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        private async Task<bool> IsOperator(HttpClient client, ApplicationUser user)
        {
            var response = await client.GetAsync($"Agencies/Operators/GetAgencyOperatorByMember?memberId={user.MemeberId}&sessionGuid={user.SesstionGuid}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> RegisterAsync(string login, string email, string password)
        {
            var client = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await client.PutAsync($"Users/AddUser?userLogin={login}&userEmail={email}&userPassword={password}", null);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An error occured during registration: {ErrorMessage}. Login: {login}. Email: {email}", ex.Message, login, email);
            }
            return false;
        }

        public async Task<HttpResponseMessage> ChangePassowrdAsync(string password, string newPassword)
        {
            var client = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await client.PutAsync($"", null);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured when changing the password: {ErrorMessage}", ex.Message);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public async Task<HttpResponseMessage> PasswordRecoveryAsync(string email)
        {
            var client = _httpClientFactory.CreateClient("api");
            try
            {
                var response = await client.PutAsync($"{email}", null);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }

    public interface IAuthenticationClient
    {
        Task<ApplicationUser> LogInAsync(string login, string password);

        Task<bool> RegisterAsync(string login, string email, string password);

        Task<HttpResponseMessage> ChangePassowrdAsync(string password, string newPassword);

        Task<HttpResponseMessage> PasswordRecoveryAsync(string email);

        Task SetRoleAsync(ApplicationUser user);
    }
}
