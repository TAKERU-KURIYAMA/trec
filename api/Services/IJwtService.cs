namespace Services;

public interface IJwtService
{
    string GenerateToken(string userId, string LoginId);
    (bool IsValid, string? UserId) ValidateToken(string token);
}
