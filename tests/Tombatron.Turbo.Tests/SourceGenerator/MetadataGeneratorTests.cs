using System.Collections.Immutable;
using FluentAssertions;
using Tombatron.Turbo.SourceGenerator;
using Tombatron.Turbo.SourceGenerator.Models;
using Xunit;

namespace Tombatron.Turbo.Tests.SourceGenerator;

public class MetadataGeneratorTests
{
    [Fact]
    public void GenerateMetadataSource_WithNoFiles_GeneratesEmptyDictionaries()
    {
        // Arrange
        var files = ImmutableArray<RazorFileInfo>.Empty;

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files);

        // Assert
        result.Should().Contain("public static class TurboFrameMetadata");
        result.Should().Contain("StaticFrames");
        result.Should().Contain("PrefixFrames");
        result.Should().Contain("TryGetTemplate");
    }

    [Fact]
    public void GenerateMetadataSource_WithStaticFrames_GeneratesStaticDictionary()
    {
        // Arrange
        var frame = new TurboFrame("cart-items", null, "content", 1, false);
        var file = new RazorFileInfo(
            "Pages/Cart/Index.cshtml",
            "Cart_Index",
            ImmutableArray.Create(frame));
        var files = ImmutableArray.Create(file);

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files);

        // Assert
        result.Should().Contain("\"cart-items\"");
        result.Should().Contain("Cart_Index_Frame_cart_items");
    }

    [Fact]
    public void GenerateMetadataSource_WithDynamicFrames_GeneratesPrefixDictionary()
    {
        // Arrange
        var frame = new TurboFrame("item_@Model.Id", "item_", "content", 1, true);
        var file = new RazorFileInfo(
            "Pages/Products/Index.cshtml",
            "Products_Index",
            ImmutableArray.Create(frame));
        var files = ImmutableArray.Create(file);

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files);

        // Assert
        result.Should().Contain("\"item_\"");
        result.Should().Contain("Products_Index_Prefix_item_");
    }

    [Fact]
    public void GenerateMetadataSource_WithMixedFrames_GeneratesBothDictionaries()
    {
        // Arrange
        var staticFrame = new TurboFrame("header", null, "content", 1, false);
        var dynamicFrame = new TurboFrame("item_@Model.Id", "item_", "content", 5, true);
        var file = new RazorFileInfo(
            "Pages/Index.cshtml",
            "Index",
            ImmutableArray.Create(staticFrame, dynamicFrame));
        var files = ImmutableArray.Create(file);

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files);

        // Assert
        result.Should().Contain("\"header\"");
        result.Should().Contain("\"item_\"");
    }

    [Fact]
    public void GenerateMetadataSource_WithMultipleFiles_AggregatesAllFrames()
    {
        // Arrange
        var file1 = new RazorFileInfo(
            "Pages/Cart/Index.cshtml",
            "Cart_Index",
            ImmutableArray.Create(new TurboFrame("cart-total", null, "content", 1, false)));

        var file2 = new RazorFileInfo(
            "Pages/Products/Index.cshtml",
            "Products_Index",
            ImmutableArray.Create(new TurboFrame("product-list", null, "content", 1, false)));

        var files = ImmutableArray.Create(file1, file2);

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files);

        // Assert
        result.Should().Contain("\"cart-total\"");
        result.Should().Contain("\"product-list\"");
    }

    [Fact]
    public void GenerateMetadataSource_UsesCorrectNamespace()
    {
        // Arrange
        var files = ImmutableArray<RazorFileInfo>.Empty;
        string customNamespace = "MyApp.Generated";

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files, customNamespace);

        // Assert
        result.Should().Contain($"namespace {customNamespace};");
    }

    [Fact]
    public void GenerateMetadataSource_GeneratesTryGetTemplateMethod()
    {
        // Arrange
        var frame = new TurboFrame("test-frame", null, "content", 1, false);
        var file = new RazorFileInfo("test.cshtml", "Test", ImmutableArray.Create(frame));
        var files = ImmutableArray.Create(file);

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files);

        // Assert
        result.Should().Contain("public static bool TryGetTemplate(string frameId, out string? templateName)");
        result.Should().Contain("StaticFrames.TryGetValue(frameId, out templateName)");
        result.Should().Contain("frameId.StartsWith(kvp.Key");
    }

    [Fact]
    public void GenerateMetadataSource_EscapesSpecialCharacters()
    {
        // Arrange
        var frame = new TurboFrame("frame\"with\"quotes", null, "content", 1, false);
        var file = new RazorFileInfo("test.cshtml", "Test", ImmutableArray.Create(frame));
        var files = ImmutableArray.Create(file);

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files);

        // Assert
        result.Should().Contain("\\\"");
    }

    [Fact]
    public void GenerateMetadataSource_IncludesAutoGeneratedComment()
    {
        // Arrange
        var files = ImmutableArray<RazorFileInfo>.Empty;

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files);

        // Assert
        result.Should().StartWith("// <auto-generated/>");
    }

    [Fact]
    public void GenerateMetadataSource_IncludesNullableEnable()
    {
        // Arrange
        var files = ImmutableArray<RazorFileInfo>.Empty;

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files);

        // Assert
        result.Should().Contain("#nullable enable");
    }

    [Fact]
    public void GenerateMetadataSource_UsesFrozenDictionary()
    {
        // Arrange
        var files = ImmutableArray<RazorFileInfo>.Empty;

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files);

        // Assert
        result.Should().Contain("using System.Collections.Frozen;");
        result.Should().Contain("FrozenDictionary<string, string>");
        result.Should().Contain(".ToFrozenDictionary()");
    }

    [Fact]
    public void GenerateMetadataSource_DeduplicatesPrefixes()
    {
        // Arrange - two frames with same prefix
        var frame1 = new TurboFrame("item_@Model.Id1", "item_", "content", 1, true);
        var frame2 = new TurboFrame("item_@Model.Id2", "item_", "content", 2, true);
        var file = new RazorFileInfo("test.cshtml", "Test", ImmutableArray.Create(frame1, frame2));
        var files = ImmutableArray.Create(file);

        // Act
        string result = MetadataGenerator.GenerateMetadataSource(files);

        // Assert - prefix should only appear once
        int count = result.Split(new[] { "\"item_\"" }, StringSplitOptions.None).Length - 1;
        count.Should().Be(1, "prefix should only appear once in PrefixFrames dictionary");
    }
}
