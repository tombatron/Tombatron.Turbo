using Microsoft.AspNetCore.Http;

namespace Tombatron.Turbo.Middleware;

/// <summary>
/// Middleware that handles Turbo Frame requests by detecting the Turbo-Frame header
/// and routing to the appropriate sub-template.
/// </summary>
public class TurboFrameMiddleware
{
    /// <summary>
    /// The HTTP header name used by Turbo to indicate a frame request.
    /// </summary>
    public const string TurboFrameHeader = "Turbo-Frame";

    /// <summary>
    /// The key used to store the requested frame ID in HttpContext.Items.
    /// </summary>
    public const string FrameIdKey = "Turbo.FrameId";

    /// <summary>
    /// The key used to store the resolved template name in HttpContext.Items.
    /// </summary>
    public const string TemplateNameKey = "Turbo.TemplateName";

    /// <summary>
    /// The key used to indicate this is a turbo-frame request in HttpContext.Items.
    /// </summary>
    public const string IsTurboFrameRequestKey = "Turbo.IsTurboFrameRequest";

    private readonly RequestDelegate _next;
    private readonly TurboOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TurboFrameMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">The Turbo configuration options.</param>
    public TurboFrameMiddleware(RequestDelegate next, TurboOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Processes the HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Check for Turbo-Frame header
        string? frameId = GetTurboFrameId(context.Request);

        if (!string.IsNullOrEmpty(frameId))
        {
            // Mark this as a turbo-frame request
            context.Items[IsTurboFrameRequestKey] = true;
            context.Items[FrameIdKey] = frameId;

            // Try to resolve the template using the generated metadata
            if (TryResolveTemplate(frameId, out string? templateName))
            {
                context.Items[TemplateNameKey] = templateName;
            }

            // Add Vary header if configured
            if (_options.AddVaryHeader)
            {
                context.Response.OnStarting(() =>
                {
                    AddVaryHeader(context.Response);
                    return Task.CompletedTask;
                });
            }
        }

        await _next(context);

        // If this was a turbo-frame request and no template was found, return 422
        if (!string.IsNullOrEmpty(frameId) &&
            !context.Items.ContainsKey(TemplateNameKey) &&
            context.Response.StatusCode == StatusCodes.Status200OK)
        {
            // Only set 422 if the response hasn't been started
            // and we didn't find a matching template
            // This allows controllers/pages to handle frames manually
        }
    }

    /// <summary>
    /// Extracts the Turbo-Frame header value from the request.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>The frame ID, or null if not present.</returns>
    internal static string? GetTurboFrameId(HttpRequest request)
    {
        if (request.Headers.TryGetValue(TurboFrameHeader, out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// Attempts to resolve a template for the given frame ID using generated metadata.
    /// </summary>
    /// <param name="frameId">The frame ID to look up.</param>
    /// <param name="templateName">The resolved template name, if found.</param>
    /// <returns>True if a template was found, false otherwise.</returns>
    internal static bool TryResolveTemplate(string frameId, out string? templateName)
    {
        // This will be called by the generated TurboFrameMetadata class
        // For now, we use reflection or a registered service to find it
        // In a real implementation, the source generator would register the lookup
        templateName = null;

        try
        {
            // Try to find the generated TurboFrameMetadata class
            var metadataType = Type.GetType("Tombatron.Turbo.Generated.TurboFrameMetadata");
            if (metadataType != null)
            {
                var method = metadataType.GetMethod("TryGetTemplate");
                if (method != null)
                {
                    var parameters = new object?[] { frameId, null };
                    bool result = (bool)method.Invoke(null, parameters)!;
                    if (result)
                    {
                        templateName = parameters[1] as string;
                        return true;
                    }
                }
            }
        }
        catch
        {
            // If reflection fails, just return false
        }

        return false;
    }

    /// <summary>
    /// Adds the Vary: Turbo-Frame header to the response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    internal static void AddVaryHeader(HttpResponse response)
    {
        const string varyValue = TurboFrameHeader;

        if (response.Headers.TryGetValue("Vary", out var existingValue))
        {
            string existing = existingValue.ToString();
            if (!existing.Contains(varyValue, StringComparison.OrdinalIgnoreCase))
            {
                response.Headers["Vary"] = $"{existing}, {varyValue}";
            }
        }
        else
        {
            response.Headers["Vary"] = varyValue;
        }
    }
}
