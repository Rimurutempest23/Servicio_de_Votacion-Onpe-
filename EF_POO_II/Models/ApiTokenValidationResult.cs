namespace EF_POO_II.Models;

public class ApiTokenValidationResult
{
    public bool IsValid { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}
