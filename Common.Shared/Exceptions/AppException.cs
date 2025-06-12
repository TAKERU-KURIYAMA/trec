using System.Net;
using System.Text;
using Common.Shared.Constants;

namespace Common.Shared.Exceptions
{
    /// <summary>
    /// アプリケーション固有の例外クラス
    /// エラーコード、HTTPステータス、メッセージを統一的に管理する
    /// </summary>
    [Serializable]
    public class AppException : Exception
    {
        /// <summary>
        /// エラー情報
        /// </summary>
        public ApiError ErrorInfo { get; }

        /// <summary>
        /// 内部エラーコード（デバッグ用）
        /// </summary>
        public int? InternalCode { get; }

        /// <summary>
        /// 基本コンストラクタ - ApiErrorからの生成
        /// </summary>
        /// <param name="errorInfo">エラー情報</param>
        /// <param name="innerException">内部例外（任意）</param>
        /// <param name="internalCode">内部エラーコード（任意）</param>
        public AppException(
            ApiError errorInfo, 
            Exception? innerException = null, 
            int? internalCode = null) 
            : base(errorInfo.Message, innerException)
        {
            ErrorInfo = errorInfo;
            InternalCode = internalCode;
        }

        /// <summary>
        /// カスタムメッセージ付きコンストラクタ
        /// 標準エラーコードにカスタムメッセージを設定したい場合に使用
        /// </summary>
        /// <param name="errorInfo">ベースとなるエラー情報</param>
        /// <param name="customMessage">カスタムメッセージ</param>
        /// <param name="innerException">内部例外（任意）</param>
        /// <param name="internalCode">内部エラーコード（任意）</param>
        public AppException(
            ApiError errorInfo, 
            string customMessage, 
            Exception? innerException = null, 
            int? internalCode = null) 
            : base(customMessage, innerException)
        {
            // カスタムメッセージでApiErrorを更新
            ErrorInfo = errorInfo with { Message = customMessage };
            InternalCode = internalCode;
        }

        /// <summary>
        /// 例外情報の詳細な文字列表現を生成
        /// ログ出力時により詳細な情報を提供する
        /// </summary>
        /// <returns>フォーマット済みの例外情報</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            
            // 基本情報
            sb.AppendLine($"[AppException]");
            sb.AppendLine($"  Code: {ErrorInfo.Code}");
            sb.AppendLine($"  Status: {ErrorInfo.StatusCode} ({(int)ErrorInfo.StatusCode})");
            sb.AppendLine($"  Message: {ErrorInfo.Message}");
            
            // 内部コードがあれば追加
            if (InternalCode.HasValue)
            {
                sb.AppendLine($"  InternalCode: {InternalCode.Value}");
            }
            
            // 内部例外があれば追加
            if (InnerException != null)
            {
                sb.AppendLine($"  InnerException: {InnerException.GetType().Name}");
                sb.AppendLine($"  InnerMessage: {InnerException.Message}");
                
                // 更に内部例外がある場合
                if (InnerException.InnerException != null)
                {
                    sb.AppendLine($"  InnerInnerException: {InnerException.InnerException.GetType().Name}");
                    sb.AppendLine($"  InnerInnerMessage: {InnerException.InnerException.Message}");
                }
            }
            
            // スタックトレース
            if (!string.IsNullOrEmpty(StackTrace))
            {
                sb.AppendLine($"  StackTrace: {StackTrace}");
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// 一般的な例外からAppExceptionを作成するヘルパーメソッド
        /// try-catchブロックで捕捉した例外を統一的に処理する
        /// </summary>
        /// <param name="exception">元の例外</param>
        /// <param name="internalCode">内部エラーコード（任意）</param>
        /// <returns>AppException</returns>
        public static AppException FromException(Exception exception, int? internalCode = null)
        {
            // 既にAppExceptionの場合はそのまま返す
            if (exception is AppException appEx)
            {
                return appEx;
            }

            // 一般的な例外タイプに応じてエラーコードを決定
            var errorInfo = exception switch
            {
                ArgumentException => ApplicationConstants.ErrorCodes.ParameterError,
                ArgumentNullException => ApplicationConstants.ErrorCodes.ParameterError,
                UnauthorizedAccessException => ApplicationConstants.ErrorCodes.AuthorizationError,
                TimeoutException => ApplicationConstants.ErrorCodes.ServerError,
                _ => ApplicationConstants.ErrorCodes.ServerError
            };

            return new AppException(errorInfo, exception, internalCode);
        }

        /// <summary>
        /// レスポンス用の簡潔なエラー情報を取得
        /// APIレスポンスに含める最小限の情報を返す
        /// </summary>
        /// <returns>クライアント向けエラー情報</returns>
        public object ToResponseObject()
        {
            return new
            {
                code = ErrorInfo.Code,
                message = ErrorInfo.Message,
                // 本番環境ではInternalCodeやStackTraceは含めない
#if DEBUG
                internalCode = InternalCode,
                stackTrace = StackTrace
#endif
            };
        }
    }
}