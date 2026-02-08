using Microsoft.AspNetCore.Builder;
using Tombatron.Turbo.Middleware;

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
}
