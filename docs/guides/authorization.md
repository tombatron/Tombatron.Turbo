# Authorization Guide

Tombatron.Turbo provides multiple mechanisms for securing stream subscriptions. This guide covers the built-in security features and how to implement custom authorization.

## Security Mechanisms

### 1. Signed Stream Names (Default)

The default and recommended approach. Stream names are cryptographically signed by the server, and clients can only subscribe to streams they've been given signed tokens for.

**How it works:**

1. Server renders a `<turbo>` tag helper, which signs the stream name
2. Client receives the signed token in the HTML
3. Client uses the signed token to subscribe
4. Server validates the signature before allowing subscription

**Configuration:**

```csharp
builder.Services.AddTurbo(options =>
{
    options.UseSignedStreamNames = true;
    options.SignedStreamNameExpiration = TimeSpan.FromHours(24);
});
```

**Security model:** If the server rendered the stream subscription, the client is implicitly authorized. This is secure because:

- Tokens are cryptographically signed using ASP.NET Core Data Protection
- Tokens expire after a configurable duration
- Tokens cannot be forged without the server's key

### 2. Custom Authorization

Implement `ITurboStreamAuthorization` for fine-grained control:

```csharp
public class CustomStreamAuthorization : ITurboStreamAuthorization
{
    public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
    {
        // Implement your authorization logic
        return true;
    }
}
```

Register your implementation:

```csharp
builder.Services.AddSingleton<ITurboStreamAuthorization, CustomStreamAuthorization>();
```

## Common Authorization Patterns

### User-Specific Streams

Only allow users to subscribe to their own stream:

```csharp
public class UserStreamAuthorization : ITurboStreamAuthorization
{
    public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
    {
        // Allow public streams
        if (streamName.StartsWith("public:"))
        {
            return true;
        }

        // User streams require authentication
        if (streamName.StartsWith("user:"))
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            string userId = streamName.Substring(5); // Remove "user:" prefix
            string? currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userId == currentUserId;
        }

        return false;
    }
}
```

### Role-Based Access

Restrict certain streams to specific roles:

```csharp
public class RoleBasedAuthorization : ITurboStreamAuthorization
{
    public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
    {
        // Admin streams require Admin role
        if (streamName.StartsWith("admin:"))
        {
            return user?.IsInRole("Admin") == true;
        }

        // Moderator streams require Moderator or Admin role
        if (streamName.StartsWith("mod:"))
        {
            return user?.IsInRole("Moderator") == true
                || user?.IsInRole("Admin") == true;
        }

        // All other streams are public
        return true;
    }
}
```

### Resource-Based Access

Check access to specific resources:

```csharp
public class ResourceAuthorization : ITurboStreamAuthorization
{
    private readonly IAuthorizationService _authService;
    private readonly IServiceProvider _serviceProvider;

    public ResourceAuthorization(
        IAuthorizationService authService,
        IServiceProvider serviceProvider)
    {
        _authService = authService;
        _serviceProvider = serviceProvider;
    }

    public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
    {
        // Parse stream name: "order:123"
        if (streamName.StartsWith("order:"))
        {
            if (!int.TryParse(streamName.Substring(6), out int orderId))
            {
                return false;
            }

            using var scope = _serviceProvider.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            var order = orderService.GetOrder(orderId);

            if (order == null)
            {
                return false;
            }

            // Only order owner can subscribe
            return order.CustomerId == user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        return true;
    }
}
```

### Group/Room Access

Control access to chat rooms or groups:

```csharp
public class RoomAuthorization : ITurboStreamAuthorization
{
    private readonly IRoomService _roomService;

    public RoomAuthorization(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
    {
        if (!streamName.StartsWith("room:"))
        {
            return true;
        }

        string roomId = streamName.Substring(5);
        string? userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        return _roomService.IsMember(roomId, userId);
    }
}
```

## Combining Authorization Methods

You can use both signed stream names and custom authorization together:

```csharp
builder.Services.AddTurbo(options =>
{
    // Signed names provide the first layer of security
    options.UseSignedStreamNames = true;
});

// Custom authorization provides additional checks
builder.Services.AddSingleton<ITurboStreamAuthorization, CustomAuthorization>();
```

With this setup:
1. Client must have a valid signed token (from server-rendered HTML)
2. Custom authorization performs additional checks (role, resource access, etc.)

## Stream Name Conventions for Security

Design stream names to support authorization:

| Pattern | Description | Authorization |
|---------|-------------|---------------|
| `public:announcements` | Public streams | Allow all |
| `user:{userId}` | User-specific | Match user ID |
| `admin:dashboard` | Admin only | Check admin role |
| `room:{roomId}` | Chat rooms | Check membership |
| `order:{orderId}` | Order updates | Check ownership |

## Testing Authorization

Write unit tests for your authorization logic:

```csharp
public class AuthorizationTests
{
    [Fact]
    public void UserCanSubscribeToOwnStream()
    {
        var auth = new UserStreamAuthorization();
        var user = CreateUser("user123");

        var result = auth.CanSubscribe(user, "user:user123");

        Assert.True(result);
    }

    [Fact]
    public void UserCannotSubscribeToOtherUserStream()
    {
        var auth = new UserStreamAuthorization();
        var user = CreateUser("user123");

        var result = auth.CanSubscribe(user, "user:other456");

        Assert.False(result);
    }

    [Fact]
    public void AnonymousCannotSubscribeToUserStream()
    {
        var auth = new UserStreamAuthorization();

        var result = auth.CanSubscribe(null, "user:user123");

        Assert.False(result);
    }

    private ClaimsPrincipal CreateUser(string userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }
}
```

## Security Best Practices

1. **Enable signed stream names** - This is on by default; keep it enabled
2. **Set reasonable token expiration** - 24 hours is the default; adjust based on your needs
3. **Use specific stream names** - `user:123` instead of just `123`
4. **Validate on server** - Never trust client-provided stream names without validation
5. **Log authorization failures** - Monitor for potential attacks
6. **Test authorization thoroughly** - Cover all edge cases

## See Also

- [TurboOptions](../api/TurboOptions.md) - Configuration options
- [Turbo Streams](turbo-streams.md) - Stream overview
- [Testing](testing.md) - Testing authorization
