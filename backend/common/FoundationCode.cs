using System.Net;

namespace Common
{
    public static class FoundationCode
    {
        public static class Errors
        {
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

        public record ErrorInfo(int Code, string Message, HttpStatusCode StatusCode);
    }
}
