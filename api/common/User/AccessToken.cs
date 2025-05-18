using Api.Models;
using Common;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Api.common
{
    public class AccessToken
    {
        public string Token { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string LoginId { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime Expiration { get; set; }
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// ユーザー情報からアクセストークンを生成
        /// </summary>
        public static AccessToken Generate(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__Secret")!);

            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.AddMinutes(int.Parse(Environment.GetEnvironmentVariable("ACCESS_TOKEN_EXPIRE_MIN")!));

            var claims = new[]
            {
                new Claim("sub", user.UserCommonId.ToString()),
                new Claim("name", user.DisplayName),
                new Claim("loginId", user.LoginId)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                IssuedAt = issuedAt,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(securityToken);

            return new AccessToken
            {
                Token = tokenString,
                Subject = user.UserCommonId.ToString(),
                UserName = user.DisplayName,
                LoginId = user.LoginId,
                IssuedAt = issuedAt,
                Expiration = expiresAt,
                Algorithm = SecurityAlgorithms.HmacSha256
            };
        }

        /// <summary>
        /// Authorization ヘッダーからアクセストークンを復元
        /// </summary>
        public static AccessToken FromToken(string authHeader)
        {
            // "Bearer xxxxx..." を分離
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                throw new AppException(FoundationCode.Errors.AUTHORIZATION_ERROR);

            var token = authHeader.Substring("Bearer ".Length).Trim();

            if (string.IsNullOrWhiteSpace(token) || token.Count(c => c == '.') != 2)
                throw new AppException(FoundationCode.Errors.AUTHORIZATION_ERROR);

            // クレームマッピングを無効化（"sub" → URI化防止）
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__Secret")!);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
                throw new AppException(FoundationCode.Errors.AUTHORIZATION_ERROR);

            var sub = principal.FindFirst("sub")?.Value;
            var name = principal.FindFirst("name")?.Value;
            var loginId = principal.FindFirst("loginId")?.Value;

            if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(loginId))
                throw new AppException(FoundationCode.Errors.AUTHORIZATION_ERROR);

            return new AccessToken
            {
                Token = token,
                Subject = sub,
                UserName = name,
                LoginId = loginId,
                IssuedAt = jwtToken.IssuedAt.ToUniversalTime(),
                Expiration = jwtToken.ValidTo.ToUniversalTime(),
                Algorithm = jwtToken.SignatureAlgorithm
            };
        }
    }
}
