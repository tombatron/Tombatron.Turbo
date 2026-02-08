using System.Collections.Immutable;
using FluentAssertions;
using Tombatron.Turbo.SourceGenerator.Models;
using Xunit;

namespace Tombatron.Turbo.Tests.SourceGenerator;

public class RazorFileInfoTests
{
    [Fact]
    public void HasFrames_WithFrames_ReturnsTrue()
    {
        // Arrange
        var frame = new TurboFrame("test", null, "content", 1, false);
        var fileInfo = new RazorFileInfo("test.cshtml", "Test", ImmutableArray.Create(frame));

        // Act & Assert
        fileInfo.HasFrames.Should().BeTrue();
    }

    [Fact]
    public void HasFrames_WithNoFrames_ReturnsFalse()
    {
        // Arrange
        var fileInfo = new RazorFileInfo("test.cshtml", "Test", ImmutableArray<TurboFrame>.Empty);

        // Act & Assert
        fileInfo.HasFrames.Should().BeFalse();
    }

    [Fact]
    public void StaticFrames_FiltersCorrectly()
    {
        // Arrange
        var staticFrame = new TurboFrame("static", null, "content", 1, false);
        var dynamicFrame = new TurboFrame("item_@id", "item_", "content", 2, true);
        var fileInfo = new RazorFileInfo(
            "test.cshtml",
            "Test",
            ImmutableArray.Create(staticFrame, dynamicFrame));

        // Act
        var result = fileInfo.StaticFrames.ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("static");
    }

    [Fact]
    public void DynamicFrames_FiltersCorrectly()
    {
        // Arrange
        var staticFrame = new TurboFrame("static", null, "content", 1, false);
        var dynamicFrame = new TurboFrame("item_@id", "item_", "content", 2, true);
        var fileInfo = new RazorFileInfo(
            "test.cshtml",
            "Test",
            ImmutableArray.Create(staticFrame, dynamicFrame));

        // Act
        var result = fileInfo.DynamicFrames.ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("item_@id");
    }

    [Fact]
    public void RecordEquality_WithSameFrameInstance_WorksCorrectly()
    {
        // Arrange
        var frames = ImmutableArray.Create(new TurboFrame("test", null, "content", 1, false));
        var fileInfo1 = new RazorFileInfo("test.cshtml", "Test", frames);
        var fileInfo2 = new RazorFileInfo("test.cshtml", "Test", frames);

        // Act & Assert
        // ImmutableArray uses reference equality, so same instance should work
        fileInfo1.Should().Be(fileInfo2);
    }

    [Fact]
    public void RecordProperties_AreAccessible()
    {
        // Arrange
        var frame = new TurboFrame("test", null, "content", 1, false);
        var fileInfo = new RazorFileInfo("test.cshtml", "Test", ImmutableArray.Create(frame));

        // Act & Assert
        fileInfo.FilePath.Should().Be("test.cshtml");
        fileInfo.ViewName.Should().Be("Test");
        fileInfo.Frames.Should().HaveCount(1);
    }
}
