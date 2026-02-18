using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tombatron.Turbo.Rendering;
using Tombatron.Turbo.Results;
using Xunit;

namespace Tombatron.Turbo.Tests.Results;

public class TurboPartialResultTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldResolveRendererAndWriteHtml()
    {
        // Arrange
        var expectedHtml = "<div>Hello, World!</div>";
        var mockRenderer = new Mock<IPartialRenderer>();
        mockRenderer
            .Setup(r => r.RenderAsync("_TestPartial", null))
            .ReturnsAsync(expectedHtml);

        var httpContext = CreateHttpContext(mockRenderer.Object);
        var result = new TurboPartialResult("_TestPartial");

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.ContentType.Should().Be("text/html; charset=utf-8");
        var body = await ReadResponseBody(httpContext);
        body.Should().Be(expectedHtml);
    }

    [Fact]
    public async Task ExecuteAsync_WithModel_ShouldPassModelToRenderer()
    {
        // Arrange
        var model = new { Name = "Test" };
        var expectedHtml = "<div>Test</div>";
        var mockRenderer = new Mock<IPartialRenderer>();
        mockRenderer
            .Setup(r => r.RenderAsync("_TestPartial", It.IsAny<object?>()))
            .ReturnsAsync(expectedHtml);

        var httpContext = CreateHttpContext(mockRenderer.Object);
        var result = new TurboPartialResult("_TestPartial", model);

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        var body = await ReadResponseBody(httpContext);
        body.Should().Be(expectedHtml);
        mockRenderer.Verify(r => r.RenderAsync("_TestPartial", It.Is<object?>(m => m == model)), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetContentTypeToTextHtml()
    {
        // Arrange
        var mockRenderer = new Mock<IPartialRenderer>();
        mockRenderer
            .Setup(r => r.RenderAsync(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync(string.Empty);

        var httpContext = CreateHttpContext(mockRenderer.Object);
        var result = new TurboPartialResult("_TestPartial");

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.ContentType.Should().Be("text/html; charset=utf-8");
    }

    [Fact]
    public async Task ExecuteAsync_WhenRendererNotRegistered_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection().BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = services };
        var result = new TurboPartialResult("_TestPartial");

        // Act
        Func<Task> act = () => result.ExecuteAsync(httpContext);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithStatusCode422_ShouldSet422StatusCode()
    {
        // Arrange
        var mockRenderer = new Mock<IPartialRenderer>();
        mockRenderer
            .Setup(r => r.RenderAsync(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync("<div>Errors</div>");

        var httpContext = CreateHttpContext(mockRenderer.Object);
        var result = new TurboPartialResult("_TestPartial", statusCode: 422);

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task ExecuteAsync_WithStatusCode422_ShouldStillRenderHtml()
    {
        // Arrange
        var expectedHtml = "<div>Validation errors here</div>";
        var mockRenderer = new Mock<IPartialRenderer>();
        mockRenderer
            .Setup(r => r.RenderAsync("_ErrorForm", null))
            .ReturnsAsync(expectedHtml);

        var httpContext = CreateHttpContext(mockRenderer.Object);
        var result = new TurboPartialResult("_ErrorForm", statusCode: 422);

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.ContentType.Should().Be("text/html; charset=utf-8");
        var body = await ReadResponseBody(httpContext);
        body.Should().Be(expectedHtml);
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultStatusCode_ShouldSet200StatusCode()
    {
        // Arrange
        var mockRenderer = new Mock<IPartialRenderer>();
        mockRenderer
            .Setup(r => r.RenderAsync(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync(string.Empty);

        var httpContext = CreateHttpContext(mockRenderer.Object);
        var result = new TurboPartialResult("_TestPartial");

        // Act
        await result.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(200);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_WithNullOrEmptyPartialName_ShouldThrow(string? partialName)
    {
        // Act
        Action act = () => new TurboPartialResult(partialName!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    private static DefaultHttpContext CreateHttpContext(IPartialRenderer renderer)
    {
        var services = new ServiceCollection();
        services.AddSingleton(renderer);
        var serviceProvider = services.BuildServiceProvider();

        return new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Response = { Body = new MemoryStream() }
        };
    }

    private static async Task<string> ReadResponseBody(DefaultHttpContext httpContext)
    {
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
