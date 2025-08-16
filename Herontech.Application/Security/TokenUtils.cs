namespace Herontech.Application.Security;

using System.Security.Cryptography;
using System.Text;

public static class TokenUtils
{
    public static string CreateSecureToken(int bytes = 32) // 256 bits
    {
        return Base64UrlEncode(RandomNumberGenerator.GetBytes(bytes));
    }

    public static byte[] Sha256(string value)
    {
        using SHA256 sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(value));
    }

    public static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}