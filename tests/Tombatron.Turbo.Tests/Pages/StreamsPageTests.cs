using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Tombatron.Turbo;
using Tombatron.Turbo.Streams;
using Xunit;

namespace Tombatron.Turbo.Tests.Pages;

/// <summary>
/// Tests for the Streams demo page model.
/// These tests verify that the page handlers correctly interact with ITurbo
/// and return proper responses for Turbo.js compatibility.
/// </summary>
public class StreamsPageTests
{
    private readonly Mock<ITurbo> _mockTurbo;

    public StreamsPageTests()
    {
        _mockTurbo = new Mock<ITurbo>();

        // Setup the Stream method to complete successfully
        _mockTurbo
            .Setup(t => t.Stream(It.IsAny<string>(), It.IsAny<Action<ITurboStreamBuilder>>()))
            .Returns(Task.CompletedTask);

        _mockTurbo
            .Setup(t => t.Broadcast(It.IsAny<Action<ITurboStreamBuilder>>()))
            .Returns(Task.CompletedTask);
    }

    private StreamsIndexModel CreatePageModel()
    {
        var pageModel = new StreamsIndexModel(_mockTurbo.Object);

        // Setup HttpContext with session
        var httpContext = new DefaultHttpContext();
        var session = new MockSession();
        httpContext.Session = session;

        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        return pageModel;
    }

    #region OnPostSendNotification Tests

