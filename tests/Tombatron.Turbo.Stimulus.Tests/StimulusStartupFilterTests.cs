using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Tombatron.Turbo.Stimulus.Tests;

public class StimulusStartupFilterTests
{
    [Fact]
    public void Configure_AddsImportMapPins_ToTurboOptions()
    {
        var turboOptions = new TurboOptions();
        var stimulusOptions = new StimulusOptions();
        var registry = new StimulusControllerRegistry();
        var env = CreateMockEnvironment(new[] { "hello_controller.js" });

        var services = new ServiceCollection();
        services.AddSingleton(turboOptions);
        services.AddSingleton(stimulusOptions);
        services.AddSingleton(registry);
        services.AddSingleton<IWebHostEnvironment>(env);
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new Mock<IApplicationBuilder>();
        appBuilder.Setup(b => b.ApplicationServices).Returns(serviceProvider);

        appBuilder.Setup(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appBuilder.Object);

        var filter = new StimulusStartupFilter();
        var nextCalled = false;
        var configuredAction = filter.Configure(_ => { nextCalled = true; });

        configuredAction(appBuilder.Object);

        // Verify import map pins were added
        turboOptions.ImportMap.Entries.Should().ContainKey("@hotwired/stimulus");
        turboOptions.ImportMap.Entries["@hotwired/stimulus"].Path.Should().Be(stimulusOptions.StimulusCdnUrl);
        turboOptions.ImportMap.Entries["@hotwired/stimulus"].Preload.Should().BeTrue();

        turboOptions.ImportMap.Entries.Should().ContainKey("_stimulus/application");
        turboOptions.ImportMap.Entries["_stimulus/application"].Preload.Should().BeTrue();

        turboOptions.ImportMap.Entries.Should().ContainKey("_stimulus/controllers");
        turboOptions.ImportMap.Entries["_stimulus/controllers"].Path.Should().Be(stimulusOptions.IndexEndpointPath);

        turboOptions.ImportMap.Entries.Should().ContainKey("controllers/hello");
        turboOptions.ImportMap.Entries["controllers/hello"].Path.Should().Be("/controllers/hello_controller.js");
        turboOptions.ImportMap.Entries["controllers/hello"].Preload.Should().BeFalse();

        // Verify next was called
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public void Configure_DiscoversControllers_AndPopulatesRegistry()
    {
        var turboOptions = new TurboOptions();
        var stimulusOptions = new StimulusOptions();
        var registry = new StimulusControllerRegistry();
        var env = CreateMockEnvironment(new[] { "hello_controller.js", "goodbye_controller.js" });

        var services = new ServiceCollection();
        services.AddSingleton(turboOptions);
        services.AddSingleton(stimulusOptions);
        services.AddSingleton(registry);
        services.AddSingleton<IWebHostEnvironment>(env);
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new Mock<IApplicationBuilder>();
        appBuilder.Setup(b => b.ApplicationServices).Returns(serviceProvider);
        appBuilder.Setup(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appBuilder.Object);

        var filter = new StimulusStartupFilter();
        var configuredAction = filter.Configure(_ => { });
        configuredAction(appBuilder.Object);

        registry.Controllers.Should().HaveCount(2);
    }

    private static IWebHostEnvironment CreateMockEnvironment(string[] controllerFiles)
    {
        var mockProvider = new Mock<IFileProvider>();
        var files = controllerFiles.Select(name =>
        {
            var mockFile = new Mock<IFileInfo>();
            mockFile.Setup(f => f.Name).Returns(name);
            mockFile.Setup(f => f.IsDirectory).Returns(false);
            mockFile.Setup(f => f.Exists).Returns(true);
            return mockFile.Object;
        }).ToList();

        var mockContents = new Mock<IDirectoryContents>();
        mockContents.Setup(d => d.Exists).Returns(true);
        mockContents.Setup(d => d.GetEnumerator()).Returns(() => files.GetEnumerator());

        mockProvider.Setup(p => p.GetDirectoryContents("controllers"))
            .Returns(mockContents.Object);

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootFileProvider).Returns(mockProvider.Object);
        mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

        return mockEnv.Object;
    }
}
