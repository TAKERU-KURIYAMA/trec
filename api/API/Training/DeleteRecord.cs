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
    public static class DeleteRecord
    {
        [Function("DeleteTrainingRecord")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "training/record")] HttpRequest req, FunctionContext context)
        {
            var log = context.GetLogger("DeleteTrainingRecord");
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

                    DeleteTrainingRecordSet(DBcontext, input, user, menu);

                    await DBcontext.SaveChangesAsync();
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
                Logger.Error(log, new AppException(FoundationCode.Errors.SERVER_ERROR, ex));
                return Response.CreateErrorResponse(FoundationCode.Errors.SERVER_ERROR);
            }
        }

        private static void DeleteTrainingRecordSet(MessageRDBContext context
            , Input input, User user, TrainingMenu menu)
        {
            TrainingRecordSet? trainingRecordSet = context.TrainingRecordSets.Where(
                    x => x.UserCommonId == user.UserCommonId
                    && x.MenuId == menu.MenuId
                    && x.TrainingDate == input.DateTimeTrainingDate
                    && x.SetNumber == int.Parse(input.SetNumber!)).FirstOrDefault();
            if (trainingRecordSet == null)
            {
                throw new AppException(FoundationCode.Errors.CLIENT_PARAMETER_ERROR, "指定されたセット番号は存在しません");
            }
            else
            {
                context.TrainingRecordSets.Remove(trainingRecordSet);
            }

            context.SaveChanges();
            TrainingUtil.AggregateDailyTrainingRecord(context, input.DateTimeTrainingDate, user, menu!);

            context.SaveChanges();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private static ObjectResult Terminate(ILogger log)
        {
            dynamic resJson = new ExpandoObject();

            Logger.Exit(log, resJson);
            return Response.CreateOkResponse(resJson);
        }

        private sealed class Input
        {
            public string Authorization { get; set; } = string.Empty;
            public string? MenuId { get; set; } = string.Empty;
            public string? TrainingDate { get; set; } = string.Empty;
            public DateOnly DateTimeTrainingDate { get; set; }
            public string? SetNumber { get; set; } = string.Empty;

            public void SetParameter(HttpRequest req)
            {
                Authorization = req.Headers[FoundationCode.Header.AUTHORIZATION].ToString();
                MenuId = req.Query[FoundationCode.Body.MENU_ID].ToString();
                TrainingDate = req.Query[FoundationCode.Body.TRAINING_DATE].ToString();
                SetNumber = req.Query[FoundationCode.Body.SET_NUMBER].ToString();
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
                    throw new AppException(FoundationCode.Errors.CLIENT_PARAMETER_ERROR, "training_dateに使用できない文字が含まれています");
                }

                ValidateUtil.IndispensableParam(SetNumber!, FoundationCode.Body.SET_NUMBER);
                ValidateUtil.FormatParam(SetNumber!, FoundationCode.Body.SET_NUMBER, FoundationCode.Regex.Pattern.SET_NUMBER);
            }
        }
    }
}


