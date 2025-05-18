using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Dynamic;
using Common;

namespace API;

public class PostToken
{
    public async Task<IActionResult> Run(
        AuthRDBContext dbContext,
        IJwtService jwtService,
        [FromBody] PostTokenRequest request)
    {

        try
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                throw new AppException(FoundationCode.Errors.InvalidInput);
            }

            var passwordHash = HashPassword(request.Password, user.PasswordSalt);

            if (user.PasswordHash != passwordHash)
            {
                throw new AppException(FoundationCode.Errors.InvalidInput);
            }

            var token = jwtService.GenerateToken(user.UserCommonId, user.Email);
            dynamic resobj = new ExpandoObject();
            PostTokenResponse responseObj = new PostTokenResponse { AccessToken = token };
            return Utils.ResponseBuilder.Success(responseObj);
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

    public class PostTokenRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class PostTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private static string HashPassword(string password, string salt)
    {
        var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }
}
