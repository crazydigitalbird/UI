using Microsoft.AspNetCore.Identity;

namespace UI.Models
{
    public class ApplicationUser : IdentityUser
    {        
        public Role Role { get; set; }

        public string Hash { get; set; }
    }
}
