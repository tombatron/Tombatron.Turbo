using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;

namespace Tombatron.Turbo.TagHelpers;

/// <summary>
/// Rendering mode for the turbo-scripts tag helper.
/// </summary>
public enum TurboScriptsMode
{
    /// <summary>Render standard script tags (backward-compatible default).</summary>
    Traditional,

    /// <summary>Render an import map block with module preloads.</summary>
    Importmap
}

/// <summary>
/// Tag helper that renders the required Turbo.js and SignalR bridge script tags.
/// </summary>
[HtmlTargetElement("turbo-scripts", TagStructure = TagStructure.WithoutEndTag)]
public class TurboScriptsTagHelper : TagHelper
{
    internal const string ContentPathPrefix = "/_content/Tombatron.Turbo/dist/";
    internal const string BridgeBundledMinJs = "turbo-signalr.bundled.min.js";
    internal const string BridgeBundledJs = "turbo-signalr.bundled.js";
    internal const string BridgeBundledEsmJs = "turbo-signalr.bundled.esm.js";

    private readonly IWebHostEnvironment _environment;
    private readonly TurboOptions _options;

    /// <summary>
    /// Gets or sets the rendering mode.
    /// </summary>
    [HtmlAttributeName("mode")]
    public TurboScriptsMode Mode { get; set; } = TurboScriptsMode.Traditional;

    /// <summary>
    /// Initializes a new instance of the <see cref="TurboScriptsTagHelper"/> class.
    /// </summary>
    public TurboScriptsTagHelper(IWebHostEnvironment environment, TurboOptions options)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

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

        output.TagName = null;

        if (Mode == TurboScriptsMode.Traditional)
        {
            RenderTraditional(output);
        }
        else
        {
            RenderImportmap(output);
        }
    }

    private void RenderTraditional(TagHelperOutput output)
    {
        var sb = new StringBuilder();
        var entries = _options.ImportMap.Entries;
        bool isDevelopment = _environment.IsDevelopment();

        foreach (var entry in entries)
        {
            if (!entry.Value.Preload)
            {
                continue;
            }

            if (IsBridgeEntry(entry.Value.Path))
            {
                string bridgeFile = isDevelopment ? BridgeBundledJs : BridgeBundledMinJs;
                sb.AppendLine($"<script src=\"{ContentPathPrefix}{bridgeFile}\"></script>");
            }
            else
            {
                sb.AppendLine($"<script type=\"module\" src=\"{entry.Value.Path}\"></script>");
            }
        }

        output.Content.SetHtmlContent(sb.ToString());
    }

    private void RenderImportmap(TagHelperOutput output)
    {
        var sb = new StringBuilder();
        var entries = _options.ImportMap.Entries;

        // Build the importmap JSON
        var imports = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            imports[entry.Key] = entry.Value.Path;
        }

        var importMap = new { imports };
        string json = JsonSerializer.Serialize(importMap, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        sb.AppendLine($"<script type=\"importmap\">");
        sb.AppendLine(json);
        sb.AppendLine("</script>");

        // Render modulepreload links for preloaded entries
        foreach (var entry in entries)
        {
            if (entry.Value.Preload)
            {
                sb.AppendLine($"<link rel=\"modulepreload\" href=\"{entry.Value.Path}\">");
            }
        }

        // Render module import statements for preloaded entries
        sb.AppendLine("<script type=\"module\">");

        foreach (var entry in entries)
        {
            if (entry.Value.Preload)
            {
                sb.AppendLine($"import \"{entry.Key}\";");
            }
        }

        sb.AppendLine("</script>");

        output.Content.SetHtmlContent(sb.ToString());
    }

    private static bool IsBridgeEntry(string path) =>
        path.StartsWith(ContentPathPrefix, StringComparison.OrdinalIgnoreCase) &&
        path.Contains("turbo-signalr", StringComparison.OrdinalIgnoreCase);
}
