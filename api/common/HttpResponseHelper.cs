using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Api.Common
{
    /// <summary>
    /// HTTP レスポンス作成のヘルパークラス
    /// </summary>
    public static class HttpResponseHelper
    {
        /// <summary>
        /// 成功レスポンスを作成する
        /// </summary>
        /// <param name="data">レスポンスデータ</param>
        /// <param name="message">メッセージ</param>
        /// <returns>OkObjectResult</returns>
        public static OkObjectResult CreateSuccessResponse(object? data = null, string message = "Success")
        {
            var response = new ApiResponse<object>
            {
                IsSuccess = true,
                Data = data,
                UserMessage = message,
                ErrorCode = null,
                DeveloperMessage = null
            };

            return new OkObjectResult(response);
        }

        /// <summary>
        /// エラーレスポンスを作成する
        /// </summary>
        /// <param name="errorCode">エラーコード</param>
        /// <param name="userMessage">ユーザー向けメッセージ</param>
        /// <param name="developerMessage">開発者向けメッセージ</param>
        /// <param name="statusCode">HTTPステータスコード</param>
        /// <returns>ObjectResult</returns>
        public static ObjectResult CreateErrorResponse(string errorCode, string userMessage, string? developerMessage = null, int statusCode = 400)
        {
            var response = new ApiResponse<object>
            {
                IsSuccess = false,
                Data = null,
                UserMessage = userMessage,
                ErrorCode = errorCode,
                DeveloperMessage = developerMessage
            };

            return new ObjectResult(response)
            {
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// バリデーションエラーレスポンスを作成する
        /// </summary>
        /// <param name="errors">エラー詳細</param>
        /// <param name="userMessage">ユーザー向けメッセージ</param>
        /// <returns>BadRequestObjectResult</returns>
        public static BadRequestObjectResult CreateValidationErrorResponse(object errors, string userMessage = "入力値にエラーがあります")
        {
            var response = new ApiResponse<object>
            {
                IsSuccess = false,
                Data = errors,
                UserMessage = userMessage,
                ErrorCode = ApplicationConstants.ErrorCodes.ParameterError,
                DeveloperMessage = "Validation failed"
            };

            return new BadRequestObjectResult(response);
        }

        /// <summary>
        /// 認証エラーレスポンスを作成する
        /// </summary>
        /// <param name="userMessage">ユーザー向けメッセージ</param>
        /// <returns>UnauthorizedObjectResult</returns>
        public static UnauthorizedObjectResult CreateUnauthorizedResponse(string userMessage = "認証が必要です")
        {
            var response = new ApiResponse<object>
            {
                IsSuccess = false,
                Data = null,
                UserMessage = userMessage,
                ErrorCode = ApplicationConstants.ErrorCodes.AuthenticationError,
                DeveloperMessage = "Authentication required"
            };

            return new UnauthorizedObjectResult(response);
        }

        /// <summary>
        /// 権限エラーレスポンスを作成する
        /// </summary>
        /// <param name="userMessage">ユーザー向けメッセージ</param>
        /// <returns>ObjectResult</returns>
        public static ObjectResult CreateForbiddenResponse(string userMessage = "この操作を実行する権限がありません")
        {
            var response = new ApiResponse<object>
            {
                IsSuccess = false,
                Data = null,
                UserMessage = userMessage,
                ErrorCode = ApplicationConstants.ErrorCodes.AuthorizationError,
                DeveloperMessage = "Forbidden"
            };

            return new ObjectResult(response)
            {
                StatusCode = 403
            };
        }

        /// <summary>
        /// Not Foundレスポンスを作成する
        /// </summary>
        /// <param name="userMessage">ユーザー向けメッセージ</param>
        /// <returns>NotFoundObjectResult</returns>
        public static NotFoundObjectResult CreateNotFoundResponse(string userMessage = "指定されたデータが見つかりません")
        {
            var response = new ApiResponse<object>
            {
                IsSuccess = false,
                Data = null,
                UserMessage = userMessage,
                ErrorCode = ApplicationConstants.ErrorCodes.DataNotFound,
                DeveloperMessage = "Resource not found"
            };

            return new NotFoundObjectResult(response);
        }

        /// <summary>
        /// サーバーエラーレスポンスを作成する
        /// </summary>
        /// <param name="userMessage">ユーザー向けメッセージ</param>
        /// <param name="developerMessage">開発者向けメッセージ</param>
        /// <returns>ObjectResult</returns>
        public static ObjectResult CreateServerErrorResponse(string userMessage = "システムエラーが発生しました", string? developerMessage = null)
        {
            var response = new ApiResponse<object>
            {
                IsSuccess = false,
                Data = null,
                UserMessage = userMessage,
                ErrorCode = ApplicationConstants.ErrorCodes.ServerError,
                DeveloperMessage = developerMessage
            };

            return new ObjectResult(response)
            {
                StatusCode = 500
            };
        }

        /// <summary>
        /// ContentResultを作成する
        /// </summary>
        /// <param name="content">コンテンツ</param>
        /// <param name="contentType">コンテンツタイプ</param>
        /// <returns>ContentResult</returns>
        public static ContentResult CreateContentResult(string content, string contentType)
        {
            return new ContentResult
            {
                Content = content,
                ContentType = contentType
            };
        }
    }
}