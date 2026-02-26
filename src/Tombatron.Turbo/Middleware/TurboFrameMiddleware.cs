using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Tombatron.Turbo.Middleware;

/// <summary>
/// Middleware that detects Turbo and SignalR headers and makes their values available
/// via <see cref="HttpContext.Items"/> for use in page handlers and views.
/// </summary>
/// <remarks>
/// <para>
/// This middleware inspects three incoming headers and stores their values in
/// <see cref="HttpContext.Items"/>:
/// </para>
/// <list type="bullet">
///   <item><c>Turbo-Frame</c> — the requested frame ID (keys: <see cref="IsTurboFrameRequestKey"/>, <see cref="FrameIdKey"/>).</item>
///   <item><c>X-Turbo-Request-Id</c> — used for morph/refresh originator suppression (key: <see cref="RequestIdKey"/>).</item>
///   <item><c>X-SignalR-Connection-Id</c> — used for broadcast originator exclusion (key: <see cref="ConnectionIdKey"/>).</item>
/// </list>
/// <para>
/// When a <c>Turbo-Frame</c> header is present and <see cref="TurboOptions.AddVaryHeader"/> is enabled,
/// the middleware registers a callback that appends <c>Vary: Turbo-Frame</c> to the response.
/// </para>
/// <para>
/// Page handlers can use the extension methods in <see cref="TurboHttpContextExtensions"/>
/// to query these values:
/// </para>
/// <code>
/// public IActionResult OnGetItems()
/// {
///     if (HttpContext.IsTurboFrameRequest())
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
    /// The HTTP header name used to track the SignalR connection ID of the current user.
    /// </summary>
    public const string ConnectionIdHeader = "X-SignalR-Connection-Id";

    /// <summary>
    /// The key used to store the Turbo request ID in HttpContext.Items.
    /// </summary>
    public const string RequestIdKey = "Turbo.RequestId";

    /// <summary>
    /// The key used to store the SignalR connection ID in HttpContext.Items.
    /// </summary>
    public const string ConnectionIdKey = "Turbo.ConnectionId";

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
    /// Detects the <c>Turbo-Frame</c>, <c>X-Turbo-Request-Id</c>, and <c>X-SignalR-Connection-Id</c>
    /// headers, stores their values in <see cref="HttpContext.Items"/>, and conditionally registers
    /// a <c>Vary: Turbo-Frame</c> response header callback.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Check for Turbo-Frame header
        if (HasTurboFrameId(context.Request, out var frameId))
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
        if (HasTurboRequestId(context.Request, out var requestId))
        {
            context.Items[RequestIdKey] = requestId;

            _logger.LogDebug(
                "Turbo-Request-Id detected: {RequestId} on {Method} {Path}",
                requestId,
                context.Request.Method,
                context.Request.Path);
        }

        // Check for the X-SignalR-Connection-Id header
        if (HasConnectionId(context.Request, out var connectionId))
        {
            context.Items[ConnectionIdKey] = connectionId;

            _logger.LogDebug(
                "ConnectionId: {ConnectionId} on {Method} {Path}",
                connectionId,
                context.Request.Method,
                context.Request.Path);
        }

        await _next(context);
    }

    /// <summary>
    /// Extracts the Turbo-Frame header value from the request while also giving a signal of its
    /// existence.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="frameId">The frame ID, or null if not present.</param>
    /// <returns>A boolean indicating the presence of the header.</returns>
    internal static bool HasTurboFrameId(HttpRequest request, out string? frameId)
    {
        var hasTurboFrameId = request.Headers.TryGetValue(TurboFrameHeader, out var values);

        frameId = values.FirstOrDefault();

        return hasTurboFrameId && !string.IsNullOrEmpty(frameId);
    }

    /// <summary>
    /// Extracts the X-Turbo-Request-Id header value from the request.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="requestId">The request ID, or null if not present.</param>
    /// <returns>A boolean indicating the presence of the header.</returns>
    internal static bool HasTurboRequestId(HttpRequest request, out string? requestId)
    {
        var hasTurboRequestId = request.Headers.TryGetValue(TurboRequestIdHeader, out var values);

        requestId = values.FirstOrDefault();

        return hasTurboRequestId && !string.IsNullOrEmpty(requestId);
    }

    /// <summary>
    /// Extracts the X-SignalR-Connection-Id header value from the request.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="connectionId">The connection ID, or null if not present</param>
    /// <returns>A boolean indicating the presence of the header.</returns>
    internal static bool HasConnectionId(HttpRequest request, out string? connectionId)
    {
        var hasConnectionId = request.Headers.TryGetValue(ConnectionIdHeader, out var values);

        connectionId = values.FirstOrDefault();

        return hasConnectionId && !string.IsNullOrEmpty(connectionId);
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
