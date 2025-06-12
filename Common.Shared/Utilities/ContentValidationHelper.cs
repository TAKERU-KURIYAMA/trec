using Common.Shared.Constants;
using Common.Shared.Exceptions;

namespace Common.Shared.Utilities
{
    /// <summary>
    /// コンテンツ関連のバリデーションヘルパークラス
    /// Content-Typeやその他のHTTPコンテンツ検証を提供
    /// </summary>
    public static class ContentValidationHelper
    {
        /// <summary>
        /// Content-Typeが application/x-www-form-urlencoded かどうかを検証
        /// 無効な場合は例外を発生させる
        /// </summary>
        /// <param name="contentType">検証するContent-Type</param>
        /// <exception cref="AppException">Content-Typeが不正な場合</exception>
        public static void ValidateTypeXWwwFormUrlencoded(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "Content-Typeが指定されていません");
            }

            // Content-Typeにはcharsetなどのパラメータが含まれる場合があるため、セミコロンで分割
            // 例: "application/x-www-form-urlencoded; charset=UTF-8"
            var types = contentType.Split(';');
            var mainType = types[0].Trim();

            if (!string.Equals(mainType, ApplicationConstants.ContentTypes.FormUrlEncoded, StringComparison.OrdinalIgnoreCase))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"Content-Typeが不正です。期待値: {ApplicationConstants.ContentTypes.FormUrlEncoded}, 実際の値: {mainType}");
            }
        }

        /// <summary>
        /// Content-Typeが application/json かどうかを検証
        /// 無効な場合は例外を発生させる
        /// </summary>
        /// <param name="contentType">検証するContent-Type</param>
        /// <exception cref="AppException">Content-Typeが不正な場合</exception>
        public static void ValidateTypeApplicationJson(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "Content-Typeが指定されていません");
            }

            var types = contentType.Split(';');
            var mainType = types[0].Trim();

            if (!string.Equals(mainType, ApplicationConstants.ContentTypes.Json, StringComparison.OrdinalIgnoreCase))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"Content-Typeが不正です。期待値: {ApplicationConstants.ContentTypes.Json}, 実際の値: {mainType}");
            }
        }

        /// <summary>
        /// Content-Typeが指定された種類のいずれかに一致するかを検証
        /// </summary>
        /// <param name="contentType">検証するContent-Type</param>
        /// <param name="allowedTypes">許可されるContent-Typeのリスト</param>
        /// <exception cref="AppException">Content-Typeが許可されていない場合</exception>
        public static void ValidateContentType(string? contentType, params string[] allowedTypes)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "Content-Typeが指定されていません");
            }

            if (allowedTypes == null || allowedTypes.Length == 0)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "許可されるContent-Typeが指定されていません");
            }

            var types = contentType.Split(';');
            var mainType = types[0].Trim();

            var isValid = allowedTypes.Any(allowedType => 
                string.Equals(mainType, allowedType, StringComparison.OrdinalIgnoreCase));

            if (!isValid)
            {
                var allowedTypesString = string.Join(", ", allowedTypes);
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"Content-Typeが不正です。許可される値: [{allowedTypesString}], 実際の値: {mainType}");
            }
        }

        /// <summary>
        /// Content-Lengthが指定された範囲内かどうかを検証
        /// </summary>
        /// <param name="contentLength">検証するContent-Length</param>
        /// <param name="minLength">最小長（デフォルト: 0）</param>
        /// <param name="maxLength">最大長（デフォルト: int.MaxValue）</param>
        /// <exception cref="AppException">Content-Lengthが範囲外の場合</exception>
        public static void ValidateContentLength(long? contentLength, long minLength = 0, long maxLength = int.MaxValue)
        {
            if (!contentLength.HasValue)
            {
                if (minLength > 0)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "Content-Lengthが指定されていません");
                }
                return;
            }

            if (contentLength.Value < minLength)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"コンテンツサイズが小さすぎます。最小サイズ: {minLength} bytes, 実際のサイズ: {contentLength.Value} bytes");
            }

            if (contentLength.Value > maxLength)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"コンテンツサイズが大きすぎます。最大サイズ: {maxLength} bytes, 実際のサイズ: {contentLength.Value} bytes");
            }
        }

        /// <summary>
        /// リクエストボディが必須かどうかを検証
        /// POST/PUT/PATCHリクエストでコンテンツが空の場合にエラーを発生
        /// </summary>
        /// <param name="httpMethod">HTTPメソッド</param>
        /// <param name="contentLength">Content-Length</param>
        /// <exception cref="AppException">必須のリクエストボディが空の場合</exception>
        public static void ValidateRequiredBody(string httpMethod, long? contentLength)
        {
            var methodsRequiringBody = new[] { "POST", "PUT", "PATCH" };
            
            if (methodsRequiringBody.Contains(httpMethod.ToUpperInvariant()))
            {
                if (!contentLength.HasValue || contentLength.Value == 0)
                {
                    throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                        $"{httpMethod}リクエストにはリクエストボディが必要です");
                }
            }
        }

        /// <summary>
        /// ファイルアップロード用のContent-Type検証
        /// マルチパートフォームデータかどうかを確認
        /// </summary>
        /// <param name="contentType">検証するContent-Type</param>
        /// <exception cref="AppException">ファイルアップロード用でない場合</exception>
        public static void ValidateMultipartFormData(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, "Content-Typeが指定されていません");
            }

            if (!contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    "ファイルアップロードにはmultipart/form-dataのContent-Typeが必要です");
            }
        }

        /// <summary>
        /// Content-Typeからcharsetを抽出
        /// </summary>
        /// <param name="contentType">Content-Type</param>
        /// <returns>charset値、見つからない場合は"utf-8"</returns>
        public static string ExtractCharset(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return "utf-8";
            }

            var parts = contentType.Split(';');
            foreach (var part in parts.Skip(1))
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("charset=", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring(8).Trim();
                }
            }

            return "utf-8";
        }
    }
}