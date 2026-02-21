using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tombatron.Turbo.Stimulus.Tests;

public class StimulusServiceCollectionExtensionsTests
{
    [Fact]
    public void AddStimulus_RegistersStimulusOptions()
    {
        var services = new ServiceCollection();
        services.AddStimulus();

        var provider = services.BuildServiceProvider();
        var options = provider.GetService<StimulusOptions>();

        options.Should().NotBeNull();
    }

    [Fact]
    public void AddStimulus_RegistersStimulusControllerRegistry()
    {
        var services = new ServiceCollection();
        services.AddStimulus();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetService<StimulusControllerRegistry>();

        registry.Should().NotBeNull();
    }

    [Fact]
    public void AddStimulus_RegistersStartupFilter()
    {
        var services = new ServiceCollection();
        services.AddStimulus();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStartupFilter));

        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(StimulusStartupFilter));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddStimulus_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();
        services.AddStimulus(options =>
        {
            options.ControllersPath = "my-controllers";
            options.StimulusCdnUrl = "https://example.com/stimulus.js";
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<StimulusOptions>();

        options.ControllersPath.Should().Be("my-controllers");
        options.StimulusCdnUrl.Should().Be("https://example.com/stimulus.js");
    }

    [Fact]
    public void AddStimulus_ThrowsOnNullServices()
    {
        IServiceCollection? services = null;
        var act = () => services!.AddStimulus();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddStimulus_ThrowsOnNullConfigure()
    {
        var services = new ServiceCollection();
        var act = () => services.AddStimulus(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddStimulus_DefaultOptions_AreCorrect()
    {
        var services = new ServiceCollection();
        services.AddStimulus();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<StimulusOptions>();

        options.ControllersPath.Should().Be("controllers");
        options.IndexEndpointPath.Should().Be("/_stimulus/controllers/index.js");
        options.EnableHotReload.Should().BeNull();
    }
}
