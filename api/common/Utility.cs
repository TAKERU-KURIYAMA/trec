
using Api.Models;
using System.Security.Cryptography;

namespace Common
{
    /// <summary>
    /// 共通処理クラス
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Content-Typeチェック（application/x-www-form-urlencoded）
        /// </summary>
        /// <param name="type"></param>
        public static void ValidateTypeXWwwFormUrlencoded(string type)
        {
            if(!string.IsNullOrEmpty(type))
            {
                // application/x-www-form-urlencoded; charset=UTF-8
                string[] types = type.Split(";");
                if (types[0] == FoundationCode.Content.TYPE_X_WWW_FORM)
                {
                    return;
                }
            }
            throw new AppException(FoundationCode.Errors.CLIENT_PARAMETER_ERROR, "Content-Typeが不正です");
        }

        /// <summary>
        /// JST時刻取得
        /// </summary>
        /// <returns></returns>
        public static DateTime GetJstNow()
        {
            TimeZoneInfo JST = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, JST);
        }

        /// <summary>
        /// ログインIDからuserオブジェクト取得
        /// </summary>
        /// <param name="context"></param>
        /// <param name="loginId"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public static User GetUser(MessageRDBContext context, string loginId)
        {
            User? userData = context.Users.Where(x => x.LoginId == loginId).FirstOrDefault();

            if (userData == null)
            {
                throw new AppException(FoundationCode.Errors.USER_NOT_FOUND);
            }
            return userData;
        }

        /// <summary>
        /// 共通IDからuserオブジェクト取得
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userCommonId"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public static User GetUserCommonId(MessageRDBContext context, string userCommonId)
        {
            User? userData = context.Users.Where(x => x.UserCommonId == userCommonId).FirstOrDefault();

            if (userData == null)
            {
                throw new AppException(FoundationCode.Errors.USER_NOT_FOUND);
            }
            return userData;
        }


        /// <summary>
        /// パスワード用ソルト生成
        /// </summary>
        /// <returns></returns>
        public static string GenerateSalt()
        {
            var buffer = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// パスワードハッシュ化
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string HashPassword(string password, string salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }
    }
}