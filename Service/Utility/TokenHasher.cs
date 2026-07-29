using System.Security.Cryptography;
using System.Text;

namespace Service.Utility;
public static class TokenHasher
{
    public static string HashToken(string rawToken)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(rawToken);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        
        // Convert the byte array to a hex string to store in the DB
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}