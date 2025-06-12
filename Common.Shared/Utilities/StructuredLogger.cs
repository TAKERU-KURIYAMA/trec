using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Common.Shared.Exceptions;

namespace Common.Shared.Utilities
{
    /// <summary>
    /// 構造化ログ出力のヘルパークラス
    /// セキュリティを考慮した統一的なログフォーマットを提供
    /// </summary>
    public static class StructuredLogger
    {
        /// <summary>
        /// センシティブなヘッダー名（ログに出力しない）
        /// </summary>
        private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization",
            "Cookie",
            "Set-Cookie",
            "X-API-Key",
            "X-Auth-Token"
        };

        /// <summary>
        /// センシティブなクエリパラメータ名（ログに出力しない）
        /// </summary>
        private static readonly HashSet<string> SensitiveQueryParams = new(StringComparer.OrdinalIgnoreCase)
        {
            "password",
            "token",
            "api_key",
            "secret",
            "auth"
        };

        /// <summary>
        /// APIリクエスト開始ログを出力
        /// セキュリティを考慮してセンシティブな情報は除外
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="request">HTTPリクエスト</param>
        /// <param name="actionName">実行するアクション名（任意）</param>
        public static void LogRequestStart(ILogger logger, HttpRequest request, string? actionName = null)
        {
            if (logger == null) return;

            try
            {
                var logData = new
                {
                    Event = "RequestStart",
                    Action = actionName,
                    Method = request.Method,
                    Path = request.Path.Value,
                    QueryString = SanitizeQueryString(request.QueryString.Value),
                    ContentType = request.ContentType,
                    ContentLength = request.ContentLength,
                    UserAgent = request.Headers.UserAgent.FirstOrDefault(),
                    RemoteIp = GetClientIpAddress(request),
                    Headers = SanitizeHeaders(request.Headers),
                    Timestamp = DateTimeOffset.UtcNow
                };

                logger.LogInformation("API Request Started: {LogData}", JsonConvert.SerializeObject(logData));
            }
            catch (Exception ex)
            {
                // ログ出力でエラーが発生した場合の最小限のログ
                logger.LogWarning(ex, "Failed to log request start for {Method} {Path}", 
                    request.Method, request.Path.Value);
            }
        }

