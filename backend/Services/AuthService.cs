using backend.Models;
namespace Services;

public class AuthService : IAuthService
{
    private readonly AuthRDBContext _dbContext;

    public AuthService(AuthRDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RegisterUserAsync(string userCommonId, string email, string passwordHash, string passwordSalt, string displayName)
    {
        var user = new User
        {
            UserCommonId = userCommonId,
            Email = email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            DisplayName = displayName
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
    }
}
