using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Tombatron.Turbo.TagHelpers;

/// <summary>
/// Tag helper for rendering turbo-frame elements with proper attributes.
/// </summary>
/// <remarks>
/// This tag helper renders a standard HTML <c>&lt;turbo-frame&gt;</c> element.
/// The <c>asp-frame-prefix</c> attribute is used for compile-time validation only
/// and is stripped from the rendered output.
///
/// For turbo-frame requests, create a page handler that checks for the Turbo-Frame
/// header and returns a partial view:
/// <code>
/// public IActionResult OnGetItems()
/// {
///     if (Request.Headers.ContainsKey("Turbo-Frame"))
///     {
///         return Partial("_CartItems", Model);
///     }
///     return RedirectToPage();
/// }
/// </code>
///
/// Usage:
/// <code>
/// &lt;turbo-frame id="cart-items" src="/Cart?handler=Items"&gt;
///     Loading...
/// &lt;/turbo-frame&gt;
/// </code>
/// </remarks>
[HtmlTargetElement("turbo-frame", TagStructure = TagStructure.NormalOrSelfClosing)]
public class TurboFrameTagHelper : TagHelper
{
    /// <summary>
    /// The unique identifier for this turbo-frame element.
    /// </summary>
    [HtmlAttributeName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The frame prefix used for dynamic ID validation.
    /// This attribute is consumed by the Roslyn analyzer and source generator,
    /// and is stripped from the rendered output.
    /// </summary>
    [HtmlAttributeName("asp-frame-prefix")]
    public string? FramePrefix { get; set; }

    /// <summary>
    /// The URL to load content from lazily or when refreshed.
    /// Maps to the <c>src</c> attribute.
    /// </summary>
    [HtmlAttributeName("src")]
    public string? Src { get; set; }

    /// <summary>
    /// Whether to lazily load the frame content.
    /// When set to "lazy", the frame content is loaded when the frame scrolls into view.
    /// Maps to the <c>loading</c> attribute.
    /// </summary>
    [HtmlAttributeName("loading")]
    public string? Loading { get; set; }

    /// <summary>
    /// Whether the frame is disabled.
    /// A disabled frame will not intercept navigation.
    /// </summary>
    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// The navigation target for the frame.
    /// Can be "_top" to break out of the frame.
    /// Maps to the <c>target</c> attribute.
    /// </summary>
    [HtmlAttributeName("target")]
    public string? Target { get; set; }

    /// <summary>
    /// Controls whether the browser should restore scroll position after navigation.
    /// Maps to the <c>autoscroll</c> attribute.
    /// </summary>
    [HtmlAttributeName("autoscroll")]
    public bool Autoscroll { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        // Keep the tag name as turbo-frame
        output.TagName = "turbo-frame";

        // Remove the asp-frame-prefix attribute from output (it's only for compile-time)
        output.Attributes.RemoveAll("asp-frame-prefix");

        // Get the frame id from context attributes if not set via property binding
        var frameId = Id ?? context.AllAttributes["id"]?.Value?.ToString();

        // Ensure the id attribute is present (it may have been consumed by property binding)
        if (!string.IsNullOrEmpty(frameId) && !output.Attributes.ContainsName("id"))
        {
            output.Attributes.SetAttribute("id", frameId);
        }

        // Add src attribute if specified
        if (!string.IsNullOrEmpty(Src))
        {
            output.Attributes.SetAttribute("src", Src);
        }

        // Add loading attribute if specified
        if (!string.IsNullOrEmpty(Loading))
        {
            output.Attributes.SetAttribute("loading", Loading);
        }

        // Add disabled attribute if true
        if (Disabled)
        {
            output.Attributes.SetAttribute("disabled", "disabled");
        }

        // Add target attribute if specified
        if (!string.IsNullOrEmpty(Target))
        {
            output.Attributes.SetAttribute("target", Target);
        }

        // Add autoscroll attribute if true
        if (Autoscroll)
        {
            output.Attributes.SetAttribute("autoscroll", "autoscroll");
        }
    }
}
