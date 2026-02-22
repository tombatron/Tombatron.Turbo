using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Tombatron.Turbo.Middleware;

/// <summary>
/// Middleware that detects Turbo Frame requests and makes the frame ID available
/// via HttpContext.Items for use in page handlers and views.
/// </summary>
/// <remarks>
/// This middleware detects the Turbo-Frame header and stores the requested frame ID
/// in HttpContext.Items. Page handlers can then check for this and return appropriate
/// partial views:
/// <code>
/// public IActionResult OnGetItems()
/// {
///     if (HttpContext.Items.ContainsKey(TurboFrameMiddleware.IsTurboFrameRequestKey))
///     {
///         return Partial("_Items", Model);
///     }
///     return RedirectToPage();
/// }
/// </code>
/// </remarks>
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
    /// The key used to indicate this is a turbo-frame request in HttpContext.Items.
    /// </summary>
    public const string IsTurboFrameRequestKey = "Turbo.IsTurboFrameRequest";

    /// <summary>
    /// The HTTP header name used by Turbo to identify the originating request for refresh suppression.
    /// </summary>
    public const string TurboRequestIdHeader = "X-Turbo-Request-Id";

    /// <summary>
    /// The key used to store the Turbo request ID in HttpContext.Items.
    /// </summary>
    public const string RequestIdKey = "Turbo.RequestId";

    private readonly RequestDelegate _next;
    private readonly TurboOptions _options;
    private readonly ILogger<TurboFrameMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TurboFrameMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">The Turbo configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public TurboFrameMiddleware(RequestDelegate next, TurboOptions options, ILogger<TurboFrameMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            // Mark this as a turbo-frame request and store the frame ID
            context.Items[IsTurboFrameRequestKey] = true;
            context.Items[FrameIdKey] = frameId;

            _logger.LogDebug(
                "Turbo-Frame request detected for frame {FrameId} on {Method} {Path}",
                frameId,
                context.Request.Method,
                context.Request.Path);

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

        // Check for X-Turbo-Request-Id header
        var requestId = GetTurboRequestId(context.Request);

        if (!string.IsNullOrEmpty(requestId))
        {
            context.Items[RequestIdKey] = requestId;

            _logger.LogDebug(
                "Turbo-Request-Id detected: {RequestId} on {Method} {Path}",
                requestId,
                context.Request.Method,
                context.Request.Path);
        }

        await _next(context);
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
    /// Extracts the X-Turbo-Request-Id header value from the request.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>The request ID, or null if not present.</returns>
    internal static string? GetTurboRequestId(HttpRequest request)
    {
        if (request.Headers.TryGetValue(TurboRequestIdHeader, out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
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
