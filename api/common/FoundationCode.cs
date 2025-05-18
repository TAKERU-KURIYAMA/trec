using System.Net;

namespace Common
{
    public static class FoundationCode
    {
        public static class Errors
        {

            public static readonly AppException.Value SUCCESS =
                new AppException.Value() { Code = "00000", Status = HttpStatusCode.OK, Msg = "OK" };

            public static readonly AppException.Value CLIENT_PARAMETER_ERROR =
                new AppException.Value() { Code = "00001", Status = HttpStatusCode.BadRequest, Msg = "パラメータエラー" };

            public static readonly AppException.Value SERVER_ERROR =
                new AppException.Value() { Code = "10001", Status = HttpStatusCode.InternalServerError, Msg = "インターナルサーバーエラー" };

            public static readonly AppException.Value LOGIN_ID_DUPLICATE = 
                new AppException.Value() { Code = "10002", Status = HttpStatusCode.BadRequest, Msg = "既に登録されているログインID" };

            public static readonly AppException.Value COMMON_ID_GENERATION_FAILUE =
                new AppException.Value() { Code = "10003", Status = HttpStatusCode.BadRequest, Msg = "ID生成に失敗" };

            public static readonly AppException.Value USER_NOT_FOUND =
                new AppException.Value() { Code = "10004", Status = HttpStatusCode.BadRequest, Msg = "ユーザーが存在しない" };

            public static readonly AppException.Value PASSWORD_INVALID =
                new AppException.Value() { Code = "10005", Status = HttpStatusCode.BadRequest, Msg = "パスワード不一致" };

            public static readonly AppException.Value TOKEN_GENERATION_FAILUE =
                new AppException.Value() { Code = "10006", Status = HttpStatusCode.BadRequest, Msg = "トークン発行に失敗" };

            public static readonly AppException.Value AUTHORIZATION_ERROR =
                new AppException.Value() { Code = "10007", Status = HttpStatusCode.BadRequest, Msg = "トークン検証に失敗" };

            public static readonly ErrorInfo DuplicateData = new(
                1001,
                "既に同じデータが存在します。",
                HttpStatusCode.Conflict
            );

            public static readonly ErrorInfo NotFound = new(
                1002,
                "データが見つかりません。",
                HttpStatusCode.NotFound
            );

            public static readonly ErrorInfo InvalidInput = new(
                1003,
                "入力値が不正です。",
                HttpStatusCode.BadRequest
            );

            public static readonly ErrorInfo Unauthorized = new(
                1004,
                "認証に失敗しました。",
                HttpStatusCode.Unauthorized
            );

            public static readonly ErrorInfo Unexpected = new(
                9999,
                "不明なエラーが発生しました。",
                HttpStatusCode.InternalServerError
            );
        }

        public static class Const
        {
            public const int ID_RETRY_COUNT = 3;
            public const int TOKEN_RETRY_COUNT = 3;
        }
        public record ErrorInfo(int Code, string Message, HttpStatusCode StatusCode);
        /// <summary>
        /// コンテンツタイプ用
        /// </summary>
        public static class Content
        {
            public const string TYPE_JSON = "application/json";
            public const string TYPE_X_WWW_FORM = "application/x-www-form-urlencoded";
        }

        public static class Header
        {
            public const string AUTHORIZATION = "Authorization";
            public const string X_AUTH_SESSION = "x-auth-session";

        }

        public static class Body
        {
            public const string LOGIN_ID = "login_id";
            public const string PASSWORD = "password";
            public const string DISPLAY_NAME = "display_name";

            public const string MENU_ID = "menu_id";
            public const string TRAINING_DATE = "training_date";
            public const string SET_NUMBER = "set_number";
            public const string REPS = "reps";
            public const string WEIGHT = "weight";


        }
        /// <summary>
        /// 正規表現
        /// </summary>
        public static class Regex
        {
            /// <summary>
            /// パターン
            /// </summary>
            public static class Pattern
            {
                public const string LOGIN_ID = @"^[0-9a-zA-Z]{1,32}$";
                public const string PASSWORD = @"^[0-9a-zA-Z]{64}$";
                public const string DISPLAY_NAME = @"^.{1,64}$";
                public const string SESSION_ID = @"^[0-9a-zA-Z]{32}$";

                public const string MENU_ID = @"^[0-9a-zA-Z_]{1,64}$";
                public const string TRAINING_DATE = @"^\d{4}-\d{2}-\d{2}$";
                public const string SET_NUMBER = @"^\d{1,3}$";
                public const string REPS = @"^\d{1,3}$";
                public const string WEIGHT = @"^\d{1,3}$";


            }

        }
    }
}
