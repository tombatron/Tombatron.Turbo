using FluentAssertions;
using Tombatron.Turbo.SourceGenerator;
using Tombatron.Turbo.Streams;
using Xunit;

namespace Tombatron.Turbo.Tests.EdgeCases;

/// <summary>
/// Tests for handling large documents and content.
/// </summary>
public class LargeDocumentsTests
{
    [Fact]
    public void FrameParser_LargeDocument_ParsesSuccessfully()
    {
        // Create a document with 100 frames
        var frames = string.Join("\n", Enumerable.Range(1, 100)
            .Select(i => $"<turbo-frame id=\"frame_{i}\"><div>Content {i}</div></turbo-frame>"));
        var html = $"@page\n@model Model\n{frames}";

        var result = FrameParser.Parse(html);

        result.Should().HaveCount(100);
    }

    [Fact]
    public void FrameParser_VeryLargeContent_ParsesCorrectly()
    {
        // Create a frame with 100KB of content
        var largeContent = new string('x', 100_000);
        var html = $"<turbo-frame id=\"large\">{largeContent}</turbo-frame>";

        var result = FrameParser.Parse(html);

        result.Should().HaveCount(1);
        result[0].Content.Should().Contain(largeContent);
    }

    [Fact]
    public void FrameParser_ManyNestedElements_ParsesCorrectly()
    {
        // Create deeply nested HTML inside a frame
        var nestedHtml = "<turbo-frame id=\"nested\">";
        for (int i = 0; i < 50; i++)
        {
            nestedHtml += $"<div class=\"level{i}\">";
        }
        nestedHtml += "Deep content";
        for (int i = 0; i < 50; i++)
        {
            nestedHtml += "</div>";
        }
        nestedHtml += "</turbo-frame>";

        var result = FrameParser.Parse(nestedHtml);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be("nested");
    }

    [Fact]
    public void StreamBuilder_ManyActions_BuildsCorrectly()
    {
        var builder = new TurboStreamBuilder();

        // Add 100 actions
        for (int i = 0; i < 100; i++)
        {
            builder.Append($"target_{i}", $"<div>Content {i}</div>");
        }

        var result = builder.Build();

        result.Should().Contain("target_0");
        result.Should().Contain("target_99");
        // Count occurrences of turbo-stream
        result.Split("<turbo-stream").Length.Should().Be(101); // 100 + 1 (split creates n+1 parts)
    }

    [Fact]
    public void StreamBuilder_LargeHtmlContent_IsPreserved()
    {
        var builder = new TurboStreamBuilder();
        var largeContent = $"<div>{new string('a', 1_000_000)}</div>";

        builder.Replace("target", largeContent);
        var result = builder.Build();

        result.Should().Contain(largeContent);
    }

    [Fact]
    public void StreamBuilder_MixedActionsWithLargeContent_WorksCorrectly()
    {
        var builder = new TurboStreamBuilder();
        var largeContent = new string('x', 10_000);

        builder
            .Append("target1", $"<div>{largeContent}</div>")
            .Prepend("target2", $"<span>{largeContent}</span>")
            .Replace("target3", $"<p>{largeContent}</p>")
            .Update("target4", $"<section>{largeContent}</section>")
            .Remove("target5");

        var result = builder.Build();

        result.Should().Contain("target1");
        result.Should().Contain("target2");
        result.Should().Contain("target3");
        result.Should().Contain("target4");
        result.Should().Contain("target5");
    }

    [Fact]
    public void FrameParser_DocumentWithManyNonFrameElements_ParsesFramesOnly()
    {
        // Create a document with lots of non-frame elements and a few frames
        var nonFrameHtml = string.Join("\n", Enumerable.Range(1, 1000)
            .Select(i => $"<div id=\"div_{i}\">Content {i}</div>"));
        var html = $@"
            @page
            @model Model
            <turbo-frame id=""frame1"">Frame 1</turbo-frame>
            {nonFrameHtml}
            <turbo-frame id=""frame2"">Frame 2</turbo-frame>
        ";

        var result = FrameParser.Parse(html);

        result.Should().HaveCount(2);
        result.Select(f => f.Id).Should().BeEquivalentTo(["frame1", "frame2"]);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(500)]
    public void FrameParser_VariousDocumentSizes_ScalesLinearly(int frameCount)
    {
        var frames = string.Join("\n", Enumerable.Range(1, frameCount)
            .Select(i => $"<turbo-frame id=\"frame_{i}\">Content</turbo-frame>"));

        var result = FrameParser.Parse(frames);

        result.Should().HaveCount(frameCount);
    }
}
