using System.Security.Cryptography;
using Backend.Application.Common.Abstractions;
using Backend.Domain.Entities.IdentityAccess;

namespace Backend.Infrastructure.Auth;

internal sealed class InfrastructurePasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"pbkdf2_sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(UserAccount userAccount, string password)
    {
        var parts = userAccount.PasswordHash.Split('$');

        if (parts is not ["pbkdf2_sha256", var iterationValue, var saltValue, var hashValue] ||
            !int.TryParse(iterationValue, out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;

        try
        {
            salt = Convert.FromBase64String(saltValue);
            expectedHash = Convert.FromBase64String(hashValue);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
