using System.Net;

namespace Common.Shared.Constants
{
    /// <summary>
    /// アプリケーション全体で使用する定数を定義
    /// マジックナンバーの排除と設定の一元管理を目的とする
    /// </summary>
    public static class ApplicationConstants
    {
        /// <summary>
        /// エラーコードとHTTPステータスの定義
        /// </summary>
        public static class ErrorCodes
        {
            public static readonly ApiError Success = new("00000", HttpStatusCode.OK, "OK");
            public static readonly ApiError ParameterError = new("00001", HttpStatusCode.BadRequest, "パラメータエラー");
            public static readonly ApiError ServerError = new("10001", HttpStatusCode.InternalServerError, "インターナルサーバーエラー");
            public static readonly ApiError LoginIdDuplicate = new("10002", HttpStatusCode.BadRequest, "既に登録されているログインID");
            public static readonly ApiError IdGenerationFailure = new("10003", HttpStatusCode.BadRequest, "ID生成に失敗");
            public static readonly ApiError UserNotFound = new("10004", HttpStatusCode.BadRequest, "ユーザーが存在しない");
            public static readonly ApiError PasswordInvalid = new("10005", HttpStatusCode.BadRequest, "パスワード不一致");
            public static readonly ApiError TokenGenerationFailure = new("10006", HttpStatusCode.BadRequest, "トークン発行に失敗");
            public static readonly ApiError AuthorizationError = new("10007", HttpStatusCode.Unauthorized, "トークン検証に失敗");
            public static readonly ApiError DataNotFound = new("10008", HttpStatusCode.NotFound, "データが見つかりません");
            public static readonly ApiError DuplicateData = new("10009", HttpStatusCode.Conflict, "既に同じデータが存在します");
            public static readonly ApiError InvalidInput = new("10010", HttpStatusCode.BadRequest, "入力値が不正です");
        }

        /// <summary>
        /// リトライ設定
        /// </summary>
        public static class RetrySettings
        {
            public const int IdGenerationRetryCount = 3;
            public const int TokenGenerationRetryCount = 3;
            public const int DatabaseRetryCount = 3;
        }

        /// <summary>
        /// HTTPコンテンツタイプ
        /// </summary>
        public static class ContentTypes
        {
            public const string Json = "application/json";
            public const string FormUrlEncoded = "application/x-www-form-urlencoded";
        }

        /// <summary>
        /// HTTPヘッダー名
        /// </summary>
        public static class Headers
        {
            public const string Authorization = "Authorization";
            public const string AuthSession = "x-auth-session";
            public const string ContentType = "Content-Type";
        }

        /// <summary>
        /// JWT設定
        /// </summary>
        public static class JwtSettings
        {
            public const int DefaultExpirationHours = 1;
            public const int MinSecretKeyLength = 32; // JWTのセキュリティ要件
            public const string BearerPrefix = "Bearer ";
        }

        /// <summary>
        /// トレーニング関連の設定
        /// </summary>
        public static class TrainingSettings
        {
            public const int DefaultDashboardDays = 7; // ダッシュボードで表示する日数
            public const int MaxWeightKg = 999; // 最大重量（kg）
            public const int MaxReps = 999; // 最大回数
            public const int MaxSets = 99; // 最大セット数
        }

        /// <summary>
        /// 入力値検証用の正規表現パターン
        /// </summary>
        public static class ValidationPatterns
        {
            public const string LoginId = @"^[0-9a-zA-Z]{1,32}$";
            public const string Password = @"^[0-9a-zA-Z]{64}$"; // ハッシュ化後のパスワード
            public const string DisplayName = @"^.{1,64}$";
            public const string MenuId = @"^[0-9a-zA-Z_]{1,64}$";
            public const string DateFormat = @"^\d{4}-\d{2}-\d{2}$"; // YYYY-MM-DD
            public const string NumberFormat = @"^\d{1,3}$"; // 1-999の数値
        }
    }

    /// <summary>
    /// エラー情報を格納するレコード型
    /// immutableで型安全なエラー表現
    /// </summary>
    public record ApiError(string Code, HttpStatusCode StatusCode, string Message);
}