using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Tombatron.Turbo.Middleware;
using Tombatron.Turbo.Rendering;
using Tombatron.Turbo.Streams;
using Xunit;

namespace Tombatron.Turbo.Tests.Streams;

/// <summary>
/// Tests for the TurboService class.
/// </summary>
public class TurboServiceTests
{
    private readonly Mock<IHubContext<TurboHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<ILogger<TurboService>> _mockLogger;
    private readonly Mock<IPartialRenderer> _mockPartialRenderer;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly TurboService _service;

    public TurboServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<TurboHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockLogger = new Mock<ILogger<TurboService>>();
        _mockPartialRenderer = new Mock<IPartialRenderer>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);

        _service = new TurboService(_mockHubContext.Object, _mockLogger.Object, _mockPartialRenderer.Object, _mockHttpContextAccessor.Object);
    }

    [Fact]
    public async Task Stream_WithValidStreamName_BroadcastsToGroup()
    {
        // Arrange
        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.Stream("test-stream", builder => builder.Append("target", "<div>Content</div>"));

        // Assert
        _mockClients.Verify(c => c.Group("test-stream"), Times.Once);
        capturedHtml.Should().Contain("action=\"append\"");
        capturedHtml.Should().Contain("target=\"target\"");
        capturedHtml.Should().Contain("<div>Content</div>");
    }

    [Fact]
    public async Task Stream_WithNullStreamName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.Stream((string)null!, builder => builder.Append("target", "<div>Content</div>")));
    }

    [Fact]
    public async Task Stream_WithEmptyStreamName_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.Stream("", builder => builder.Append("target", "<div>Content</div>")));
    }

    [Fact]
    public async Task Stream_WithWhitespaceStreamName_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.Stream("   ", builder => builder.Append("target", "<div>Content</div>")));
    }

    [Fact]
    public async Task Stream_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.Stream("test-stream", null!));
    }

    [Fact]
    public async Task Stream_WithNoActions_DoesNotBroadcast()
    {
        // Act
        await _service.Stream("test-stream", _ => { });

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), default),
            Times.Never);
    }

    [Fact]
    public async Task Stream_WithMultipleStreamNames_BroadcastsToAllGroups()
    {
        // Arrange
        var streamNames = new[] { "stream-1", "stream-2", "stream-3" };

        // Act
        await _service.Stream(streamNames, builder => builder.Append("target", "<div>Content</div>"));

        // Assert
        foreach (string streamName in streamNames)
        {
            _mockClients.Verify(c => c.Group(streamName), Times.Once);
        }
    }

    [Fact]
    public async Task Stream_WithNullStreamNames_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.Stream((IEnumerable<string>)null!, builder => builder.Append("target", "<div>Content</div>")));
    }

    [Fact]
    public async Task Stream_WithEmptyStreamNames_DoesNotBroadcast()
    {
        // Arrange
        var streamNames = Array.Empty<string>();

        // Act
        await _service.Stream(streamNames, builder => builder.Append("target", "<div>Content</div>"));

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), default),
            Times.Never);
    }

    [Fact]
    public async Task Stream_WithNullInStreamNames_ThrowsArgumentException()
    {
        // Arrange
        var streamNames = new[] { "stream-1", null!, "stream-2" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.Stream(streamNames, builder => builder.Append("target", "<div>Content</div>")));
    }

    [Fact]
    public async Task Stream_WithEmptyStringInStreamNames_ThrowsArgumentException()
    {
        // Arrange
        var streamNames = new[] { "stream-1", "", "stream-2" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.Stream(streamNames, builder => builder.Append("target", "<div>Content</div>")));
    }

    [Fact]
    public async Task Broadcast_SendsToAllClients()
    {
        // Arrange
        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.Broadcast(builder => builder.Update("announcement", "<p>Hello everyone!</p>"));

        // Assert
        _mockClients.Verify(c => c.All, Times.Once);
        capturedHtml.Should().Contain("action=\"update\"");
        capturedHtml.Should().Contain("<p>Hello everyone!</p>");
    }

    [Fact]
    public async Task Broadcast_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.Broadcast(null!));
    }

    [Fact]
    public async Task Broadcast_WithNoActions_DoesNotBroadcast()
    {
        // Act
        await _service.Broadcast(_ => { });

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), default),
            Times.Never);
    }

    [Fact]
    public void Constructor_WithNullHubContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboService(null!, _mockLogger.Object, _mockPartialRenderer.Object, _mockHttpContextAccessor.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboService(_mockHubContext.Object, null!, _mockPartialRenderer.Object, _mockHttpContextAccessor.Object));
    }

    [Fact]
    public void Constructor_WithNullPartialRenderer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboService(_mockHubContext.Object, _mockLogger.Object, null!, _mockHttpContextAccessor.Object));
    }

    [Fact]
    public void Constructor_WithNullHttpContextAccessor_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboService(_mockHubContext.Object, _mockLogger.Object, _mockPartialRenderer.Object, null!));
    }

    [Fact]
    public async Task Stream_WithMultipleActions_BuildsAllActions()
    {
        // Arrange
        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.Stream("test-stream", builder =>
        {
            builder.Append("list", "<li>Item 1</li>");
            builder.Append("list", "<li>Item 2</li>");
            builder.Update("counter", "<span>2</span>");
        });

        // Assert
        capturedHtml.Should().Contain("<li>Item 1</li>");
        capturedHtml.Should().Contain("<li>Item 2</li>");
        capturedHtml.Should().Contain("<span>2</span>");
    }

    // Async overload tests

    [Fact]
    public async Task StreamAsync_WithValidStreamName_BroadcastsToGroup()
    {
        // Arrange
        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.Stream("test-stream", async builder =>
        {
            await Task.Yield();
            builder.Append("target", "<div>Content</div>");
        });

        // Assert
        _mockClients.Verify(c => c.Group("test-stream"), Times.Once);
        capturedHtml.Should().Contain("action=\"append\"");
        capturedHtml.Should().Contain("target=\"target\"");
        capturedHtml.Should().Contain("<div>Content</div>");
    }

    [Fact]
    public async Task StreamAsync_WithNullStreamName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.Stream((string)null!, async builder =>
            {
                await Task.Yield();
                builder.Append("target", "<div>Content</div>");
            }));
    }

    [Fact]
    public async Task StreamAsync_WithNullBuildAsync_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.Stream("test-stream", (Func<ITurboStreamBuilder, Task>)null!));
    }

    [Fact]
    public async Task StreamAsync_WithMultipleStreamNames_BroadcastsToAllGroups()
    {
        // Arrange
        var streamNames = new[] { "stream-1", "stream-2", "stream-3" };

        // Act
        await _service.Stream(streamNames, async builder =>
        {
            await Task.Yield();
            builder.Append("target", "<div>Content</div>");
        });

        // Assert
        foreach (string streamName in streamNames)
        {
            _mockClients.Verify(c => c.Group(streamName), Times.Once);
        }
    }

    [Fact]
    public async Task BroadcastAsync_SendsToAllClients()
    {
        // Arrange
        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.Broadcast(async builder =>
        {
            await Task.Yield();
            builder.Update("announcement", "<p>Hello everyone!</p>");
        });

        // Assert
        _mockClients.Verify(c => c.All, Times.Once);
        capturedHtml.Should().Contain("action=\"update\"");
        capturedHtml.Should().Contain("<p>Hello everyone!</p>");
    }

    [Fact]
    public async Task BroadcastAsync_WithNullBuildAsync_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.Broadcast((Func<ITurboStreamBuilder, Task>)null!));
    }

    [Fact]
    public async Task StreamAsync_SetsRendererOnBuilder()
    {
        // Arrange
        _mockPartialRenderer
            .Setup(r => r.RenderAsync<string>(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("<div>Rendered</div>");

        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.Stream("test-stream", async builder =>
        {
            // Use the extension method that requires the renderer
            var template = new PartialTemplate<string>("Pages/Shared/_Test.cshtml", "Test");
            await builder.AppendAsync("target", template, "model");
        });

        // Assert
        _mockPartialRenderer.Verify(r => r.RenderAsync<string>("Pages/Shared/_Test.cshtml", "model"), Times.Once);
        capturedHtml.Should().Contain("<div>Rendered</div>");
    }

    // StreamRefresh / BroadcastRefresh tests

    [Fact]
    public async Task StreamRefresh_WithRequestIdInHttpContext_SendsRefreshWithRequestId()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[TurboFrameMiddleware.RequestIdKey] = "abc-123";
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.StreamRefresh("test-stream");

        // Assert
        _mockClients.Verify(c => c.Group("test-stream"), Times.Once);
        capturedHtml.Should().Be("<turbo-stream action=\"refresh\" request-id=\"abc-123\"></turbo-stream>");
    }

    [Fact]
    public async Task StreamRefresh_WithoutRequestIdInHttpContext_SendsRefreshWithoutRequestId()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.StreamRefresh("test-stream");

        // Assert
        capturedHtml.Should().Be("<turbo-stream action=\"refresh\"></turbo-stream>");
    }

    [Fact]
    public async Task StreamRefresh_WithNullHttpContext_SendsRefreshWithoutRequestId()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.StreamRefresh("test-stream");

        // Assert
        capturedHtml.Should().Be("<turbo-stream action=\"refresh\"></turbo-stream>");
    }

    [Fact]
    public async Task StreamRefresh_MultipleStreams_SendsToAllGroups()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[TurboFrameMiddleware.RequestIdKey] = "req-456";
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var streamNames = new[] { "stream-1", "stream-2" };

        // Act
        await _service.StreamRefresh(streamNames);

        // Assert
        foreach (var streamName in streamNames)
        {
            _mockClients.Verify(c => c.Group(streamName), Times.Once);
        }
    }

    [Fact]
    public async Task BroadcastRefresh_WithRequestIdInHttpContext_SendsRefreshWithRequestId()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[TurboFrameMiddleware.RequestIdKey] = "abc-123";
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.BroadcastRefresh();

        // Assert
        _mockClients.Verify(c => c.All, Times.Once);
        capturedHtml.Should().Be("<turbo-stream action=\"refresh\" request-id=\"abc-123\"></turbo-stream>");
    }

    [Fact]
    public async Task BroadcastRefresh_WithoutRequestIdInHttpContext_SendsRefreshWithoutRequestId()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.BroadcastRefresh();

        // Assert
        capturedHtml.Should().Be("<turbo-stream action=\"refresh\"></turbo-stream>");
    }

    [Fact]
    public async Task BroadcastRefresh_WithNullHttpContext_SendsRefreshWithoutRequestId()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        string? capturedHtml = null;
        _mockClientProxy
            .Setup(c => c.SendCoreAsync(TurboHub.TurboStreamMethod, It.IsAny<object?[]>(), default))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                capturedHtml = args[0] as string;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.BroadcastRefresh();

        // Assert
        capturedHtml.Should().Be("<turbo-stream action=\"refresh\"></turbo-stream>");
    }
}
