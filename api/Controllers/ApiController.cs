
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using Api.Common;

namespace API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// APIバージョン情報を取得
        /// システムの稼働状況確認とバージョン管理に使用
        /// </summary>
        /// <returns>バージョン情報</returns>
        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            try
            {
                StructuredLogger.LogRequestStart(Request.Method, Request.Path.ToString(), nameof(GetVersion));

                var responseData = new
                {
                    version = "v1.0.0",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    timestamp = DateTimeHelper.GetJstNow()
                };

                StructuredLogger.LogRequestEnd(Request.Method, Request.Path.ToString(), 200, TimeSpan.Zero, responseData);
                return HttpResponseHelper.CreateSuccessResponse(responseData);
            }
            catch (AppException aex)
            {
                StructuredLogger.LogError($"Error in {nameof(GetVersion)}: {aex.UserMessage}", aex, Request.Path.ToString());
                return HttpResponseHelper.CreateErrorResponse(aex.ErrorCode, aex.UserMessage);
            }
            catch (Exception ex)
            {
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, "システムエラーが発生しました", ex);
                StructuredLogger.LogError($"Unhandled error in {nameof(GetVersion)}: {ex.Message}", ex, Request.Path.ToString());
                return HttpResponseHelper.CreateErrorResponse(ApplicationConstants.ErrorCodes.ServerError, "システムエラーが発生しました");
            }
        }
    }
}