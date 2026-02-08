using FluentAssertions;
using Tombatron.Turbo.Analyzers;
using Tombatron.Turbo.SourceGenerator;
using Tombatron.Turbo.SourceGenerator.Models;
using Xunit;

namespace Tombatron.Turbo.Tests.Analyzers;

/// <summary>
/// Tests for the TurboFrameAnalyzer diagnostic logic.
/// These tests verify the diagnostic detection logic without the full Roslyn analyzer infrastructure.
/// </summary>
public class TurboFrameAnalyzerTests
{
    [Fact]
    public void StaticFrame_WithoutPrefix_NoDiagnostic()
    {
        // Arrange
        string content = @"<turbo-frame id=""cart-items"">
    <div>Content</div>
</turbo-frame>";

        // Act
        var frames = FrameParser.Parse(content);

        // Assert
        frames.Should().HaveCount(1);
        var frame = frames[0];
        frame.IsDynamic.Should().BeFalse();
        frame.HasPrefix.Should().BeFalse();

        // This should NOT trigger any diagnostic
        var diagnosticType = GetDiagnosticType(frame);
        diagnosticType.Should().BeNull();
    }

    [Fact]
    public void DynamicFrame_WithoutPrefix_TriggersTURBO001()
    {
        // Arrange
        string content = @"<turbo-frame id=""item_@Model.Id"">
    <div>Content</div>
</turbo-frame>";

        // Act
        var frames = FrameParser.Parse(content);

        // Assert
        frames.Should().HaveCount(1);
        var frame = frames[0];
        frame.IsDynamic.Should().BeTrue();
        frame.HasPrefix.Should().BeFalse();

        // This SHOULD trigger TURBO001
        var diagnosticType = GetDiagnosticType(frame);
        diagnosticType.Should().Be("TURBO001");
    }

    [Fact]
    public void DynamicFrame_WithMatchingPrefix_NoDiagnostic()
    {
        // Arrange
        string content = @"<turbo-frame id=""item_@Model.Id"" asp-frame-prefix=""item_"">
    <div>Content</div>
</turbo-frame>";

        // Act
        var frames = FrameParser.Parse(content);

        // Assert
        frames.Should().HaveCount(1);
        var frame = frames[0];
        frame.IsDynamic.Should().BeTrue();
        frame.HasPrefix.Should().BeTrue();
        frame.Prefix.Should().Be("item_");

        // This should NOT trigger any diagnostic
        var diagnosticType = GetDiagnosticType(frame);
        diagnosticType.Should().BeNull();
    }

    [Fact]
    public void DynamicFrame_WithMismatchedPrefix_TriggersTURBO002()
    {
        // Arrange
        string content = @"<turbo-frame id=""item_@Model.Id"" asp-frame-prefix=""product_"">
    <div>Content</div>
</turbo-frame>";

        // Act
        var frames = FrameParser.Parse(content);

        // Assert
        frames.Should().HaveCount(1);
        var frame = frames[0];
        frame.IsDynamic.Should().BeTrue();
        frame.HasPrefix.Should().BeTrue();
        frame.Prefix.Should().Be("product_");

        // This SHOULD trigger TURBO002
        var diagnosticType = GetDiagnosticType(frame);
        diagnosticType.Should().Be("TURBO002");
    }

    [Fact]
    public void StaticFrame_WithPrefix_TriggersTURBO003()
    {
        // Arrange
        string content = @"<turbo-frame id=""cart-items"" asp-frame-prefix=""cart-"">
    <div>Content</div>
</turbo-frame>";

        // Act
        var frames = FrameParser.Parse(content);

        // Assert
        frames.Should().HaveCount(1);
        var frame = frames[0];
        frame.IsDynamic.Should().BeFalse();
        frame.HasPrefix.Should().BeTrue();

        // This SHOULD trigger TURBO003
        var diagnosticType = GetDiagnosticType(frame);
        diagnosticType.Should().Be("TURBO003");
    }

