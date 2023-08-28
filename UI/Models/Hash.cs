using System.Security.Cryptography;
using System.Text;

namespace UI.Models
{
    public class Hash
    {
        public static string HashPassword(string password)
        {
            byte[] hashPassword = SHA512.HashData(Encoding.Unicode.GetBytes(password));
            return Convert.ToBase64String(hashPassword);
        }
    }
}
