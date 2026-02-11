using FluentAssertions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Tombatron.Turbo.TagHelpers;
using Xunit;

namespace Tombatron.Turbo.Tests.TagHelpers;

/// <summary>
/// Tests for the TurboFrameTagHelper.
/// </summary>
public class TurboFrameTagHelperTests
{
    private static TurboFrameTagHelper CreateTagHelper()
    {
        return new TurboFrameTagHelper();
    }

    [Fact]
    public void Process_SetsTagNameToTurboFrame()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.TagName.Should().Be("turbo-frame");
    }

    [Fact]
    public void Process_RemovesAspFramePrefixAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.FramePrefix = "item_";
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");
        output.Attributes.Add("asp-frame-prefix", "item_");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes.Should().NotContain(a => a.Name == "asp-frame-prefix");
    }

    [Fact]
    public void Process_WithSrc_AddsSrcAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Src = "/products/123";
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes.Should().Contain(a => a.Name == "src" && a.Value.ToString() == "/products/123");
    }

    [Fact]
    public void Process_WithoutSrc_DoesNotAddSrcAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes.Should().NotContain(a => a.Name == "src");
    }

    [Fact]
    public void Process_WithLoading_AddsLoadingAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Loading = "lazy";
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes.Should().Contain(a => a.Name == "loading" && a.Value.ToString() == "lazy");
    }

    [Fact]
    public void Process_WithDisabled_AddsDisabledAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Disabled = true;
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes.Should().Contain(a => a.Name == "disabled");
    }

    [Fact]
    public void Process_WithDisabledFalse_DoesNotAddDisabledAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Disabled = false;
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes.Should().NotContain(a => a.Name == "disabled");
    }

    [Fact]
    public void Process_WithTarget_AddsTargetAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Target = "_top";
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes.Should().Contain(a => a.Name == "target" && a.Value.ToString() == "_top");
    }

    [Fact]
    public void Process_WithAutoscroll_AddsAutoscrollAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Autoscroll = true;
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes.Should().Contain(a => a.Name == "autoscroll");
    }

    [Fact]
    public void Process_WithAutoscrollFalse_DoesNotAddAutoscrollAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Autoscroll = false;
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes.Should().NotContain(a => a.Name == "autoscroll");
    }

    [Fact]
    public void Process_WithAllAttributes_SetsAllAttributes()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.FramePrefix = "item_";
        tagHelper.Src = "/products/list";
        tagHelper.Loading = "lazy";
        tagHelper.Disabled = true;
        tagHelper.Target = "_top";
        tagHelper.Autoscroll = true;
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");
        output.Attributes.Add("asp-frame-prefix", "item_");
        output.Attributes.Add("id", "item_123");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes.Should().NotContain(a => a.Name == "asp-frame-prefix");
        output.Attributes.Should().Contain(a => a.Name == "id");
        output.Attributes.Should().Contain(a => a.Name == "src");
        output.Attributes.Should().Contain(a => a.Name == "loading");
        output.Attributes.Should().Contain(a => a.Name == "disabled");
        output.Attributes.Should().Contain(a => a.Name == "target");
        output.Attributes.Should().Contain(a => a.Name == "autoscroll");
    }

    [Fact]
    public void Process_PreservesExistingAttributes()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("turbo-frame");
        output.Attributes.Add("id", "cart-items");
        output.Attributes.Add("class", "my-frame");
        output.Attributes.Add("data-custom", "value");

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes.Should().Contain(a => a.Name == "id" && a.Value.ToString() == "cart-items");
        output.Attributes.Should().Contain(a => a.Name == "class" && a.Value.ToString() == "my-frame");
        output.Attributes.Should().Contain(a => a.Name == "data-custom" && a.Value.ToString() == "value");
    }

    [Fact]
    public void Process_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var output = CreateTagHelperOutput("turbo-frame");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => tagHelper.Process(null!, output));
    }

    [Fact]
    public void Process_WithNullOutput_ThrowsArgumentNullException()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateTagHelperContext();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => tagHelper.Process(context, null!));
    }

    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            tagName: "turbo-frame",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());
    }

    private static TagHelperOutput CreateTagHelperOutput(string tagName)
    {
        return new TagHelperOutput(
            tagName: tagName,
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }
}
