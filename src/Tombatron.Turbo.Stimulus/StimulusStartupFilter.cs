using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Tombatron.Turbo.Stimulus;

/// <summary>
/// Startup filter that wires up Stimulus controller discovery and import map integration.
/// Runs after all services are configured but before the first request.
/// </summary>
internal sealed class StimulusStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            var turboOptions = app.ApplicationServices.GetRequiredService<TurboOptions>();
            var stimulusOptions = app.ApplicationServices.GetRequiredService<StimulusOptions>();
            var registry = app.ApplicationServices.GetRequiredService<StimulusControllerRegistry>();
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            var logger = app.ApplicationServices.GetRequiredService<ILogger<StimulusStartupFilter>>();

            // Initial discovery
            registry.Rebuild(env.WebRootFileProvider, stimulusOptions.ControllersPath, logger);

            // Add import map pins for Stimulus core
            turboOptions.ImportMap.Pin("@hotwired/stimulus", stimulusOptions.StimulusCdnUrl, preload: true);
            turboOptions.ImportMap.Pin(
                "_stimulus/application",
                "/_content/Tombatron.Turbo.Stimulus/_stimulus/application.js",
                preload: true);

            // Add import map pin for the controller index
            turboOptions.ImportMap.Pin(
                "_stimulus/controllers",
                stimulusOptions.IndexEndpointPath,
                preload: true);

            // Add a pin for each discovered controller
            foreach (var controller in registry.Controllers)
            {
                turboOptions.ImportMap.Pin(
                    $"controllers/{controller.StimulusIdentifier}",
                    controller.ImportPath,
                    preload: false);
            }

            logger.LogInformation(
                "Stimulus controller discovery complete. {Count} controller(s) registered.",
                registry.Controllers.Count);

            // Insert middleware
            app.UseMiddleware<StimulusMiddleware>();

            // Hot reload in development
            var hotReload = stimulusOptions.EnableHotReload ?? env.IsDevelopment();

            if (hotReload)
            {
                SetupHotReload(env, stimulusOptions, registry, turboOptions, logger);
            }

            next(app);
        };
    }

    private static void SetupHotReload(
        IWebHostEnvironment env,
        StimulusOptions options,
        StimulusControllerRegistry registry,
        TurboOptions turboOptions,
        ILogger logger)
    {
        var pattern = $"{options.ControllersPath}/**/*_controller.js";
        var changeToken = env.WebRootFileProvider.Watch(pattern);

        if (changeToken != null)
        {
            RegisterChangeCallback(changeToken, env, options, registry, turboOptions, logger);
        }
    }

    private static void RegisterChangeCallback(
        IChangeToken changeToken,
        IWebHostEnvironment env,
        StimulusOptions options,
        StimulusControllerRegistry registry,
        TurboOptions turboOptions,
        ILogger logger)
    {
        changeToken.RegisterChangeCallback(_ =>
        {
            logger.LogInformation("Stimulus: Controller file change detected, rebuilding registry...");

            // Remove old controller pins
            foreach (var controller in registry.Controllers)
            {
                turboOptions.ImportMap.Unpin($"controllers/{controller.StimulusIdentifier}");
            }

            // Rebuild
            registry.Rebuild(env.WebRootFileProvider, options.ControllersPath, logger);

            // Re-add controller pins
            foreach (var controller in registry.Controllers)
            {
                turboOptions.ImportMap.Pin(
                    $"controllers/{controller.StimulusIdentifier}",
                    controller.ImportPath,
                    preload: false);
            }

            logger.LogInformation(
                "Stimulus controller discovery complete. {Count} controller(s) registered.",
                registry.Controllers.Count);

            // Re-watch for next change
            var newToken = env.WebRootFileProvider.Watch($"{options.ControllersPath}/**/*_controller.js");

            if (newToken != null)
            {
                RegisterChangeCallback(newToken, env, options, registry, turboOptions, logger);
            }
        }, null);
    }
}
