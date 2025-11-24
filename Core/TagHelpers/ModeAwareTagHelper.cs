using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using CanLove_Backend.Domain.Case.ViewModels.Basic;

namespace CanLove_Backend.Core.TagHelpers;

[HtmlTargetElement("input", Attributes = "form-mode")]
[HtmlTargetElement("select", Attributes = "form-mode")]
[HtmlTargetElement("textarea", Attributes = "form-mode")]
public class ModeAwareTagHelper : TagHelper
{
    [HtmlAttributeName("form-mode")]
    public CaseFormMode FormMode { get; set; }

    [HtmlAttributeName("readonly-when")]
    public string? ReadonlyWhen { get; set; }

    [HtmlAttributeName("disabled-when")]
    public string? DisabledWhen { get; set; }

    [HtmlAttributeName("hidden-when")]
    public string? HiddenWhen { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        bool shouldReadonly = MatchesMode(ReadonlyWhen, FormMode, defaultTo: false);
        bool shouldDisabled = MatchesMode(DisabledWhen, FormMode, defaultTo: false);
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

        if (shouldReadonly)
        {
            // select 不支援 readonly，用 disabled
            if (string.Equals(output.TagName, "select", StringComparison.OrdinalIgnoreCase))
            {
                output.Attributes.SetAttribute("disabled", "disabled");
            }
            else
            {
                output.Attributes.SetAttribute("readonly", "readonly");
            }
        }

        if (shouldDisabled)
        {
            output.Attributes.SetAttribute("disabled", "disabled");
        }

        // 清理自訂屬性，避免輸出到 HTML
        output.Attributes.RemoveAll("form-mode");
        output.Attributes.RemoveAll("readonly-when");
        output.Attributes.RemoveAll("disabled-when");
        output.Attributes.RemoveAll("hidden-when");
    }

    private static bool MatchesMode(string? csvModes, CaseFormMode current, bool defaultTo)
    {
        if (string.IsNullOrWhiteSpace(csvModes)) return defaultTo;
        var tokens = csvModes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var token in tokens)
        {
            if (Enum.TryParse<CaseFormMode>(token, ignoreCase: true, out var parsed))
            {
                if (parsed == current) return true;
            }
        }
        return false;
    }
}