        /// <summary>
        /// APIリクエスト完了ログを出力
        /// レスポンス時間とステータスコードを記録
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="request">HTTPリクエスト</param>
        /// <param name="statusCode">レスポンスステータスコード</param>
        /// <param name="responseData">レスポンスデータ（任意、センシティブな情報は含めないこと）</param>
        /// <param name="elapsedMilliseconds">処理時間（ミリ秒）</param>
        /// <param name="actionName">実行したアクション名（任意）</param>
        public static void LogRequestEnd(
            ILogger logger, 
            HttpRequest request, 
            int statusCode, 
            object? responseData = null, 
            long? elapsedMilliseconds = null,
            string? actionName = null)
        {
            if (logger == null) return;

            try
            {
                var logData = new
                {
                    Event = "RequestEnd",
                    Action = actionName,
                    Method = request.Method,
                    Path = request.Path.Value,
                    StatusCode = statusCode,
                    ElapsedMs = elapsedMilliseconds,
                    ResponseData = SanitizeResponseData(responseData),
                    Timestamp = DateTimeOffset.UtcNow
                };

                // ステータスコードに応じてログレベルを調整
                var logLevel = statusCode >= 500 ? LogLevel.Error : 
                              statusCode >= 400 ? LogLevel.Warning : 
                              LogLevel.Information;

                logger.Log(logLevel, "API Request Completed: {LogData}", JsonConvert.SerializeObject(logData));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to log request end for {Method} {Path}", 
                    request.Method, request.Path.Value);
            }
        }

        /// <summary>
        /// エラーログを構造化して出力
        /// AppExceptionを詳細にログ出力
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="exception">AppException</param>
        /// <param name="request">HTTPリクエスト（任意）</param>
        /// <param name="additionalContext">追加のコンテキスト情報（任意）</param>
        public static void LogError(
            ILogger logger, 
            AppException exception, 
            HttpRequest? request = null, 
            object? additionalContext = null)
        {
            if (logger == null) return;

            try
            {
                var logData = new
                {
                    Event = "Error",
                    ErrorCode = exception.ErrorInfo.Code,
                    ErrorMessage = exception.ErrorInfo.Message,
                    StatusCode = (int)exception.ErrorInfo.StatusCode,
                    InternalCode = exception.InternalCode,
                    Method = request?.Method,
                    Path = request?.Path.Value,
                    UserAgent = request?.Headers.UserAgent.FirstOrDefault(),
                    RemoteIp = request != null ? GetClientIpAddress(request) : null,
                    InnerException = exception.InnerException?.GetType().Name,
                    InnerMessage = exception.InnerException?.Message,
                    AdditionalContext = additionalContext,
                    StackTrace = exception.StackTrace,
                    Timestamp = DateTimeOffset.UtcNow
                };

                logger.LogError(exception, "Application Error: {LogData}", JsonConvert.SerializeObject(logData));
            }
            catch (Exception ex)
            {
                // ログ出力でエラーが発生した場合のフォールバック
                logger.LogError(ex, "Failed to log structured error. Original error: {OriginalMessage}", 
                    exception.Message);
            }
        }

        /// <summary>
        /// 一般的な例外ログを構造化して出力
        /// AppException以外の例外用
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="exception">例外</param>
        /// <param name="request">HTTPリクエスト（任意）</param>
        /// <param name="context">コンテキスト情報</param>
        public static void LogUnhandledException(
            ILogger logger, 
            Exception exception, 
            HttpRequest? request = null, 
            string? context = null)
        {
            if (logger == null) return;

            try
            {
                var logData = new
                {
                    Event = "UnhandledException",
                    ExceptionType = exception.GetType().Name,
                    Message = exception.Message,
                    Context = context,
                    Method = request?.Method,
                    Path = request?.Path.Value,
                    UserAgent = request?.Headers.UserAgent.FirstOrDefault(),
                    RemoteIp = request != null ? GetClientIpAddress(request) : null,
                    InnerException = exception.InnerException?.GetType().Name,
                    InnerMessage = exception.InnerException?.Message,
                    StackTrace = exception.StackTrace,
                    Timestamp = DateTimeOffset.UtcNow
                };

                logger.LogError(exception, "Unhandled Exception: {LogData}", JsonConvert.SerializeObject(logData));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to log unhandled exception. Original error: {OriginalMessage}", 
                    exception.Message);
            }
        }

        /// <summary>
        /// ビジネスイベントのログを出力
        /// 重要なビジネスロジックの実行をログ記録
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="eventName">イベント名</param>
        /// <param name="data">イベントデータ</param>
        /// <param name="userId">ユーザーID（任意）</param>
        public static void LogBusinessEvent(
            ILogger logger, 
            string eventName, 
            object? data = null, 
            string? userId = null)
        {
            if (logger == null) return;

            try
            {
                var logData = new
                {
                    Event = "BusinessEvent",
                    EventName = eventName,
                    UserId = userId,
                    Data = SanitizeResponseData(data),
                    Timestamp = DateTimeOffset.UtcNow
                };

                logger.LogInformation("Business Event: {LogData}", JsonConvert.SerializeObject(logData));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to log business event: {EventName}", eventName);
            }
        }

        /// <summary>
        /// クエリ文字列からセンシティブな情報を除去
        /// </summary>
        private static string? SanitizeQueryString(string? queryString)
        {
            if (string.IsNullOrEmpty(queryString)) return queryString;

            var sanitized = new StringBuilder();
            var pairs = queryString.TrimStart('?').Split('&');

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0];
                    var value = SensitiveQueryParams.Contains(key) ? "***" : keyValue[1];
                    
                    if (sanitized.Length > 0) sanitized.Append('&');
                    sanitized.Append($"{key}={value}");
                }
            }

            return sanitized.Length > 0 ? "?" + sanitized.ToString() : null;
        }

        /// <summary>
        /// HTTPヘッダーからセンシティブな情報を除去
        /// </summary>
        private static Dictionary<string, string> SanitizeHeaders(IHeaderDictionary headers)
        {
            var sanitized = new Dictionary<string, string>();

            foreach (var header in headers)
            {
                var value = SensitiveHeaders.Contains(header.Key) ? "***" : string.Join(",", header.Value!);
                sanitized[header.Key] = value;
            }

            return sanitized;
        }

        /// <summary>
        /// レスポンスデータからセンシティブな情報を除去
        /// パスワードやトークンなどの機密情報をマスク
        /// </summary>
        private static object? SanitizeResponseData(object? data)
        {
            if (data == null) return null;

            try
            {
                // JSON文字列に変換してからセンシティブなフィールドをマスク
                var json = JsonConvert.SerializeObject(data);
                
                // 一般的なセンシティブフィールドをマスク
                var sensitiveFields = new[] { "password", "token", "secret", "key", "auth" };
                
                foreach (var field in sensitiveFields)
                {
                    var pattern = $"\"{field}\"\\s*:\\s*\"[^\"]*\"";
                    json = System.Text.RegularExpressions.Regex.Replace(
                        json, pattern, $"\"{field}\":\"***\"", 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }

                return JsonConvert.DeserializeObject(json);
            }
            catch
            {
                // JSON変換に失敗した場合は元のデータをそのまま返す
                return data;
            }
        }

        /// <summary>
        /// クライアントのIPアドレスを取得
        /// プロキシ経由の場合も考慮
        /// </summary>
        private static string? GetClientIpAddress(HttpRequest request)
        {
            // X-Forwarded-For ヘッダーを優先（プロキシ経由の場合）
            var forwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // X-Real-IP ヘッダー
            var realIp = request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // 直接接続の場合
            return request.HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}