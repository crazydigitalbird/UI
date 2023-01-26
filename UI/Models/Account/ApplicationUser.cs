using Microsoft.AspNetCore.Identity;

namespace UI.Models
{
    public class ApplicationUser : IdentityUser
    {        
        public new int Id { get; set; }

        public Role Role { get; set; }

        public string Hash { get; set; }
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
