using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace CanLove_Backend.TagHelpers
{
    /// <summary>
    /// 日期格式化 Tag Helper
    /// </summary>
    [HtmlTargetElement("date", Attributes = "value")]
    public class DateTagHelper : TagHelper
    {
        /// <summary>
        /// 要格式化的日期值
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// 日期格式 (預設: yyyy/MM/dd)
        /// 支援格式: date, datetime, full-datetime, month
        /// </summary>
        public string Format { get; set; } = "date";

        /// <summary>
        /// 自訂格式字串 (當 Format 為 custom 時使用)
        /// </summary>
        public string CustomFormat { get; set; } = "yyyy/MM/dd";

        /// <summary>
        /// 空值時顯示的文字
        /// </summary>
        public string EmptyText { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            
            if (Value == null)
            {
                output.Content.SetContent(EmptyText);
                return;
            }

            string formattedDate = Format switch
            {
                "date" => GetFormattedDate("yyyy/MM/dd"),
                "datetime" => GetFormattedDate("yyyy/MM/dd HH:mm"),
                "full-datetime" => GetFormattedDate("yyyy/MM/dd HH:mm:ss"),
                "month" => GetFormattedDate("yyyy/MM"),
                "custom" => GetFormattedDate(CustomFormat),
                _ => GetFormattedDate("yyyy/MM/dd")
            };

            output.Content.SetContent(formattedDate);
        }

        private string GetFormattedDate(string format)
        {
            if (Value is DateTime dt)
                return dt.ToString(format);
            
            if (Value is DateTime dtn)
                return dtn.ToString(format);
            
            if (Value is DateOnly dateOnly)
                return dateOnly.ToString(format);
            
            if (Value == null)
                return "";
            
            return Value.ToString() ?? "";
        }
    }

    /// <summary>
    /// 日期時間格式化 Tag Helper
    /// </summary>
    [HtmlTargetElement("datetime", Attributes = "value")]
    public class DateTimeTagHelper : TagHelper
    {
        /// <summary>
        /// 要格式化的日期時間值
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// 是否顯示秒數
        /// </summary>
        public bool ShowSeconds { get; set; } = false;

        /// <summary>
        /// 空值時顯示的文字
        /// </summary>
        public string EmptyText { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            
            if (Value == null)
            {
                output.Content.SetContent(EmptyText);
                return;
            }

            string format = ShowSeconds ? "yyyy/MM/dd HH:mm:ss" : "yyyy/MM/dd HH:mm";
            string formattedDate = GetFormattedDateTime(format);
            output.Content.SetContent(formattedDate);
        }

        private string GetFormattedDateTime(string format)
        {
            if (Value is DateTime dt)
                return dt.ToString(format);
            
            if (Value is DateTime dtn)
                return dtn.ToString(format);
            
            if (Value is DateOnly dateOnly)
                return dateOnly.ToString(format);
            
            if (Value == null)
                return "";
            
            return Value.ToString() ?? "";
        }
    }
}
