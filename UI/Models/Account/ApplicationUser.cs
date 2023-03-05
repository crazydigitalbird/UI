using Core.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace UI.Models
{
    public class ApplicationUser : IdentityUser
    {        
        public new int Id { get; set; }

        public int MemeberId { get; internal set; }

        public string SesstionGuid { get; set; }

        public Role Role { get; set; }

        public string Hash { get; set; }

        public static explicit operator ApplicationUser(User user)
        {
            var appUser = new ApplicationUser()
            {
                Id = user.Id,
                UserName = user.Login,
                Email = user.Email,
                EmailConfirmed = user.Confirmed
            };

            if (user.UserAdmins.Count > 0)
            {
                appUser.Role = Role.Admin;
            }
            else if (user.AgencyMembers?.FirstOrDefault()?.AgencyAdmins.Count > 0)
            {
                appUser.Role = Role.AdminAgency;
            }
            else if (user.AgencyMembers?.FirstOrDefault()?.AgencyOperators.Count > 0)
            {
                appUser.Role = Role.Operator;
            }
            else
            {
                appUser.Role = Role.User;
            }

            return appUser;
        }

    }

    public class MyApplicationUserIsUpdateRoleComparer : IEqualityComparer<ApplicationUser>
    {
        public bool Equals(ApplicationUser x, ApplicationUser y)
        {
            return x.Id == y.Id && x.Role != y.Role;
        }

        public int GetHashCode(ApplicationUser user)
        {
            return user.Id.GetHashCode();
        }
    }
}
