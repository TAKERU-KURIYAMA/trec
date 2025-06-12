using System.Text.RegularExpressions;
using Common.Shared.Constants;
using Common.Shared.Exceptions;

namespace Common.Shared.Utilities
{
    /// <summary>
    /// 入力値バリデーションのヘルパークラス
    /// 統一されたバリデーションルールと エラーハンドリングを提供
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// 必須パラメータの検証
        /// null、空文字、空白のみの文字列をエラーとする
        /// </summary>
        /// <param name="value">検証する値</param>
        /// <param name="parameterName">パラメータ名（エラーメッセージで使用）</param>
        /// <exception cref="AppException">値が無効な場合</exception>
        public static void ValidateRequired(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}が指定されていません");
            }
        }

        /// <summary>
        /// 必須パラメータの検証（オブジェクト版）
        /// nullをエラーとする
        /// </summary>
        /// <param name="value">検証する値</param>
        /// <param name="parameterName">パラメータ名（エラーメッセージで使用）</param>
        /// <exception cref="AppException">値がnullの場合</exception>
        public static void ValidateRequired<T>(T? value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}が指定されていません");
            }
        }

        /// <summary>
        /// 正規表現パターンによる形式検証
        /// </summary>
        /// <param name="value">検証する値</param>
        /// <param name="pattern">正規表現パターン</param>
        /// <param name="parameterName">パラメータ名（エラーメッセージで使用）</param>
        /// <param name="allowNullOrEmpty">null・空文字を許可するかどうか（デフォルト: false）</param>
        /// <exception cref="AppException">形式が不正な場合</exception>
        public static void ValidateFormat(string? value, string pattern, string parameterName, 
            bool allowNullOrEmpty = false)
        {
            if (allowNullOrEmpty && string.IsNullOrEmpty(value))
            {
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}が指定されていません");
            }

            if (!Regex.IsMatch(value, pattern, RegexOptions.Compiled))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}の形式が不正です");
            }
        }

        /// <summary>
        /// 正規表現パターンによる形式検証（カスタムエラー版）
        /// </summary>
        /// <param name="value">検証する値</param>
        /// <param name="pattern">正規表現パターン</param>
        /// <param name="errorMessage">カスタムエラーメッセージ</param>
        /// <param name="allowNullOrEmpty">null・空文字を許可するかどうか（デフォルト: false）</param>
        /// <exception cref="AppException">形式が不正な場合</exception>
        public static void ValidateFormat(string? value, string pattern, string errorMessage, 
            bool allowNullOrEmpty = false)
        {
            if (allowNullOrEmpty && string.IsNullOrEmpty(value))
            {
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, errorMessage);
            }

            if (!Regex.IsMatch(value, pattern, RegexOptions.Compiled))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, errorMessage);
            }
        }

        /// <summary>
        /// ログインIDの形式検証
        /// 英数字のみ、1-32文字
        /// </summary>
        /// <param name="loginId">検証するログインID</param>
        /// <exception cref="AppException">形式が不正な場合</exception>
        public static void ValidateLoginId(string? loginId)
        {
            ValidateFormat(loginId, ApplicationConstants.ValidationPatterns.LoginId, 
                "ログインID", false);
        }

        /// <summary>
        /// 表示名の形式検証
        /// 1-64文字
        /// </summary>
        /// <param name="displayName">検証する表示名</param>
        /// <exception cref="AppException">形式が不正な場合</exception>
        public static void ValidateDisplayName(string? displayName)
        {
            ValidateFormat(displayName, ApplicationConstants.ValidationPatterns.DisplayName, 
                "表示名", false);
        }

        /// <summary>
        /// メニューIDの形式検証
        /// 英数字とアンダースコアのみ、1-64文字
        /// </summary>
        /// <param name="menuId">検証するメニューID</param>
        /// <exception cref="AppException">形式が不正な場合</exception>
        public static void ValidateMenuId(string? menuId)
        {
            ValidateFormat(menuId, ApplicationConstants.ValidationPatterns.MenuId, 
                "メニューID", false);
        }

        /// <summary>
        /// 日付形式の検証
        /// YYYY-MM-DD形式
        /// </summary>
        /// <param name="dateString">検証する日付文字列</param>
        /// <exception cref="AppException">形式が不正な場合</exception>
        public static void ValidateDateFormat(string? dateString)
        {
            ValidateFormat(dateString, ApplicationConstants.ValidationPatterns.DateFormat, 
                "日付はYYYY-MM-DD形式で入力してください", false);
        }

        /// <summary>
        /// 数値の範囲検証
        /// </summary>
        /// <param name="value">検証する値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="parameterName">パラメータ名（エラーメッセージで使用）</param>
        /// <exception cref="AppException">範囲外の場合</exception>
        public static void ValidateRange(int value, int min, int max, string parameterName)
        {
            if (value < min || value > max)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}は{min}から{max}の範囲で入力してください（入力値: {value}）");
            }
        }

        /// <summary>
        /// 数値の範囲検証（decimal版）
        /// </summary>
        /// <param name="value">検証する値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="parameterName">パラメータ名（エラーメッセージで使用）</param>
        /// <exception cref="AppException">範囲外の場合</exception>
        public static void ValidateRange(decimal value, decimal min, decimal max, string parameterName)
        {
            if (value < min || value > max)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}は{min}から{max}の範囲で入力してください（入力値: {value}）");
            }
        }

        /// <summary>
        /// トレーニング重量の検証
        /// 0.1kg から 999kg まで
        /// </summary>
        /// <param name="weight">検証する重量</param>
        /// <exception cref="AppException">範囲外の場合</exception>
        public static void ValidateTrainingWeight(decimal? weight)
        {
            if (weight.HasValue)
            {
                ValidateRange(weight.Value, 0.1m, ApplicationConstants.TrainingSettings.MaxWeightKg, "重量");
            }
        }

        /// <summary>
        /// トレーニング回数の検証
        /// 1回 から 999回 まで
        /// </summary>
        /// <param name="reps">検証する回数</param>
        /// <exception cref="AppException">範囲外の場合</exception>
        public static void ValidateTrainingReps(int reps)
        {
            ValidateRange(reps, 1, ApplicationConstants.TrainingSettings.MaxReps, "回数");
        }

        /// <summary>
        /// トレーニングセット数の検証
        /// 1セット から 99セット まで
        /// </summary>
        /// <param name="sets">検証するセット数</param>
        /// <exception cref="AppException">範囲外の場合</exception>
        public static void ValidateTrainingSets(int sets)
        {
            ValidateRange(sets, 1, ApplicationConstants.TrainingSettings.MaxSets, "セット数");
        }

        /// <summary>
        /// 文字列の長さ検証
        /// </summary>
        /// <param name="value">検証する文字列</param>
        /// <param name="maxLength">最大長</param>
        /// <param name="parameterName">パラメータ名（エラーメッセージで使用）</param>
        /// <param name="allowNull">nullを許可するかどうか（デフォルト: false）</param>
        /// <exception cref="AppException">長さが上限を超えた場合</exception>
        public static void ValidateMaxLength(string? value, int maxLength, string parameterName, 
            bool allowNull = false)
        {
            if (value == null && allowNull)
            {
                return;
            }

            if (value == null)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}が指定されていません");
            }

            if (value.Length > maxLength)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}は{maxLength}文字以内で入力してください（入力値: {value.Length}文字）");
            }
        }

        /// <summary>
        /// コレクションの要素数検証
        /// </summary>
        /// <param name="collection">検証するコレクション</param>
        /// <param name="maxCount">最大要素数</param>
        /// <param name="parameterName">パラメータ名（エラーメッセージで使用）</param>
        /// <exception cref="AppException">要素数が上限を超えた場合</exception>
        public static void ValidateMaxCount<T>(ICollection<T>? collection, int maxCount, string parameterName)
        {
            if (collection == null)
            {
                return;
            }

            if (collection.Count > maxCount)
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}の要素数は{maxCount}個以内にしてください（入力値: {collection.Count}個）");
            }
        }

        /// <summary>
        /// 日付の妥当性検証
        /// 文字列から日付にパースできるかどうかを確認
        /// </summary>
        /// <param name="dateString">検証する日付文字列</param>
        /// <param name="parameterName">パラメータ名（エラーメッセージで使用）</param>
        /// <returns>パースされた日付</returns>
        /// <exception cref="AppException">日付として不正な場合</exception>
        public static DateTime ValidateAndParseDateTime(string? dateString, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}が指定されていません");
            }

            if (!DateTime.TryParse(dateString, out DateTime result))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}の形式が不正です");
            }

            return result;
        }

        /// <summary>
        /// DateOnlyの妥当性検証
        /// 文字列からDateOnlyにパースできるかどうかを確認
        /// </summary>
        /// <param name="dateString">検証する日付文字列</param>
        /// <param name="parameterName">パラメータ名（エラーメッセージで使用）</param>
        /// <returns>パースされた日付</returns>
        /// <exception cref="AppException">日付として不正な場合</exception>
        public static DateOnly ValidateAndParseDateOnly(string? dateString, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}が指定されていません");
            }

            if (!DateOnly.TryParse(dateString, out DateOnly result))
            {
                throw new AppException(ApplicationConstants.ErrorCodes.ParameterError, 
                    $"{parameterName}の形式が不正です");
            }

            return result;
        }
    }
}