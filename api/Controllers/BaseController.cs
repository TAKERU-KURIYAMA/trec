using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Api.Common;

namespace API.Controllers
{
    /// <summary>
    /// 全APIコントローラーの基底クラス
    /// 認証ユーザー情報の取得や共通エラーハンドリングを提供
    /// </summary>
    [ApiController]
    [Authorize] // デフォルトで認証必須
    public abstract class BaseController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected BaseController(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 現在認証されているユーザーのIDを取得
        /// </summary>
        /// <returns>ユーザーID（認証されていない場合はnull）</returns>
        protected string? GetCurrentUserId()
        {
            return User?.FindFirstValue("user_id");
        }

        /// <summary>
        /// 現在認証されているユーザーのログインIDを取得
        /// </summary>
        /// <returns>ログインID（認証されていない場合はnull）</returns>
        protected string? GetCurrentUserLoginId()
        {
            return User?.FindFirstValue("login_id");
        }

        /// <summary>
        /// 現在認証されているユーザーの表示名を取得
        /// </summary>
        /// <returns>表示名（認証されていない場合はnull）</returns>
        protected string? GetCurrentUserDisplayName()
        {
            return User?.FindFirstValue("display_name");
        }

        /// <summary>
        /// 認証されているユーザーIDを取得（認証されていない場合は例外）
        /// </summary>
        /// <returns>ユーザーID</returns>
        /// <exception cref="UnauthorizedAccessException">認証されていない場合</exception>
        protected string GetRequiredUserId()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("ユーザー認証が必要です");
            }
            return userId;
        }

        /// <summary>
        /// 現在のユーザーが管理者かどうかを判定
        /// </summary>
        /// <returns>管理者の場合true</returns>
        protected bool IsAdmin()
        {
            var loginId = GetCurrentUserLoginId();
            return loginId == ApplicationConstants.AdminSettings.AdminLoginId;
        }

        /// <summary>
        /// 管理者権限が必要な操作の事前チェック
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">管理者権限がない場合</exception>
        protected void RequireAdmin()
        {
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("管理者権限が必要です");
            }
        }

        /// <summary>
        /// 共通エラーレスポンスを生成
        /// </summary>
        /// <param name="errorCode">エラーコード</param>
        /// <param name="message">エラーメッセージ</param>
        /// <returns>エラーレスポンス</returns>
        protected IActionResult CreateErrorResponse(string errorCode, string message)
        {
            _logger.LogError("API Error: {ErrorCode} - {Message}", errorCode, message);
            return HttpResponseHelper.CreateErrorResponse(errorCode, message);
        }

        /// <summary>
        /// 成功レスポンスを生成
        /// </summary>
        /// <param name="data">レスポンスデータ</param>
        /// <returns>成功レスポンス</returns>
        protected IActionResult CreateSuccessResponse(object data)
        {
            var response = HttpResponseHelper.CreateSuccessResponse(data);
            response.StatusCode = StatusCodes.Status200OK;
            return response;
        }

        /// <summary>
        /// バリデーションエラーレスポンスを生成
        /// </summary>
        /// <returns>バリデーションエラーレスポンス</returns>
        protected IActionResult CreateValidationErrorResponse()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? new string[0]
                    );

                _logger.LogWarning("Validation failed: {@ValidationErrors}", errors);
                
                return CreateErrorResponse(
                    ApplicationConstants.ErrorCodes.ParameterError,
                    "入力値に不正があります: " + string.Join(", ", errors.Values.SelectMany(v => v))
                );
            }

            return CreateErrorResponse(ApplicationConstants.ErrorCodes.ParameterError, "入力値が不正です");
        }

        /// <summary>
        /// 例外からエラーレスポンスを生成
        /// </summary>
        /// <param name="ex">例外</param>
        /// <param name="context">コンテキスト情報</param>
        /// <returns>エラーレスポンス</returns>
        protected IActionResult HandleException(Exception ex, string context)
        {
            _logger.LogError(ex, "Unhandled exception in {Context}", context);

            return ex switch
            {
                UnauthorizedAccessException => CreateErrorResponse(
                    ApplicationConstants.ErrorCodes.Unauthorized, 
                    "認証が必要です"
                ),
                ArgumentException argEx => CreateErrorResponse(
                    ApplicationConstants.ErrorCodes.ParameterError,
                    $"パラメーターエラー: {argEx.Message}"
                ),
                AppException appEx => CreateErrorResponse(
                    appEx.ErrorCode,
                    appEx.Message
                ),
                _ => CreateErrorResponse(
                    ApplicationConstants.ErrorCodes.InternalError,
                    "内部エラーが発生しました"
                )
            };
        }
    }
}