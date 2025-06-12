using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Common.Shared.Constants;
using Common.Shared.Exceptions;

namespace Common.Shared.Utilities
{
    /// <summary>
    /// JWT（JSON Web Token）操作のヘルパークラス
    /// トークンの生成、検証、クレーム抽出などの共通処理を提供
    /// </summary>
    public static class JwtHelper
    {
        /// <summary>
        /// JWT設定の検証
        /// セキュリティ要件を満たしているかチェック
        /// </summary>
        /// <param name="secretKey">秘密鍵</param>
        /// <param name="issuer">発行者</param>
        /// <param name="audience">対象者</param>
        /// <exception cref="AppException">設定が無効な場合</exception>
        public static void ValidateJwtConfiguration(string secretKey, string issuer, string audience)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "JWT秘密鍵が設定されていません");
            }

            if (secretKey.Length < ApplicationConstants.JwtSettings.MinSecretKeyLength)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"JWT秘密鍵は{ApplicationConstants.JwtSettings.MinSecretKeyLength}文字以上である必要があります");
            }

            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "JWT発行者が設定されていません");
            }

            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "JWT対象者が設定されていません");
            }
        }

        /// <summary>
        /// セキュリティキーを生成
        /// 秘密鍵からSymmetricSecurityKeyを作成
        /// </summary>
        /// <param name="secretKey">秘密鍵</param>
        /// <returns>SymmetricSecurityKey</returns>
        public static SymmetricSecurityKey CreateSecurityKey(string secretKey)
        {
            ValidateJwtConfiguration(secretKey, "dummy", "dummy"); // 秘密鍵のみ検証
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }

        /// <summary>
        /// 署名用の認証情報を生成
        /// HMAC-SHA256アルゴリズムを使用
        /// </summary>
        /// <param name="secretKey">秘密鍵</param>
        /// <returns>SigningCredentials</returns>
        public static SigningCredentials CreateSigningCredentials(string secretKey)
        {
            var key = CreateSecurityKey(secretKey);
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        /// <summary>
        /// トークン検証パラメータを生成
        /// 厳密な検証ルールを設定
        /// </summary>
        /// <param name="secretKey">秘密鍵</param>
        /// <param name="issuer">発行者</param>
        /// <param name="audience">対象者</param>
        /// <param name="clockSkew">時刻のずれ許容値（デフォルト: 0秒）</param>
        /// <returns>TokenValidationParameters</returns>
        public static TokenValidationParameters CreateValidationParameters(
            string secretKey, 
            string issuer, 
            string audience,
            TimeSpan? clockSkew = null)
        {
            ValidateJwtConfiguration(secretKey, issuer, audience);

            return new TokenValidationParameters
            {
                // 署名検証を有効化
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = CreateSecurityKey(secretKey),

                // 発行者検証を有効化
                ValidateIssuer = true,
                ValidIssuer = issuer,

                // 対象者検証を有効化  
                ValidateAudience = true,
                ValidAudience = audience,

                // 有効期限検証を有効化
                ValidateLifetime = true,
                
                // 時刻のずれ許容値（厳密に設定）
                ClockSkew = clockSkew ?? TimeSpan.Zero,

                // クレーム名の設定
                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = ClaimTypes.Role
            };
        }

        /// <summary>
        /// JWTトークンから安全にクレームを抽出
        /// トークンの検証とクレーム取得を一度に実行
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <param name="secretKey">秘密鍵</param>
        /// <param name="issuer">発行者</param>
        /// <param name="audience">対象者</param>
        /// <returns>クレームとユーザーIDのタプル</returns>
        /// <exception cref="AppException">トークンが無効な場合</exception>
        public static (ClaimsPrincipal Principal, string UserId) ValidateTokenAndExtractClaims(
            string token, 
            string secretKey, 
            string issuer, 
            string audience)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = CreateValidationParameters(secretKey, issuer, audience);

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                
                // ユーザーIDを抽出（sub クレーム）
                var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (string.IsNullOrEmpty(userId))
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.AuthorizationError, 
                        "トークンにユーザーIDが含まれていません");
                }

                return (principal, userId);
            }
            catch (SecurityTokenException ex)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.AuthorizationError, 
                    "トークンの検証に失敗しました", ex);
            }
            catch (ArgumentException ex)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.AuthorizationError, 
                    "トークンの形式が不正です", ex);
            }
        }

        /// <summary>
        /// トークンの単純な検証（有効/無効の判定のみ）
        /// クレームの詳細が不要な場合に使用
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <param name="secretKey">秘密鍵</param>
        /// <param name="issuer">発行者</param>
        /// <param name="audience">対象者</param>
        /// <returns>トークンが有効かどうか</returns>
        public static bool IsTokenValid(string token, string secretKey, string issuer, string audience)
        {
            try
            {
                ValidateTokenAndExtractClaims(token, secretKey, issuer, audience);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// トークンの有効期限を取得
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <returns>有効期限（UTC）、無効なトークンの場合はnull</returns>
        public static DateTime? GetTokenExpiration(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                return jsonToken.ValidTo;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// トークンが期限切れかどうかを判定
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <param name="clockSkew">時刻のずれ許容値（デフォルト: 0秒）</param>
        /// <returns>期限切れかどうか</returns>
        public static bool IsTokenExpired(string token, TimeSpan? clockSkew = null)
        {
            var expiration = GetTokenExpiration(token);
            if (!expiration.HasValue) return true;

            var now = DateTime.UtcNow;
            var allowedSkew = clockSkew ?? TimeSpan.Zero;
            
            return expiration.Value.Add(allowedSkew) <= now;
        }

        /// <summary>
        /// Authorizationヘッダーからトークンを抽出
        /// "Bearer "プレフィックスを除去
        /// </summary>
        /// <param name="authorizationHeader">Authorizationヘッダーの値</param>
        /// <returns>JWTトークン、無効な場合はnull</returns>
        public static string? ExtractTokenFromHeader(string? authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader))
                return null;

            if (!authorizationHeader.StartsWith(ApplicationConstants.JwtSettings.BearerPrefix, 
                StringComparison.OrdinalIgnoreCase))
                return null;

            var token = authorizationHeader.Substring(ApplicationConstants.JwtSettings.BearerPrefix.Length).Trim();
            return string.IsNullOrWhiteSpace(token) ? null : token;
        }

        /// <summary>
        /// トークンのクレームを安全に取得
        /// 指定されたクレームタイプの値を取得し、存在しない場合はデフォルト値を返す
        /// </summary>
        /// <param name="principal">ClaimsPrincipal</param>
        /// <param name="claimType">クレームタイプ</param>
        /// <param name="defaultValue">デフォルト値</param>
        /// <returns>クレームの値</returns>
        public static string GetClaimValue(ClaimsPrincipal principal, string claimType, string defaultValue = "")
        {
            return principal.FindFirstValue(claimType) ?? defaultValue;
        }

        /// <summary>
        /// トークンのデバッグ情報を取得（開発用）
        /// 本番環境では使用しないこと
        /// </summary>
        /// <param name="token">JWTトークン</param>
        /// <returns>デバッグ情報</returns>
        public static object? GetTokenDebugInfo(string token)
        {
#if DEBUG
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                
                return new
                {
                    Header = jsonToken.Header,
                    Claims = jsonToken.Claims.Select(c => new { c.Type, c.Value }),
                    ValidFrom = jsonToken.ValidFrom,
                    ValidTo = jsonToken.ValidTo,
                    Issuer = jsonToken.Issuer,
                    Audiences = jsonToken.Audiences
                };
            }
            catch
            {
                return null;
            }
#else
            return "Debug info only available in development environment";
#endif
        }
    }
}