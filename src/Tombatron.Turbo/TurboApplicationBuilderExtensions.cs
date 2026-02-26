using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Tombatron.Turbo.Middleware;
using Tombatron.Turbo.Streams;

namespace Tombatron.Turbo;

/// <summary>
/// Extension methods for configuring Turbo middleware in the application pipeline.
/// </summary>
public static class TurboApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the Turbo middleware to the application pipeline.
    /// This must be called after <c>UseRouting()</c> and before <c>UseEndpoints()</c>.
    /// </summary>
    /// <remarks>
    /// The middleware detects the <c>Turbo-Frame</c>, <c>X-Turbo-Request-Id</c>, and
    /// <c>X-SignalR-Connection-Id</c> headers and stores their values in
    /// <c>HttpContext.Items</c> so downstream handlers can query them via the
    /// extension methods in <see cref="TurboHttpContextExtensions"/>.
    /// When a <c>Turbo-Frame</c> header is present and <see cref="TurboOptions.AddVaryHeader"/>
    /// is enabled, a <c>Vary: Turbo-Frame</c> response header is appended automatically.
    /// </remarks>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when app is null.</exception>
    /// <example>
    /// <code>
    /// app.UseRouting();
    /// app.UseTurbo();
    /// app.UseEndpoints(endpoints => { ... });
    /// </code>
    /// </example>
    public static IApplicationBuilder UseTurbo(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        app.UseMiddleware<TurboFrameMiddleware>();

        return app;
    }

    /// <summary>
    /// Maps the Turbo SignalR hub to the configured path.
    /// This must be called to enable Turbo Streams functionality.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the hub to.</param>
    /// <param name="configureOptions">An optional callback to configure <see cref="HttpConnectionDispatcherOptions"/> for the hub endpoint.</param>
    /// <returns>The hub endpoint convention builder for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when endpoints is null.</exception>
    /// <example>
    /// <code>
    /// app.MapTurboHub();
    /// // or with explicit path:
    /// app.MapTurboHub("/my-turbo-hub");
    /// </code>
    /// </example>
    public static HubEndpointConventionBuilder MapTurboHub(this IEndpointRouteBuilder endpoints, Action<HttpConnectionDispatcherOptions>? configureOptions = null)
    {
        if (endpoints == null)
        {
            throw new ArgumentNullException(nameof(endpoints));
        }

        var options = endpoints.ServiceProvider.GetRequiredService<TurboOptions>();

        if (configureOptions != null)
        {
            return endpoints.MapHub<TurboHub>(options.HubPath, configureOptions);
        }

        return endpoints.MapHub<TurboHub>(options.HubPath);
    }

    /// <summary>
    /// Maps the Turbo SignalR hub to a custom path.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the hub to.</param>
    /// <param name="path">The path to map the hub to.</param>
    /// <param name="configureOptions">An optional callback to configure <see cref="HttpConnectionDispatcherOptions"/> for the hub endpoint.</param>
    /// <returns>The hub endpoint convention builder for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when endpoints or path is null.</exception>
    /// <exception cref="ArgumentException">Thrown when path is empty or whitespace.</exception>
    /// <example>
    /// <code>
    /// app.MapTurboHub("/custom-turbo-hub");
    /// </code>
    /// </example>
    public static HubEndpointConventionBuilder MapTurboHub(this IEndpointRouteBuilder endpoints, string path, Action<HttpConnectionDispatcherOptions>? configureOptions = null)
    {
        if (endpoints == null)
        {
            throw new ArgumentNullException(nameof(endpoints));
        }

        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty or whitespace.", nameof(path));
        }

        if (configureOptions != null)
        {
            return endpoints.MapHub<TurboHub>(path, configureOptions);
        }

        return endpoints.MapHub<TurboHub>(path);
    }
}
