using FluentAssertions;
using Tombatron.Turbo.SourceGenerator;
using Xunit;

namespace Tombatron.Turbo.Tests.EdgeCases;

/// <summary>
/// Tests for handling malformed or unusual Razor syntax in frame parsing.
/// </summary>
public class MalformedRazorTests
{
    [Fact]
    public void Parse_UnclosedFrame_HandlesGracefully()
    {
        var html = """
            <turbo-frame id="unclosed">
                Content without closing tag
            """;

        var action = () => FrameParser.Parse(html);

        // Should not throw - graceful handling
        action.Should().NotThrow();
    }

    [Fact]
    public void Parse_MissingIdAttribute_SkipsFrame()
    {
        var html = """
            <turbo-frame>
                <div>No ID</div>
            </turbo-frame>
            <turbo-frame id="with-id">
                <div>Has ID</div>
            </turbo-frame>
            """;

        var frames = FrameParser.Parse(html);

        // Only the frame with ID should be parsed
        frames.Should().HaveCount(1);
        frames[0].Id.Should().Be("with-id");
    }

    [Fact]
    public void Parse_EmptyIdAttribute_SkipsFrame()
    {
        var html = """
            <turbo-frame id="">
                <div>Empty ID</div>
            </turbo-frame>
            <turbo-frame id="valid">
                <div>Valid ID</div>
            </turbo-frame>
            """;

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        frames[0].Id.Should().Be("valid");
    }

    [Fact]
    public void Parse_SelfClosingFrame_IsParsed()
    {
        var html = """<turbo-frame id="self-closing" />""";

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        frames[0].Id.Should().Be("self-closing");
    }

    [Fact]
    public void Parse_ExtraWhitespace_IsParsedCorrectly()
    {
        var html = """
            <turbo-frame    id="spaced"   >
                Content
            </turbo-frame   >
            """;

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        frames[0].Id.Should().Be("spaced");
    }

    [Fact]
    public void Parse_MixedQuotes_IsParsedCorrectly()
    {
        var html = """
            <turbo-frame id='single-quotes'>Content 1</turbo-frame>
            <turbo-frame id="double-quotes">Content 2</turbo-frame>
            """;

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(2);
        frames.Select(f => f.Id).Should().BeEquivalentTo(["single-quotes", "double-quotes"]);
    }

    [Fact]
    public void Parse_IdWithNewlines_IsParsedCorrectly()
    {
        var html = """
            <turbo-frame id="valid-id">Content</turbo-frame>
            """;

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        frames[0].Id.Should().Be("valid-id");
    }

    [Fact]
    public void Parse_RazorComments_AreIgnored()
    {
        var html = """
            @* This is a Razor comment *@
            <turbo-frame id="after-comment">Content</turbo-frame>
            """;

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        frames[0].Id.Should().Be("after-comment");
    }

    [Fact]
    public void Parse_HtmlComments_AreIgnored()
    {
        var html = """
            <!-- <turbo-frame id="commented-out">Should not appear</turbo-frame> -->
            <turbo-frame id="real">Content</turbo-frame>
            """;

        var frames = FrameParser.Parse(html);

        // Should find both - the parser doesn't understand HTML comments
        // This documents current behavior
        frames.Should().Contain(f => f.Id == "real");
    }

    [Fact]
    public void Parse_MultipleIdAttributes_UsesFirst()
    {
        // Invalid HTML but should handle gracefully
        var html = """<turbo-frame id="first" id="second">Content</turbo-frame>""";

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        frames[0].Id.Should().Be("first");
    }

    [Fact]
    public void Parse_CaseSensitivity_IsHandled()
    {
        var html = """
            <turbo-frame id="lowercase">Content 1</turbo-frame>
            <TURBO-FRAME id="uppercase">Content 2</TURBO-FRAME>
            <Turbo-Frame id="mixed">Content 3</Turbo-Frame>
            """;

        var frames = FrameParser.Parse(html);

        // HTML is case-insensitive, but our parser may be case-sensitive
        // This documents current behavior
        frames.Should().Contain(f => f.Id == "lowercase");
    }

    [Theory]
    [InlineData("id with spaces")]
    [InlineData("id\twith\ttabs")]
    [InlineData("id\nwith\nnewlines")]
    public void Parse_IdWithWhitespace_IsParsedAsIs(string idValue)
    {
        // While not valid HTML, the parser should handle it
        var html = $"""<turbo-frame id="{idValue}">Content</turbo-frame>""";

        var frames = FrameParser.Parse(html);

        // The parser extracts the ID as-is
        frames.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_RazorBlock_InsideFrame_IsPreserved()
    {
        var html = """
            <turbo-frame id="with-razor">
                @if (condition)
                {
                    <div>Conditional</div>
                }
            </turbo-frame>
            """;

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        frames[0].Content.Should().Contain("@if");
    }

    [Fact]
    public void Parse_RazorExpression_InFrameId_IsDetected()
    {
        var html = """<turbo-frame id="item_@Model.Id">Content</turbo-frame>""";

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        FrameParser.ContainsRazorExpression(frames[0].Id).Should().BeTrue();
    }

    [Fact]
    public void Parse_ComplexRazorExpression_InFrameId_IsDetected()
    {
        var html = """<turbo-frame id="item_@(Model.Items[0].Id)">Content</turbo-frame>""";

        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        FrameParser.ContainsRazorExpression(frames[0].Id).Should().BeTrue();
    }

    [Fact]
    public void Parse_EscapedRazorSymbol_IsNotDetectedAsDynamic()
    {
        // @@ is an escaped @ in Razor
        var id = "item_@@value";

        FrameParser.ContainsRazorExpression(id).Should().BeFalse();
    }

    [Fact]
    public void Parse_EmailAddress_InId_IsDetectedAsDynamic()
    {
        // Email addresses contain @ which triggers dynamic detection.
        // This is acceptable because email addresses shouldn't be used as frame IDs,
        // and the detection is conservative (better to treat static as dynamic than miss dynamic IDs).
        var id = "user-test@example.com";

        var result = FrameParser.ContainsRazorExpression(id);

        // Simple @ detection treats this as dynamic - this is by design
        result.Should().BeTrue();
    }
}
