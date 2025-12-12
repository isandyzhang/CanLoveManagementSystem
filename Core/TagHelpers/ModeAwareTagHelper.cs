using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CanLove_Backend.Core.TagHelpers;

/// <summary>
/// Tag Helper 用於根據表單模式控制欄位的唯讀/禁用狀態
/// </summary>
[HtmlTargetElement("input", Attributes = "form-mode")]
[HtmlTargetElement("select", Attributes = "form-mode")]
[HtmlTargetElement("textarea", Attributes = "form-mode")]
public class ModeAwareTagHelper : TagHelper
{
    [HtmlAttributeName("form-mode")]
    public CanLove_Backend.Application.ViewModels.Case.Basic.CaseFormMode FormMode { get; set; }

    /// <summary>
    /// 指定在哪些模式下鎖定欄位（input/textarea 設為 readonly，select 設為 disabled）
    /// 可用逗號分隔多個模式，例如: "ReadOnly,Review"
    /// </summary>
    [HtmlAttributeName("locked-when")]
    public string? LockedWhen { get; set; }

    /// <summary>
    /// 已棄用：請改用 locked-when
    /// </summary>
    [Obsolete("請改用 locked-when 屬性")]
    [HtmlAttributeName("readonly-when")]
    public string? ReadonlyWhen { get; set; }

    /// <summary>
    /// 已棄用：請改用 locked-when
    /// </summary>
    [Obsolete("請改用 locked-when 屬性")]
    [HtmlAttributeName("disabled-when")]
    public string? DisabledWhen { get; set; }

    /// <summary>
    /// 指定在哪些模式下隱藏欄位
    /// </summary>
    [HtmlAttributeName("hidden-when")]
    public string? HiddenWhen { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // locked-when 優先，若有設定則忽略舊的 readonly-when 和 disabled-when
        bool shouldLock = MatchesMode(LockedWhen, FormMode, defaultTo: false);
        
        // 向後兼容：如果沒有使用 locked-when，則檢查舊的屬性
#pragma warning disable CS0618 // 暫時忽略 Obsolete 警告
        bool shouldReadonly = !shouldLock && MatchesMode(ReadonlyWhen, FormMode, defaultTo: false);
        bool shouldDisabled = !shouldLock && MatchesMode(DisabledWhen, FormMode, defaultTo: false);
#pragma warning restore CS0618

        bool shouldHidden = MatchesMode(HiddenWhen, FormMode, defaultTo: false);

        if (shouldHidden)
        {
            // 對 input 可改 type=hidden，其餘元素用 style 隱藏
            if (string.Equals(output.TagName, "input", StringComparison.OrdinalIgnoreCase))
            {
                output.Attributes.SetAttribute("type", "hidden");
            }
            else
            {
                var existingStyle = output.Attributes.FirstOrDefault(a => a.Name == "style")?.Value?.ToString() ?? string.Empty;
                var newStyle = string.IsNullOrWhiteSpace(existingStyle) ? "display:none" : existingStyle + ";display:none";
                output.Attributes.SetAttribute("style", newStyle);
            }
        }

        // 處理 locked-when（新的統一屬性）
        if (shouldLock)
        {
            // select 不支援 readonly，用 disabled
            if (string.Equals(output.TagName, "select", StringComparison.OrdinalIgnoreCase))
            {
                output.Attributes.SetAttribute("disabled", "disabled");
            }
            else
            {
                // 注意：input[type=date] 等在多數瀏覽器中即使 readonly 仍可開啟選擇器
                // 為了讓「檢視/刪除/審核」等鎖定畫面保持一致不可編輯，日期/時間類一律用 disabled。
                if (string.Equals(output.TagName, "input", StringComparison.OrdinalIgnoreCase))
                {
                    var type = output.Attributes.FirstOrDefault(a => a.Name == "type")?.Value?.ToString();
                    var typeLower = type?.Trim().ToLowerInvariant();
                    if (typeLower is "date" or "datetime-local" or "month" or "week" or "time")
                    {
                        output.Attributes.SetAttribute("disabled", "disabled");
                    }
                    else
                    {
                        output.Attributes.SetAttribute("readonly", "readonly");
                    }
                }
                else
                {
                    output.Attributes.SetAttribute("readonly", "readonly");
                }
            }
        }

        // 向後兼容：處理舊的 readonly-when
        if (shouldReadonly)
        {
            // select 不支援 readonly，用 disabled
            if (string.Equals(output.TagName, "select", StringComparison.OrdinalIgnoreCase))
            {
                output.Attributes.SetAttribute("disabled", "disabled");
            }
            else
            {
                // 同 locked-when：日期/時間類 input 需用 disabled 才能真正鎖定互動
                if (string.Equals(output.TagName, "input", StringComparison.OrdinalIgnoreCase))
                {
                    var type = output.Attributes.FirstOrDefault(a => a.Name == "type")?.Value?.ToString();
                    var typeLower = type?.Trim().ToLowerInvariant();
                    if (typeLower is "date" or "datetime-local" or "month" or "week" or "time")
                    {
                        output.Attributes.SetAttribute("disabled", "disabled");
                    }
                    else
                    {
                        output.Attributes.SetAttribute("readonly", "readonly");
                    }
                }
                else
                {
                    output.Attributes.SetAttribute("readonly", "readonly");
                }
            }
        }

        // 向後兼容：處理舊的 disabled-when
        if (shouldDisabled)
        {
            output.Attributes.SetAttribute("disabled", "disabled");
        }

        // 清理自訂屬性，避免輸出到 HTML
        output.Attributes.RemoveAll("form-mode");
        output.Attributes.RemoveAll("locked-when");
        output.Attributes.RemoveAll("readonly-when");
        output.Attributes.RemoveAll("disabled-when");
        output.Attributes.RemoveAll("hidden-when");
    }

    private static bool MatchesMode(string? csvModes, CanLove_Backend.Application.ViewModels.Case.Basic.CaseFormMode current, bool defaultTo)
    {
        if (string.IsNullOrWhiteSpace(csvModes)) return defaultTo;
        var tokens = csvModes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var token in tokens)
        {
            if (Enum.TryParse<CanLove_Backend.Application.ViewModels.Case.Basic.CaseFormMode>(token, ignoreCase: true, out var parsed))
            {
                if (parsed == current) return true;
            }
        }
        return false;
    }
}
