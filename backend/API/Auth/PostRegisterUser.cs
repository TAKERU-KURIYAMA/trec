using backend.Models;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using System.Security.Cryptography;

namespace API;

public class PostRegisterUser
{
    public async Task<IActionResult> Run(
        AuthRDBContext dbContext,
        IAuthService authService,
        [FromBody] PostRegisterUserRequest request)
    {

        try
        {
            // 重複チェック
            if (await dbContext.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new AppException(FoundationCode.Errors.InvalidInput);
            }

            var salt = GenerateSalt();
            var passwordHash = HashPassword(request.Password, salt);
            var userId = GenerateUserId();

            await authService.RegisterUserAsync(
                userId,
                request.Email,
                passwordHash,
                salt,
                request.DisplayName
            );

            return Utils.ResponseBuilder.Success();        
        }
        catch (AppException ex)
        {
            return Utils.ResponseBuilder.Fail(ex);
        }
        catch (Exception)
        {
            var response = new ApiResponse
            {
                Code = "9999",
                Message = "不明なエラーが発生しました。"
            };

            var result = new ObjectResult(response);
            result.StatusCode = 500;
            return result;
        }

    }

    public class PostRegisterUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    private static string GenerateSalt()
    {
        var buffer = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);
        return Convert.ToBase64String(buffer);
    }

    private static string HashPassword(string password, string salt)
    {
        var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }

    private static string GenerateUserId()
    {
        var random = new Random();
        var randomPart = random.NextInt64(0, 9999999999999L);
        return "123" + randomPart.ToString().PadLeft(13, '0');
    }
}
