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
    public static class PostRecord
    {
        [Function("PostTrainingRecord")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "training/record")] HttpRequest req, FunctionContext context)
        {
            var log = context.GetLogger("PostTrainingRecord");
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

                    TrainingRecordSet trainingRecordSet = RegisterTrainingRecordSet(DBcontext, input, user, menu);

                    await DBcontext.SaveChangesAsync();
                    return Terminate(log, trainingRecordSet);
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

        private static TrainingRecordSet RegisterTrainingRecordSet(MessageRDBContext context
            , Input input, User user, TrainingMenu menu)
        {
            bool weightParsed = int.TryParse(input.Weight, out var parsedWeight);

            TrainingRecordSet? trainingRecordSet = context.TrainingRecordSets.Where(
                x => x.UserCommonId == user.UserCommonId
                && x.MenuId == menu.MenuId
                && x.TrainingDate == input.DateTimeTrainingDate
                && x.SetNumber == int.Parse(input.SetNumber!)).FirstOrDefault();

            if (trainingRecordSet == null )
            {
                trainingRecordSet = new TrainingRecordSet
                {
                    UserCommonId = user.UserCommonId,
                    MenuId = menu.MenuId,
                    TrainingDate = input.DateTimeTrainingDate,
                    SetNumber = int.Parse(input.SetNumber!),
                    Reps = int.Parse(input.Reps!),
                    Weight = weightParsed ? parsedWeight : null,
                    CreatedAt = Utility.GetJstNow(),
                    UpdatedAt = Utility.GetJstNow()
                };
                context.TrainingRecordSets.Add(trainingRecordSet);
            }
            else
            {
                trainingRecordSet.SetNumber = int.Parse(input.SetNumber!);
                trainingRecordSet.Reps = int.Parse(input.Reps!);
                trainingRecordSet.Weight = weightParsed ? parsedWeight : null;
                trainingRecordSet.UpdatedAt = Utility.GetJstNow();
            } 
            context.SaveChanges();
            TrainingUtil.AggregateDailyTrainingRecord(context, input.DateTimeTrainingDate, user, menu!);

            context.SaveChanges();
            return trainingRecordSet;
        }

        /// <summary>
        /// èIóπèàóù
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private static ObjectResult Terminate(ILogger log, TrainingRecordSet trainingRecordSet)
        {
            dynamic resJson = new ExpandoObject();
            resJson.training_record_set = new
            {
                trainingRecordSet.UserCommonId,
                trainingRecordSet.MenuId,
                trainingRecordSet.TrainingDate,
                trainingRecordSet.SetNumber,
                trainingRecordSet.Reps,
                trainingRecordSet.Weight,
                trainingRecordSet.CreatedAt,
                trainingRecordSet.UpdatedAt
            };


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
            public string? Reps { get; set; } = string.Empty;
            public string? Weight { get; set; } = string.Empty;

            public void SetParameter(HttpRequest req)
            {
                Authorization = req.Headers[FoundationCode.Header.AUTHORIZATION].ToString();
                MenuId = req.Form.FirstOrDefault(x => x.Key == FoundationCode.Body.MENU_ID).Value.ToString();
                TrainingDate = req.Form.FirstOrDefault(x => x.Key == FoundationCode.Body.TRAINING_DATE).Value.ToString();
                SetNumber = req.Form.FirstOrDefault(x => x.Key == FoundationCode.Body.SET_NUMBER).Value.ToString();
                Reps = req.Form.FirstOrDefault(x => x.Key == FoundationCode.Body.REPS).Value.ToString();
                Weight = req.Form.FirstOrDefault(x => x.Key == FoundationCode.Body.WEIGHT).Value.ToString();
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

                ValidateUtil.IndispensableParam(SetNumber!, FoundationCode.Body.SET_NUMBER);
                ValidateUtil.FormatParam(SetNumber!, FoundationCode.Body.SET_NUMBER, FoundationCode.Regex.Pattern.SET_NUMBER);

                ValidateUtil.IndispensableParam(Reps!, FoundationCode.Body.REPS);
                ValidateUtil.FormatParam(Reps!, FoundationCode.Body.REPS, FoundationCode.Regex.Pattern.REPS);
                if (!string.IsNullOrEmpty(Weight))
                {
                    ValidateUtil.FormatParam(Weight!, FoundationCode.Body.WEIGHT, FoundationCode.Regex.Pattern.WEIGHT);
                }
            }
        }
    }
}


