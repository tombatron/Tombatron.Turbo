using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Tombatron.Turbo.Middleware;
using Xunit;

namespace Tombatron.Turbo.Tests;

/// <summary>
/// Tests for the TurboHttpContextExtensions.
/// </summary>
public class TurboHttpContextExtensionsTests
{
    [Fact]
    public void IsTurboFrameRequest_WhenTrue_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.IsTurboFrameRequestKey] = true;

        // Act
        bool result = context.IsTurboFrameRequest();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTurboFrameRequest_WhenFalse_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.IsTurboFrameRequestKey] = false;

        // Act
        bool result = context.IsTurboFrameRequest();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTurboFrameRequest_WhenNotSet_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        bool result = context.IsTurboFrameRequest();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTurboFrameRequest_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        HttpContext? context = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context!.IsTurboFrameRequest());
    }

    [Fact]
    public void GetTurboFrameId_WhenSet_ReturnsFrameId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.FrameIdKey] = "cart-items";

        // Act
        string? result = context.GetTurboFrameId();

        // Assert
        result.Should().Be("cart-items");
    }

    [Fact]
    public void GetTurboFrameId_WhenNotSet_ReturnsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        string? result = context.GetTurboFrameId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTurboFrameId_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        HttpContext? context = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context!.GetTurboFrameId());
    }

    [Fact]
    public void IsTurboFrameRequest_WithMatchingFrameId_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.FrameIdKey] = "cart-items";

        // Act
        bool result = context.IsTurboFrameRequest("cart-items");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTurboFrameRequest_WithNonMatchingFrameId_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.FrameIdKey] = "cart-items";

        // Act
        bool result = context.IsTurboFrameRequest("product-list");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTurboFrameRequest_WithEmptyFrameId_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.FrameIdKey] = "cart-items";

        // Act
        bool result = context.IsTurboFrameRequest("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTurboFrameRequest_WithNullFrameId_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.FrameIdKey] = "cart-items";

        // Act
        bool result = context.IsTurboFrameRequest(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTurboFrameRequestWithPrefix_WithMatchingPrefix_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.FrameIdKey] = "item_123";

        // Act
        bool result = context.IsTurboFrameRequestWithPrefix("item_");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTurboFrameRequestWithPrefix_WithNonMatchingPrefix_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.FrameIdKey] = "item_123";

        // Act
        bool result = context.IsTurboFrameRequestWithPrefix("product_");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTurboFrameRequestWithPrefix_WithEmptyPrefix_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.FrameIdKey] = "item_123";

        // Act
        bool result = context.IsTurboFrameRequestWithPrefix("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTurboFrameRequestWithPrefix_WithNoFrameId_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        bool result = context.IsTurboFrameRequestWithPrefix("item_");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("item_1", "item_", true)]
    [InlineData("item_42", "item_", true)]
    [InlineData("item_", "item_", true)]
    [InlineData("product_123", "item_", false)]
    [InlineData("item123", "item_", false)]
    public void IsTurboFrameRequestWithPrefix_VariousCases(string frameId, string prefix, bool expected)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.FrameIdKey] = frameId;

        // Act
        bool result = context.IsTurboFrameRequestWithPrefix(prefix);

        // Assert
        result.Should().Be(expected);
    }

    // GetTurboRequestId tests

    [Fact]
    public void GetTurboRequestId_WhenSet_ReturnsRequestId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[TurboFrameMiddleware.RequestIdKey] = "abc-123";

        // Act
        var result = context.GetTurboRequestId();

        // Assert
        result.Should().Be("abc-123");
    }

    [Fact]
    public void GetTurboRequestId_WhenNotSet_ReturnsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = context.GetTurboRequestId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTurboRequestId_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        HttpContext? context = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context!.GetTurboRequestId());
    }

    [Fact]
    public void IsTurboStreamRequest_WhenAcceptHeaderContainsTurboStream_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Accept = "text/vnd.turbo-stream.html";

        // Act
        bool result = context.IsTurboStreamRequest();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTurboStreamRequest_WhenAcceptHeaderMissing_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        bool result = context.IsTurboStreamRequest();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTurboStreamRequest_WithMixedAcceptHeader_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Accept = "text/html, application/xhtml+xml, text/vnd.turbo-stream.html";

        // Act
        bool result = context.IsTurboStreamRequest();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTurboStreamRequest_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        HttpContext? context = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => context!.IsTurboStreamRequest());
    }
}