    [Fact]
    public async Task OnPostSendNotification_ReturnsNoContent()
    {
        // Arrange
        var pageModel = CreatePageModel();

        // Act
        IActionResult result = await pageModel.OnPostSendNotification("Test message");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task OnPostSendNotification_CallsStreamWithCorrectStreamName()
    {
        // Arrange
        var pageModel = CreatePageModel();

        // Act
        await pageModel.OnPostSendNotification("Test message");

        // Assert
        _mockTurbo.Verify(t => t.Stream("demo-notifications", It.IsAny<Action<ITurboStreamBuilder>>()), Times.Once);
    }

    [Fact]
    public async Task OnPostSendNotification_WithNullMessage_UsesDefaultMessage()
    {
        // Arrange
        var pageModel = CreatePageModel();
        Action<ITurboStreamBuilder>? capturedAction = null;

        _mockTurbo
            .Setup(t => t.Stream("demo-notifications", It.IsAny<Action<ITurboStreamBuilder>>()))
            .Callback<string, Action<ITurboStreamBuilder>>((_, action) => capturedAction = action)
            .Returns(Task.CompletedTask);

        // Act
        await pageModel.OnPostSendNotification(null!);

        // Assert
        capturedAction.Should().NotBeNull();

        // Execute the captured action to verify the content
        var builder = new TurboStreamBuilder();
        capturedAction!(builder);
        string html = builder.Build();

        html.Should().Contain("Hello from the server!");
    }

    [Fact]
    public async Task OnPostSendNotification_EscapesHtmlInMessage()
    {
        // Arrange
        var pageModel = CreatePageModel();
        Action<ITurboStreamBuilder>? capturedAction = null;

        _mockTurbo
            .Setup(t => t.Stream("demo-notifications", It.IsAny<Action<ITurboStreamBuilder>>()))
            .Callback<string, Action<ITurboStreamBuilder>>((_, action) => capturedAction = action)
            .Returns(Task.CompletedTask);

        // Act
        await pageModel.OnPostSendNotification("<script>alert('xss')</script>");

        // Assert
        capturedAction.Should().NotBeNull();

        var builder = new TurboStreamBuilder();
        capturedAction!(builder);
        string html = builder.Build();

        // Should be escaped, not contain raw script tag
        html.Should().NotContain("<script>");
        html.Should().Contain("&lt;script&gt;");
    }

    #endregion

    #region OnPostIncrementCounter Tests

    [Fact]
    public async Task OnPostIncrementCounter_ReturnsNoContent()
    {
        // Arrange
        var pageModel = CreatePageModel();

        // Act
        IActionResult result = await pageModel.OnPostIncrementCounter();

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task OnPostIncrementCounter_IncrementsSessionCounter()
    {
        // Arrange
        var pageModel = CreatePageModel();

        // Act
        await pageModel.OnPostIncrementCounter();
        await pageModel.OnPostIncrementCounter();
        await pageModel.OnPostIncrementCounter();

        // Assert - verify the session was updated
        int? counter = pageModel.HttpContext.Session.GetInt32("counter");
        counter.Should().Be(3);
    }

    [Fact]
    public async Task OnPostIncrementCounter_BroadcastsUpdatedCounter()
    {
        // Arrange
        var pageModel = CreatePageModel();
        Action<ITurboStreamBuilder>? capturedAction = null;

        _mockTurbo
            .Setup(t => t.Stream("demo-notifications", It.IsAny<Action<ITurboStreamBuilder>>()))
            .Callback<string, Action<ITurboStreamBuilder>>((_, action) => capturedAction = action)
            .Returns(Task.CompletedTask);

        // Act
        await pageModel.OnPostIncrementCounter();

        // Assert
        capturedAction.Should().NotBeNull();

        var builder = new TurboStreamBuilder();
        capturedAction!(builder);
        string html = builder.Build();

        html.Should().Contain("action=\"update\"");
        html.Should().Contain("target=\"counter-value\"");
        html.Should().Contain("<strong>1</strong>");
    }

    #endregion

    #region OnPostBroadcast Tests

    [Fact]
    public async Task OnPostBroadcast_ReturnsNoContent()
    {
        // Arrange
        var pageModel = CreatePageModel();

        // Act
        IActionResult result = await pageModel.OnPostBroadcast("Test broadcast");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task OnPostBroadcast_CallsBroadcast()
    {
        // Arrange
        var pageModel = CreatePageModel();

        // Act
        await pageModel.OnPostBroadcast("Test broadcast");

        // Assert
        _mockTurbo.Verify(t => t.Broadcast(It.IsAny<Action<ITurboStreamBuilder>>()), Times.Once);
    }

    [Fact]
    public async Task OnPostBroadcast_WithNullMessage_UsesDefaultMessage()
    {
        // Arrange
        var pageModel = CreatePageModel();
        Action<ITurboStreamBuilder>? capturedAction = null;

        _mockTurbo
            .Setup(t => t.Broadcast(It.IsAny<Action<ITurboStreamBuilder>>()))
            .Callback<Action<ITurboStreamBuilder>>(action => capturedAction = action)
            .Returns(Task.CompletedTask);

        // Act
        await pageModel.OnPostBroadcast(null!);

        // Assert
        capturedAction.Should().NotBeNull();

        var builder = new TurboStreamBuilder();
        capturedAction!(builder);
        string html = builder.Build();

        html.Should().Contain("Broadcast message to all clients!");
    }

    #endregion

    #region OnPostClearNotifications Tests

    [Fact]
    public async Task OnPostClearNotifications_ReturnsNoContent()
    {
        // Arrange
        var pageModel = CreatePageModel();

        // Act
        IActionResult result = await pageModel.OnPostClearNotifications();

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task OnPostClearNotifications_UpdatesWithEmptyContent()
    {
        // Arrange
        var pageModel = CreatePageModel();
        Action<ITurboStreamBuilder>? capturedAction = null;

        _mockTurbo
            .Setup(t => t.Stream("demo-notifications", It.IsAny<Action<ITurboStreamBuilder>>()))
            .Callback<string, Action<ITurboStreamBuilder>>((_, action) => capturedAction = action)
            .Returns(Task.CompletedTask);

        // Act
        await pageModel.OnPostClearNotifications();

        // Assert
        capturedAction.Should().NotBeNull();

        var builder = new TurboStreamBuilder();
        capturedAction!(builder);
        string html = builder.Build();

        html.Should().Contain("action=\"update\"");
        html.Should().Contain("target=\"notification-list\"");
        html.Should().Contain("<template></template>");
    }

    #endregion

    /// <summary>
    /// Mock session implementation for testing.
    /// </summary>
    private class MockSession : ISession
    {
        private readonly Dictionary<string, byte[]> _data = new();

        public string Id => "test-session-id";
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _data.Keys;

        public void Clear() => _data.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _data.Remove(key);

        public void Set(string key, byte[] value) => _data[key] = value;

        public bool TryGetValue(string key, out byte[] value)
        {
            bool found = _data.TryGetValue(key, out byte[]? result);
            value = result ?? Array.Empty<byte>();
            return found;
        }
    }
}

/// <summary>
/// Page model for the Streams demo page - matches the sample app implementation.
/// This is a simplified version for testing purposes.
/// </summary>
public class StreamsIndexModel : PageModel
{
    private readonly ITurbo _turbo;

    public StreamsIndexModel(ITurbo turbo)
    {
        _turbo = turbo;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostSendNotification(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "Hello from the server!";
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        await _turbo.Stream("demo-notifications", builder =>
        {
            builder.Append("notification-list",
                $"<div class=\"stream-notification\"><strong>{timestamp}</strong>: {System.Net.WebUtility.HtmlEncode(message)}</div>");
        });

        return new NoContentResult();
    }

    public async Task<IActionResult> OnPostIncrementCounter()
    {
        int counter = HttpContext.Session.GetInt32("counter") ?? 0;
        counter++;
        HttpContext.Session.SetInt32("counter", counter);

        await _turbo.Stream("demo-notifications", builder =>
        {
            builder.Update("counter-value", $"<strong>{counter}</strong>");
        });

        return new NoContentResult();
    }

    public async Task<IActionResult> OnPostBroadcast(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "Broadcast message to all clients!";
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        await _turbo.Broadcast(builder =>
        {
            builder.Append("broadcast-list",
                $"<div class=\"stream-notification\"><strong>[{timestamp}] Broadcast:</strong> {System.Net.WebUtility.HtmlEncode(message)}</div>");
        });

        return new NoContentResult();
    }

    public async Task<IActionResult> OnPostClearNotifications()
    {
        await _turbo.Stream("demo-notifications", builder =>
        {
            builder.Update("notification-list", "");
        });

        return new NoContentResult();
    }
}
