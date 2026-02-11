using FluentAssertions;
using Tombatron.Turbo.SourceGenerator;
using Xunit;

namespace Tombatron.Turbo.Tests.EdgeCases;

/// <summary>
/// Tests for parsing nested turbo-frame elements.
/// </summary>
public class NestedFramesTests
{
    [Fact]
    public void Parse_SingleLevelNesting_FindsBothFrames()
    {
        var html = """
            <turbo-frame id="outer">
                <div>Outer content</div>
                <turbo-frame id="inner">
                    <div>Inner content</div>
                </turbo-frame>
            </turbo-frame>
            """;

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(2);
        frames.Select(f => f.Id).Should().Contain("outer");
        frames.Select(f => f.Id).Should().Contain("inner");
    }

    [Fact]
    public void Parse_DoubleLevelNesting_FindsAllFrames()
    {
        var html = """
            <turbo-frame id="level1">
                <turbo-frame id="level2">
                    <turbo-frame id="level3">
                        <div>Deep content</div>
                    </turbo-frame>
                </turbo-frame>
            </turbo-frame>
            """;

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(3);
        frames.Select(f => f.Id).Should().BeEquivalentTo(["level1", "level2", "level3"]);
    }

    [Fact]
    public void Parse_SiblingFrames_FindsAllFrames()
    {
        var html = """
            <div>
                <turbo-frame id="sibling1">Content 1</turbo-frame>
                <turbo-frame id="sibling2">Content 2</turbo-frame>
                <turbo-frame id="sibling3">Content 3</turbo-frame>
            </div>
            """;

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_MixedNestingAndSiblings_FindsAllFrames()
    {
        var html = """
            <turbo-frame id="container">
                <turbo-frame id="child1">
                    <turbo-frame id="grandchild">Nested</turbo-frame>
                </turbo-frame>
                <turbo-frame id="child2">Sibling</turbo-frame>
            </turbo-frame>
            <turbo-frame id="separate">Outside</turbo-frame>
            """;

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(5);
        frames.Select(f => f.Id).Should().BeEquivalentTo([
            "container", "child1", "grandchild", "child2", "separate"
        ]);
    }

    [Fact]
    public void Parse_NestedFrame_HasCorrectContent()
    {
        var html = """
            <turbo-frame id="outer">
                <h1>Outer Title</h1>
                <turbo-frame id="inner">
                    <p>Inner paragraph</p>
                </turbo-frame>
                <p>Outer paragraph</p>
            </turbo-frame>
            """;

        var frames = FrameParser.Parse(html);
        var outerFrame = frames.First(f => f.Id == "outer");
        var innerFrame = frames.First(f => f.Id == "inner");

        outerFrame.Content.Should().Contain("<h1>Outer Title</h1>");
        outerFrame.Content.Should().Contain("<turbo-frame id=\"inner\">");
        innerFrame.Content.Should().Contain("<p>Inner paragraph</p>");
    }

    [Fact]
    public void Parse_DeeplyNestedFrames_HandlesCorrectly()
    {
        // Create 10 levels of nesting
        var html = "";
        for (int i = 1; i <= 10; i++)
        {
            html += $"<turbo-frame id=\"level{i}\">\n";
        }
        html += "<div>Deep content</div>";
        for (int i = 10; i >= 1; i--)
        {
            html += "\n</turbo-frame>";
        }

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(10);
        for (int i = 1; i <= 10; i++)
        {
            frames.Select(f => f.Id).Should().Contain($"level{i}");
        }
    }

    [Fact]
    public void Parse_UnclosedNestedFrame_HandlesGracefully()
    {
        var html = """
            <turbo-frame id="outer">
                <turbo-frame id="unclosed">
                    Content without closing tag
            </turbo-frame>
            """;

        // Should not throw, but may not parse correctly
        var action = () => FrameParser.Parse(html);
        action.Should().NotThrow();
    }

    [Fact]
    public void Parse_SameIdInNestedFrames_FindsBoth()
    {
        // While not recommended, duplicate IDs should still be parsed
        var html = """
            <turbo-frame id="duplicate">
                <turbo-frame id="duplicate">
                    Inner
                </turbo-frame>
            </turbo-frame>
            """;

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(2);
        frames.Should().AllSatisfy(f => f.Id.Should().Be("duplicate"));
    }
}
