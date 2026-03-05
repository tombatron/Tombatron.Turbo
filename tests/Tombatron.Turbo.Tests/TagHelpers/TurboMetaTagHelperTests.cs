using FluentAssertions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Tombatron.Turbo.TagHelpers;
using Xunit;

namespace Tombatron.Turbo.Tests.TagHelpers;

public class TurboMetaTagHelperTests
{
    private static TurboMetaTagHelper CreateTagHelper()
    {
        return new TurboMetaTagHelper();
    }

    [Fact]
    public void Process_SuppressesOutput()
    {
        var tagHelper = CreateTagHelper();
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        output.TagName.Should().BeNull();
    }

    [Fact]
    public void Process_WithRefreshMethodMorph_EmitsCorrectMetaTag()
    {
        var tagHelper = CreateTagHelper();
        tagHelper.RefreshMethod = TurboRefreshMethod.Morph;
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().Contain("<meta name=\"turbo-refresh-method\" content=\"morph\">");
    }

    [Fact]
    public void Process_WithRefreshMethodReplace_EmitsCorrectMetaTag()
    {
        var tagHelper = CreateTagHelper();
        tagHelper.RefreshMethod = TurboRefreshMethod.Replace;
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().Contain("<meta name=\"turbo-refresh-method\" content=\"replace\">");
    }

    [Fact]
    public void Process_WithoutRefreshMethod_DoesNotEmitRefreshMethodMeta()
    {
        var tagHelper = CreateTagHelper();
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().NotContain("turbo-refresh-method");
    }

    [Fact]
    public void Process_WithRefreshScrollPreserve_EmitsCorrectMetaTag()
    {
        var tagHelper = CreateTagHelper();
        tagHelper.RefreshScroll = TurboRefreshScroll.Preserve;
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().Contain("<meta name=\"turbo-refresh-scroll\" content=\"preserve\">");
    }

    [Fact]
    public void Process_WithRefreshScrollReset_EmitsCorrectMetaTag()
    {
        var tagHelper = CreateTagHelper();
        tagHelper.RefreshScroll = TurboRefreshScroll.Reset;
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().Contain("<meta name=\"turbo-refresh-scroll\" content=\"reset\">");
    }

    [Fact]
    public void Process_WithoutRefreshScroll_DoesNotEmitRefreshScrollMeta()
    {
        var tagHelper = CreateTagHelper();
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().NotContain("turbo-refresh-scroll");
    }

    [Fact]
    public void Process_WithCacheControl_EmitsCorrectMetaTag()
    {
        var tagHelper = CreateTagHelper();
        tagHelper.CacheControl = "no-cache";
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().Contain("<meta name=\"turbo-cache-control\" content=\"no-cache\">");
    }

    [Fact]
    public void Process_WithoutCacheControl_DoesNotEmitCacheControlMeta()
    {
        var tagHelper = CreateTagHelper();
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().NotContain("turbo-cache-control");
    }

    [Fact]
    public void Process_WithVisitControl_EmitsCorrectMetaTag()
    {
        var tagHelper = CreateTagHelper();
        tagHelper.VisitControl = "reload";
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().Contain("<meta name=\"turbo-visit-control\" content=\"reload\">");
    }

    [Fact]
    public void Process_WithoutVisitControl_DoesNotEmitVisitControlMeta()
    {
        var tagHelper = CreateTagHelper();
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().NotContain("turbo-visit-control");
    }

    [Fact]
    public void Process_WithAllAttributes_EmitsAllMetaTags()
    {
        var tagHelper = CreateTagHelper();
        tagHelper.RefreshMethod = TurboRefreshMethod.Morph;
        tagHelper.RefreshScroll = TurboRefreshScroll.Preserve;
        tagHelper.CacheControl = "no-cache";
        tagHelper.VisitControl = "reload";
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().Contain("<meta name=\"turbo-refresh-method\" content=\"morph\">");
        html.Should().Contain("<meta name=\"turbo-refresh-scroll\" content=\"preserve\">");
        html.Should().Contain("<meta name=\"turbo-cache-control\" content=\"no-cache\">");
        html.Should().Contain("<meta name=\"turbo-visit-control\" content=\"reload\">");
    }

    [Fact]
    public void Process_WithNoAttributes_EmitsNoMetaTags()
    {
        var tagHelper = CreateTagHelper();
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().BeEmpty();
    }

    [Fact]
    public void Process_CacheControlWithSpecialCharacters_EncodesContent()
    {
        var tagHelper = CreateTagHelper();
        tagHelper.CacheControl = "<script>alert('xss')</script>";
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().NotContain("<script>");
        html.Should().Contain("&lt;script&gt;");
    }

    [Fact]
    public void Process_VisitControlWithSpecialCharacters_EncodesContent()
    {
        var tagHelper = CreateTagHelper();
        tagHelper.VisitControl = "\"onload=\"alert(1)";
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var html = output.PostElement.GetContent();
        html.Should().NotContain("\"onload=\"");
    }

    [Fact]
    public void Process_WithNullContext_ThrowsArgumentNullException()
    {
        var tagHelper = CreateTagHelper();
        var output = CreateTagHelperOutput();

        Assert.Throws<ArgumentNullException>(() => tagHelper.Process(null!, output));
    }

    [Fact]
    public void Process_WithNullOutput_ThrowsArgumentNullException()
    {
        var tagHelper = CreateTagHelper();
        var context = CreateTagHelperContext();

        Assert.Throws<ArgumentNullException>(() => tagHelper.Process(context, null!));
    }

    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            tagName: "turbo-meta",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            tagName: "turbo-meta",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }
}
