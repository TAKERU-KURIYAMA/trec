using System;

namespace Common.Shared.Utilities
{
    /// <summary>
    /// 日時操作のヘルパークラス
    /// タイムゾーン変換や標準的な日時処理を提供
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// 日本標準時のタイムゾーン情報
        /// </summary>
        private static readonly TimeZoneInfo JstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

        /// <summary>
        /// 現在のJST（日本標準時）を取得
        /// UTCからJSTに変換して返す
        /// </summary>
        /// <returns>JST での現在日時</returns>
        public static DateTime GetJstNow()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, JstTimeZone);
        }

        /// <summary>
        /// 指定されたUTC時刻をJSTに変換
        /// </summary>
        /// <param name="utcDateTime">UTC時刻</param>
        /// <returns>JST時刻</returns>
        public static DateTime ConvertUtcToJst(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTime(utcDateTime, TimeZoneInfo.Utc, JstTimeZone);
        }

        /// <summary>
        /// 指定されたJST時刻をUTCに変換
        /// </summary>
        /// <param name="jstDateTime">JST時刻</param>
        /// <returns>UTC時刻</returns>
        public static DateTime ConvertJstToUtc(DateTime jstDateTime)
        {
            return TimeZoneInfo.ConvertTime(jstDateTime, JstTimeZone, TimeZoneInfo.Utc);
        }

        /// <summary>
        /// 日付のみを取得（時刻部分を除去）
        /// </summary>
        /// <param name="dateTime">日時</param>
        /// <returns>日付のみ（時刻は00:00:00）</returns>
        public static DateTime GetDateOnly(DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// 現在のJST日付を取得（時刻部分を除去）
        /// </summary>
        /// <returns>JST での現在日付</returns>
        public static DateTime GetJstDateOnly()
        {
            return GetDateOnly(GetJstNow());
        }

        /// <summary>
        /// DateOnlyをJST基準で取得
        /// </summary>
        /// <returns>JST での現在日付（DateOnly）</returns>
        public static DateOnly GetJstDateOnlyType()
        {
            return DateOnly.FromDateTime(GetJstNow());
        }

        /// <summary>
        /// DateTimeをDateOnlyに変換
        /// </summary>
        /// <param name="dateTime">変換元の日時</param>
        /// <returns>DateOnly</returns>
        public static DateOnly ToDateOnly(DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }

        /// <summary>
        /// 2つの日付間の日数を計算
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <returns>日数（終了日 - 開始日）</returns>
        public static int CalculateDaysDifference(DateTime startDate, DateTime endDate)
        {
            return (int)(endDate.Date - startDate.Date).TotalDays;
        }

        /// <summary>
        /// 指定された日付が今日かどうか判定（JST基準）
        /// </summary>
        /// <param name="dateTime">判定する日時</param>
        /// <returns>今日かどうか</returns>
        public static bool IsToday(DateTime dateTime)
        {
            var today = GetJstDateOnly();
            var targetDate = GetDateOnly(dateTime);
            return today == targetDate;
        }

        /// <summary>
        /// 指定された日付が今週かどうか判定（JST基準、月曜日を週の開始とする）
        /// </summary>
        /// <param name="dateTime">判定する日時</param>
        /// <returns>今週かどうか</returns>
        public static bool IsThisWeek(DateTime dateTime)
        {
            var today = GetJstNow();
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var endOfWeek = startOfWeek.AddDays(6);
            
            var targetDate = GetDateOnly(dateTime);
            return targetDate >= GetDateOnly(startOfWeek) && targetDate <= GetDateOnly(endOfWeek);
        }
    }
}