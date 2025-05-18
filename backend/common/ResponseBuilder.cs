using Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Utils
{
    public static class ResponseBuilder
    {
        public static IActionResult Success<T>(T data)
        {
            return new OkObjectResult(new ApiResponse<T>
            {
                Code = "0000",
                Message = "成功",
                Data = data
            });
        }

        public static IActionResult Success()
        {
            return new OkObjectResult(new ApiResponse
            {
                Code = "0000",
                Message = "成功"
            });
        }

        public static IActionResult Fail(AppException ex)
        {
            var response = new ApiResponse
            {
                Code = ex.ErrorCode.ToString("D4"),
                Message = ex.Message
            };

            var result = new ObjectResult(response);
            result.StatusCode = (int)ex.StatusCode;
            return result;
        }

        public static IActionResult Fail(Exception ex)
        {
            var response = new ApiResponse
            {
                Code = "9999",
                Message = "不明なエラーが発生しました。"
            };

            var result = new ObjectResult(response);
            result.StatusCode = 500;
            return result;
        }
    }
}
