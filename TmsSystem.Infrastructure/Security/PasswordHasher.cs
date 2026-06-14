using System.Security.Cryptography;
using System.Text;

namespace TmsSystem.Infrastructure.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        if (storedHash.StartsWith("PBKDF2$", StringComparison.Ordinal))
        {
            return VerifyPbkdf2(password, storedHash);
        }

        var sha256Bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var sha256Hex = Convert.ToHexString(sha256Bytes);
        var sha256Base64 = Convert.ToBase64String(sha256Bytes);

        return FixedEquals(storedHash, sha256Hex)
            || FixedEquals(storedHash, sha256Base64)
            || FixedEquals(storedHash, password);
    }

    public static string Sha256Base64(string value) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static bool VerifyPbkdf2(string password, string storedHash)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expectedHash = Convert.FromBase64String(parts[3]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static bool FixedEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
