using Api.Models;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Dynamic;

namespace API.Account;

public class PostUser
{
    [Function("PostAccountUser")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "account/user")] HttpRequest req, FunctionContext context)
    {
        var log = context.GetLogger("PostAccountUser");

        try
        {
            Logger.Entry(log, req);

            Input input = new Input();
            input.SetParameter(req);
            input.Validation();
            // 環境変数から接続文字列を取得
            // using ブロックで明示的に DbContext を使用
            DbContextOptions<MessageRDBContext> options = new DbContextOptionsBuilder<MessageRDBContext>()
                .UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"))
                .Options;
            using (MessageRDBContext DBcontext = new MessageRDBContext(options))
            {
                //ログインID重複チェック
                ConfirmIdUnique(DBcontext, input.LoginId);

                //アカウントの登録
                User user = PostAccount(DBcontext, input);
                return Terminate(log, user.UserCommonId);

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

    private static string GenerateUserId(MessageRDBContext context)
    {
        Random random = new Random();
        string userId = string.Empty;
        bool memberIdFlg = false;

        for (int i = 0; i < FoundationCode.Const.ID_RETRY_COUNT; i++) 
        {
            long randomPart = random.NextInt64(0, 9999999999999L);

            userId = "123" + randomPart.ToString().PadLeft(13, '0');
            if (!context.Users.AsNoTracking().Any(x => x.UserCommonId == userId))
            {
                memberIdFlg = true;
                break;
            }
        }
        if (!memberIdFlg)
        {
            throw new AppException(FoundationCode.Errors.COMMON_ID_GENERATION_FAILUE);
        }

        return userId;
    }

    /// <summary>
    /// ログインID重複チェック
    /// </summary>
    /// <param name="context"></param>
    /// <param name="loginId"></param>
    /// <exception cref="AppException"></exception>
    private static void ConfirmIdUnique(MessageRDBContext context, string loginId)
    {
        User? userData = context.Users.Where(x => x.LoginId == loginId).FirstOrDefault();

        if (userData != null) 
        {
            throw new AppException(FoundationCode.Errors.LOGIN_ID_DUPLICATE);
        }
    }

    /// <summary>
    /// アカウント発行
    /// </summary>
    /// <param name="context"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    private static User PostAccount(MessageRDBContext context, Input input)
    {
        string salt = Utility.GenerateSalt();
        string passwordHash = Utility.HashPassword(input.Password, salt);
        string userId = GenerateUserId(context);
        DateTime JstNow = Utility.GetJstNow();

        User user = context.Users.Add(new User
        {
            UserCommonId = userId,
            LoginId = input.LoginId,
            PasswordHash = passwordHash,
            PasswordSalt = salt,
            DisplayName = input.DisplayName,
            CreatedAt = JstNow,
            UpdatedAt = JstNow,
        }).Entity;
        context.SaveChanges();
        return user;
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    private static ObjectResult Terminate(ILogger log, string commonId)
    {
        dynamic response = new ExpandoObject();
        response.common_id = commonId;

        Logger.Exit(log, response);
        return Response.CreateOkResponse(response);
    }

    /// <summary>
    /// 入力値クラス
    /// </summary>
    private sealed class Input
    {
        public string LoginId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Input()
        {
        }

        /// <summary>
        /// パラメータ取得
        /// </summary>
        public void SetParameter(HttpRequest req)
        {
            // Content-Typeチェック
            Utility.ValidateTypeXWwwFormUrlencoded(req.ContentType!);

            // Requestから取得
            LoginId = req.Form.FirstOrDefault(x => x.Key == FoundationCode.Body.LOGIN_ID).Value.ToString();
            Password = req.Form.FirstOrDefault(x => x.Key == FoundationCode.Body.PASSWORD).Value.ToString();
            DisplayName = req.Form.FirstOrDefault(x => x.Key == FoundationCode.Body.DISPLAY_NAME).Value.ToString();
        }

        /// <summary>
        /// バリデーション
        /// </summary>
        public void Validation()
        {
            ValidateUtil.IndispensableParam(LoginId, FoundationCode.Body.LOGIN_ID);
            ValidateUtil.FormatParam(LoginId, FoundationCode.Body.LOGIN_ID, FoundationCode.Regex.Pattern.LOGIN_ID);

            ValidateUtil.IndispensableParam(Password, FoundationCode.Body.PASSWORD);
            ValidateUtil.FormatParam(Password, FoundationCode.Body.PASSWORD, FoundationCode.Regex.Pattern.PASSWORD);

            ValidateUtil.IndispensableParam(DisplayName, FoundationCode.Body.DISPLAY_NAME);
            ValidateUtil.FormatParam(DisplayName, FoundationCode.Body.DISPLAY_NAME, FoundationCode.Regex.Pattern.DISPLAY_NAME);

        }
    }

}
