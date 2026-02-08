using FluentAssertions;
using Xunit;

namespace Tombatron.Turbo.Tests;

public class TurboOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new TurboOptions();

        // Assert
        options.HubPath.Should().Be("/turbo-hub");
        options.UseSignedStreamNames.Should().BeTrue();
        options.SignedStreamNameExpiration.Should().Be(TimeSpan.FromHours(24));
        options.AddVaryHeader.Should().BeTrue();
        options.DefaultUserStreamPattern.Should().Be("user:{0}");
        options.DefaultSessionStreamPattern.Should().Be("session:{0}");
        options.EnableAutoReconnect.Should().BeTrue();
        options.MaxReconnectAttempts.Should().Be(5);
    }

    [Fact]
    public void Validate_WithDefaultValues_ShouldNotThrow()
    {
        // Arrange
        var options = new TurboOptions();

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithNullOrEmptyHubPath_ShouldThrow(string? hubPath)
    {
        // Arrange
        var options = new TurboOptions { HubPath = hubPath! };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("HubPath cannot be null or empty.");
    }

    [Theory]
    [InlineData("turbo-hub")]
    [InlineData("hub")]
    [InlineData("turbo/hub")]
    public void Validate_WithHubPathNotStartingWithSlash_ShouldThrow(string hubPath)
    {
        // Arrange
        var options = new TurboOptions { HubPath = hubPath };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("HubPath must start with a forward slash.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithNullOrEmptyDefaultUserStreamPattern_ShouldThrow(string? pattern)
    {
        // Arrange
        var options = new TurboOptions { DefaultUserStreamPattern = pattern! };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("DefaultUserStreamPattern cannot be null or empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithNullOrEmptyDefaultSessionStreamPattern_ShouldThrow(string? pattern)
    {
        // Arrange
        var options = new TurboOptions { DefaultSessionStreamPattern = pattern! };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("DefaultSessionStreamPattern cannot be null or empty.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithNegativeMaxReconnectAttempts_ShouldThrow(int attempts)
    {
        // Arrange
        var options = new TurboOptions { MaxReconnectAttempts = attempts };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("MaxReconnectAttempts cannot be negative.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(100)]
    public void Validate_WithValidMaxReconnectAttempts_ShouldNotThrow(int attempts)
    {
        // Arrange
        var options = new TurboOptions { MaxReconnectAttempts = attempts };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SetProperties_ShouldRetainValues()
    {
        // Arrange
        var options = new TurboOptions
        {
            HubPath = "/custom-hub",
            UseSignedStreamNames = false,
            SignedStreamNameExpiration = TimeSpan.FromHours(12),
            AddVaryHeader = false,
            DefaultUserStreamPattern = "u:{0}",
            DefaultSessionStreamPattern = "s:{0}",
            EnableAutoReconnect = false,
            MaxReconnectAttempts = 10
        };

        // Assert
        options.HubPath.Should().Be("/custom-hub");
        options.UseSignedStreamNames.Should().BeFalse();
        options.SignedStreamNameExpiration.Should().Be(TimeSpan.FromHours(12));
        options.AddVaryHeader.Should().BeFalse();
        options.DefaultUserStreamPattern.Should().Be("u:{0}");
        options.DefaultSessionStreamPattern.Should().Be("s:{0}");
        options.EnableAutoReconnect.Should().BeFalse();
        options.MaxReconnectAttempts.Should().Be(10);
    }

    [Fact]
    public void SignedStreamNameExpiration_CanBeSetToNull()
    {
        // Arrange
        var options = new TurboOptions
        {
            SignedStreamNameExpiration = null
        };

        // Assert
        options.SignedStreamNameExpiration.Should().BeNull();
    }
}
