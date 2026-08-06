using EF_POO_II.Models;

namespace EF_POO_II.Data.Services;

public interface IApiTokenService
{
    string CreateToken(string username, string role, DateTime expiresAt);

    ApiTokenValidationResult ValidateToken(string? authorizationHeader);
}
