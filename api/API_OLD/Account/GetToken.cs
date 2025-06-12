using Api.common;
using Api.Models;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Dynamic;


namespace API.Account;
/// <summary>
/// </summary>
/// <param name="req"></param>
/// <param name="log"></param>
public static class GetToken
{
    [Function("GetAccountToken")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "account/token")] HttpRequest req, FunctionContext context)
    {
        var log = context.GetLogger("GetAccountToken");
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
                // ユーザーの取得
                User user = Utility.GetUser(DBcontext, input.LoginId);
                // パスワード検証
                ConfirmPassword(user, input.Password);
                // トークン発行
                AccessToken token = AccessToken.Generate(user);
                RefreshToken refreshToken = RefreshToken.Generate(DBcontext, user);
                DBcontext.SaveChanges();
                return Terminate(log, token.Token, refreshToken.Token);
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

    /// <summary>
    /// パスワード検証
    /// </summary>
    /// <param name="user"></param>
    /// <param name="reqPassword"></param>
    private static void ConfirmPassword(User user, string reqPassword)
    {
        if (Utility.HashPassword(reqPassword, user.PasswordSalt) != user.PasswordHash) 
        {
            throw new AppException(FoundationCode.Errors.PASSWORD_INVALID);
        }
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    private static ObjectResult Terminate(ILogger log, string token, string refreshToken)
    {
        dynamic response = new ExpandoObject();
        response.token = token;
        response.refresh_token = refreshToken;

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
            // Requestから取得
            LoginId = req.Query[FoundationCode.Body.LOGIN_ID].ToString();
            Password = req.Query[FoundationCode.Body.PASSWORD].ToString();
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
        }
    }
}



