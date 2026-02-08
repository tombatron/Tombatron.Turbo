using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tombatron.Turbo.Tests;

public class TurboServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTurbo_WithNoConfiguration_ShouldRegisterDefaultOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTurbo();

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TurboOptions>();

        options.Should().NotBeNull();
        options.HubPath.Should().Be("/turbo-hub");
        options.RequireAuthentication.Should().BeTrue();
    }

    [Fact]
    public void AddTurbo_WithCustomConfiguration_ShouldRegisterCustomOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTurbo(options =>
        {
            options.HubPath = "/my-hub";
            options.RequireAuthentication = false;
            options.MaxReconnectAttempts = 10;
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TurboOptions>();

        options.Should().NotBeNull();
        options.HubPath.Should().Be("/my-hub");
        options.RequireAuthentication.Should().BeFalse();
        options.MaxReconnectAttempts.Should().Be(10);
    }

    [Fact]
    public void AddTurbo_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddTurbo();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddTurbo_WithNullConfigure_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action act = () => services.AddTurbo(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configure");
    }

    [Fact]
    public void AddTurbo_WithInvalidConfiguration_ShouldThrowDuringConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action act = () => services.AddTurbo(options =>
        {
            options.HubPath = "invalid-path"; // Missing leading slash
        });

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("HubPath must start with a forward slash.");
    }

    [Fact]
    public void AddTurbo_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddTurbo();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddTurbo_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action act = () =>
        {
            services.AddTurbo();
            services.AddTurbo();
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddTurbo_ShouldRegisterSignalR()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTurbo();

        // Assert
        services.Should().Contain(s =>
            s.ServiceType.FullName != null &&
            s.ServiceType.FullName.Contains("SignalR"));
    }
}
