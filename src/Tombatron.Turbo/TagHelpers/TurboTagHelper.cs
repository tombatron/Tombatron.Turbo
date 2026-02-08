using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;

namespace Tombatron.Turbo.TagHelpers;

/// <summary>
/// Tag helper that renders a turbo-stream-source-signalr element for real-time updates.
/// </summary>
/// <remarks>
/// This tag helper generates the client-side element that connects to the Turbo SignalR hub
/// and subscribes to the specified stream(s). When stream updates are pushed from the server,
/// they are automatically rendered by Turbo.js.
/// </remarks>
/// <example>
/// <code>
/// &lt;!-- Subscribe to a specific stream --&gt;
/// &lt;turbo stream="notifications"&gt;&lt;/turbo&gt;
///
/// &lt;!-- Subscribe to multiple streams --&gt;
/// &lt;turbo stream="notifications,updates"&gt;&lt;/turbo&gt;
///
/// &lt;!-- Auto-generate stream name based on user (authenticated: user:{id}, anonymous: session:{id}) --&gt;
/// &lt;turbo&gt;&lt;/turbo&gt;
///
/// &lt;!-- Custom hub URL --&gt;
/// &lt;turbo stream="messages" hub-url="/my-hub"&gt;&lt;/turbo&gt;
/// </code>
/// </example>
[HtmlTargetElement("turbo")]
public class TurboTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TurboOptions _options;

    /// <summary>
    /// Gets or sets the stream name(s) to subscribe to.
    /// Multiple stream names can be comma-separated.
    /// If not specified, a default stream name is generated based on the current user.
    /// </summary>
    [HtmlAttributeName("stream")]
    public string? Stream { get; set; }

    /// <summary>
    /// Gets or sets the SignalR hub URL.
    /// If not specified, uses the hub path from TurboOptions.
    /// </summary>
    [HtmlAttributeName("hub-url")]
    public string? HubUrl { get; set; }

    /// <summary>
    /// Gets or sets an optional ID for the generated element.
    /// </summary>
    [HtmlAttributeName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TurboTagHelper"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="options">The Turbo options.</param>
    public TurboTagHelper(IHttpContextAccessor httpContextAccessor, TurboOptions options)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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

        // Determine the stream name(s)
        string streamName = GetStreamName();

        // Determine the hub URL
        string hubUrl = GetHubUrl();

        // Handle multiple streams (comma-separated)
        string[] streams = ParseStreamNames(streamName);

        if (streams.Length == 0)
        {
            // No valid streams, suppress output
            output.SuppressOutput();
            return;
        }

        if (streams.Length == 1)
        {
            // Single stream - render one element
            RenderSingleStream(output, streams[0], hubUrl);
        }
        else
        {
            // Multiple streams - render a container with multiple elements
            RenderMultipleStreams(output, streams, hubUrl);
        }
    }

    /// <summary>
    /// Gets the stream name to subscribe to.
    /// </summary>
    /// <returns>The stream name.</returns>
    private string GetStreamName()
    {
        // If explicitly specified, use it
        if (!string.IsNullOrWhiteSpace(Stream))
        {
            return Stream;
        }

        // Generate default stream name based on user context
        return GenerateDefaultStreamName();
    }

    /// <summary>
    /// Generates a default stream name based on the current user.
    /// </summary>
    /// <returns>The generated stream name.</returns>
    internal string GenerateDefaultStreamName()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            return string.Empty;
        }

        ClaimsPrincipal? user = httpContext.User;

        // For authenticated users, use user:{userId}
        if (user?.Identity?.IsAuthenticated == true)
        {
            string? userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }

            // Fallback to name if no identifier
            string? userName = user.Identity.Name;

            if (!string.IsNullOrWhiteSpace(userName))
            {
                return $"user:{userName}";
            }
        }

        // For anonymous users, use session:{sessionId}
        try
        {
            ISession? session = httpContext.Session;

            if (session != null)
            {
                string sessionId = session.Id;

                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    return $"session:{sessionId}";
                }
            }
        }
        catch (InvalidOperationException)
        {
            // Session not configured or not available
        }

        // Fallback to connection:{connectionId}
        string connectionId = httpContext.Connection.Id;

        if (!string.IsNullOrWhiteSpace(connectionId))
        {
            return $"connection:{connectionId}";
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the hub URL to connect to.
    /// </summary>
    /// <returns>The hub URL.</returns>
    private string GetHubUrl()
    {
        if (!string.IsNullOrWhiteSpace(HubUrl))
        {
            return HubUrl;
        }

        return _options.HubPath;
    }

    /// <summary>
    /// Parses comma-separated stream names into an array.
    /// </summary>
    /// <param name="streamNames">The comma-separated stream names.</param>
    /// <returns>An array of stream names.</returns>
    internal static string[] ParseStreamNames(string streamNames)
    {
        if (string.IsNullOrWhiteSpace(streamNames))
        {
            return Array.Empty<string>();
        }

        return streamNames
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
    }

    /// <summary>
    /// Renders a single turbo-stream-source-signalr element.
    /// </summary>
    private void RenderSingleStream(TagHelperOutput output, string streamName, string hubUrl)
    {
        output.TagName = "turbo-stream-source-signalr";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("stream", streamName);
        output.Attributes.SetAttribute("hub-url", hubUrl);

        if (!string.IsNullOrWhiteSpace(Id))
        {
            output.Attributes.SetAttribute("id", Id);
        }

        // Clear any content - this element has no content
        output.Content.Clear();
    }

    /// <summary>
    /// Renders multiple turbo-stream-source-signalr elements in a container.
    /// </summary>
    private void RenderMultipleStreams(TagHelperOutput output, string[] streams, string hubUrl)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-turbo-streams", "true");

        if (!string.IsNullOrWhiteSpace(Id))
        {
            output.Attributes.SetAttribute("id", Id);
        }

        // Generate child elements
        var content = new System.Text.StringBuilder();

        foreach (string stream in streams)
        {
            content.Append($"<turbo-stream-source-signalr stream=\"{EscapeAttribute(stream)}\" hub-url=\"{EscapeAttribute(hubUrl)}\"></turbo-stream-source-signalr>");
        }

        output.Content.SetHtmlContent(content.ToString());
    }

    /// <summary>
    /// Escapes special characters for use in HTML attributes.
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <returns>The escaped value.</returns>
    internal static string EscapeAttribute(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("'", "&#39;");
    }
}
