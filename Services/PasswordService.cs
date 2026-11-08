using Microsoft.AspNetCore.Identity;

namespace Calorix.Services;

/// <summary>
/// Hash de contraseñas con PBKDF2 (HMAC-SHA512, 100,000 iteraciones, sal aleatoria)
/// usando el PasswordHasher de ASP.NET Core Identity. Nunca se guarda la contraseña en texto plano.
/// </summary>
public static class PasswordService
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Dummy = new();

    public static string Hash(string password) => Hasher.HashPassword(Dummy, password);

    public static bool Verify(string hash, string password)
    {
        if (string.IsNullOrEmpty(hash) || !IsHashed(hash)) return false;
        return Hasher.VerifyHashedPassword(Dummy, hash, password) != PasswordVerificationResult.Failed;
    }

    /// <summary>Un hash v3 de Identity es Base64 y empieza con el byte de formato 0x01.</summary>
    public static bool IsHashed(string value)
    {
        if (value.Length < 60) return false;
        Span<byte> buffer = stackalloc byte[value.Length];
        return Convert.TryFromBase64String(value, buffer, out int n) && n > 13 && buffer[0] == 0x01;
    }
}
