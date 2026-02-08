using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using System.Security.Claims;
using Tombatron.Turbo.TagHelpers;
using Xunit;

namespace Tombatron.Turbo.Tests.TagHelpers;

/// <summary>
/// Tests for the TurboTagHelper class.
/// </summary>
public class TurboTagHelperTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly TurboOptions _options;
    private readonly Mock<HttpContext> _mockHttpContext;

    public TurboTagHelperTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _options = new TurboOptions { HubPath = "/turbo-hub" };
        _mockHttpContext = new Mock<HttpContext>();

        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);
    }

    private TurboTagHelper CreateTagHelper()
    {
        return new TurboTagHelper(_mockHttpContextAccessor.Object, _options);
    }

    private static (TagHelperContext, TagHelperOutput) CreateTagHelperContextAndOutput()
    {
        var context = new TagHelperContext(
            tagName: "turbo",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test-id");

        var output = new TagHelperOutput(
            tagName: "turbo",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCachedResult, encoder) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        return (context, output);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullHttpContextAccessor_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboTagHelper(null!, _options));
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboTagHelper(_mockHttpContextAccessor.Object, null!));
    }

    #endregion

    #region Process Tests - Single Stream

    [Fact]
    public void Process_WithExplicitStream_RendersCorrectElement()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Stream = "notifications";
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.TagName.Should().Be("turbo-stream-source-signalr");
        output.Attributes["stream"].Value.Should().Be("notifications");
        output.Attributes["hub-url"].Value.Should().Be("/turbo-hub");
    }

    [Fact]
    public void Process_WithCustomHubUrl_UsesCustomUrl()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Stream = "test";
        tagHelper.HubUrl = "/custom-hub";
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes["hub-url"].Value.Should().Be("/custom-hub");
    }

    [Fact]
    public void Process_WithId_IncludesIdAttribute()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Stream = "test";
        tagHelper.Id = "my-turbo-element";
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes["id"].Value.Should().Be("my-turbo-element");
    }

    #endregion

    #region Process Tests - Multiple Streams

    [Fact]
    public void Process_WithMultipleStreams_RendersContainer()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Stream = "notifications,updates,alerts";
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.TagName.Should().Be("div");
        output.Attributes["data-turbo-streams"].Value.Should().Be("true");

        string content = output.Content.GetContent();
        content.Should().Contain("stream=\"notifications\"");
        content.Should().Contain("stream=\"updates\"");
        content.Should().Contain("stream=\"alerts\"");
    }

    [Fact]
    public void Process_WithMultipleStreamsAndId_IncludesIdOnContainer()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Stream = "stream1,stream2";
        tagHelper.Id = "my-container";
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.TagName.Should().Be("div");
        output.Attributes["id"].Value.Should().Be("my-container");
    }

    [Fact]
    public void Process_WithStreamsContainingSpaces_TrimsNames()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Stream = "  stream1  ,  stream2  ";
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        string content = output.Content.GetContent();
        content.Should().Contain("stream=\"stream1\"");
        content.Should().Contain("stream=\"stream2\"");
    }

    #endregion

    #region Process Tests - Default Stream Name Generation

    [Fact]
    public void Process_WithAuthenticatedUser_GeneratesUserStream()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);

        _mockHttpContext.Setup(c => c.User).Returns(user);

        var tagHelper = CreateTagHelper();
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes["stream"].Value.Should().Be("user:user-123");
    }

    [Fact]
    public void Process_WithAuthenticatedUserNoId_UsesName()
    {
        // Arrange
        var identity = new ClaimsIdentity("test");
        identity.AddClaim(new Claim(ClaimTypes.Name, "john.doe"));
        var user = new ClaimsPrincipal(identity);

        _mockHttpContext.Setup(c => c.User).Returns(user);

        var tagHelper = CreateTagHelper();
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes["stream"].Value.Should().Be("user:john.doe");
    }

    [Fact]
    public void Process_WithSession_GeneratesSessionStream()
    {
        // Arrange
        var mockSession = new Mock<ISession>();
        mockSession.Setup(s => s.Id).Returns("session-abc123");

        var identity = new ClaimsIdentity(); // Not authenticated
        var user = new ClaimsPrincipal(identity);

        _mockHttpContext.Setup(c => c.User).Returns(user);
        _mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

        var tagHelper = CreateTagHelper();
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes["stream"].Value.Should().Be("session:session-abc123");
    }

    [Fact]
    public void Process_WithNoSessionAndNoAuth_UsesConnectionId()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // Not authenticated
        var user = new ClaimsPrincipal(identity);

        var mockConnection = new Mock<ConnectionInfo>();
        mockConnection.Setup(c => c.Id).Returns("conn-xyz789");

        _mockHttpContext.Setup(c => c.User).Returns(user);
        _mockHttpContext.Setup(c => c.Session).Throws(new InvalidOperationException("Session not configured"));
        _mockHttpContext.Setup(c => c.Connection).Returns(mockConnection.Object);

        var tagHelper = CreateTagHelper();
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes["stream"].Value.Should().Be("connection:conn-xyz789");
    }

    [Fact]
    public void Process_WithNullHttpContext_SuppressesOutput()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var tagHelper = CreateTagHelper();
        var (context, output) = CreateTagHelperContextAndOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.TagName.Should().BeNull();
    }

    #endregion

    #region Process Tests - Null Parameter Validation

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

    #region ParseStreamNames Tests

    [Fact]
    public void ParseStreamNames_WithSingleStream_ReturnsOneElement()
    {
        // Act
        string[] result = TurboTagHelper.ParseStreamNames("notifications");

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Be("notifications");
    }

    [Fact]
    public void ParseStreamNames_WithMultipleStreams_ReturnsAll()
    {
        // Act
        string[] result = TurboTagHelper.ParseStreamNames("a,b,c");

        // Assert
        result.Should().HaveCount(3);
        result.Should().ContainInOrder("a", "b", "c");
    }

    [Fact]
    public void ParseStreamNames_WithSpaces_TrimsEntries()
    {
        // Act
        string[] result = TurboTagHelper.ParseStreamNames("  a  ,  b  ,  c  ");

        // Assert
        result.Should().ContainInOrder("a", "b", "c");
    }

    [Fact]
    public void ParseStreamNames_WithEmptyEntries_RemovesThem()
    {
        // Act
        string[] result = TurboTagHelper.ParseStreamNames("a,,b,  ,c");

        // Assert
        result.Should().HaveCount(3);
        result.Should().ContainInOrder("a", "b", "c");
    }

    [Fact]
    public void ParseStreamNames_WithNull_ReturnsEmpty()
    {
        // Act
        string[] result = TurboTagHelper.ParseStreamNames(null!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseStreamNames_WithEmptyString_ReturnsEmpty()
    {
        // Act
        string[] result = TurboTagHelper.ParseStreamNames("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseStreamNames_WithWhitespace_ReturnsEmpty()
    {
        // Act
        string[] result = TurboTagHelper.ParseStreamNames("   ");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region EscapeAttribute Tests

    [Fact]
    public void EscapeAttribute_EscapesAmpersand()
    {
        // Act
        string result = TurboTagHelper.EscapeAttribute("a&b");

        // Assert
        result.Should().Be("a&amp;b");
    }

    [Fact]
    public void EscapeAttribute_EscapesQuotes()
    {
        // Act
        string result = TurboTagHelper.EscapeAttribute("a\"b");

        // Assert
        result.Should().Be("a&quot;b");
    }

    [Fact]
    public void EscapeAttribute_EscapesLessThan()
    {
        // Act
        string result = TurboTagHelper.EscapeAttribute("a<b");

        // Assert
        result.Should().Be("a&lt;b");
    }

    [Fact]
    public void EscapeAttribute_EscapesGreaterThan()
    {
        // Act
        string result = TurboTagHelper.EscapeAttribute("a>b");

        // Assert
        result.Should().Be("a&gt;b");
    }

    [Fact]
    public void EscapeAttribute_EscapesSingleQuote()
    {
        // Act
        string result = TurboTagHelper.EscapeAttribute("a'b");

        // Assert
        result.Should().Be("a&#39;b");
    }

    [Fact]
    public void EscapeAttribute_EscapesAllSpecialCharacters()
    {
        // Act
        string result = TurboTagHelper.EscapeAttribute("<a href=\"test\">text & 'stuff'</a>");

        // Assert
        result.Should().Be("&lt;a href=&quot;test&quot;&gt;text &amp; &#39;stuff&#39;&lt;/a&gt;");
    }

    [Fact]
    public void EscapeAttribute_WithNull_ReturnsNull()
    {
        // Act
        string result = TurboTagHelper.EscapeAttribute(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void EscapeAttribute_WithEmpty_ReturnsEmpty()
    {
        // Act
        string result = TurboTagHelper.EscapeAttribute("");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GenerateDefaultStreamName Tests

    [Fact]
    public void GenerateDefaultStreamName_WithNullHttpContext_ReturnsEmpty()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        var tagHelper = CreateTagHelper();

        // Act
        string result = tagHelper.GenerateDefaultStreamName();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GenerateDefaultStreamName_PrefersNameIdentifier()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "id-123"),
            new Claim(ClaimTypes.Name, "john.doe")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);

        _mockHttpContext.Setup(c => c.User).Returns(user);

        var tagHelper = CreateTagHelper();

        // Act
        string result = tagHelper.GenerateDefaultStreamName();

        // Assert
        result.Should().Be("user:id-123");
    }

    #endregion
}
