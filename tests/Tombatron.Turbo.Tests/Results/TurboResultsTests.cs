using FluentAssertions;
using Tombatron.Turbo.Rendering;
using Tombatron.Turbo.Results;
using Xunit;

namespace Tombatron.Turbo.Tests.Results;

public class TurboResultsTests
{
    [Fact]
    public void Partial_WithStringName_ShouldReturnTurboPartialResult()
    {
        // Act
        var result = TurboResults.Partial("/Pages/Shared/_Test.cshtml");

        // Assert
        result.Should().BeOfType<TurboPartialResult>();
    }

    [Fact]
    public void Partial_WithStringNameAndModel_ShouldReturnTurboPartialResult()
    {
        // Arrange
        var model = new { Name = "Test" };

        // Act
        var result = TurboResults.Partial("/Pages/Shared/_Test.cshtml", model);

        // Assert
        result.Should().BeOfType<TurboPartialResult>();
    }

    [Fact]
    public void Partial_WithPartialTemplate_ShouldReturnTurboPartialResult()
    {
        // Arrange
        var template = new PartialTemplate("/Pages/Shared/_Test.cshtml", "Test");

        // Act
        var result = TurboResults.Partial(template);

        // Assert
        result.Should().BeOfType<TurboPartialResult>();
    }

    [Fact]
    public void Partial_WithTypedPartialTemplateAndModel_ShouldReturnTurboPartialResult()
    {
        // Arrange
        var template = new PartialTemplate<string>("/Pages/Shared/_Test.cshtml", "Test");

        // Act
        var result = TurboResults.Partial(template, "model-value");

        // Assert
        result.Should().BeOfType<TurboPartialResult>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Partial_WithNullOrEmptyStringName_ShouldThrow(string? partialName)
    {
        // Act
        Action act = () => TurboResults.Partial(partialName!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Partial_WithNullOrEmptyStringNameAndModel_ShouldThrow(string? partialName)
    {
        // Act
        Action act = () => TurboResults.Partial(partialName!, new { });

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
