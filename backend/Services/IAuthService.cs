namespace Services;

public interface IAuthService
{
    Task RegisterUserAsync(string userCommonId, string email, string passwordHash, string passwordSalt, string displayName);
}
