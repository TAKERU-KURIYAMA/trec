using Api.common;
using Api.Models;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Dynamic;


namespace API.Training
{
    /// <summary>
    /// </summary>
    /// <param name="req"></param>
    /// <param name="log"></param>
    public static class GetRecord
    {
        [Function("GetTrainingRecord")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "training/record")] HttpRequest req, FunctionContext context)
        {
            var log = context.GetLogger("GetTrainingRecord");
            try
            {
                Logger.Entry(log, req);

                Input input = new Input();
                input.SetParameter(req);
                input.Validation();

                DbContextOptions<MessageRDBContext> options = new DbContextOptionsBuilder<MessageRDBContext>()
                .UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"))
                .Options;
                using (MessageRDBContext DBcontext = new MessageRDBContext(options))
                {
                    AccessToken accessToken = AccessToken.FromToken(input.Authorization);

                    string userCommonId = accessToken.Subject;

                    User user = Utility.GetUserCommonId(DBcontext, userCommonId);

                    TrainingMenu menu = TrainingUtil.ValidateMenuId(DBcontext, input.MenuId!);

                    List<TrainingRecordSet> retList = GetTrainingRecordSet(DBcontext, input, user, menu);

                    await DBcontext.SaveChangesAsync();
                    return Terminate(log, retList);
                }
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

        private static List<TrainingRecordSet> GetTrainingRecordSet(MessageRDBContext context
            , Input input, User user, TrainingMenu menu)
        {
            return context.TrainingRecordSets.Where(
                    x => x.UserCommonId == user.UserCommonId
                    && x.MenuId == menu.MenuId
                    && x.TrainingDate == input.DateTimeTrainingDate).ToList();
        }

        /// <summary>
        /// èIóπèàóù
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private static ObjectResult Terminate(ILogger log, List<TrainingRecordSet> retList)
        {
            dynamic resJson = new ExpandoObject();
            resJson.records = retList.Select(x => new
            {
                x.MenuId,
                x.TrainingDate,
                x.SetNumber,
                x.Reps,
                x.Weight,
                x.CreatedAt,
                x.UpdatedAt
            }).ToList();

            Logger.Exit(log, resJson);
            return Response.CreateOkResponse(resJson);
        }

        private sealed class Input
        {
            public string Authorization { get; set; } = string.Empty;
            public string? MenuId { get; set; } = string.Empty;
            public string? TrainingDate { get; set; } = string.Empty;
            public DateOnly DateTimeTrainingDate { get; set; }

            public void SetParameter(HttpRequest req)
            {
                Authorization = req.Headers[FoundationCode.Header.AUTHORIZATION].ToString();
                MenuId = req.Query[FoundationCode.Body.MENU_ID].ToString();
                TrainingDate = req.Query[FoundationCode.Body.TRAINING_DATE].ToString();
            }

            public void Validation()
            {
                ValidateUtil.IndispensableParam(Authorization, FoundationCode.Header.AUTHORIZATION);

                ValidateUtil.IndispensableParam(MenuId!, FoundationCode.Body.MENU_ID);
                ValidateUtil.FormatParam(MenuId!, FoundationCode.Body.MENU_ID, FoundationCode.Regex.Pattern.MENU_ID);

                ValidateUtil.IndispensableParam(TrainingDate!, FoundationCode.Body.TRAINING_DATE);
                ValidateUtil.FormatParam(TrainingDate!, FoundationCode.Body.TRAINING_DATE, FoundationCode.Regex.Pattern.TRAINING_DATE);
                if (DateOnly.TryParse(TrainingDate!, out DateOnly outPut) && outPut <= DateOnly.FromDateTime(Utility.GetJstNow().Date))
                {
                    DateTimeTrainingDate = outPut;
                }
                else 
                {
                    throw new AppException(FoundationCode.Errors.CLIENT_PARAMETER_ERROR, "training_dateÇ…égópÇ≈Ç´Ç»Ç¢ï∂éöÇ™ä‹Ç‹ÇÍÇƒÇ¢Ç‹Ç∑");
                }
            }
        }
    }
}


