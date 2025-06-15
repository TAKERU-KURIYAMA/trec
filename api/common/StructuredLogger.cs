using System.Text.Json;

namespace Api.Common
{
    /// <summary>
    /// 構造化ログ出力のヘルパークラス
    /// </summary>
    public static class StructuredLogger
    {
        /// <summary>
        /// 情報ログを出力する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="additionalData">追加データ</param>
        public static void LogInformation(string message, object? additionalData = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Level = "Information",
                Message = message,
                Data = additionalData
            };

            Console.WriteLine(JsonSerializer.Serialize(logEntry));
        }

        /// <summary>
        /// 警告ログを出力する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="additionalData">追加データ</param>
        public static void LogWarning(string message, object? additionalData = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Level = "Warning",
                Message = message,
                Data = additionalData
            };

            Console.WriteLine(JsonSerializer.Serialize(logEntry));
        }

        /// <summary>
        /// エラーログを出力する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="exception">例外</param>
        /// <param name="additionalData">追加データ</param>
        public static void LogError(string message, Exception? exception = null, object? additionalData = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Level = "Error",
                Message = message,
                Exception = exception?.ToString(),
                Data = additionalData
            };

            Console.WriteLine(JsonSerializer.Serialize(logEntry));
        }

        /// <summary>
        /// デバッグログを出力する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="additionalData">追加データ</param>
        public static void LogDebug(string message, object? additionalData = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Level = "Debug",
                Message = message,
                Data = additionalData
            };

            Console.WriteLine(JsonSerializer.Serialize(logEntry));
        }

        /// <summary>
        /// トレースログを出力する
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="additionalData">追加データ</param>
        public static void LogTrace(string message, object? additionalData = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Level = "Trace",
                Message = message,
                Data = additionalData
            };

            Console.WriteLine(JsonSerializer.Serialize(logEntry));
        }

        /// <summary>
        /// リクエスト開始ログを出力する
        /// </summary>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="path">パス</param>
        /// <param name="additionalData">追加データ</param>
        public static void LogRequestStart(string method, string path, object? additionalData = null)
        {
            LogInformation($"Request started: {method} {path}", additionalData);
        }

        /// <summary>
        /// リクエスト終了ログを出力する
        /// </summary>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="path">パス</param>
        /// <param name="statusCode">ステータスコード</param>
        /// <param name="duration">処理時間</param>
        /// <param name="additionalData">追加データ</param>
        public static void LogRequestEnd(string method, string path, int statusCode, TimeSpan duration, object? additionalData = null)
        {
            LogInformation($"Request completed: {method} {path} - {statusCode} ({duration.TotalMilliseconds}ms)", additionalData);
        }

        /// <summary>
        /// 未処理例外ログを出力する
        /// </summary>
        /// <param name="exception">例外</param>
        /// <param name="additionalData">追加データ</param>
        public static void LogUnhandledException(Exception exception, object? additionalData = null)
        {
            LogError("Unhandled exception occurred", exception, additionalData);
        }

        /// <summary>
        /// ビジネスイベントログを出力する
        /// </summary>
        /// <param name="eventName">イベント名</param>
        /// <param name="eventData">イベントデータ</param>
        public static void LogBusinessEvent(string eventName, object? eventData = null)
        {
            LogInformation($"Business event: {eventName}", eventData);
        }
    }
}