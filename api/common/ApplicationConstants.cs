using System;

namespace Api.Common
{
    /// <summary>
    /// アプリケーション全体で使用する定数
    /// </summary>
    public static class ApplicationConstants
    {
        /// <summary>
        /// エラーコード定数
        /// </summary>
        public static class ErrorCodes
        {
            /// <summary>
            /// 一般的なシステムエラー
            /// </summary>
            public const string SystemError = "10001";

            /// <summary>
            /// サーバーエラー
            /// </summary>
            public const string ServerError = "10001";

            /// <summary>
            /// パラメータエラー（入力値不正等）
            /// </summary>
            public const string ParameterError = "00001";

            /// <summary>
            /// 認証エラー
            /// </summary>
            public const string AuthenticationError = "10007";

            /// <summary>
            /// データが見つからない
            /// </summary>
            public const string DataNotFound = "10008";

            /// <summary>
            /// ユーザーが見つからない
            /// </summary>
            public const string UserNotFound = "10008";

            /// <summary>
            /// ログインID重複エラー
            /// </summary>
            public const string LoginIdDuplicate = "10009";

            /// <summary>
            /// ログインIDまたはパスワードが無効
            /// </summary>
            public const string LoginIdOrPasswordInvalid = "10012";

            /// <summary>
            /// トークン生成失敗
            /// </summary>
            public const string TokenGenerationFailure = "10010";

            /// <summary>
            /// ID生成失敗
            /// </summary>
            public const string IdGenerationFailure = "10011";

            /// <summary>
            /// データベース接続エラー
            /// </summary>
            public const string DatabaseError = "10002";

            /// <summary>
            /// 認可エラー（権限不足）
            /// </summary>
            public const string AuthorizationError = "10009";

            /// <summary>
            /// データ重複エラー
            /// </summary>
            public const string DuplicateDataError = "10010";

            /// <summary>
            /// 認証が必要（Unauthorized）
            /// </summary>
            public const string Unauthorized = "10013";

            /// <summary>
            /// 内部エラー（InternalError）
            /// </summary>
            public const string InternalError = "10014";
        }

        /// <summary>
        /// JWT関連の定数
        /// </summary>
        public static class JwtConstants
        {
            /// <summary>
            /// アクセストークンの有効期間（分）
            /// </summary>
            public const int AccessTokenExpiryMinutes = 15;

            /// <summary>
            /// リフレッシュトークンの有効期間（分）
            /// </summary>
            public const int RefreshTokenExpiryMinutes = 20160; // 14日
        }

        /// <summary>
        /// JWT設定（旧来の互換性のため）
        /// </summary>
        public static class JwtSettings
        {
            /// <summary>
            /// デフォルトの有効期間（時間）
            /// </summary>
            public const int DefaultExpirationHours = 24;
        }

        /// <summary>
        /// データベース関連の定数
        /// </summary>
        public static class DatabaseConstants
        {
            /// <summary>
            /// 接続タイムアウト（秒）
            /// </summary>
            public const int ConnectionTimeoutSeconds = 30;

            /// <summary>
            /// コマンドタイムアウト（秒）
            /// </summary>
            public const int CommandTimeoutSeconds = 300;
        }

        /// <summary>
        /// リトライ設定
        /// </summary>
        public static class RetrySettings
        {
            /// <summary>
            /// 最大リトライ回数
            /// </summary>
            public const int MaxRetries = 3;

            /// <summary>
            /// リトライ間隔（ミリ秒）
            /// </summary>
            public const int DelayMs = 100;

            /// <summary>
            /// ID生成のリトライ回数
            /// </summary>
            public const int IdGenerationRetryCount = 5;

            /// <summary>
            /// データベースのリトライ回数
            /// </summary>
            public const int DatabaseRetryCount = 3;
        }

        /// <summary>
        /// バリデーションパターン
        /// </summary>
        public static class ValidationPatterns
        {
            /// <summary>
            /// ログインIDのパターン
            /// </summary>
            public const string LoginId = @"^[a-zA-Z0-9_]{3,50}$";

            /// <summary>
            /// パスワードのパターン
            /// </summary>
            public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{8,}$";
        }

        /// <summary>
        /// 管理者設定
        /// </summary>
        public static class AdminSettings
        {
            /// <summary>
            /// 管理者のログインID（軌道に乗るまでの暫定措置）
            /// </summary>
            public const string AdminLoginId = "admin";
        }

        /// <summary>
        /// コンテンツタイプ
        /// </summary>
        public static class ContentTypes
        {
            /// <summary>
            /// JSON形式
            /// </summary>
            public const string ApplicationJson = "application/json";

            /// <summary>
            /// JSON形式（短縮名）
            /// </summary>
            public const string Json = "application/json";

            /// <summary>
            /// プレーンテキスト
            /// </summary>
            public const string TextPlain = "text/plain";
        }
    }
}