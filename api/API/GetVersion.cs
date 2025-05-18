using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Dynamic;


namespace API
{
    /// <summary>
    /// </summary>
    /// <param name="req"></param>
    /// <param name="log"></param>
    public static class GetVersion
    {
        [Function("GetVersion")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "version")] HttpRequest req, FunctionContext context)
        {
            var log = context.GetLogger("GetVersion");

            try
            {
                Logger.Entry(log, req);

                return Terminate(log);
            }
            catch (AppException aex)
            {
                Logger.Error(log, aex);
                return Response.CreateErrorResponse(aex.Cause);
            }
            catch (Exception ex)
            {
                Logger.Error(log, new AppException(FoundationCode.Errors.SERVER_ERROR, ex));
                return Response.CreateErrorResponse(FoundationCode.Errors.SERVER_ERROR);
            }
        }

        /// <summary>
        /// èIóπèàóù
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private static ObjectResult Terminate(ILogger log)
        {

            dynamic resJson = new ExpandoObject();
            resJson.version = "version-1";

            Logger.Exit(log, resJson);
            return Response.CreateOkResponse(resJson);
        }
    }
}


