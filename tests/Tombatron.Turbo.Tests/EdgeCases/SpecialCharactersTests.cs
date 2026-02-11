using FluentAssertions;
using Tombatron.Turbo.SourceGenerator;
using Tombatron.Turbo.Streams;
using Xunit;

namespace Tombatron.Turbo.Tests.EdgeCases;

/// <summary>
/// Tests for handling special characters in frame IDs, stream names, and targets.
/// </summary>
public class SpecialCharactersTests
{
    #region Stream Builder - Target IDs

    [Theory]
    [InlineData("simple-id")]
    [InlineData("id_with_underscores")]
    [InlineData("id123")]
    [InlineData("CamelCaseId")]
    public void StreamBuilder_ValidTargetIds_AreAccepted(string targetId)
    {
        var builder = new TurboStreamBuilder();

        var action = () => builder.Append(targetId, "<div>content</div>");

        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("id<script>")]
    [InlineData("id\"onclick")]
    [InlineData("id'test")]
    [InlineData("id&amp")]
    [InlineData("id>test")]
    public void StreamBuilder_TargetIdsWithSpecialChars_AreEscaped(string targetId)
    {
        var builder = new TurboStreamBuilder();
        builder.Append(targetId, "<div>content</div>");
        var result = builder.Build();

        // Should not contain unescaped special characters in the target attribute
        result.Should().NotContain($"target=\"{targetId}\"");
        result.Should().Contain("target=\"");
    }

    [Fact]
    public void StreamBuilder_EscapeAttribute_EscapesAllSpecialChars()
    {
        var input = "<script>\"'&";
        var escaped = TurboStreamBuilder.EscapeAttribute(input);

        escaped.Should().Be("&lt;script&gt;&quot;&#39;&amp;");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void StreamBuilder_EscapeAttribute_HandlesNullAndEmpty(string? input)
    {
        var result = TurboStreamBuilder.EscapeAttribute(input!);
        result.Should().Be(input);
    }

    #endregion

    #region Stream Builder - Unicode

    [Theory]
    [InlineData("日本語id")]
    [InlineData("émoji-🎉")]
    [InlineData("中文")]
    [InlineData("العربية")]
    [InlineData("кириллица")]
    public void StreamBuilder_UnicodeTargetIds_AreHandled(string targetId)
    {
        var builder = new TurboStreamBuilder();

        var action = () => builder.Append(targetId, "<div>content</div>");

        action.Should().NotThrow();
        var result = builder.Build();
        result.Should().Contain(targetId);
    }

    [Fact]
    public void StreamBuilder_UnicodeContent_IsPreserved()
    {
        var builder = new TurboStreamBuilder();
        var content = "<div>こんにちは 🌍 مرحبا</div>";
        builder.Append("target", content);
        var result = builder.Build();

        result.Should().Contain(content);
    }

    #endregion

    #region Frame Parser - Special Characters

    [Fact]
    public void FrameParser_FrameIdWithHyphen_IsParsedCorrectly()
    {
        var html = """<turbo-frame id="cart-items"></turbo-frame>""";
        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        frames[0].Id.Should().Be("cart-items");
    }

    [Fact]
    public void FrameParser_FrameIdWithUnderscore_IsParsedCorrectly()
    {
        var html = """<turbo-frame id="item_123"></turbo-frame>""";
        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        frames[0].Id.Should().Be("item_123");
    }

    [Fact]
    public void FrameParser_FrameIdWithNumbers_IsParsedCorrectly()
    {
        var html = """<turbo-frame id="frame123"></turbo-frame>""";
        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        frames[0].Id.Should().Be("frame123");
    }

    [Fact]
    public void FrameParser_FrameIdWithColon_IsParsedCorrectly()
    {
        var html = """<turbo-frame id="user:123"></turbo-frame>""";
        var frames = FrameParser.Parse(html);

        frames.Should().HaveCount(1);
        frames[0].Id.Should().Be("user:123");
    }

    #endregion

    #region Frame Parser - Razor Expressions

    [Theory]
    [InlineData("item_@Model.Id", true)]
    [InlineData("@item.Id", true)]
    [InlineData("prefix_@(Model.Id)_suffix", true)]
    [InlineData("static-id", false)]
    [InlineData("id_123", false)]
    [InlineData("@@escaped", false)] // @@ is an escaped @
    public void FrameParser_ContainsRazorExpression_DetectsCorrectly(string id, bool expected)
    {
        var result = FrameParser.ContainsRazorExpression(id);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("item_@Model.Id", "item_")]
    [InlineData("prefix_@id", "prefix_")]
    [InlineData("@Model.Id", "")]
    [InlineData("static-id", "static-id")]
    public void FrameParser_GetStaticIdPortion_ExtractsCorrectly(string id, string expectedStatic)
    {
        var result = FrameParser.GetStaticIdPortion(id);
        result.Should().Be(expectedStatic);
    }

    #endregion
}
