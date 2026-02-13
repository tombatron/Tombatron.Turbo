using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tombatron.Turbo.Rendering;
using Tombatron.Turbo.Streams;

namespace Tombatron.Turbo;

/// <summary>
/// Extension methods for configuring Turbo services in the dependency injection container.
/// </summary>
public static class TurboServiceCollectionExtensions
{
    /// <summary>
    /// Adds Turbo services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    /// <example>
    /// <code>
    /// builder.Services.AddTurbo();
    /// </code>
    /// </example>
    public static IServiceCollection AddTurbo(this IServiceCollection services)
    {
        return services.AddTurbo(_ => { });
    }

    /// <summary>
    /// Adds Turbo services to the specified <see cref="IServiceCollection"/> with custom configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">An action to configure the <see cref="TurboOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configure is null.</exception>
    /// <example>
    /// <code>
    /// builder.Services.AddTurbo(options =>
    /// {
    ///     options.HubPath = "/my-turbo-hub";
    ///     options.UseSignedStreamNames = false;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddTurbo(
        this IServiceCollection services,
        Action<TurboOptions> configure)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        TurboOptions options = new();
        configure(options);
        options.Validate();

        services.AddSingleton(options);
        services.AddSignalR();

        // Register IHttpContextAccessor for TurboTagHelper
        services.AddHttpContextAccessor();

        // Register Turbo Streams services
        services.TryAddSingleton<ITurboStreamAuthorization, DefaultTurboStreamAuthorization>();
        services.AddSingleton<ITurbo, TurboService>();

        // Register partial rendering service
        services.AddSingleton<IPartialRenderer, PartialRenderer>();

        return services;
    }
}
