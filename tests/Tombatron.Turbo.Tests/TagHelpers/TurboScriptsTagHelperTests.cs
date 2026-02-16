using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;
using Moq;
using Tombatron.Turbo.TagHelpers;
using Xunit;

namespace Tombatron.Turbo.Tests.TagHelpers;

public class TurboScriptsTagHelperTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly TurboOptions _options;

    public TurboScriptsTagHelperTests()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        _options = new TurboOptions();
        _options.ImportMap.Pin("@hotwired/turbo",
            "https://unpkg.com/@hotwired/turbo@8.0.4/dist/turbo.es2017-esm.js", preload: true);
        _options.ImportMap.Pin("turbo-signalr",
            "/_content/Tombatron.Turbo/dist/turbo-signalr.bundled.esm.js", preload: true);
    }

    private TurboScriptsTagHelper CreateTagHelper()
    {
        return new TurboScriptsTagHelper(_mockEnvironment.Object, _options);
    }

    private static (TagHelperContext, TagHelperOutput) CreateTagHelperContextAndOutput()
    {
        var context = new TagHelperContext(
            tagName: "turbo-scripts",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test-id");

        var output = new TagHelperOutput(
            tagName: "turbo-scripts",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCachedResult, encoder) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        return (context, output);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullEnvironment_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboScriptsTagHelper(null!, _options));
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboScriptsTagHelper(_mockEnvironment.Object, null!));
    }

    #endregion

    #region Process - Null Parameter Tests

    [Fact]
    public void Process_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (_, output) = CreateTagHelperContextAndOutput();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => tagHelper.Process(null!, output));
    }

    [Fact]
    public void Process_WithNullOutput_ThrowsArgumentNullException()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, _) = CreateTagHelperContextAndOutput();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => tagHelper.Process(context, null!));
    }

    #endregion

    #region Default Property Values

    [Fact]
    public void Mode_DefaultIsTraditional()
    {
        // Arrange
        var tagHelper = CreateTagHelper();

        // Assert
        tagHelper.Mode.Should().Be(TurboScriptsMode.Traditional);
    }

    #endregion

    #region Tag Name Suppression

    [Fact]
    public void Process_SuppressesTagName()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.TagName.Should().BeNull();
    }

    #endregion

    #region Traditional Mode Tests

    [Fact]
    public void Process_Traditional_RendersTurboScriptTag()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().Contain("<script type=\"module\" src=\"https://unpkg.com/@hotwired/turbo@8.0.4/dist/turbo.es2017-esm.js\"></script>");
    }

    [Fact]
    public void Process_Traditional_RendersBridgeScriptTag()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().Contain("<script src=\"/_content/Tombatron.Turbo/dist/turbo-signalr.bundled.min.js\"></script>");
    }

    [Fact]
    public void Process_Traditional_Development_UsesUnminifiedBridge()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().Contain("turbo-signalr.bundled.js");
        content.Should().NotContain("turbo-signalr.bundled.min.js");
    }

    [Fact]
    public void Process_Traditional_Production_UsesMinifiedBridge()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().Contain("turbo-signalr.bundled.min.js");
    }

    [Fact]
    public void Process_Traditional_DoesNotRenderNonPreloadedEntries()
    {
        // Arrange
        _options.ImportMap.Pin("controllers/hello", "/js/controllers/hello_controller.js");
        var tagHelper = CreateTagHelper();
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().NotContain("hello_controller.js");
    }

    #endregion

    #region Importmap Mode Tests

    [Fact]
    public void Process_Importmap_RendersImportmapJson()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Mode = TurboScriptsMode.Importmap;
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().Contain("<script type=\"importmap\">");
        content.Should().Contain("\"@hotwired/turbo\"");
        content.Should().Contain("\"turbo-signalr\"");
        content.Should().Contain("https://unpkg.com/@hotwired/turbo@8.0.4/dist/turbo.es2017-esm.js");
        content.Should().Contain("/_content/Tombatron.Turbo/dist/turbo-signalr.bundled.esm.js");
    }

    [Fact]
    public void Process_Importmap_RendersModulepreloadLinks()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Mode = TurboScriptsMode.Importmap;
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().Contain("<link rel=\"modulepreload\" href=\"https://unpkg.com/@hotwired/turbo@8.0.4/dist/turbo.es2017-esm.js\">");
        content.Should().Contain("<link rel=\"modulepreload\" href=\"/_content/Tombatron.Turbo/dist/turbo-signalr.bundled.esm.js\">");
    }

    [Fact]
    public void Process_Importmap_RendersModuleImportStatements()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Mode = TurboScriptsMode.Importmap;
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().Contain("<script type=\"module\">");
        content.Should().Contain("import \"@hotwired/turbo\";");
        content.Should().Contain("import \"turbo-signalr\";");
    }

    [Fact]
    public void Process_Importmap_NonPreloadedEntriesOnlyInImportmap()
    {
        // Arrange
        _options.ImportMap.Pin("controllers/hello", "/js/controllers/hello_controller.js");
        var tagHelper = CreateTagHelper();
        tagHelper.Mode = TurboScriptsMode.Importmap;
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        // Should appear in importmap JSON
        content.Should().Contain("\"controllers/hello\"");
        content.Should().Contain("/js/controllers/hello_controller.js");
        // Should NOT appear in preload or import statements
        content.Should().NotContain("<link rel=\"modulepreload\" href=\"/js/controllers/hello_controller.js\">");
        content.Should().NotContain("import \"controllers/hello\";");
    }

    [Fact]
    public void Process_Importmap_UsesEsmBridgeFile()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Mode = TurboScriptsMode.Importmap;
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().Contain("turbo-signalr.bundled.esm.js");
        content.Should().NotContain("turbo-signalr.bundled.min.js");
    }

    #endregion

    #region Custom Pins Tests

    [Fact]
    public void Process_Importmap_CustomPinsAppearInOutput()
    {
        // Arrange
        _options.ImportMap.Pin("@hotwired/stimulus",
            "https://unpkg.com/@hotwired/stimulus@3.2.2/dist/stimulus.js", preload: true);
        _options.ImportMap.Pin("controllers/chat", "/js/controllers/chat_controller.js");
        var tagHelper = CreateTagHelper();
        tagHelper.Mode = TurboScriptsMode.Importmap;
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().Contain("\"@hotwired/stimulus\"");
        content.Should().Contain("https://unpkg.com/@hotwired/stimulus@3.2.2/dist/stimulus.js");
        content.Should().Contain("import \"@hotwired/stimulus\";");
        content.Should().Contain("<link rel=\"modulepreload\" href=\"https://unpkg.com/@hotwired/stimulus@3.2.2/dist/stimulus.js\">");
        content.Should().Contain("\"controllers/chat\"");
        content.Should().Contain("/js/controllers/chat_controller.js");
    }

    [Fact]
    public void Process_Importmap_UnpinnedEntriesDoNotAppear()
    {
        // Arrange
        _options.ImportMap.Pin("temp-module", "/temp.js", preload: true);
        _options.ImportMap.Unpin("temp-module");
        var tagHelper = CreateTagHelper();
        tagHelper.Mode = TurboScriptsMode.Importmap;
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().NotContain("temp-module");
        content.Should().NotContain("/temp.js");
    }

    #endregion
}
