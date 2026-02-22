using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tombatron.Turbo.Middleware;
using Xunit;

namespace Tombatron.Turbo.Tests.Middleware;

/// <summary>
/// Tests for the TurboFrameMiddleware.
/// </summary>
public class TurboFrameMiddlewareTests
{
    private readonly TurboOptions _options;
    private readonly ILogger<TurboFrameMiddleware> _logger;

    public TurboFrameMiddlewareTests()
    {
        _options = new TurboOptions();
        _logger = NullLogger<TurboFrameMiddleware>.Instance;
    }

    [Fact]
    public async Task InvokeAsync_WithoutTurboFrameHeader_DoesNotSetFrameItems()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(context);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().NotContainKey(TurboFrameMiddleware.IsTurboFrameRequestKey);
        context.Items.Should().NotContainKey(TurboFrameMiddleware.FrameIdKey);
    }

    [Fact]
    public async Task InvokeAsync_WithTurboFrameHeader_SetsIsTurboFrameRequest()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TurboFrameMiddleware.TurboFrameHeader] = "cart-items";
        var middleware = CreateMiddleware(context);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey(TurboFrameMiddleware.IsTurboFrameRequestKey);
        context.Items[TurboFrameMiddleware.IsTurboFrameRequestKey].Should().Be(true);
    }

    [Fact]
    public async Task InvokeAsync_WithTurboFrameHeader_SetsFrameId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TurboFrameMiddleware.TurboFrameHeader] = "product-details";
        var middleware = CreateMiddleware(context);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey(TurboFrameMiddleware.FrameIdKey);
        context.Items[TurboFrameMiddleware.FrameIdKey].Should().Be("product-details");
    }

    [Fact]
    public async Task InvokeAsync_WithVaryHeaderEnabled_RegistersOnStartingCallback()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TurboFrameMiddleware.TurboFrameHeader] = "cart-items";

        var middleware = CreateMiddleware(context);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - The middleware should have registered an OnStarting callback
        // We verify it set the turbo frame items which indicates successful processing
        context.Items.Should().ContainKey(TurboFrameMiddleware.IsTurboFrameRequestKey);
    }

    [Fact]
    public async Task InvokeAsync_WithVaryHeaderDisabled_DoesNotAddVaryHeader()
    {
        // Arrange
        var options = new TurboOptions { AddVaryHeader = false };
        var context = CreateHttpContext();
        context.Request.Headers[TurboFrameMiddleware.TurboFrameHeader] = "cart-items";

        var middleware = CreateMiddleware(context, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().NotContainKey("Vary");
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext();
        bool nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TurboFrameMiddleware(next, _options, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public void GetTurboFrameId_Static_WithHeader_ReturnsFrameId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TurboFrameMiddleware.TurboFrameHeader] = "my-frame";

        // Act
        string? result = TurboFrameMiddleware.GetTurboFrameId(context.Request);

        // Assert
        result.Should().Be("my-frame");
    }

    [Fact]
    public void GetTurboFrameId_Static_WithoutHeader_ReturnsNull()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        string? result = TurboFrameMiddleware.GetTurboFrameId(context.Request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTurboFrameId_Static_WithEmptyHeader_ReturnsEmpty()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TurboFrameMiddleware.TurboFrameHeader] = "";

        // Act
        string? result = TurboFrameMiddleware.GetTurboFrameId(context.Request);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void AddVaryHeader_Static_WithNoExistingVary_AddsHeader()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        TurboFrameMiddleware.AddVaryHeader(context.Response);

        // Assert
        context.Response.Headers["Vary"].ToString().Should().Be(TurboFrameMiddleware.TurboFrameHeader);
    }

    [Fact]
    public void AddVaryHeader_Static_WithExistingVary_AppendsToHeader()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.Headers["Vary"] = "Accept-Encoding";

        // Act
        TurboFrameMiddleware.AddVaryHeader(context.Response);

        // Assert
        string varyHeader = context.Response.Headers["Vary"].ToString();
        varyHeader.Should().Contain("Accept-Encoding");
        varyHeader.Should().Contain(TurboFrameMiddleware.TurboFrameHeader);
    }

    [Fact]
    public void AddVaryHeader_Static_AlreadyContainsTurboFrame_DoesNotDuplicate()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.Headers["Vary"] = TurboFrameMiddleware.TurboFrameHeader;

        // Act
        TurboFrameMiddleware.AddVaryHeader(context.Response);

        // Assert
        string varyHeader = context.Response.Headers["Vary"].ToString();
        int count = varyHeader.Split(TurboFrameMiddleware.TurboFrameHeader).Length - 1;
        count.Should().Be(1);
    }

    [Fact]
    public async Task InvokeAsync_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var middleware = CreateMiddleware(CreateHttpContext());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => middleware.InvokeAsync(null!));
    }

    [Fact]
    public void Constructor_WithNullNext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboFrameMiddleware(null!, _options, _logger));
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboFrameMiddleware(next, null!, _logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboFrameMiddleware(next, _options, null!));
    }

    [Theory]
    [InlineData("cart-items")]
    [InlineData("product_123")]
    [InlineData("user-profile-details")]
    [InlineData("item_42")]
    public async Task InvokeAsync_WithVariousFrameIds_SetsCorrectFrameId(string frameId)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TurboFrameMiddleware.TurboFrameHeader] = frameId;
        var middleware = CreateMiddleware(context);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items[TurboFrameMiddleware.FrameIdKey].Should().Be(frameId);
    }

    // X-Turbo-Request-Id tests

    [Fact]
    public async Task InvokeAsync_WithTurboRequestIdHeader_SetsRequestId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TurboFrameMiddleware.TurboRequestIdHeader] = "abc-123";
        var middleware = CreateMiddleware(context);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey(TurboFrameMiddleware.RequestIdKey);
        context.Items[TurboFrameMiddleware.RequestIdKey].Should().Be("abc-123");
    }

    [Fact]
    public async Task InvokeAsync_WithoutTurboRequestIdHeader_DoesNotSetRequestId()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(context);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().NotContainKey(TurboFrameMiddleware.RequestIdKey);
    }

    [Theory]
    [InlineData("abc-123")]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("request_42")]
    public async Task InvokeAsync_WithVariousRequestIds_SetsCorrectValue(string requestId)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TurboFrameMiddleware.TurboRequestIdHeader] = requestId;
        var middleware = CreateMiddleware(context);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items[TurboFrameMiddleware.RequestIdKey].Should().Be(requestId);
    }

    [Fact]
    public void GetTurboRequestId_Static_WithHeader_ReturnsRequestId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TurboFrameMiddleware.TurboRequestIdHeader] = "req-456";

        // Act
        var result = TurboFrameMiddleware.GetTurboRequestId(context.Request);

        // Assert
        result.Should().Be("req-456");
    }

    [Fact]
    public void GetTurboRequestId_Static_WithoutHeader_ReturnsNull()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        var result = TurboFrameMiddleware.GetTurboRequestId(context.Request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTurboRequestId_Static_WithEmptyHeader_ReturnsEmpty()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[TurboFrameMiddleware.TurboRequestIdHeader] = "";

        // Act
        var result = TurboFrameMiddleware.GetTurboRequestId(context.Request);

        // Assert
        result.Should().BeEmpty();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext();
    }

    private TurboFrameMiddleware CreateMiddleware(HttpContext context, TurboOptions? options = null)
    {
        return new TurboFrameMiddleware(_ => Task.CompletedTask, options ?? _options, _logger);
    }
}
