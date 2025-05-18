using Api.Models;
using Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Api.common
{
    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }


        public static RefreshToken Generate(MessageRDBContext context, User user)
        {

            string token = string.Empty;
            DateTime expiration = DateTime.MinValue;
            bool isUnique = false;
            for (int i = 0; i < FoundationCode.Const.TOKEN_RETRY_COUNT; i++)
            {
                token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                expiration = DateTime.UtcNow.AddMinutes(int.Parse(Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRE_MIN")!));

                if (!context.UserTokens.Any(x => x.RefreshToken == token))
                {
                    isUnique = true;
                    break;
                }
            }
            if (!isUnique)
            {
                throw new AppException(FoundationCode.Errors.TOKEN_GENERATION_FAILUE);
            }

            UserToken? resentToken = context.UserTokens
                .Where(x => x.UserCommonId == user.UserCommonId.ToString()).SingleOrDefault();

            if (resentToken != null)
            {
                context.UserTokens.Remove(resentToken);
            }
            UserToken userToken = context.UserTokens.Add(new UserToken
            {
                UserCommonId = user.UserCommonId.ToString(),
                RefreshToken = token,
                ExpiresAt = expiration,
                CreatedAt = DateTime.UtcNow,
            }).Entity;

            return new RefreshToken
            {
                Token = token,
                Expiration = expiration
            };
        }

        public static bool Validate(MessageRDBContext context, string token)
        {
            UserToken? userToken = context.UserTokens.Where(x => x.RefreshToken == token).FirstOrDefault();

            if (userToken == null || userToken.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }
            if (userToken != null) 
            {
                context.UserTokens.Remove(userToken);
            }
            return true;
        }
    }
}   

