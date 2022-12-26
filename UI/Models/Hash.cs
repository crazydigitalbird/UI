using System.Security.Cryptography;
using System.Text;

namespace UI.Models
{
    public class Hash
    {
        public static string HashPassword(string password)
        {
            SHA512 sha = SHA512.Create();
            byte[] hashPassword = sha.ComputeHash(Encoding.Unicode.GetBytes(password));
            return Convert.ToBase64String(hashPassword);
        }
    }
}
