
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using Common.Shared.Constants;
using Common.Shared.Exceptions;
using Common.Shared.Utilities;

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
                StructuredLogger.LogRequestStart(_logger, Request, nameof(GetVersion));

                var responseData = new
                {
                    version = "v1.0.0",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    timestamp = DateTimeHelper.GetJstNow()
                };

                StructuredLogger.LogRequestEnd(_logger, Request, 200, responseData, null, nameof(GetVersion));
                return HttpResponseHelper.CreateSuccessResponse(responseData);
            }
            catch (AppException aex)
            {
                StructuredLogger.LogError(_logger, aex, Request);
                return HttpResponseHelper.CreateErrorResponse(aex);
            }
            catch (Exception ex)
            {
                var appEx = new AppException(ApplicationConstants.ErrorCodes.ServerError, ex);
                StructuredLogger.LogError(_logger, appEx, Request);
                return HttpResponseHelper.CreateErrorResponse(ApplicationConstants.ErrorCodes.ServerError);
            }
        }
    }
}