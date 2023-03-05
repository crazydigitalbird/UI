using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class LoginRegisterModel
    {
        public LoginModel LoginModel { get; set; }

        public RegisterModel RegisterModel { get; set; }
    }

    public class LoginModel
    {
        [Required]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        public string Login { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Confirm password not match")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassowrd { get; set; }
    }
}
