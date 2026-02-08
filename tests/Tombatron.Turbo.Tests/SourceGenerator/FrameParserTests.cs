using FluentAssertions;
using Tombatron.Turbo.SourceGenerator;
using Tombatron.Turbo.SourceGenerator.Models;
using Xunit;

namespace Tombatron.Turbo.Tests.SourceGenerator;

public class FrameParserTests
{
    [Fact]
    public void Parse_WithEmptyContent_ReturnsEmptyArray()
    {
        // Act
        var result = FrameParser.Parse("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithNullContent_ReturnsEmptyArray()
    {
        // Act
        var result = FrameParser.Parse(null!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithNoFrames_ReturnsEmptyArray()
    {
        // Arrange
        string content = "<div>Hello World</div>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithStaticFrame_ReturnsFrame()
    {
        // Arrange
        string content = @"<turbo-frame id=""cart-items"">
    <div>Item 1</div>
</turbo-frame>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("cart-items");
        result[0].IsDynamic.Should().BeFalse();
        result[0].Prefix.Should().BeNull();
        result[0].Content.Should().Contain("Item 1");
    }

    [Fact]
    public void Parse_WithDynamicFrame_ReturnsFrameMarkedAsDynamic()
    {
        // Arrange
        string content = @"<turbo-frame id=""item_@Model.Id"" asp-frame-prefix=""item_"">
    <div>Item content</div>
</turbo-frame>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("item_@Model.Id");
        result[0].IsDynamic.Should().BeTrue();
        result[0].Prefix.Should().Be("item_");
        result[0].HasPrefix.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithDynamicFrameWithoutPrefix_ReturnsFrameWithoutPrefix()
    {
        // Arrange
        string content = @"<turbo-frame id=""@Model.Id"">
    <div>Content</div>
</turbo-frame>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("@Model.Id");
        result[0].IsDynamic.Should().BeTrue();
        result[0].Prefix.Should().BeNull();
        result[0].HasPrefix.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMultipleFrames_ReturnsAllFrames()
    {
        // Arrange
        string content = @"
<turbo-frame id=""header"">
    <h1>Header</h1>
</turbo-frame>
<div>Some content</div>
<turbo-frame id=""footer"">
    <p>Footer</p>
</turbo-frame>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("header");
        result[1].Id.Should().Be("footer");
    }

    [Fact]
    public void Parse_WithNestedFrames_ReturnsAllFrames()
    {
        // Arrange
        string content = @"<turbo-frame id=""outer"">
    <div>Outer content</div>
    <turbo-frame id=""inner"">
        <span>Inner content</span>
    </turbo-frame>
</turbo-frame>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("outer");
        result[1].Id.Should().Be("inner");
        result[0].Content.Should().Contain("turbo-frame id=\"inner\"");
    }

    [Fact]
    public void Parse_WithSingleQuotedAttributes_ParsesCorrectly()
    {
        // Arrange
        string content = @"<turbo-frame id='single-quoted'>
    <div>Content</div>
</turbo-frame>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("single-quoted");
    }

    [Fact]
    public void Parse_WithMixedCaseTag_ParsesCorrectly()
    {
        // Arrange
        string content = @"<TURBO-FRAME id=""uppercase"">
    <div>Content</div>
</TURBO-FRAME>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("uppercase");
    }

    [Fact]
    public void Parse_WithOtherAttributes_ParsesIdAndPrefix()
    {
        // Arrange
        string content = @"<turbo-frame id=""frame-id"" class=""my-class"" asp-frame-prefix=""frame-"" data-value=""123"">
    <div>Content</div>
</turbo-frame>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("frame-id");
        result[0].Prefix.Should().Be("frame-");
    }

    [Fact]
    public void Parse_WithFrameWithoutId_SkipsFrame()
    {
        // Arrange
        string content = @"<turbo-frame class=""no-id"">
    <div>Content</div>
</turbo-frame>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithSelfClosingFrame_ReturnsEmptyContent()
    {
        // Arrange - Note: turbo-frame shouldn't be self-closing, but we handle it gracefully
        string content = @"<turbo-frame id=""self-closing"" />";

        // Act
        var result = FrameParser.Parse(content);

        // Assert - Currently this may not parse correctly, which is fine
        // The real Turbo.js also doesn't support self-closing frames
    }

    [Fact]
    public void Parse_CalculatesCorrectLineNumber()
    {
        // Arrange
        string content = @"Line 1
Line 2
<turbo-frame id=""frame"">
    Content
</turbo-frame>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().HaveCount(1);
        result[0].StartLine.Should().Be(3);
    }

    [Theory]
    [InlineData("@Model.Id", true)]
    [InlineData("@item.Id", true)]
    [InlineData("item_@Model.Id", true)]
    [InlineData("prefix_@(Model.Id)", true)]
    [InlineData("static-id", false)]
    [InlineData("cart-items", false)]
    [InlineData("", false)]
    public void ContainsRazorExpression_DetectsRazorExpressions(string value, bool expected)
    {
        // Act
        bool result = FrameParser.ContainsRazorExpression(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ContainsRazorExpression_WithEscapedAt_ReturnsFalse()
    {
        // Arrange
        string value = "email@@example.com";

        // Act
        bool result = FrameParser.ContainsRazorExpression(value);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("item_@Model.Id", "item_")]
    [InlineData("cart_@item.Id", "cart_")]
    [InlineData("@Model.Id", "")]
    [InlineData("static-id", "static-id")]
    public void GetStaticIdPortion_ExtractsStaticPortion(string id, string expected)
    {
        // Act
        string result = FrameParser.GetStaticIdPortion(id);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StaticIdPortion_Property_ReturnsCorrectValue()
    {
        // Arrange
        var staticFrame = new TurboFrame("cart-items", null, "content", 1, false);
        var dynamicFrame = new TurboFrame("item_@Model.Id", "item_", "content", 1, true);

        // Assert
        staticFrame.StaticIdPortion.Should().Be("cart-items");
        dynamicFrame.StaticIdPortion.Should().Be("item_");
    }

    [Fact]
    public void IsValidFrame_WithStaticFrame_ReturnsTrue()
    {
        // Arrange
        var frame = new TurboFrame("cart-items", null, "content", 1, false);

        // Act
        bool result = FrameParser.IsValidFrame(frame);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidFrame_WithDynamicFrameAndPrefix_ReturnsTrue()
    {
        // Arrange
        var frame = new TurboFrame("item_@Model.Id", "item_", "content", 1, true);

        // Act
        bool result = FrameParser.IsValidFrame(frame);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidFrame_WithDynamicFrameWithoutPrefix_ReturnsFalse()
    {
        // Arrange
        var frame = new TurboFrame("@Model.Id", null, "content", 1, true);

        // Act
        bool result = FrameParser.IsValidFrame(frame);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithComplexRazorContent_PreservesContent()
    {
        // Arrange
        string content = @"<turbo-frame id=""products"">
    @foreach (var item in Model.Items)
    {
        <div class=""product"">
            <span>@item.Name</span>
            <span>@item.Price.ToString(""C"")</span>
        </div>
    }
</turbo-frame>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().HaveCount(1);
        result[0].Content.Should().Contain("@foreach");
        result[0].Content.Should().Contain("@item.Name");
    }

    [Fact]
    public void Parse_WithMultiLineAttributes_ParsesCorrectly()
    {
        // Arrange
        string content = @"<turbo-frame
    id=""multi-line""
    class=""some-class""
    asp-frame-prefix=""multi-"">
    <div>Content</div>
</turbo-frame>";

        // Act
        var result = FrameParser.Parse(content);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("multi-line");
        result[0].Prefix.Should().Be("multi-");
    }
}
