# Testing Guide

This guide covers strategies for testing applications that use Tombatron.Turbo.

## Testing Turbo Frames

### Unit Testing Page Handlers

Test that handlers correctly return partials for Turbo Frame requests:

```csharp
public class CartPageTests
{
    [Fact]
    public void OnGetRefresh_WithTurboFrameHeader_ReturnsPartial()
    {
        // Arrange
        var pageModel = new CartModel();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Turbo-Frame"] = "cart-items";

        // Set up HttpContext.Items as middleware would
        httpContext.Items["Turbo.IsTurboFrameRequest"] = true;
        httpContext.Items["Turbo.FrameId"] = "cart-items";

        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = pageModel.OnGetRefresh();

        // Assert
        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_CartItems", partialResult.ViewName);
    }

    [Fact]
    public void OnGetRefresh_WithoutTurboFrameHeader_RedirectsToPage()
    {
        // Arrange
        var pageModel = new CartModel();
        var httpContext = new DefaultHttpContext();
        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = pageModel.OnGetRefresh();

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
    }
}
```

### Testing with the Extensions

If using `HttpContext.IsTurboFrameRequest()`:

```csharp
[Fact]
public void IsTurboFrameRequest_WithHeader_ReturnsTrue()
{
    // Arrange
    var httpContext = new DefaultHttpContext();
    httpContext.Items[TurboFrameMiddleware.IsTurboFrameRequestKey] = true;

    // Act
    var result = httpContext.IsTurboFrameRequest();

    // Assert
    Assert.True(result);
}

[Fact]
public void IsTurboFrameRequest_WithSpecificFrame_MatchesCorrectly()
{
    // Arrange
    var httpContext = new DefaultHttpContext();
    httpContext.Items[TurboFrameMiddleware.IsTurboFrameRequestKey] = true;
    httpContext.Items[TurboFrameMiddleware.FrameIdKey] = "cart-items";

    // Act & Assert
    Assert.True(httpContext.IsTurboFrameRequest("cart-items"));
    Assert.False(httpContext.IsTurboFrameRequest("other-frame"));
}
```

## Testing Turbo Streams

### Unit Testing Stream Builder

```csharp
public class TurboStreamBuilderTests
{
    [Fact]
    public void Append_GeneratesCorrectHtml()
    {
        // Arrange
        var builder = new TurboStreamBuilder();

        // Act
        builder.Append("notifications", "<div>Hello</div>");
        var html = builder.Build();

        // Assert
        Assert.Contains("action=\"append\"", html);
        Assert.Contains("target=\"notifications\"", html);
        Assert.Contains("<div>Hello</div>", html);
    }

    [Fact]
    public void ChainedActions_GeneratesMultipleStreams()
    {
        // Arrange
        var builder = new TurboStreamBuilder();

        // Act
        builder
            .Update("count", "5")
            .Remove("old-item");
        var html = builder.Build();

        // Assert
        Assert.Contains("action=\"update\"", html);
        Assert.Contains("action=\"remove\"", html);
    }
}
```

### Testing ITurbo Service

Mock the `IHubContext` to test stream broadcasts:

```csharp
public class TurboServiceTests
{
    [Fact]
    public async Task Stream_SendsToCorrectGroup()
    {
        // Arrange
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClients
            .Setup(c => c.Group("notifications"))
            .Returns(mockClientProxy.Object);

        var mockHubContext = new Mock<IHubContext<TurboHub>>();
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        var service = new TurboService(mockHubContext.Object);

        // Act
        await service.Stream("notifications", b => b.Update("test", "value"));

        // Assert
        mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "TurboStream",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

### Testing Stream Authorization

```csharp
public class AuthorizationTests
{
    [Fact]
    public void CanSubscribe_UserOwnStream_ReturnsTrue()
    {
        // Arrange
        var auth = new UserStreamAuthorization();
        var user = CreateUserPrincipal("user123");

        // Act
        var result = auth.CanSubscribe(user, "user:user123");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanSubscribe_DifferentUserStream_ReturnsFalse()
    {
        // Arrange
        var auth = new UserStreamAuthorization();
        var user = CreateUserPrincipal("user123");

        // Act
        var result = auth.CanSubscribe(user, "user:different456");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("public:announcements", true)]
    [InlineData("user:123", false)]
    [InlineData("admin:dashboard", false)]
    public void CanSubscribe_AnonymousUser_AllowsOnlyPublic(string stream, bool expected)
    {
        // Arrange
        var auth = new UserStreamAuthorization();

        // Act
        var result = auth.CanSubscribe(null, stream);

        // Assert
        Assert.Equal(expected, result);
    }

    private ClaimsPrincipal CreateUserPrincipal(string userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }
}
```

## Integration Testing

### Testing with WebApplicationFactory

```csharp
public class TurboFrameIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TurboFrameIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TurboFrameRequest_ReturnsPartialContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Turbo-Frame", "cart-items");

