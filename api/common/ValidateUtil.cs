using System.Text.RegularExpressions;

namespace Common
{
    /// <summary>
    /// バリデーション関連
    /// </summary>
    public static class ValidateUtil
    {
        /// <summary>
        /// 必須チェック
        /// </summary>
        /// <param name="argValue">チェックする値</param>
        /// <param name="argMessage">メッセージに指定する値</param>
        public static void IndispensableParam(string argValue, string argMessage)
        {
            if (string.IsNullOrEmpty(argValue))
            {
                throw new AppException(FoundationCode.Errors.CLIENT_PARAMETER_ERROR, $"{argMessage}が指定されていません");
            }
        }

        /// <summary>
        /// Patternチェック
        /// </summary>
        /// <param name="argValue">チェックする値</param>
        /// <param name="argMessage">メッセージに指定する値</param>
        /// <param name="pattern">チェックするパターン</param>
        /// <param name="nullOrEnptyPermission">nullと空文字を許可するフラグ(true:nullと空文字を許可、false:nullと空文字を不許可)</param>
        public static void FormatParam(string argValue, string argMessage , string pattern,bool nullOrEnptyPermission = false)
        {
            if (nullOrEnptyPermission == true && string.IsNullOrEmpty(argValue))
            {
                return;
            }
            if (!Regex.IsMatch(argValue, pattern))
            {
                throw new AppException(FoundationCode.Errors.CLIENT_PARAMETER_ERROR, $"{argMessage}に使用できない文字が含まれています");
            }
        }

        /// <summary>
        /// Patternチェック
        /// </summary>
        /// <param name="argValue">チェックする値</param>
        /// <param name="pattern">チェックするパターン</param>
        /// <param name="error">エラー内容</param>
        /// <param name="nullOrEnptyPermission">nullと空文字を許可するフラグ(true:nullと空文字を許可、false:nullと空文字を不許可)</param>
        public static void FormatParam(string argValue, string pattern, AppException error, bool nullOrEnptyPermission = false)
        {
            if (nullOrEnptyPermission == true && string.IsNullOrEmpty(argValue))
            {
                return;
            }
            if (!Regex.IsMatch(argValue, pattern))
            {
                throw error;
            }
        }
    }
}