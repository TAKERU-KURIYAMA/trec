using Azure;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs;
using Services;
using System.Dynamic;
using System.Security.Cryptography;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace API.account
{
    /// <summary>
    /// </summary>
    /// <param name="req"></param>
    /// <param name="log"></param>
    public static class Account
    {
        [FunctionName("PosttAccount")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "account")] HttpRequest req, ILogger log)
        {
            try
            {
                Logger.Entry(log, req);

                using (AuthRDBContext context = new AuthRDBContext(connectionstring))
                {
                    return new OkObjectResult("Inspection-387");
                    return Terminate(log);
                }
            }
            catch (AppException aex)
            {
                Logger.Error(log, aex);
                return Response.CreateErrorResponse(aex.Cause);
            }
            catch (Exception ex)
            {
                return Commonization.Exception(ex, log);
            }
        }

        /// <summary>
        /// èIóπèàóù
        /// </summary>
        /// <param name="log">ÉçÉO</param>
        /// <returns>é¿çsåãâ </returns>
        private static ObjectResult Terminate(ILogger log)
        {
            dynamic resJson = new ExpandoObject();
            Logger.Exit(log);
            return Response.CreateOkResponse(resJson);
        }
    }
}


