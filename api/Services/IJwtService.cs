namespace Services;

/// <summary>
/// JWT（JSON Web Token）サービスのインターフェース
/// トークンの生成・検証・管理に関するメソッドを定義
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// JWTトークンを生成
    /// </summary>
    /// <param name="userId">ユーザーID（必須）</param>
    /// <param name="loginId">ログインID（任意）</param>
    /// <param name="additionalClaims">追加のクレーム（任意）</param>
    /// <param name="expirationHours">有効期限（時間、デフォルト1時間）</param>
    /// <returns>生成されたJWTトークン</returns>
    string GenerateToken(string userId, string loginId, Dictionary<string, string>? additionalClaims = null, int expirationHours = 1);

    /// <summary>
    /// JWTトークンを検証し、ユーザー情報を取得
    /// </summary>
    /// <param name="token">検証するJWTトークン</param>
    /// <returns>検証結果とユーザーID</returns>
    (bool IsValid, string? UserId) ValidateToken(string token);

    /// <summary>
    /// トークンからクレーム情報を取得
    /// </summary>
    /// <param name="token">JWTトークン</param>
    /// <returns>クレーム一覧、無効なトークンの場合はnull</returns>
    Dictionary<string, string>? GetTokenClaims(string token);

    /// <summary>
    /// トークンの有効期限を確認
    /// </summary>
    /// <param name="token">JWTトークン</param>
    /// <returns>有効期限、無効なトークンの場合はnull</returns>
    DateTime? GetTokenExpiration(string token);

    /// <summary>
    /// トークンが期限切れかどうかを判定
    /// </summary>
    /// <param name="token">JWTトークン</param>
    /// <returns>期限切れかどうか</returns>
    bool IsTokenExpired(string token);

    /// <summary>
    /// Authorizationヘッダーからトークンを抽出
    /// </summary>
    /// <param name="authorizationHeader">Authorizationヘッダーの値</param>
    /// <returns>抽出されたJWTトークン、無効な場合はnull</returns>
    string? ExtractTokenFromAuthorizationHeader(string? authorizationHeader);
}
