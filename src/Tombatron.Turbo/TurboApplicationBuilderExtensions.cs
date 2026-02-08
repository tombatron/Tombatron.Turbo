using Microsoft.AspNetCore.Builder;
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
    /// <returns>The hub endpoint convention builder for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when endpoints is null.</exception>
    /// <example>
    /// <code>
    /// app.MapTurboHub();
    /// // or with explicit path:
    /// app.MapTurboHub("/my-turbo-hub");
    /// </code>
    /// </example>
    public static HubEndpointConventionBuilder MapTurboHub(this IEndpointRouteBuilder endpoints)
    {
        if (endpoints == null)
        {
            throw new ArgumentNullException(nameof(endpoints));
        }

        TurboOptions options = endpoints.ServiceProvider.GetRequiredService<TurboOptions>();
        return endpoints.MapHub<TurboHub>(options.HubPath);
    }

    /// <summary>
    /// Maps the Turbo SignalR hub to a custom path.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the hub to.</param>
    /// <param name="path">The path to map the hub to.</param>
    /// <returns>The hub endpoint convention builder for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when endpoints or path is null.</exception>
    /// <exception cref="ArgumentException">Thrown when path is empty or whitespace.</exception>
    /// <example>
    /// <code>
    /// app.MapTurboHub("/custom-turbo-hub");
    /// </code>
    /// </example>
    public static HubEndpointConventionBuilder MapTurboHub(this IEndpointRouteBuilder endpoints, string path)
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

        return endpoints.MapHub<TurboHub>(path);
    }
}
