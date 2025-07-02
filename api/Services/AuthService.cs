using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Api.Common;
using System.Security.Cryptography;
using System.Text;

namespace Services;

/// <summary>
/// 認証関連のビジネスロジックを担当するサービス
/// ユーザー登録、ログイン、パスワード管理などを提供
/// </summary>
public class AuthService : IAuthService
{
    private readonly TrecPlansRDBContext _dbContext;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// AuthServiceのコンストラクタ
    /// </summary>
    /// <param name="dbContext">データベースコンテキスト</param>
    /// <param name="logger">ロガー</param>
    public AuthService(TrecPlansRDBContext dbContext, ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 新規ユーザーを登録
    /// 入力値検証、重複チェック、セキュアなパスワードハッシュ化を実行
    /// </summary>
    /// <param name="userCommonId">ユーザー共通ID（システム内部用）</param>
    /// <param name="loginId">ログインID（ユーザー入力）</param>
    /// <param name="clientHashedPassword">フロントエンドでSHA256済みのパスワードハッシュ</param>
    /// <param name="displayName">表示名</param>
    /// <returns>登録されたユーザー情報</returns>
    /// <exception cref="AppException">登録に失敗した場合</exception>
    public async Task<User> RegisterUserAsync(string userCommonId, string loginId, string clientHashedPassword, string displayName)
    {
        // 入力値検証
        ValidateUserRegistrationInput(userCommonId, loginId, clientHashedPassword, displayName);

        try
        {
            // ログインIDの重複チェック（データベースレベルでも確認）
            var existingUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.LoginId == loginId);

            if (existingUser != null)
            {
                _logger.LogWarning("ユーザー登録失敗：ログインIDが重複 - LoginId: {LoginId}", loginId);
                throw new AppException(ApplicationConstants.ErrorCodes.LoginIdDuplicate, "このログインIDは既に使用されています");
            }

            // セキュアなパスワードハッシュ化
            var (passwordHash, passwordSalt) = HashPassword(clientHashedPassword);

            // ユーザーエンティティを作成
            var user = new User
            {
                UserCommonId = userCommonId,
                LoginId = loginId,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // 成功ログ（パスワード情報は含めない）
            _logger.LogInformation("ユーザー登録成功 - UserCommonId: {UserCommonId}, LoginId: {LoginId}, DisplayName: {DisplayName}", 
                userCommonId, loginId, displayName);

            // ビジネスイベントログ
            StructuredLogger.LogBusinessEvent("UserRegistered", 
                new { userCommonId, loginId, displayName });

            return user;
        }
        catch (DbUpdateException ex)
        {
            // データベース制約違反などの処理
            _logger.LogError(ex, "ユーザー登録時にデータベースエラーが発生 - LoginId: {LoginId}", loginId);
            
            if (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.LoginIdDuplicate, 
                    "ログインIDが既に使用されています", ex);
            }
            
            throw new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                "ユーザー登録中にエラーが発生しました", ex);
        }
        catch (Exception ex) when (!(ex is AppException))
        {
            _logger.LogError(ex, "ユーザー登録時に予期しないエラーが発生 - LoginId: {LoginId}", loginId);
            throw new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                "ユーザー登録中に予期しないエラーが発生しました", ex);
        }
    }

    /// <summary>
    /// ログイン認証を実行
    /// ログインIDとパスワードを検証し、認証に成功した場合はユーザー情報を返す
    /// </summary>
    /// <param name="loginId">ログインID</param>
    /// <param name="clientHashedPassword">フロントエンドでSHA256済みのパスワードハッシュ</param>
    /// <returns>認証成功時はユーザー情報、失敗時はnull</returns>
    public async Task<User?> AuthenticateUserAsync(string loginId, string clientHashedPassword)
    {
        // 入力値検証
        if (string.IsNullOrWhiteSpace(loginId))
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ログインIDが指定されていません");
        }

        if (string.IsNullOrWhiteSpace(clientHashedPassword))
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "パスワードが指定されていません");
        }

        try
        {
            // ユーザー情報を取得
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.LoginId == loginId);

            if (user == null)
            {
                _logger.LogWarning("ログイン失敗：ユーザーが存在しない - LoginId: {LoginId}", loginId);
                return null;
            }

            // パスワード検証
            if (!VerifyPassword(clientHashedPassword, user.PasswordHash, user.PasswordSalt))
            {
                _logger.LogWarning("ログイン失敗：パスワード不一致 - LoginId: {LoginId}", loginId);
                return null;
            }

            // 認証成功ログ
            _logger.LogInformation("ログイン成功 - UserCommonId: {UserCommonId}, LoginId: {LoginId}", 
                user.UserCommonId, loginId);

            // ビジネスイベントログ
            StructuredLogger.LogBusinessEvent("UserLoggedIn", 
                new { loginId });

            return user;
        }
        catch (Exception ex) when (!(ex is AppException))
        {
            _logger.LogError(ex, "ログイン認証時に予期しないエラーが発生 - LoginId: {LoginId}", loginId);
            throw new AppException(ApplicationConstants.ErrorCodes.ServerError, 
                "認証処理中にエラーが発生しました", ex);
        }
    }

    /// <summary>
    /// ユーザー共通IDの生成
    /// 衝突を避けるために一意性を保証
    /// </summary>
    /// <returns>一意のユーザー共通ID</returns>
    public async Task<string> GenerateUserCommonIdAsync()
    {
        const int maxRetries = ApplicationConstants.RetrySettings.IdGenerationRetryCount;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            // UUIDベースのIDを生成（16文字に短縮）
            var candidateId = Guid.NewGuid().ToString("N").Substring(0, 16); // 最初の16文字のみ使用

            // データベースで一意性を確認
            var exists = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserCommonId == candidateId);

            if (!exists)
            {
                _logger.LogDebug("ユーザー共通ID生成成功 - 試行回数: {Attempt}", attempt);
                return candidateId;
            }

            _logger.LogWarning("ユーザー共通ID重複 - 試行回数: {Attempt}/{MaxRetries}", attempt, maxRetries);
        }

        _logger.LogError("ユーザー共通ID生成失敗 - 最大試行回数に達した: {MaxRetries}", maxRetries);
        throw new AppException(ApplicationConstants.ErrorCodes.IdGenerationFailure, 
            "ユーザーIDの生成に失敗しました");
    }

    /// <summary>
    /// ユーザー登録の入力値検証
    /// ビジネスルールに従った検証を実行
    /// </summary>
    /// <param name="userCommonId">ユーザー共通ID</param>
    /// <param name="loginId">ログインID</param>
    /// <param name="clientHashedPassword">フロントエンドでSHA256済みのパスワードハッシュ</param>
    /// <param name="displayName">表示名</param>
    /// <exception cref="AppException">検証に失敗した場合</exception>
    private static void ValidateUserRegistrationInput(string userCommonId, string loginId, string clientHashedPassword, string displayName)
    {
        if (string.IsNullOrWhiteSpace(userCommonId))
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ユーザー共通IDが指定されていません");
        }

        if (string.IsNullOrWhiteSpace(loginId))
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ログインIDが指定されていません");
        }

        if (string.IsNullOrWhiteSpace(clientHashedPassword))
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "パスワードが指定されていません");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "表示名が指定されていません");
        }

        // 長さチェック
        if (loginId.Length > 32)
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ログインIDは32文字以下で入力してください");
        }

        if (displayName.Length > 64)
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "表示名は64文字以下で入力してください");
        }

        if (clientHashedPassword.Length != 64)
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "パスワードハッシュの形式が正しくありません");
        }

        // 形式チェック（正規表現使用）
        if (!System.Text.RegularExpressions.Regex.IsMatch(loginId, ApplicationConstants.ValidationPatterns.LoginId))
        {
            throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "ログインIDは英数字のみ使用可能です");
        }
    }

    /// <summary>
    /// セキュアなパスワードハッシュ化
    /// フロントエンドでSHA256済みのハッシュ値を受け取り、サーバー側でソルト付きSHA256を実行
    /// </summary>
    /// <param name="clientHashedPassword">フロントエンドでSHA256済みのパスワードハッシュ</param>
    /// <returns>ハッシュ化されたパスワードとソルトのタプル</returns>
    private static (string Hash, string Salt) HashPassword(string clientHashedPassword)
    {
        // ランダムソルトを生成（256ビット）
        var saltBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        
        // ソルトを文字列として扱い、その後Base64エンコード
        var saltString = Convert.ToBase64String(saltBytes);
        var saltForStorage = Convert.ToBase64String(Encoding.UTF8.GetBytes(saltString));

        // クライアント側のハッシュ値 + ソルト文字列をSHA256でハッシュ化
        var combinedBytes = Encoding.UTF8.GetBytes(clientHashedPassword + saltString);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(combinedBytes);
        var hash = Convert.ToBase64String(hashBytes);

        return (hash, saltForStorage);
    }

    /// <summary>
    /// パスワードの検証
    /// フロントエンドでSHA256済みのハッシュ値が保存されたハッシュと一致するかを確認
    /// </summary>
    /// <param name="clientHashedPassword">フロントエンドでSHA256済みのパスワードハッシュ</param>
    /// <param name="storedHash">保存されたハッシュ</param>
    /// <param name="storedSalt">保存されたソルト</param>
    /// <returns>パスワードが一致する場合はtrue</returns>
    private static bool VerifyPassword(string clientHashedPassword, string storedHash, string storedSalt)
    {
        try
        {
            // Base64エンコードされたソルトをデコード
            var saltBytes = Convert.FromBase64String(storedSalt);
            var decodedSalt = Encoding.UTF8.GetString(saltBytes);
            
            // クライアント側のハッシュ値 + デコードされたソルトをSHA256でハッシュ化
            var combinedBytes = Encoding.UTF8.GetBytes(clientHashedPassword + decodedSalt);
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(combinedBytes);
            var computedHash = Convert.ToBase64String(hashBytes);

            // タイミング攻撃を防ぐための定数時間比較
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHash),
                Encoding.UTF8.GetBytes(storedHash)
            );
        }
        catch
        {
            // ハッシュ形式が不正な場合は認証失敗
            return false;
        }
    }
}
