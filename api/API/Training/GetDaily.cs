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
    public static class GetDaily
    {
        [Function("GetTrainingDaily")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "training/daily")] HttpRequest req, FunctionContext context)
        {
            var log = context.GetLogger("GetTrainingDaily");
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

                    List<DailyTrainingRecord> retList = GetDailyTrainingRecord(DBcontext, input, user, menu);

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

        private static List<DailyTrainingRecord> GetDailyTrainingRecord(MessageRDBContext context
            , Input input, User user, TrainingMenu menu)
        {
            return context.DailyTrainingRecords.Where(
                    x => x.UserCommonId == user.UserCommonId
                    && x.MenuId == menu.MenuId
                    && x.TrainingDate >= input.DateTimeFromDate
                    && x.TrainingDate <= input.DateTimeToDate).ToList();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private static ObjectResult Terminate(ILogger log, List<DailyTrainingRecord> retList)
        {
            dynamic resJson = new ExpandoObject();
            resJson.records = retList.Select(x => new
            {
                x.TrainingDate,
                x.SetCount,
                x.MaxReps,
                x.MaxRepsWeight,
                x.MaxWeight,
                x.TotalLoadAmount,
                x.TotalReps
            }).ToList();

            Logger.Exit(log, resJson);
            return Response.CreateOkResponse(resJson);
        }

        private sealed class Input
        {
            public string Authorization { get; set; } = string.Empty;
            public string? MenuId { get; set; } = string.Empty;
            public string? FromDate { get; set; } = string.Empty;
            public DateOnly DateTimeFromDate { get; set; }
            public string? ToDate { get; set; } = string.Empty;
            public DateOnly DateTimeToDate { get; set; }

            public void SetParameter(HttpRequest req)
            {
                Authorization = req.Headers[FoundationCode.Header.AUTHORIZATION].ToString();
                MenuId = req.Query[FoundationCode.Body.MENU_ID].ToString();
                FromDate = req.Query[FoundationCode.Body.FROM_DATE].ToString();
                ToDate = req.Query[FoundationCode.Body.TO_DATE].ToString();

                if (string.IsNullOrEmpty(ToDate))
                {
                    ToDate = DateOnly.FromDateTime(Utility.GetJstNow().Date).ToString("yyyy-MM-dd");
                }

                if (string.IsNullOrEmpty(FromDate))
                {
                    FromDate = DateOnly.FromDateTime(Utility.GetJstNow().Date).AddMonths(-1).ToString("yyyy-MM-dd");
                }
            }

            public void Validation()
            {
                ValidateUtil.IndispensableParam(Authorization, FoundationCode.Header.AUTHORIZATION);

                ValidateUtil.IndispensableParam(MenuId!, FoundationCode.Body.MENU_ID);
                ValidateUtil.FormatParam(MenuId!, FoundationCode.Body.MENU_ID, FoundationCode.Regex.Pattern.MENU_ID);

                ValidateUtil.FormatParam(FromDate!, FoundationCode.Body.FROM_DATE, FoundationCode.Regex.Pattern.FROM_DATE);
                if (DateOnly.TryParse(FromDate!, out DateOnly fromOutPut) && fromOutPut <= DateOnly.FromDateTime(Utility.GetJstNow().Date))
                {
                    DateTimeFromDate = fromOutPut;
                }
                else 
                {
                    throw new AppException(FoundationCode.Errors.CLIENT_PARAMETER_ERROR, "from_dateに使用できない文字が含まれています");
                }

                ValidateUtil.FormatParam(ToDate!, FoundationCode.Body.TO_DATE, FoundationCode.Regex.Pattern.TO_DATE);
                if (DateOnly.TryParse(ToDate!, out DateOnly toOutPut) && toOutPut <= DateOnly.FromDateTime(Utility.GetJstNow().Date))
                {
                    DateTimeToDate = toOutPut;
                }
                else
                {
                    throw new AppException(FoundationCode.Errors.CLIENT_PARAMETER_ERROR, "to_dateに使用できない文字が含まれています");
                }
                if (DateTimeFromDate > DateTimeToDate || (DateTimeToDate.DayNumber - DateTimeFromDate.DayNumber) >= 366)
                {
                    throw new AppException(FoundationCode.Errors.CLIENT_PARAMETER_ERROR, "日付設定が不正です");
                }
            }
        }
    }
}


