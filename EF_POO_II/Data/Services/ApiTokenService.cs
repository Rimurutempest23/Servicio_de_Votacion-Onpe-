using EF_POO_II.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EF_POO_II.Data.Services;

public class ApiTokenService : IApiTokenService
{
    private readonly IConfiguration _configuration;

    public ApiTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(string username, string role, DateTime expiresAt)
    {
        var payload = JsonSerializer.Serialize(new ApiTokenPayload(username, role, expiresAt));
        var payloadEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
        var signature = CreateSignature(payloadEncoded);

        return $"{payloadEncoded}.{signature}";
    }

    public ApiTokenValidationResult ValidateToken(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return new ApiTokenValidationResult();
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();
        var parts = token.Split('.');

        if (parts.Length != 2)
        {
            return new ApiTokenValidationResult();
        }

        var expectedSignature = CreateSignature(parts[0]);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(parts[1])))
        {
            return new ApiTokenValidationResult();
        }

        var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
        var payload = JsonSerializer.Deserialize<ApiTokenPayload>(payloadJson);

        if (payload == null || payload.ExpiresAt <= DateTime.UtcNow)
        {
            return new ApiTokenValidationResult();
        }

        return new ApiTokenValidationResult
        {
            IsValid = true,
            Username = payload.Username,
            Role = payload.Role,
            ExpiresAt = payload.ExpiresAt
        };
    }

    private string CreateSignature(string payloadEncoded)
    {
        var secret = _configuration["Security:TokenSecret"] ?? "EF_POO_II_DEFAULT_SECRET";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadEncoded)));
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }

    private sealed record ApiTokenPayload(string Username, string Role, DateTime ExpiresAt);
}
