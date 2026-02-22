using Microsoft.AspNetCore.Http;
using Tombatron.Turbo.Middleware;

namespace Tombatron.Turbo;

/// <summary>
/// Extension methods for accessing Turbo Frame request information from HttpContext.
/// </summary>
public static class TurboHttpContextExtensions
{
    /// <summary>
    /// Gets whether the current request is a Turbo Frame request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>True if the request contains the Turbo-Frame header.</returns>
    public static bool IsTurboFrameRequest(this HttpContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return context.Items.TryGetValue(TurboFrameMiddleware.IsTurboFrameRequestKey, out var value)
               && value is true;
    }

    /// <summary>
    /// Gets the requested Turbo Frame ID from the current request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The frame ID, or null if this is not a Turbo Frame request.</returns>
    public static string? GetTurboFrameId(this HttpContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Items.TryGetValue(TurboFrameMiddleware.FrameIdKey, out var value))
        {
            return value as string;
        }

        return null;
    }

    /// <summary>
    /// Checks if the current Turbo Frame request matches the specified frame ID.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="frameId">The frame ID to check against.</param>
    /// <returns>True if the request is for the specified frame.</returns>
    public static bool IsTurboFrameRequest(this HttpContext context, string frameId)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (string.IsNullOrEmpty(frameId))
        {
            return false;
        }

        string? requestedFrameId = context.GetTurboFrameId();
        return string.Equals(requestedFrameId, frameId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the Turbo request ID from the current request, used for refresh originator suppression.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The request ID, or null if not present.</returns>
    public static string? GetTurboRequestId(this HttpContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Items.TryGetValue(TurboFrameMiddleware.RequestIdKey, out var value))
        {
            return value as string;
        }

        return null;
    }

    /// <summary>
    /// Gets whether the current request is a Turbo Stream request
    /// (i.e., the Accept header contains <c>text/vnd.turbo-stream.html</c>).
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>True if the request accepts Turbo Stream responses.</returns>
    public static bool IsTurboStreamRequest(this HttpContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return context.Request.Headers.Accept.ToString()
            .Contains("text/vnd.turbo-stream.html", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the current Turbo Frame request matches frames with the specified prefix.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="prefix">The frame ID prefix to check against.</param>
    /// <returns>True if the request is for a frame with the specified prefix.</returns>
    public static bool IsTurboFrameRequestWithPrefix(this HttpContext context, string prefix)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (string.IsNullOrEmpty(prefix))
        {
            return false;
        }

        string? requestedFrameId = context.GetTurboFrameId();
        return requestedFrameId != null &&
               requestedFrameId.StartsWith(prefix, StringComparison.Ordinal);
    }
}
