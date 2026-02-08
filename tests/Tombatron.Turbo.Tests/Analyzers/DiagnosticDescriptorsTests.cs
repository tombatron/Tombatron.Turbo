using FluentAssertions;
using Microsoft.CodeAnalysis;
using Tombatron.Turbo.Analyzers;
using Xunit;

namespace Tombatron.Turbo.Tests.Analyzers;

/// <summary>
/// Tests for the DiagnosticDescriptors class.
/// </summary>
public class DiagnosticDescriptorsTests
{
    [Fact]
    public void Category_IsCorrect()
    {
        // Assert
        DiagnosticDescriptors.Category.Should().Be("Tombatron.Turbo");
    }

    [Fact]
    public void DynamicIdWithoutPrefix_HasCorrectProperties()
    {
        // Arrange
        var descriptor = DiagnosticDescriptors.DynamicIdWithoutPrefix;

        // Assert
        descriptor.Id.Should().Be("TURBO001");
        descriptor.Title.ToString().Should().Be("Dynamic ID without prefix");
        descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
        descriptor.IsEnabledByDefault.Should().BeTrue();
        descriptor.Category.Should().Be("Tombatron.Turbo");
    }

    [Fact]
    public void PrefixMismatch_HasCorrectProperties()
    {
        // Arrange
        var descriptor = DiagnosticDescriptors.PrefixMismatch;

        // Assert
        descriptor.Id.Should().Be("TURBO002");
        descriptor.Title.ToString().Should().Be("Prefix mismatch");
        descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
        descriptor.IsEnabledByDefault.Should().BeTrue();
        descriptor.Category.Should().Be("Tombatron.Turbo");
    }

    [Fact]
    public void UnnecessaryPrefix_HasCorrectProperties()
    {
        // Arrange
        var descriptor = DiagnosticDescriptors.UnnecessaryPrefix;

        // Assert
        descriptor.Id.Should().Be("TURBO003");
        descriptor.Title.ToString().Should().Be("Unnecessary prefix");
        descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Info);
        descriptor.IsEnabledByDefault.Should().BeTrue();
        descriptor.Category.Should().Be("Tombatron.Turbo");
    }

    [Fact]
    public void PrefixMismatch_MessageFormat_IncludesPlaceholders()
    {
        // Arrange
        var descriptor = DiagnosticDescriptors.PrefixMismatch;

        // Assert
        descriptor.MessageFormat.ToString().Should().Contain("{0}");
        descriptor.MessageFormat.ToString().Should().Contain("{1}");
    }

    [Fact]
    public void AllDescriptors_HaveDescriptions()
    {
        // Assert
        DiagnosticDescriptors.DynamicIdWithoutPrefix.Description.ToString().Should().NotBeNullOrEmpty();
        DiagnosticDescriptors.PrefixMismatch.Description.ToString().Should().NotBeNullOrEmpty();
        DiagnosticDescriptors.UnnecessaryPrefix.Description.ToString().Should().NotBeNullOrEmpty();
    }
}
