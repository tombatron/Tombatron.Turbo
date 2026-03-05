using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Tombatron.Turbo.TagHelpers;

/// <summary>
/// Tag helper that renders Turbo-related meta tags from a single <c>&lt;turbo-meta&gt;</c> element.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// &lt;turbo-meta refresh-method="Morph" refresh-scroll="Preserve" cache-control="no-cache" /&gt;
/// </code>
/// Outputs:
/// <code>
/// &lt;meta name="turbo-refresh-method" content="morph"&gt;
/// &lt;meta name="turbo-refresh-scroll" content="preserve"&gt;
/// &lt;meta name="turbo-cache-control" content="no-cache"&gt;
/// </code>
/// </remarks>
[HtmlTargetElement("turbo-meta", TagStructure = TagStructure.WithoutEndTag)]
public class TurboMetaTagHelper : TagHelper
{
    /// <summary>
    /// The refresh method to use. Emits a <c>&lt;meta name="turbo-refresh-method"&gt;</c> tag.
    /// </summary>
    [HtmlAttributeName("refresh-method")]
    public TurboRefreshMethod? RefreshMethod { get; set; }

    /// <summary>
    /// The scroll behavior during refresh. Emits a <c>&lt;meta name="turbo-refresh-scroll"&gt;</c> tag.
    /// </summary>
    [HtmlAttributeName("refresh-scroll")]
    public TurboRefreshScroll? RefreshScroll { get; set; }

    /// <summary>
    /// The cache control directive. Emits a <c>&lt;meta name="turbo-cache-control"&gt;</c> tag.
    /// </summary>
    [HtmlAttributeName("cache-control")]
    public string? CacheControl { get; set; }

    /// <summary>
    /// The visit control directive. Emits a <c>&lt;meta name="turbo-visit-control"&gt;</c> tag.
    /// </summary>
    [HtmlAttributeName("visit-control")]
    public string? VisitControl { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        output.SuppressOutput();

        if (RefreshMethod.HasValue)
        {
            output.PostElement.AppendHtml($"<meta name=\"turbo-refresh-method\" content=\"{RefreshMethod.Value.ToString().ToLowerInvariant()}\">");
        }

        if (RefreshScroll.HasValue)
        {
            output.PostElement.AppendHtml($"<meta name=\"turbo-refresh-scroll\" content=\"{RefreshScroll.Value.ToString().ToLowerInvariant()}\">");
        }

        if (!string.IsNullOrEmpty(CacheControl))
        {
            output.PostElement.AppendHtml($"<meta name=\"turbo-cache-control\" content=\"{System.Text.Encodings.Web.HtmlEncoder.Default.Encode(CacheControl)}\">");
        }

        if (!string.IsNullOrEmpty(VisitControl))
        {
            output.PostElement.AppendHtml($"<meta name=\"turbo-visit-control\" content=\"{System.Text.Encodings.Web.HtmlEncoder.Default.Encode(VisitControl)}\">");
        }
    }
}
