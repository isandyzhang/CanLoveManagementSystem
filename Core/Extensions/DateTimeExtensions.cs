using System;

namespace CanLove_Backend.Core.Extensions
{
    /// <summary>
    /// DateTime 擴展方法，提供統一的日期格式
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 取得台灣時間（UTC+8）
        /// </summary>
        public static DateTime TaiwanTime => DateTime.UtcNow.AddHours(8);

        /// <summary>
        /// 轉換為台灣日期格式 (yyyy/MM/dd)
        /// </summary>
        public static string ToTaiwanDateString(this DateOnly? date)
        {
            return date?.ToString("yyyy/MM/dd") ?? "";
        }

        /// <summary>
        /// 轉換為台灣日期格式 (yyyy/MM/dd)
        /// </summary>
        public static string ToTaiwanDateString(this DateOnly date)
        {
            return date.ToString("yyyy/MM/dd");
        }

        /// <summary>
        /// 轉換為台灣日期時間格式 (yyyy/MM/dd HH:mm)
        /// </summary>
        public static string ToTaiwanDateTimeString(this DateTime? date)
        {
            return date?.ToString("yyyy/MM/dd HH:mm") ?? "";
        }
    }
}