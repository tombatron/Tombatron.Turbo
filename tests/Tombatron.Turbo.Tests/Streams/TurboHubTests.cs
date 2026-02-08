using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Tombatron.Turbo.Streams;
using Xunit;

namespace Tombatron.Turbo.Tests.Streams;

/// <summary>
/// Tests for the TurboHub class.
/// </summary>
public class TurboHubTests
{
    private readonly Mock<ITurboStreamAuthorization> _mockAuthorization;
    private readonly TurboOptions _options;
    private readonly Mock<ILogger<TurboHub>> _mockLogger;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<HubCallerContext> _mockContext;

    public TurboHubTests()
    {
        _mockAuthorization = new Mock<ITurboStreamAuthorization>();
        _options = new TurboOptions();
        _mockLogger = new Mock<ILogger<TurboHub>>();
        _mockGroups = new Mock<IGroupManager>();
        _mockContext = new Mock<HubCallerContext>();

        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal());
    }

    private TurboHub CreateHub()
    {
        var hub = new TurboHub(_mockAuthorization.Object, _options, _mockLogger.Object);

        // Use reflection to set the protected properties
        var contextProperty = typeof(Hub).GetProperty("Context");
        contextProperty?.SetValue(hub, _mockContext.Object);

        var groupsProperty = typeof(Hub).GetProperty("Groups");
        groupsProperty?.SetValue(hub, _mockGroups.Object);

        return hub;
    }

    [Fact]
    public async Task Subscribe_WithValidStreamName_ReturnsTrue()
    {
        // Arrange
        _mockAuthorization.Setup(a => a.CanSubscribe(It.IsAny<ClaimsPrincipal?>(), "test-stream"))
            .Returns(true);
        var hub = CreateHub();

        // Act
        bool result = await hub.Subscribe("test-stream");

        // Assert
        result.Should().BeTrue();
        _mockGroups.Verify(g => g.AddToGroupAsync("test-connection-id", "test-stream", default), Times.Once);
    }

    [Fact]
    public async Task Subscribe_WithUnauthorizedUser_ReturnsFalse()
    {
        // Arrange
        _mockAuthorization.Setup(a => a.CanSubscribe(It.IsAny<ClaimsPrincipal?>(), "private-stream"))
            .Returns(false);
        var hub = CreateHub();

        // Act
        bool result = await hub.Subscribe("private-stream");

        // Assert
        result.Should().BeFalse();
        _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Subscribe_WithNullStreamName_ThrowsArgumentNullException()
    {
        // Arrange
        var hub = CreateHub();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => hub.Subscribe(null!));
    }

    [Fact]
    public async Task Subscribe_WithEmptyStreamName_ThrowsArgumentException()
    {
        // Arrange
        var hub = CreateHub();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => hub.Subscribe(""));
    }

    [Fact]
    public async Task Subscribe_WithWhitespaceStreamName_ThrowsArgumentException()
    {
        // Arrange
        var hub = CreateHub();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => hub.Subscribe("   "));
    }

    [Fact]
    public async Task Unsubscribe_WithValidStreamName_RemovesFromGroup()
    {
        // Arrange
        var hub = CreateHub();

        // Act
        await hub.Unsubscribe("test-stream");

        // Assert
        _mockGroups.Verify(g => g.RemoveFromGroupAsync("test-connection-id", "test-stream", default), Times.Once);
    }

    [Fact]
    public async Task Unsubscribe_WithNullStreamName_ThrowsArgumentNullException()
    {
        // Arrange
        var hub = CreateHub();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => hub.Unsubscribe(null!));
    }

    [Fact]
    public async Task Unsubscribe_WithEmptyStreamName_ThrowsArgumentException()
    {
        // Arrange
        var hub = CreateHub();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => hub.Unsubscribe(""));
    }

    [Fact]
    public async Task Unsubscribe_WithWhitespaceStreamName_ThrowsArgumentException()
    {
        // Arrange
        var hub = CreateHub();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => hub.Unsubscribe("   "));
    }

    [Fact]
    public async Task OnConnectedAsync_CompletesSuccessfully()
    {
        // Arrange
        var hub = CreateHub();

        // Act & Assert - Should not throw
        await hub.OnConnectedAsync();
    }

    [Fact]
    public async Task OnDisconnectedAsync_WithException_CompletesSuccessfully()
    {
        // Arrange
        var hub = CreateHub();
        var exception = new Exception("Test exception");

        // Act & Assert - Should not throw
        await hub.OnDisconnectedAsync(exception);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WithoutException_CompletesSuccessfully()
    {
        // Arrange
        var hub = CreateHub();

        // Act & Assert - Should not throw
        await hub.OnDisconnectedAsync(null);
    }

    [Fact]
    public void Constructor_WithNullAuthorization_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboHub(null!, _options, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboHub(_mockAuthorization.Object, null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TurboHub(_mockAuthorization.Object, _options, null!));
    }

    [Fact]
    public void TurboStreamMethod_HasCorrectValue()
    {
        // Assert
        TurboHub.TurboStreamMethod.Should().Be("TurboStream");
    }

    [Fact]
    public void ValidateStreamName_WithValidName_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        TurboHub.ValidateStreamName("valid-stream-name");
    }

    [Fact]
    public void ValidateStreamName_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => TurboHub.ValidateStreamName(null!));
    }

    [Fact]
    public void ValidateStreamName_WithEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => TurboHub.ValidateStreamName(""));
    }

    [Fact]
    public void ValidateStreamName_WithWhitespace_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => TurboHub.ValidateStreamName("   "));
    }

    [Fact]
    public async Task Subscribe_PassesCorrectUserToAuthorization()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(user);
        _mockAuthorization.Setup(a => a.CanSubscribe(It.IsAny<ClaimsPrincipal?>(), "test-stream"))
            .Returns(true);

        var hub = CreateHub();

        // Act
        await hub.Subscribe("test-stream");

        // Assert
        _mockAuthorization.Verify(a => a.CanSubscribe(
            It.Is<ClaimsPrincipal?>(u => VerifyUserHasClaim(u, ClaimTypes.NameIdentifier, "user-123")),
            "test-stream"), Times.Once);
    }

    private static bool VerifyUserHasClaim(ClaimsPrincipal? user, string claimType, string expectedValue)
    {
        if (user == null)
        {
            return false;
        }

        var claim = user.FindFirst(claimType);
        return claim != null && claim.Value == expectedValue;
    }
}
