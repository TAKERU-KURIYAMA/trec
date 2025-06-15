using System;

namespace Api.Common
{
    /// <summary>
    /// アプリケーション固有の例外クラス
    /// ビジネスロジックエラーやAPI固有のエラーを表現
    /// </summary>
    public class AppException : Exception
    {
        /// <summary>
        /// エラーコード
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// ユーザー向けメッセージ
        /// </summary>
        public string UserMessage { get; }

        /// <summary>
        /// 詳細情報（デバッグ用）
        /// </summary>
        public object? Details { get; }

        /// <summary>
        /// エラー情報（互換性のため）
        /// </summary>
        public object ErrorInfo => new { ErrorCode, UserMessage, Details };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="errorCode">エラーコード</param>
        /// <param name="userMessage">ユーザー向けメッセージ</param>
        /// <param name="details">詳細情報（任意）</param>
        /// <param name="innerException">内部例外（任意）</param>
        public AppException(
            string errorCode, 
            string userMessage, 
            object? details = null, 
            Exception? innerException = null)
            : base($"[{errorCode}] {userMessage}", innerException)
        {
            ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
            UserMessage = userMessage ?? throw new ArgumentNullException(nameof(userMessage));
            Details = details;
        }

        /// <summary>
        /// システムエラー用のファクトリメソッド
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        /// <param name="innerException">内部例外</param>
        /// <returns>システムエラーのAppException</returns>
        public static AppException CreateSystemError(string message, Exception? innerException = null)
        {
            return new AppException(
                ApplicationConstants.ErrorCodes.SystemError,
                message,
                null,
                innerException
            );
        }

        /// <summary>
        /// パラメータエラー用のファクトリメソッド
        /// </summary>
        /// <param name="parameterName">パラメータ名</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns>パラメータエラーのAppException</returns>
        public static AppException CreateParameterError(string parameterName, string? message = null)
        {
            var userMessage = message ?? $"パラメータ '{parameterName}' の値が不正です";
            return new AppException(
                ApplicationConstants.ErrorCodes.ParameterError,
                userMessage,
                new { ParameterName = parameterName }
            );
        }

        /// <summary>
        /// データ未発見エラー用のファクトリメソッド
        /// </summary>
        /// <param name="resourceType">リソースの種類</param>
        /// <param name="identifier">識別子</param>
        /// <returns>データ未発見エラーのAppException</returns>
        public static AppException CreateDataNotFoundError(string resourceType, string identifier)
        {
            return new AppException(
                ApplicationConstants.ErrorCodes.DataNotFound,
                $"{resourceType}が見つかりません",
                new { ResourceType = resourceType, Identifier = identifier }
            );
        }

        /// <summary>
        /// 認証エラー用のファクトリメソッド
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        /// <returns>認証エラーのAppException</returns>
        public static AppException CreateAuthenticationError(string? message = null)
        {
            return new AppException(
                ApplicationConstants.ErrorCodes.AuthenticationError,
                message ?? "認証に失敗しました"
            );
        }

        /// <summary>
        /// 認可エラー用のファクトリメソッド
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        /// <returns>認可エラーのAppException</returns>
        public static AppException CreateAuthorizationError(string? message = null)
        {
            return new AppException(
                ApplicationConstants.ErrorCodes.AuthorizationError,
                message ?? "この操作を実行する権限がありません"
            );
        }
    }
}