using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Api.Common;

namespace Services;

/// <summary>
/// JWT（JSON Web Token）サービス
/// ユーザー認証用のトークンの生成・検証を担当
/// 共通ライブラリのJwtHelperを活用してセキュリティと保守性を向上
/// </summary>
public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly ILogger<JwtService> _logger;

    /// <summary>
    /// JwtServiceのコンストラクタ
    /// 設定値の検証を行い、不正な値の場合は例外を発生させる
    /// </summary>
    /// <param name="secretKey">JWT署名用の秘密鍵（32文字以上推奨）</param>
    /// <param name="issuer">トークン発行者（通常はアプリケーション名）</param>
    /// <param name="audience">トークン対象者（通常はクライアントアプリ）</param>
    /// <param name="logger">ロガー</param>
    /// <exception cref="AppException">設定値が無効な場合</exception>
    public JwtService(string secretKey, string issuer, string audience, ILogger<JwtService> logger)
    {
        // 共通ライブラリを使用して設定値を検証
        JwtHelper.ValidateJwtConfiguration(secretKey, issuer, audience);
        
        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
        _logger = logger;

        // サービス初期化の成功をログ出力
        _logger.LogInformation("JwtService initialized successfully for issuer: {Issuer}", issuer);
    }

    /// <summary>
    /// JWTトークンを生成
    /// ユーザーのログイン成功時に呼び出される
    /// </summary>
    /// <param name="userId">ユーザーID（必須）</param>
    /// <param name="loginId">ログインID（表示用、任意）</param>
    /// <param name="additionalClaims">追加のクレーム（任意）</param>
    /// <param name="expirationHours">有効期限（時間、デフォルト1時間）</param>
    /// <returns>生成されたJWTトークン</returns>
    /// <exception cref="AppException">トークン生成に失敗した場合</exception>
    public string GenerateToken(
        string userId, 
        string loginId, 
        Dictionary<string, string>? additionalClaims = null,
        int expirationHours = ApplicationConstants.JwtSettings.DefaultExpirationHours)
    {
        // 入力値検証
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ユーザーIDが指定されていません");
        }

        if (expirationHours <= 0 || expirationHours > 24)
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "有効期限は1-24時間の範囲で指定してください");
        }

        try
        {
            // 署名用認証情報を生成
            var signingCredentials = JwtHelper.CreateSigningCredentials(_secretKey);

            // 基本クレームを設定
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // ログインIDが指定されている場合は追加
            if (!string.IsNullOrWhiteSpace(loginId))
            {
                claims.Add(new Claim("login_id", loginId));
            }

            // 追加クレームがある場合は追加
            if (additionalClaims != null)
            {
                foreach (var claim in additionalClaims)
                {
                    if (!string.IsNullOrWhiteSpace(claim.Key) && !string.IsNullOrWhiteSpace(claim.Value))
                    {
                        claims.Add(new Claim(claim.Key, claim.Value));
                    }
                }
            }

            // トークンを生成
            var expiration = DateTime.UtcNow.AddHours(expirationHours);
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiration,
                signingCredentials: signingCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // 成功ログ（セキュリティのためトークン本体はログに出力しない）
            _logger.LogInformation("JWT token generated successfully for user: {UserId}, expires: {Expiration}", 
                userId, expiration);

            return tokenString;
        }
        catch (Exception ex) when (!(ex is AppException))
        {
            // 予期しないエラーをログ出力し、AppExceptionとして再スロー
            _logger.LogError(ex, "Failed to generate JWT token for user: {UserId}", userId);
            throw new AppException(ApplicationConstants.ErrorCodes.TokenGenerationFailure, 
                "トークンの生成中にエラーが発生しました", ex);
        }
    }

    /// <summary>
    /// JWTトークンを検証し、ユーザー情報を取得
    /// API呼び出し時の認証に使用される
    /// </summary>
    /// <param name="token">検証するJWTトークン</param>
    /// <returns>検証結果とユーザーID</returns>
    public (bool IsValid, string? UserId) ValidateToken(string token)
    {
        // null や空文字列のチェック
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Token validation failed: Empty token provided");
            return (false, null);
        }

        try
        {
            // 共通ライブラリを使用してトークンを検証
            var (principal, userId) = JwtHelper.ValidateTokenAndExtractClaims(token, _secretKey, _issuer, _audience);

            // 成功ログ（トークン本体は機密情報のためログに含めない）
            _logger.LogDebug("Token validation successful for user: {UserId}", userId);

            return (true, userId);
        }
        catch (AppException ex)
        {
            // 認証エラーは想定内なので、Warningレベルでログ出力
            _logger.LogWarning("Token validation failed: {ErrorMessage}", ex.UserMessage);
            return (false, null);
        }
        catch (Exception ex)
        {
            // 予期しないエラーはErrorレベルでログ出力
            _logger.LogError(ex, "Unexpected error during token validation");
            return (false, null);
        }
    }

    /// <summary>
    /// トークンからクレーム情報を取得
    /// 追加のユーザー情報が必要な場合に使用
    /// </summary>
    /// <param name="token">JWTトークン</param>
    /// <returns>クレーム一覧、無効なトークンの場合はnull</returns>
    public Dictionary<string, string>? GetTokenClaims(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var (principal, _) = JwtHelper.ValidateTokenAndExtractClaims(token, _secretKey, _issuer, _audience);
            
            // クレームを辞書形式で返却
            return principal.Claims.ToDictionary(c => c.Type, c => c.Value);
        }
        catch
        {
            // エラーの場合は詳細をログに記録しつつnullを返却
            _logger.LogDebug("Failed to extract claims from token");
            return null;
        }
    }

    /// <summary>
    /// トークンの有効期限を確認
    /// 期限切れ間近の警告などに使用
    /// </summary>
    /// <param name="token">JWTトークン</param>
    /// <returns>有効期限、無効なトークンの場合はnull</returns>
    public DateTime? GetTokenExpiration(string token)
    {
        return JwtHelper.GetTokenExpiration(token);
    }

    /// <summary>
    /// トークンが期限切れかどうかを判定
    /// 事前チェックに使用
    /// </summary>
    /// <param name="token">JWTトークン</param>
    /// <returns>期限切れかどうか</returns>
    public bool IsTokenExpired(string token)
    {
        return JwtHelper.IsTokenExpired(token);
    }

    /// <summary>
    /// Authorizationヘッダーからトークンを抽出
    /// HTTPリクエストのヘッダー解析に使用
    /// </summary>
    /// <param name="authorizationHeader">Authorizationヘッダーの値</param>
    /// <returns>抽出されたJWTトークン、無効な場合はnull</returns>
    public string? ExtractTokenFromAuthorizationHeader(string? authorizationHeader)
    {
        return JwtHelper.ExtractTokenFromHeader(authorizationHeader);
    }
}
