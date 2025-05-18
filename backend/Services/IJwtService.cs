namespace Services;

public interface IJwtService
{
    string GenerateToken(string userId, string email);
    (bool IsValid, string? UserId) ValidateToken(string token);
}
