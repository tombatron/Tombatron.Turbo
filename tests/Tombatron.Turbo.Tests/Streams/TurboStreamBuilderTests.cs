using System.Text;
using FluentAssertions;
using Tombatron.Turbo.Streams;
using Xunit;

namespace Tombatron.Turbo.Tests.Streams;

/// <summary>
/// Tests for the TurboStreamBuilder class.
/// </summary>
public class TurboStreamBuilderTests
{
    [Fact]
    public void Append_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Append("messages", "<div>Hello</div>").Build();

        result.Should().Be("<turbo-stream action=\"append\" target=\"messages\"><template><div>Hello</div></template></turbo-stream>");
    }

    [Fact]
    public void Prepend_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Prepend("messages", "<div>Hello</div>").Build();

        result.Should().Be("<turbo-stream action=\"prepend\" target=\"messages\"><template><div>Hello</div></template></turbo-stream>");
    }

    [Fact]
    public void Replace_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Replace("message-1", "<div id=\"message-1\">Updated</div>").Build();

        result.Should().Be("<turbo-stream action=\"replace\" target=\"message-1\"><template><div id=\"message-1\">Updated</div></template></turbo-stream>");
    }

    [Fact]
    public void Update_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Update("content", "<p>New content</p>").Build();

        result.Should().Be("<turbo-stream action=\"update\" target=\"content\"><template><p>New content</p></template></turbo-stream>");
    }

    [Fact]
    public void Remove_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Remove("message-1").Build();

        result.Should().Be("<turbo-stream action=\"remove\" target=\"message-1\"></turbo-stream>");
    }

    [Fact]
    public void Before_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Before("message-2", "<div>Before message 2</div>").Build();

        result.Should().Be("<turbo-stream action=\"before\" target=\"message-2\"><template><div>Before message 2</div></template></turbo-stream>");
    }

    [Fact]
    public void After_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.After("message-1", "<div>After message 1</div>").Build();

        result.Should().Be("<turbo-stream action=\"after\" target=\"message-1\"><template><div>After message 1</div></template></turbo-stream>");
    }

    // Refresh tests

    [Fact]
    public void Refresh_WithoutRequestId_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Refresh().Build();

        result.Should().Be("<turbo-stream action=\"refresh\"></turbo-stream>");
    }

    [Fact]
    public void Refresh_WithRequestId_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Refresh("abc-123").Build();

        result.Should().Be("<turbo-stream action=\"refresh\" request-id=\"abc-123\"></turbo-stream>");
    }

    [Fact]
    public void Refresh_WithNull_GeneratesHtmlWithoutRequestId()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Refresh(null).Build();

        result.Should().Be("<turbo-stream action=\"refresh\"></turbo-stream>");
    }

    [Fact]
    public void Refresh_WithEmpty_GeneratesHtmlWithoutRequestId()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Refresh(string.Empty).Build();

        result.Should().Be("<turbo-stream action=\"refresh\"></turbo-stream>");
    }

    [Fact]
    public void Refresh_WithSpecialCharacters_EscapesRequestId()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Refresh("id\"with<special>&chars").Build();

        result.Should().Be("<turbo-stream action=\"refresh\" request-id=\"id&quot;with&lt;special&gt;&amp;chars\"></turbo-stream>");
    }

    [Fact]
    public void Refresh_SupportsMethodChaining()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Refresh("abc-123");

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void Refresh_CombinedWithOtherActions_BuildsAll()
    {
        var builder = new TurboStreamBuilder();

        var result = builder
            .Append("list", "<li>Item</li>")
            .Refresh("abc-123")
            .Build();

        result.Should().Contain("action=\"append\"");
        result.Should().Contain("action=\"refresh\"");
        result.Should().Contain("request-id=\"abc-123\"");
    }

    [Fact]
    public void GenerateRefreshAction_WithoutRequestId_ProducesValidHtml()
    {
        var builder = new StringBuilder();

        TurboStreamBuilder.GenerateRefreshAction(null, builder);

        var result = builder.ToString();

        result.Should().Be("<turbo-stream action=\"refresh\"></turbo-stream>");
    }

    [Fact]
    public void GenerateRefreshAction_WithRequestId_ProducesValidHtml()
    {
        var builder = new StringBuilder();

        TurboStreamBuilder.GenerateRefreshAction("req-789", builder);

        var result = builder.ToString();

        result.Should().Be("<turbo-stream action=\"refresh\" request-id=\"req-789\"></turbo-stream>");
    }

    [Fact]
    public void Build_WithNoActions_ReturnsEmptyString()
    {
        var builder = new TurboStreamBuilder();

        string result = builder.Build();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Build_WithMultipleActions_ConcatenatesAll()
    {
        var builder = new TurboStreamBuilder();

        string result = builder
            .Append("list", "<li>Item 1</li>")
            .Append("list", "<li>Item 2</li>")
            .Update("counter", "<span>2</span>")
            .Build();

        result.Should().Contain("action=\"append\"");
        result.Should().Contain("action=\"update\"");
        result.Should().Contain("<li>Item 1</li>");
        result.Should().Contain("<li>Item 2</li>");
        result.Should().Contain("<span>2</span>");
    }

    [Fact]
    public void Append_WithNullTarget_ThrowsArgumentNullException()
    {
        var builder = new TurboStreamBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.Append(null!, "<div>Content</div>"));
    }

    [Fact]
    public void Append_WithEmptyTarget_ThrowsArgumentException()
    {
        var builder = new TurboStreamBuilder();

        Assert.Throws<ArgumentException>(() => builder.Append(string.Empty, "<div>Content</div>"));
    }

    [Fact]
    public void Append_WithWhitespaceTarget_ThrowsArgumentException()
    {
        var builder = new TurboStreamBuilder();

        Assert.Throws<ArgumentException>(() => builder.Append("   ", "<div>Content</div>"));
    }

    [Fact]
    public void Append_WithNullHtml_ThrowsArgumentNullException()
    {
        var builder = new TurboStreamBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.Append("target", null!));
    }

    [Fact]
    public void Append_WithEmptyHtml_Succeeds()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Append("target", "").Build();

        result.Should().Contain("target=\"target\"");
        result.Should().Contain("<template></template>");
    }

    [Fact]
    public void Remove_WithNullTarget_ThrowsArgumentNullException()
    {
        var builder = new TurboStreamBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.Remove(null!));
    }

    [Fact]
    public void Remove_WithEmptyTarget_ThrowsArgumentException()
    {
        var builder = new TurboStreamBuilder();

        Assert.Throws<ArgumentException>(() => builder.Remove(string.Empty));
    }

    [Fact]
    public void EscapeAttribute_EscapesSpecialCharacters()
    {
        TurboStreamBuilder.EscapeAttribute("test\"value").Should().Be("test&quot;value");
        TurboStreamBuilder.EscapeAttribute("test<value>").Should().Be("test&lt;value&gt;");
        TurboStreamBuilder.EscapeAttribute("test&value").Should().Be("test&amp;value");
        TurboStreamBuilder.EscapeAttribute("test'value").Should().Be("test&#x27;value");
    }

    [Fact]
    public void EscapeAttribute_WithNull_ReturnsNull()
    {
        var result = TurboStreamBuilder.EscapeAttribute(null!);

        result.Should().BeNull();
    }

    [Fact]
    public void EscapeAttribute_WithEmpty_ReturnsEmpty()
    {
        var result = TurboStreamBuilder.EscapeAttribute(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateAction_ProducesValidHtml()
    {
        var builder = new StringBuilder();

        TurboStreamBuilder.GenerateAction("replace", "my-element", "<p>Content</p>", builder);

        var result = builder.ToString();

        result.Should().Be("<turbo-stream action=\"replace\" target=\"my-element\"><template><p>Content</p></template></turbo-stream>");
    }

    [Fact]
    public void GenerateRemoveAction_ProducesValidHtml()
    {
        var builder = new StringBuilder();

        TurboStreamBuilder.GenerateRemoveAction("my-element", builder);

        var result = builder.ToString();

        result.Should().Be("<turbo-stream action=\"remove\" target=\"my-element\"></turbo-stream>");
    }

    [Fact]
    public void Target_WithSpecialCharacters_IsEscaped()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Append("target\"test", "<div>Content</div>").Build();

        result.Should().Contain("target=\"target&quot;test\"");
    }

    [Theory]
    [InlineData("append")]
    [InlineData("prepend")]
    [InlineData("replace")]
    [InlineData("update")]
    [InlineData("before")]
    [InlineData("after")]
    public void AllActions_SupportMethodChaining(string action)
    {
        var builder = new TurboStreamBuilder();

        var result = action switch
        {
            "append" => builder.Append("target", "<div>Content</div>"),
            "prepend" => builder.Prepend("target", "<div>Content</div>"),
            "replace" => builder.Replace("target", "<div>Content</div>"),
            "update" => builder.Update("target", "<div>Content</div>"),
            "before" => builder.Before("target", "<div>Content</div>"),
            "after" => builder.After("target", "<div>Content</div>"),
            _ => throw new ArgumentException($"Unknown action: {action}")
        };

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void Remove_SupportsMethodChaining()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Remove("target");

        result.Should().BeSameAs(builder);
    }

    // Morph tests

    [Fact]
    public void Replace_WithMorph_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Replace("message-1", "<div>Updated</div>", morph: true).Build();

        result.Should().Be("<turbo-stream action=\"replace\" method=\"morph\" target=\"message-1\"><template><div>Updated</div></template></turbo-stream>");
    }

    [Fact]
    public void Replace_WithoutMorph_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Replace("message-1", "<div>Updated</div>", morph: false).Build();

        result.Should().Be("<turbo-stream action=\"replace\" target=\"message-1\"><template><div>Updated</div></template></turbo-stream>");
    }

    [Fact]
    public void Update_WithMorph_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.Update("content", "<p>New</p>", morph: true).Build();

        result.Should().Be("<turbo-stream action=\"update\" method=\"morph\" target=\"content\"><template><p>New</p></template></turbo-stream>");
    }

    // *All (targets) tests

    [Fact]
    public void AppendAll_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.AppendAll(".messages", "<div>Hello</div>").Build();

        result.Should().Be("<turbo-stream action=\"append\" targets=\".messages\"><template><div>Hello</div></template></turbo-stream>");
    }

    [Fact]
    public void PrependAll_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.PrependAll(".messages", "<div>Hello</div>").Build();

        result.Should().Be("<turbo-stream action=\"prepend\" targets=\".messages\"><template><div>Hello</div></template></turbo-stream>");
    }

    [Fact]
    public void ReplaceAll_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.ReplaceAll(".items", "<div>New</div>").Build();

        result.Should().Be("<turbo-stream action=\"replace\" targets=\".items\"><template><div>New</div></template></turbo-stream>");
    }

    [Fact]
    public void ReplaceAll_WithMorph_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.ReplaceAll(".items", "<div>New</div>", morph: true).Build();

        result.Should().Be("<turbo-stream action=\"replace\" method=\"morph\" targets=\".items\"><template><div>New</div></template></turbo-stream>");
    }

    [Fact]
    public void UpdateAll_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.UpdateAll(".items", "<p>Updated</p>").Build();

        result.Should().Be("<turbo-stream action=\"update\" targets=\".items\"><template><p>Updated</p></template></turbo-stream>");
    }

    [Fact]
    public void UpdateAll_WithMorph_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.UpdateAll(".items", "<p>Updated</p>", morph: true).Build();

        result.Should().Be("<turbo-stream action=\"update\" method=\"morph\" targets=\".items\"><template><p>Updated</p></template></turbo-stream>");
    }

    [Fact]
    public void RemoveAll_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.RemoveAll(".old-items").Build();

        result.Should().Be("<turbo-stream action=\"remove\" targets=\".old-items\"></turbo-stream>");
    }

    [Fact]
    public void BeforeAll_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.BeforeAll(".items", "<div>Before</div>").Build();

        result.Should().Be("<turbo-stream action=\"before\" targets=\".items\"><template><div>Before</div></template></turbo-stream>");
    }

    [Fact]
    public void AfterAll_GeneratesCorrectHtml()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.AfterAll(".items", "<div>After</div>").Build();

        result.Should().Be("<turbo-stream action=\"after\" targets=\".items\"><template><div>After</div></template></turbo-stream>");
    }

    [Fact]
    public void AppendAll_WithSpecialCharactersInSelector_EscapesTargets()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.AppendAll(".item[data-id=\"1\"]", "<div>Content</div>").Build();

        result.Should().Contain("targets=\".item[data-id=&quot;1&quot;]\"");
    }

    [Fact]
    public void AppendAll_WithNullTargets_ThrowsArgumentNullException()
    {
        var builder = new TurboStreamBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.AppendAll(null!, "<div>Content</div>"));
    }

    [Fact]
    public void AppendAll_WithEmptyTargets_ThrowsArgumentException()
    {
        var builder = new TurboStreamBuilder();

        Assert.Throws<ArgumentException>(() => builder.AppendAll(string.Empty, "<div>Content</div>"));
    }

    [Fact]
    public void RemoveAll_WithNullTargets_ThrowsArgumentNullException()
    {
        var builder = new TurboStreamBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.RemoveAll(null!));
    }

    [Theory]
    [InlineData("append")]
    [InlineData("prepend")]
    [InlineData("replace")]
    [InlineData("update")]
    [InlineData("before")]
    [InlineData("after")]
    public void AllActions_All_SupportMethodChaining(string action)
    {
        var builder = new TurboStreamBuilder();

        var result = action switch
        {
            "append" => builder.AppendAll(".target", "<div>Content</div>"),
            "prepend" => builder.PrependAll(".target", "<div>Content</div>"),
            "replace" => builder.ReplaceAll(".target", "<div>Content</div>"),
            "update" => builder.UpdateAll(".target", "<div>Content</div>"),
            "before" => builder.BeforeAll(".target", "<div>Content</div>"),
            "after" => builder.AfterAll(".target", "<div>Content</div>"),
            _ => throw new ArgumentException($"Unknown action: {action}")
        };

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void RemoveAll_SupportsMethodChaining()
    {
        var builder = new TurboStreamBuilder();

        var result = builder.RemoveAll(".target");

        result.Should().BeSameAs(builder);
    }
}
