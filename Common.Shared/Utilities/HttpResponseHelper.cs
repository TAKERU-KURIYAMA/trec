using Microsoft.AspNetCore.Mvc;
using Common.Shared.Constants;
using Common.Shared.Exceptions;

namespace Common.Shared.Utilities
{
    /// <summary>
    /// HTTP レスポンス生成のヘルパークラス
    /// 統一されたレスポンス形式とエラーハンドリングを提供
    /// </summary>
    public static class HttpResponseHelper
    {
        /// <summary>
        /// 成功レスポンスを生成
        /// データがある場合のOKレスポンス (200)
        /// </summary>
        /// <param name="data">レスポンスに含めるデータ</param>
        /// <returns>OkObjectResult</returns>
        public static OkObjectResult CreateSuccessResponse(object? data = null)
        {
            var response = new
            {
                code = ApplicationConstants.ErrorCodes.Success.Code,
                message = ApplicationConstants.ErrorCodes.Success.Message,
                result = data
            };

            var result = new OkObjectResult(response);
            result.ContentTypes.Add(ApplicationConstants.ContentTypes.Json);
            return result;
        }

        /// <summary>
        /// エラーレスポンスを生成
        /// AppExceptionからエラーレスポンスを作成
        /// </summary>
        /// <param name="exception">AppException</param>
        /// <returns>ObjectResult</returns>
        public static ObjectResult CreateErrorResponse(AppException exception)
        {
            var response = new
            {
                code = exception.ErrorInfo.Code,
                message = exception.ErrorInfo.Message,
                // デバッグ情報は開発環境でのみ含める
#if DEBUG
                debug = new
                {
                    internalCode = exception.InternalCode,
                    innerException = exception.InnerException?.Message
                }
#endif
            };

            var result = new ObjectResult(response)
            {
                StatusCode = (int)exception.ErrorInfo.StatusCode
            };
            result.ContentTypes.Add(ApplicationConstants.ContentTypes.Json);
            return result;
        }

        /// <summary>
        /// 汎用エラーレスポンスを生成
        /// ApiErrorから直接エラーレスポンスを作成
        /// </summary>
        /// <param name="errorInfo">エラー情報</param>
        /// <param name="additionalData">追加データ（任意）</param>
        /// <returns>ObjectResult</returns>
        public static ObjectResult CreateErrorResponse(ApiError errorInfo, object? additionalData = null)
        {
            var response = new
            {
                code = errorInfo.Code,
                message = errorInfo.Message,
                data = additionalData
            };

            var result = new ObjectResult(response)
            {
                StatusCode = (int)errorInfo.StatusCode
            };
            result.ContentTypes.Add(ApplicationConstants.ContentTypes.Json);
            return result;
        }

        /// <summary>
        /// バリデーションエラーレスポンスを生成
        /// モデルバリデーション失敗時の詳細エラー情報を含む
        /// </summary>
        /// <param name="validationErrors">バリデーションエラーの詳細</param>
        /// <returns>BadRequestObjectResult</returns>
        public static BadRequestObjectResult CreateValidationErrorResponse(
            Dictionary<string, string[]> validationErrors)
        {
            var response = new
            {
                code = ApplicationConstants.ErrorCodes.ParameterError.Code,
                message = ApplicationConstants.ErrorCodes.ParameterError.Message,
                validationErrors = validationErrors
            };

            var result = new BadRequestObjectResult(response);
            result.ContentTypes.Add(ApplicationConstants.ContentTypes.Json);
            return result;
        }

        /// <summary>
        /// 認証エラーレスポンスを生成
        /// 401 Unauthorized専用のレスポンス
        /// </summary>
        /// <param name="customMessage">カスタムメッセージ（任意）</param>
        /// <returns>UnauthorizedObjectResult</returns>
        public static UnauthorizedObjectResult CreateUnauthorizedResponse(string? customMessage = null)
        {
            var errorInfo = ApplicationConstants.ErrorCodes.AuthorizationError;
            var response = new
            {
                code = errorInfo.Code,
                message = customMessage ?? errorInfo.Message
            };

            var result = new UnauthorizedObjectResult(response);
            result.ContentTypes.Add(ApplicationConstants.ContentTypes.Json);
            return result;
        }

        /// <summary>
        /// データ未発見レスポンスを生成
        /// 404 Not Found専用のレスポンス
        /// </summary>
        /// <param name="resourceName">見つからなかったリソース名（任意）</param>
        /// <returns>NotFoundObjectResult</returns>
        public static NotFoundObjectResult CreateNotFoundResponse(string? resourceName = null)
        {
            var errorInfo = ApplicationConstants.ErrorCodes.DataNotFound;
            var message = resourceName != null 
                ? $"{resourceName}が見つかりません" 
                : errorInfo.Message;

            var response = new
            {
                code = errorInfo.Code,
                message = message
            };

            var result = new NotFoundObjectResult(response);
            result.ContentTypes.Add(ApplicationConstants.ContentTypes.Json);
            return result;
        }

        /// <summary>
        /// カスタムステータスコードのレスポンスを生成
        /// 任意のHTTPステータスコードでレスポンスを作成
        /// </summary>
        /// <param name="statusCode">HTTPステータスコード</param>
        /// <param name="code">アプリケーションエラーコード</param>
        /// <param name="message">メッセージ</param>
        /// <param name="data">追加データ（任意）</param>
        /// <returns>ObjectResult</returns>
        public static ObjectResult CreateCustomResponse(
            int statusCode, 
            string code, 
            string message, 
            object? data = null)
        {
            var response = new
            {
                code = code,
                message = message,
                result = data
            };

            var result = new ObjectResult(response)
            {
                StatusCode = statusCode
            };
            result.ContentTypes.Add(ApplicationConstants.ContentTypes.Json);
            return result;
        }
    }
}