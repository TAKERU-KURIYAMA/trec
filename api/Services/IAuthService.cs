namespace Services;

public interface IAuthService
{
    Task RegisterUserAsync(string userCommonId, string LoginId, string passwordHash, string passwordSalt, string displayName);
}
