using Microsoft.AspNetCore.Http;

namespace Tombatron.Turbo.Stimulus;

/// <summary>
/// Middleware that serves the generated Stimulus controller index module.
/// </summary>
internal sealed class StimulusMiddleware
{
    private readonly RequestDelegate _next;
    private readonly StimulusControllerRegistry _registry;
    private readonly StimulusOptions _options;

    public StimulusMiddleware(
        RequestDelegate next,
        StimulusControllerRegistry registry,
        StimulusOptions options)
    {
        _next = next;
        _registry = registry;
        _options = options;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Get &&
            string.Equals(context.Request.Path, _options.IndexEndpointPath, StringComparison.OrdinalIgnoreCase))
        {
            var etag = _registry.ETag;

            if (context.Request.Headers.IfNoneMatch.ToString() == etag)
            {
                context.Response.StatusCode = StatusCodes.Status304NotModified;
                return;
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/javascript";
            context.Response.Headers.ETag = etag;
            context.Response.Headers.CacheControl = "no-cache";

            await context.Response.WriteAsync(_registry.GeneratedIndexModule);
            return;
        }

        await _next(context);
    }
}
