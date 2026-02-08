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
    [Fact]
    public void Process_SetsTagNameToTurboFrame()
    {
        // Arrange
        var tagHelper = new TurboFrameTagHelper();
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
        var tagHelper = new TurboFrameTagHelper
        {
            FramePrefix = "item_"
        };
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
        var tagHelper = new TurboFrameTagHelper
        {
            Src = "/products/123"
        };
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
        var tagHelper = new TurboFrameTagHelper();
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
        var tagHelper = new TurboFrameTagHelper
        {
            Loading = "lazy"
        };
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
        var tagHelper = new TurboFrameTagHelper
        {
            Disabled = true
        };
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
        var tagHelper = new TurboFrameTagHelper
        {
            Disabled = false
        };
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
        var tagHelper = new TurboFrameTagHelper
        {
            Target = "_top"
        };
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
        var tagHelper = new TurboFrameTagHelper
        {
            Autoscroll = true
        };
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
        var tagHelper = new TurboFrameTagHelper
        {
            Autoscroll = false
        };
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
        var tagHelper = new TurboFrameTagHelper
        {
            FramePrefix = "item_",
            Src = "/products/list",
            Loading = "lazy",
            Disabled = true,
            Target = "_top",
            Autoscroll = true
        };
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
        var tagHelper = new TurboFrameTagHelper();
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
        var tagHelper = new TurboFrameTagHelper();
        var output = CreateTagHelperOutput("turbo-frame");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => tagHelper.Process(null!, output));
    }

    [Fact]
    public void Process_WithNullOutput_ThrowsArgumentNullException()
    {
        // Arrange
        var tagHelper = new TurboFrameTagHelper();
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
