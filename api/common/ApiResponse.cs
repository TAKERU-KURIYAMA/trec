using System.Text.Json.Serialization;

namespace Api.Common
{
    /// <summary>
    /// API レスポンスの統一フォーマット
    /// </summary>
    /// <typeparam name="T">データの型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 成功フラグ
        /// </summary>
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        /// <summary>
        /// レスポンスデータ
        /// </summary>
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        /// <summary>
        /// ユーザー向けメッセージ
        /// </summary>
        [JsonPropertyName("userMessage")]
        public string? UserMessage { get; set; }

        /// <summary>
        /// エラーコード
        /// </summary>
        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 開発者向けメッセージ
        /// </summary>
        [JsonPropertyName("developerMessage")]
        public string? DeveloperMessage { get; set; }

        /// <summary>
        /// タイムスタンプ
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 成功レスポンスを作成する
        /// </summary>
        /// <param name="data">データ</param>
        /// <param name="message">メッセージ</param>
        /// <returns>成功レスポンス</returns>
        public static ApiResponse<T> Success(T? data = default, string message = "Success")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Data = data,
                UserMessage = message
            };
        }

        /// <summary>
        /// エラーレスポンスを作成する
        /// </summary>
        /// <param name="errorCode">エラーコード</param>
        /// <param name="userMessage">ユーザー向けメッセージ</param>
        /// <param name="developerMessage">開発者向けメッセージ</param>
        /// <returns>エラーレスポンス</returns>
        public static ApiResponse<T> Error(string errorCode, string userMessage, string? developerMessage = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Data = default,
                UserMessage = userMessage,
                ErrorCode = errorCode,
                DeveloperMessage = developerMessage
            };
        }
    }

    /// <summary>
    /// 型指定なしのAPIレスポンス
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        /// <summary>
        /// 成功レスポンスを作成する
        /// </summary>
        /// <param name="data">データ</param>
        /// <param name="message">メッセージ</param>
        /// <returns>成功レスポンス</returns>
        public static new ApiResponse Success(object? data = null, string message = "Success")
        {
            return new ApiResponse
            {
                IsSuccess = true,
                Data = data,
                UserMessage = message
            };
        }

        /// <summary>
        /// エラーレスポンスを作成する
        /// </summary>
        /// <param name="errorCode">エラーコード</param>
        /// <param name="userMessage">ユーザー向けメッセージ</param>
        /// <param name="developerMessage">開発者向けメッセージ</param>
        /// <returns>エラーレスポンス</returns>
        public static new ApiResponse Error(string errorCode, string userMessage, string? developerMessage = null)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Data = null,
                UserMessage = userMessage,
                ErrorCode = errorCode,
                DeveloperMessage = developerMessage
            };
        }
    }
}