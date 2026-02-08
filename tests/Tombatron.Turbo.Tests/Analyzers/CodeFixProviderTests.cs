using FluentAssertions;
using Tombatron.Turbo.Analyzers;
using Xunit;

namespace Tombatron.Turbo.Tests.Analyzers;

/// <summary>
/// Tests for the code fix provider logic.
/// </summary>
public class CodeFixProviderTests
{
    [Theory]
    [InlineData("item_@Model.Id", "item_")]
    [InlineData("product_@item.Id", "product_")]
    [InlineData("cart_item_@Model.Id", "cart_item_")]
    [InlineData("user_profile_@userId", "user_profile_")]
    public void InferPrefix_ExtractsStaticPortion(string idValue, string expectedPrefix)
    {
        // Act
        string result = AddPrefixCodeFixProvider.InferPrefix(idValue);

        // Assert
        result.Should().Be(expectedPrefix);
    }

    [Theory]
    [InlineData("@Model.Id", "frame_")]
    [InlineData("@item.Id", "frame_")]
    public void InferPrefix_WithFullyDynamicId_ReturnsDefaultPrefix(string idValue, string expectedPrefix)
    {
        // Act
        string result = AddPrefixCodeFixProvider.InferPrefix(idValue);

        // Assert
        result.Should().Be(expectedPrefix);
    }

    [Fact]
    public void InferPrefix_WithNoRazorExpression_ReturnsFullId()
    {
        // This shouldn't happen in practice since we only call this for dynamic IDs
        // but let's verify the behavior
        string idValue = "static-id";

        // Act
        string result = AddPrefixCodeFixProvider.InferPrefix(idValue);

        // Assert
        // No @ found, so returns default
        result.Should().Be("frame_");
    }

    [Fact]
    public void AddPrefixCodeFixProvider_HasCorrectDiagnosticId()
    {
        // Arrange
        var provider = new AddPrefixCodeFixProvider();

        // Assert
        provider.FixableDiagnosticIds.Should().Contain("TURBO001");
    }

    [Fact]
    public void FixPrefixCodeFixProvider_HasCorrectDiagnosticId()
    {
        // Arrange
        var provider = new FixPrefixCodeFixProvider();

        // Assert
        provider.FixableDiagnosticIds.Should().Contain("TURBO002");
    }

    [Fact]
    public void RemovePrefixCodeFixProvider_HasCorrectDiagnosticId()
    {
        // Arrange
        var provider = new RemovePrefixCodeFixProvider();

        // Assert
        provider.FixableDiagnosticIds.Should().Contain("TURBO003");
    }

    [Fact]
    public void AllCodeFixProviders_ProvideFixAllProvider()
    {
        // Arrange
        var addPrefixProvider = new AddPrefixCodeFixProvider();
        var fixPrefixProvider = new FixPrefixCodeFixProvider();
        var removePrefixProvider = new RemovePrefixCodeFixProvider();

        // Assert
        addPrefixProvider.GetFixAllProvider().Should().NotBeNull();
        fixPrefixProvider.GetFixAllProvider().Should().NotBeNull();
        removePrefixProvider.GetFixAllProvider().Should().NotBeNull();
    }
}
