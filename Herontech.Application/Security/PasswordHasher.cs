using System.Security.Cryptography;

namespace Herontech.Application.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16;         // 128 bits
    private const int HashSize = 32;         // 256 bits
    private const int Iterations = 100_000;  // custo (aumente se servidor aguentar)

    public static (byte[] salt, byte[] hash) HashPassword(string password)
    {
        // gera salt aleatório
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // gera hash
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize
        );

        return (salt, hash);
    }

    public static bool VerifyPassword(string password, byte[] salt, byte[] expectedHash)
    {
        // recomputa hash a partir do password + salt
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize
        );

        // comparação em tempo constante
        return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
    }
}