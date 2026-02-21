using FluentAssertions;
using Xunit;

namespace Tombatron.Turbo.Stimulus.Tests;

public class StimulusOptionsTests
{
    [Fact]
    public void Validate_DefaultOptions_DoesNotThrow()
    {
        var options = new StimulusOptions();

        var act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ThrowsOnEmptyControllersPath(string? value)
    {
        var options = new StimulusOptions { ControllersPath = value! };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ControllersPath*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ThrowsOnEmptyIndexEndpointPath(string? value)
    {
        var options = new StimulusOptions { IndexEndpointPath = value! };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*IndexEndpointPath*");
    }

    [Fact]
    public void Validate_ThrowsWhenIndexEndpointPathDoesNotStartWithSlash()
    {
        var options = new StimulusOptions { IndexEndpointPath = "no-slash" };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*forward slash*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ThrowsOnEmptyStimulusCdnUrl(string? value)
    {
        var options = new StimulusOptions { StimulusCdnUrl = value! };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*StimulusCdnUrl*");
    }

    [Fact]
    public void EnableHotReload_DefaultsToNull()
    {
        var options = new StimulusOptions();

        options.EnableHotReload.Should().BeNull();
    }
}
