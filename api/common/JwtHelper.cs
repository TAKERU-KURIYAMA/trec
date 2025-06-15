using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Common
{
    /// <summary>
    /// JWT関連のヘルパークラス
    /// </summary>
    public static class JwtHelper
    {
        /// <summary>
        /// JWT設定を検証する
        /// </summary>
        /// <param name="secretKey">秘密鍵</param>
        /// <param name="issuer">発行者</param>
        /// <param name="audience">対象者</param>
        /// <returns>検証結果</returns>
        public static bool ValidateJwtConfiguration(string secretKey, string issuer, string audience)
        {
            return !string.IsNullOrEmpty(secretKey) &&
                   !string.IsNullOrEmpty(issuer) &&
                   !string.IsNullOrEmpty(audience) &&
                   secretKey.Length >= 32; // 最低32文字必要
        }

        /// <summary>
        /// トークン検証パラメータを作成する
        /// </summary>
        /// <param name="secretKey">秘密鍵</param>
        /// <param name="issuer">発行者</param>
        /// <param name="audience">対象者</param>
        /// <returns>TokenValidationParameters</returns>
        public static TokenValidationParameters CreateValidationParameters(string secretKey, string issuer, string audience)
        {
            var key = Encoding.ASCII.GetBytes(secretKey);
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }
        /// <summary>
        /// JWTトークンを生成する
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="secretKey">秘密鍵</param>
        /// <param name="issuer">発行者</param>
        /// <param name="audience">対象者</param>
        /// <param name="expiryMinutes">有効期限（分）</param>
        /// <returns>JWTトークン</returns>
        public static string GenerateToken(string userId, string secretKey, string issuer, string audience, int expiryMinutes)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim("user_id", userId)
                }),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// JWTトークンを検証し、ClaimsPrincipalとユーザーIDを返す
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <param name="secretKey">秘密鍵</param>
        /// <param name="issuer">発行者</param>
        /// <param name="audience">対象者</param>
        /// <returns>ClaimsPrincipalとユーザーIDのタプル</returns>
        public static (ClaimsPrincipal principal, string userId) ValidateToken(string token, string secretKey, string issuer, string audience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            var userId = principal.FindFirst("user_id")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new SecurityTokenValidationException("ユーザーIDが見つかりません");
            }

            return (principal, userId);
        }

        /// <summary>
        /// トークンの有効期限を取得する
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <returns>有効期限</returns>
        public static DateTime? GetTokenExpiry(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// トークンが有効かどうかを確認する（期限チェックのみ）
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <returns>有効な場合true</returns>
        public static bool IsTokenValid(string token)
        {
            var expiry = GetTokenExpiry(token);
            return expiry.HasValue && expiry.Value > DateTime.UtcNow;
        }

        /// <summary>
        /// トークンからユーザーIDを取得する（検証なし）
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <returns>ユーザーID</returns>
        public static string? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id" || c.Type == ClaimTypes.NameIdentifier)?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 署名資格情報を作成する
        /// </summary>
        /// <param name="secretKey">秘密鍵</param>
        /// <returns>SigningCredentials</returns>
        public static SigningCredentials CreateSigningCredentials(string secretKey)
        {
            var key = Encoding.ASCII.GetBytes(secretKey);
            return new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature);
        }

        /// <summary>
        /// トークンを検証してクレームを抽出する
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <param name="secretKey">秘密鍵</param>
        /// <param name="issuer">発行者</param>
        /// <param name="audience">対象者</param>
        /// <returns>ClaimsPrincipalとユーザーIDのタプル</returns>
        public static (ClaimsPrincipal principal, string userId) ValidateTokenAndExtractClaims(string token, string secretKey, string issuer, string audience)
        {
            return ValidateToken(token, secretKey, issuer, audience);
        }

        /// <summary>
        /// トークンの有効期限を取得する
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <returns>有効期限</returns>
        public static DateTime? GetTokenExpiration(string token)
        {
            return GetTokenExpiry(token);
        }

        /// <summary>
        /// トークンが期限切れかどうかを確認する
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <returns>期限切れの場合true</returns>
        public static bool IsTokenExpired(string token)
        {
            return !IsTokenValid(token);
        }

        /// <summary>
        /// Authorizationヘッダーからトークンを抽出する
        /// </summary>
        /// <param name="authorizationHeader">Authorizationヘッダー値</param>
        /// <returns>トークン</returns>
        public static string? ExtractTokenFromHeader(string? authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader))
                return null;

            if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            return null;
        }
    }
}