using System.Security.Cryptography;
using System.Text;

namespace POS.Application.Common.Security;

/// <summary>
/// Refresh tokens are opaque random strings, NOT JWTs -- there's nothing to decode, they're
/// just a high-entropy lookup key the client stores and presents back. Only the SHA-256
/// hash is ever persisted (see RefreshToken.TokenHash), so this class is also how callers
/// compute that hash consistently for lookups.
/// </summary>
public static class RefreshTokenGenerator
{
    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
