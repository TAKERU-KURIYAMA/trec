using Api.Models;
namespace Services;

public class AuthService : IAuthService
{
    private readonly MessageRDBContext _dbContext;

    public AuthService(MessageRDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RegisterUserAsync(string userCommonId, string LoginId, string passwordHash, string passwordSalt, string displayName)
    {
        var user = new User
        {
            UserCommonId = userCommonId,
            LoginId = LoginId,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            DisplayName = displayName
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
    }
}
