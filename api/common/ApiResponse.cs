using Microsoft.AspNetCore.Mvc;

namespace Common
{
    /// <summary>
    /// レスポンス設定用クラス
    /// </summary>
    public static class Response
    {
        /// <summary>
        /// HttpResponse生成
        /// </summary>
        /// <param name="json">セッションID</param>
        /// <returns>httpレスポンス200</returns>
        public static OkObjectResult CreateOkResponse(dynamic json)
        {
            // コード追加
            json.code = FoundationCode.Errors.SUCCESS.Code;

            OkObjectResult res = new OkObjectResult(json);

            res.ContentTypes.Add(FoundationCode.Content.TYPE_JSON);
            return res;
        }

        /// <summary>
        /// エラーコード生成
        /// </summary>
        /// <param name="errorCode">エラーコード</param>
        /// <returns>httpレスポンス400</returns>
        public static ObjectResult CreateErrorResponse(AppException.Value errorCode)
        {
            // オブジェクト生成
            dynamic json = new
            {
                code = errorCode.Code
            };

            ObjectResult res = new ObjectResult(json);

            // HTTPステータスコードを設定
            res.StatusCode = (int)errorCode.Status;

            res.ContentTypes.Add(FoundationCode.Content.TYPE_JSON);
            return res;
        }

        /// <summary>
        /// 汎用コード生成
        /// </summary>
        /// <param name="statusCode">汎用コード</param>
        /// <param name="json">セッションID</param>
        /// <returns>コード</returns>
        public static ObjectResult CreateResponse(AppException.Value statusCode, dynamic json)
        {
            // オブジェクト生成
            json.code = statusCode.Code;

            ObjectResult res = new ObjectResult(json);

            // HTTPステータスコードを設定
            res.StatusCode = (int)statusCode.Status;

            res.ContentTypes.Add(FoundationCode.Content.TYPE_JSON);
            return res;
        }

        /// <summary>
        /// 汎用コード生成(codeなし)
        /// </summary>
        /// <param name="httpStatus">HTTPステータスコード</param>
        /// <param name="json">セッションID</param>
        /// <returns>コード</returns>
        public static ObjectResult CreateResponse(int httpStatus, dynamic json)
        {
            // オブジェクト生成
            ObjectResult res = new ObjectResult(json);

            // HTTPステータスコードを設定
            res.StatusCode = httpStatus;

            res.ContentTypes.Add(FoundationCode.Content.TYPE_JSON);
            return res;
        }

    }
}