    [Fact]
    public void FullyDynamicId_WithAnyPrefix_NoDiagnostic()
    {
        // Arrange - ID starts with @, so any prefix is valid
        string content = @"<turbo-frame id=""@Model.FrameId"" asp-frame-prefix=""dynamic_"">
    <div>Content</div>
</turbo-frame>";

        // Act
        var frames = FrameParser.Parse(content);

        // Assert
        frames.Should().HaveCount(1);
        var frame = frames[0];
        frame.IsDynamic.Should().BeTrue();
        frame.HasPrefix.Should().BeTrue();
        frame.StaticIdPortion.Should().BeEmpty();

        // This should NOT trigger any diagnostic (any prefix is valid for fully dynamic IDs)
        var diagnosticType = GetDiagnosticType(frame);
        diagnosticType.Should().BeNull();
    }

    [Theory]
    [InlineData("item_@Model.Id", "item_", false)] // Exact match
    [InlineData("item_@Model.Id", "item", false)]  // Prefix is start of static portion
    [InlineData("product_123_@Model.Id", "product_", false)] // Prefix matches start
    [InlineData("@Model.Id", "anything_", false)] // Fully dynamic, any prefix valid
    public void PrefixValidation_VariousScenarios(string id, string prefix, bool shouldTriggerMismatch)
    {
        // Arrange
        string content = $@"<turbo-frame id=""{id}"" asp-frame-prefix=""{prefix}"">
    <div>Content</div>
</turbo-frame>";

        // Act
        var frames = FrameParser.Parse(content);

        // Assert
        frames.Should().HaveCount(1);
        var frame = frames[0];

        var diagnosticType = GetDiagnosticType(frame);

        if (shouldTriggerMismatch)
        {
            diagnosticType.Should().Be("TURBO002");
        }
        else
        {
            diagnosticType.Should().NotBe("TURBO002");
        }
    }

    [Fact]
    public void MultipleFrames_EachAnalyzedIndependently()
    {
        // Arrange
        string content = @"
<turbo-frame id=""static-frame"">
    <div>Static content</div>
</turbo-frame>

<turbo-frame id=""dynamic_@Model.Id"">
    <div>Dynamic without prefix - error</div>
</turbo-frame>

<turbo-frame id=""valid_@Model.Id"" asp-frame-prefix=""valid_"">
    <div>Valid dynamic</div>
</turbo-frame>";

        // Act
        var frames = FrameParser.Parse(content);

        // Assert
        frames.Should().HaveCount(3);

        // Frame 1: Static, no diagnostic
        GetDiagnosticType(frames[0]).Should().BeNull();

        // Frame 2: Dynamic without prefix, TURBO001
        GetDiagnosticType(frames[1]).Should().Be("TURBO001");

        // Frame 3: Dynamic with valid prefix, no diagnostic
        GetDiagnosticType(frames[2]).Should().BeNull();
    }

    /// <summary>
    /// Simulates the diagnostic detection logic from TurboFrameAnalyzer.
    /// Returns the diagnostic ID that would be triggered, or null if none.
    /// </summary>
    private static string? GetDiagnosticType(TurboFrame frame)
    {
        if (frame.IsDynamic)
        {
            if (!frame.HasPrefix)
            {
                return "TURBO001"; // Dynamic ID without prefix
            }

            if (!IsPrefixValid(frame))
            {
                return "TURBO002"; // Prefix mismatch
            }
        }
        else if (frame.HasPrefix)
        {
            return "TURBO003"; // Unnecessary prefix on static ID
        }

        return null;
    }

    /// <summary>
    /// Replicates the prefix validation logic from TurboFrameAnalyzer.
    /// </summary>
    private static bool IsPrefixValid(TurboFrame frame)
    {
        if (!frame.IsDynamic || !frame.HasPrefix)
        {
            return true;
        }

        string staticPortion = frame.StaticIdPortion;
        string prefix = frame.Prefix!;

        if (string.IsNullOrEmpty(staticPortion))
        {
            return true; // Any prefix is valid for fully dynamic IDs
        }

        return staticPortion.Equals(prefix, StringComparison.Ordinal) ||
               staticPortion.StartsWith(prefix, StringComparison.Ordinal);
    }
}
