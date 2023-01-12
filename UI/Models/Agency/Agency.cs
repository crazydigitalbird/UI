using System.ComponentModel.DataAnnotations;
//using System.Text.Json.Serialization;
//using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UI.Models
{
    public class Agency
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        //[Required]
        //[DataType(DataType.Password)]
        //[JsonIgnore]
        //public string Password { get; set; }

        //[Required]
        //[Compare("Password", ErrorMessage = "Confirm password not match")]
        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm Password")]
        //[JsonIgnore]
        //public string ConfirmPassowrd { get; set; }

        //[BindNever]
        //public string HashPassword { get { return Hash.HashPassword(Password); } }
    }
}
