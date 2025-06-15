using System;

namespace Api.Common
{
    /// <summary>
    /// 日時操作のヘルパークラス
    /// 日本標準時（JST）での日時処理を提供
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// 日本標準時のタイムゾーン情報
        /// </summary>
        private static readonly TimeZoneInfo JstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

        /// <summary>
        /// 現在の日本標準時のDateTimeを取得
        /// </summary>
        /// <returns>JST の現在日時</returns>
        public static DateTime GetJstNow()
        {
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, JstTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback: UTC+9を使用
                return DateTime.UtcNow.AddHours(9);
            }
        }

        /// <summary>
        /// 現在の日本標準時のDateOnlyを取得
        /// </summary>
        /// <returns>JST の現在日付</returns>
        public static DateOnly GetJstDateOnlyType()
        {
            return DateOnly.FromDateTime(GetJstNow());
        }

        /// <summary>
        /// 現在の日本標準時のTimeOnlyを取得
        /// </summary>
        /// <returns>JST の現在時刻</returns>
        public static TimeOnly GetJstTimeOnlyType()
        {
            return TimeOnly.FromDateTime(GetJstNow());
        }

        /// <summary>
        /// UTCからJSTに変換
        /// </summary>
        /// <param name="utcDateTime">UTC日時</param>
        /// <returns>JST日時</returns>
        public static DateTime ConvertUtcToJst(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("UTC日時を指定してください", nameof(utcDateTime));
            }

            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, JstTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback: UTC+9を使用
                return utcDateTime.AddHours(9);
            }
        }

        /// <summary>
        /// JSTからUTCに変換
        /// </summary>
        /// <param name="jstDateTime">JST日時</param>
        /// <returns>UTC日時</returns>
        public static DateTime ConvertJstToUtc(DateTime jstDateTime)
        {
            try
            {
                return TimeZoneInfo.ConvertTimeToUtc(jstDateTime, JstTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback: UTC-9を使用
                return jstDateTime.AddHours(-9);
            }
        }

        /// <summary>
        /// 日付文字列をJSTのDateOnlyに変換
        /// </summary>
        /// <param name="dateString">日付文字列 (yyyy-MM-dd)</param>
        /// <returns>DateOnly</returns>
        /// <exception cref="AppException">日付形式が不正な場合</exception>
        public static DateOnly ParseJstDateOnly(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                throw AppException.CreateParameterError(nameof(dateString), "日付文字列が空です");
            }

            if (!DateOnly.TryParse(dateString, out DateOnly result))
            {
                throw AppException.CreateParameterError(nameof(dateString), "日付形式が不正です（yyyy-MM-dd形式で入力してください）");
            }

            return result;
        }

        /// <summary>
        /// DateOnlyを文字列に変換
        /// </summary>
        /// <param name="date">日付</param>
        /// <param name="format">フォーマット（デフォルト: yyyy-MM-dd）</param>
        /// <returns>日付文字列</returns>
        public static string FormatDateOnly(DateOnly date, string format = "yyyy-MM-dd")
        {
            return date.ToString(format);
        }

        /// <summary>
        /// 指定された期間のJST日付範囲を取得
        /// </summary>
        /// <param name="days">過去何日分か</param>
        /// <returns>開始日と終了日のタプル</returns>
        public static (DateOnly StartDate, DateOnly EndDate) GetJstDateRange(int days)
        {
            if (days < 0)
            {
                throw AppException.CreateParameterError(nameof(days), "日数は0以上を指定してください");
            }

            var endDate = GetJstDateOnlyType();
            var startDate = endDate.AddDays(-days);

            return (startDate, endDate);
        }

        /// <summary>
        /// 月の開始日と終了日を取得
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>開始日と終了日のタプル</returns>
        public static (DateOnly StartDate, DateOnly EndDate) GetMonthRange(int year, int month)
        {
            if (month < 1 || month > 12)
            {
                throw AppException.CreateParameterError(nameof(month), "月は1-12の範囲で指定してください");
            }

            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return (startDate, endDate);
        }

        /// <summary>
        /// 現在月の開始日と終了日を取得
        /// </summary>
        /// <returns>開始日と終了日のタプル</returns>
        public static (DateOnly StartDate, DateOnly EndDate) GetCurrentMonthRange()
        {
            var now = GetJstNow();
            return GetMonthRange(now.Year, now.Month);
        }
    }
}