        // Act
        var response = await client.GetAsync("/Cart?handler=Refresh");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("<turbo-frame id=\"cart-items\">", content);
        Assert.DoesNotContain("<html>", content); // Should be partial, not full page
    }

    [Fact]
    public async Task RegularRequest_ReturnsFullPage()
    {
        // Arrange
        var client = _factory.CreateClient();
        // No Turbo-Frame header

        // Act
        var response = await client.GetAsync("/Cart");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("<html>", content);
    }

    [Fact]
    public async Task VaryHeader_IsPresent()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Turbo-Frame", "test");

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("Vary"));
        Assert.Contains("Turbo-Frame", response.Headers.GetValues("Vary"));
    }
}
```

### Testing Turbo Stream Responses

```csharp
[Fact]
public async Task PostWithTurboStream_ReturnsStreamContent()
{
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Add("Accept", "text/vnd.turbo-stream.html");

    var content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("name", "Test Item"),
        new KeyValuePair<string, string>("price", "9.99")
    });

    // Act
    var response = await client.PostAsync("/Cart?handler=AddItem", content);

    // Assert
    response.EnsureSuccessStatusCode();
    Assert.Equal("text/vnd.turbo-stream.html", response.Content.Headers.ContentType?.MediaType);

    var body = await response.Content.ReadAsStringAsync();
    Assert.Contains("<turbo-stream", body);
}
```

## Testing Tag Helpers

```csharp
public class TurboFrameTagHelperTests
{
    [Fact]
    public void Process_SetsCorrectTagName()
    {
        // Arrange
        var tagHelper = new TurboFrameTagHelper();
        var context = CreateContext();
        var output = CreateOutput("turbo-frame");

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal("turbo-frame", output.TagName);
    }

    [Fact]
    public void Process_WithSrc_AddsSrcAttribute()
    {
        // Arrange
        var tagHelper = new TurboFrameTagHelper { Src = "/api/items" };
        var context = CreateContext();
        var output = CreateOutput("turbo-frame");

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Contains(output.Attributes, a =>
            a.Name == "src" && a.Value.ToString() == "/api/items");
    }

    private TagHelperContext CreateContext()
    {
        return new TagHelperContext(
            tagName: "turbo-frame",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());
    }

    private TagHelperOutput CreateOutput(string tagName)
    {
        return new TagHelperOutput(
            tagName: tagName,
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (_, _) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }
}
```

## Mocking Strategies

### Mocking ITurbo

```csharp
[Fact]
public async Task Controller_BroadcastsUpdate()
{
    // Arrange
    var mockTurbo = new Mock<ITurbo>();
    var controller = new CartController(mockTurbo.Object);

    // Act
    await controller.AddItem(123);

    // Assert
    mockTurbo.Verify(t => t.Stream(
        It.Is<string>(s => s.StartsWith("user:")),
        It.IsAny<Action<ITurboStreamBuilder>>()),
        Times.Once);
}
```

### Mocking HttpContext for Tests

```csharp
public static class TestHelpers
{
    public static HttpContext CreateTurboFrameContext(string frameId)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Turbo-Frame"] = frameId;
        context.Items[TurboFrameMiddleware.IsTurboFrameRequestKey] = true;
        context.Items[TurboFrameMiddleware.FrameIdKey] = frameId;
        return context;
    }

    public static HttpContext CreateTurboStreamContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept"] = "text/vnd.turbo-stream.html";
        return context;
    }
}
```

## Best Practices

1. **Test both Turbo and non-Turbo paths** - Ensure fallback behavior works

2. **Use specific assertions** - Check for frame IDs, content types, etc.

3. **Test authorization thoroughly** - Cover all edge cases

4. **Integration tests for real flows** - Verify the full request/response cycle

5. **Mock external dependencies** - Use mocks for `ITurbo`, SignalR, etc.

## See Also

- [Turbo Frames Guide](turbo-frames.md)
- [Turbo Streams Guide](turbo-streams.md)
- [Authorization Guide](authorization.md)
