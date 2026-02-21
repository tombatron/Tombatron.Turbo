using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Tombatron.Turbo.Stimulus;

/// <summary>
/// Extension methods for configuring Stimulus services in the dependency injection container.
/// </summary>
public static class StimulusServiceCollectionExtensions
{
    /// <summary>
    /// Adds Stimulus controller discovery services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddStimulus(this IServiceCollection services)
    {
        return services.AddStimulus(_ => { });
    }

    /// <summary>
    /// Adds Stimulus controller discovery services to the specified <see cref="IServiceCollection"/> with custom configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">An action to configure the <see cref="StimulusOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddStimulus(
        this IServiceCollection services,
        Action<StimulusOptions> configure)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var options = new StimulusOptions();
        configure(options);
        options.Validate();

        services.AddSingleton(options);
        services.AddSingleton<StimulusControllerRegistry>();
        services.AddTransient<IStartupFilter, StimulusStartupFilter>();

        return services;
    }
}
