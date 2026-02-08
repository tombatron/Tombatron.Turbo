using FluentAssertions;
using Tombatron.Turbo.SourceGenerator;
using Tombatron.Turbo.SourceGenerator.Models;
using Xunit;

namespace Tombatron.Turbo.Tests.SourceGenerator;

public class TemplateGeneratorTests
{
    [Fact]
    public void GenerateStaticTemplate_WithValidFrame_GeneratesCorrectTemplate()
    {
        // Arrange
        var frame = new TurboFrame(
            Id: "cart-items",
            Prefix: null,
            Content: "<div>Item content</div>",
            StartLine: 1,
            IsDynamic: false);

        // Act
        string result = TemplateGenerator.GenerateStaticTemplate(frame);

        // Assert
        result.Should().Contain("Layout = null;");
        result.Should().Contain("<turbo-frame id=\"cart-items\">");
        result.Should().Contain("<div>Item content</div>");
        result.Should().Contain("</turbo-frame>");
    }

    [Fact]
    public void GenerateStaticTemplate_WithDynamicFrame_ThrowsArgumentException()
    {
        // Arrange
        var frame = new TurboFrame(
            Id: "item_@Model.Id",
            Prefix: "item_",
            Content: "content",
            StartLine: 1,
            IsDynamic: true);

        // Act
        Action act = () => TemplateGenerator.GenerateStaticTemplate(frame);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*static template for dynamic frame*");
    }

    [Fact]
    public void GenerateDynamicTemplate_WithValidFrame_GeneratesCorrectTemplate()
    {
        // Arrange
        var frame = new TurboFrame(
            Id: "item_@Model.Id",
            Prefix: "item_",
            Content: "<div>Dynamic content</div>",
            StartLine: 1,
            IsDynamic: true);

        // Act
        string result = TemplateGenerator.GenerateDynamicTemplate(frame);

        // Assert
        result.Should().Contain("Layout = null;");
        result.Should().Contain("var turboFrameId = ViewBag.TurboFrameId as string;");
        result.Should().Contain("<turbo-frame id=\"@turboFrameId\">");
        result.Should().Contain("<div>Dynamic content</div>");
        result.Should().Contain("</turbo-frame>");
    }

    [Fact]
    public void GenerateDynamicTemplate_WithStaticFrame_ThrowsArgumentException()
    {
        // Arrange
        var frame = new TurboFrame(
            Id: "static-id",
            Prefix: null,
            Content: "content",
            StartLine: 1,
            IsDynamic: false);

        // Act
        Action act = () => TemplateGenerator.GenerateDynamicTemplate(frame);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*dynamic template for static frame*");
    }

    [Fact]
    public void GenerateDynamicTemplate_WithoutPrefix_ThrowsArgumentException()
    {
        // Arrange
        var frame = new TurboFrame(
            Id: "@Model.Id",
            Prefix: null,
            Content: "content",
            StartLine: 1,
            IsDynamic: true);

        // Act
        Action act = () => TemplateGenerator.GenerateDynamicTemplate(frame);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must have a prefix*");
    }

    [Theory]
    [InlineData("Index", "cart-items", "Index.cart-items.cshtml")]
    [InlineData("Products", "product-list", "Products.product-list.cshtml")]
    [InlineData("Cart_Index", "total", "Cart_Index.total.cshtml")]
    public void GetStaticTemplateFileName_GeneratesCorrectFileName(
        string viewName, string frameId, string expected)
    {
        // Act
        string result = TemplateGenerator.GetStaticTemplateFileName(viewName, frameId);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Index", "item_", "Index.item__.cshtml")]
    [InlineData("Products", "product_", "Products.product__.cshtml")]
    public void GetDynamicTemplateFileName_GeneratesCorrectFileName(
        string viewName, string prefix, string expected)
    {
        // Act
        string result = TemplateGenerator.GetDynamicTemplateFileName(viewName, prefix);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetStaticTemplateFileName_SanitizesSpecialCharacters()
    {
        // Arrange
        string viewName = "Index";
        string frameId = "cart:items/total";

        // Act
        string result = TemplateGenerator.GetStaticTemplateFileName(viewName, frameId);

        // Assert
        result.Should().Be("Index.cart_items_total.cshtml");
    }

    [Fact]
    public void GenerateStaticTemplate_PreservesMultiLineContent()
    {
        // Arrange
        var frame = new TurboFrame(
            Id: "multi-line",
            Prefix: null,
            Content: @"<div>
    <span>Line 1</span>
    <span>Line 2</span>
</div>",
            StartLine: 1,
            IsDynamic: false);

        // Act
        string result = TemplateGenerator.GenerateStaticTemplate(frame);

        // Assert
        result.Should().Contain("<span>Line 1</span>");
        result.Should().Contain("<span>Line 2</span>");
    }
}
