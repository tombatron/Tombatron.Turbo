# Security Guide

This document covers security considerations when using Tombatron.Turbo.

## Overview

Tombatron.Turbo is designed with security in mind. This guide covers:

1. XSS Prevention
2. Stream Authorization
3. Input Validation
4. Connection Security

## XSS Prevention

### Target ID Escaping

All target IDs in Turbo Stream actions are automatically HTML-escaped to prevent XSS attacks:

```csharp
// Safe - target is automatically escaped
builder.Append("user-<script>", "<div>content</div>");
// Output: target="user-&lt;script&gt;"
```

The following characters are escaped in target attributes:
- `<` → `&lt;`
- `>` → `&gt;`
- `"` → `&quot;`
- `'` → `&#39;`
- `&` → `&amp;`

### HTML Content Responsibility

HTML content passed to stream actions is **not escaped** by design. This allows you to send valid HTML to the client. However, you are responsible for ensuring the HTML is safe:

```csharp
// YOUR responsibility to sanitize user input!
var userInput = SanitizeHtml(request.UserInput);
builder.Append("comments", $"<div>{userInput}</div>");
```

**Recommendations:**
- Always sanitize user-generated content before including it in streams
- Use Razor views to generate HTML (which handles encoding)
- Consider using a library like HtmlSanitizer for user content

### Frame ID Validation

Frame IDs are validated at compile time by the analyzer:
- Dynamic IDs require a prefix (`asp-frame-prefix`)
- This prevents injection of malicious IDs

## Stream Authorization

### Default Behavior

By default, all stream subscriptions are allowed. For production applications, implement `ITurboStreamAuthorization`:

```csharp
public class MyStreamAuthorization : ITurboStreamAuthorization
{
    public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
    {
        // Only authenticated users can subscribe
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // User-specific streams
        if (streamName.StartsWith("user:"))
        {
            var userId = streamName.Substring(5);
            return user.FindFirstValue(ClaimTypes.NameIdentifier) == userId;
        }

        return true;
    }
}
```

Register it in your services:

```csharp
services.AddTurbo(options =>
{
    options.Authorization = new MyStreamAuthorization();
});
```

### Stream Naming Patterns

Use predictable stream naming patterns for easier authorization:

| Pattern | Example | Use Case |
|---------|---------|----------|
| `user:{userId}` | `user:123` | User-specific notifications |
| `resource:{type}:{id}` | `resource:order:456` | Resource-specific updates |
| `broadcast` | `broadcast` | Public announcements |
| `role:{roleName}` | `role:admin` | Role-based streams |

### Signed Stream Names

For additional security, consider using signed stream names:

```csharp
public class SignedStreamAuthorization : ITurboStreamAuthorization
{
    private readonly IDataProtector _protector;

    public SignedStreamAuthorization(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("TurboStreams");
    }

    public string CreateSignedStreamName(string streamName)
    {
        var token = _protector.Protect(streamName);
        return $"{streamName}:{token}";
    }

    public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
    {
        var parts = streamName.Split(':', 2);
        if (parts.Length != 2)
        {
            return false;
        }

        try
        {
            var decrypted = _protector.Unprotect(parts[1]);
            return decrypted == parts[0];
        }
        catch
        {
            return false;
        }
    }
}
```

## Input Validation

### Stream Names

Stream names are validated:
- Cannot be null
- Cannot be empty or whitespace

```csharp
// Throws ArgumentNullException
await turbo.Stream(null!, builder => { });

// Throws ArgumentException
await turbo.Stream("", builder => { });
await turbo.Stream("   ", builder => { });
```

### Target IDs

Target IDs are validated:
- Cannot be null
- Cannot be empty or whitespace

```csharp
// Throws ArgumentNullException
builder.Append(null!, "<div>content</div>");

// Throws ArgumentException
builder.Append("", "<div>content</div>");
```

### HTML Content

HTML content cannot be null:

```csharp
// Throws ArgumentNullException
builder.Append("target", null!);

// Empty string is allowed
builder.Append("target", "");
```

## Connection Security

### HTTPS Requirement

For production deployments, always use HTTPS to encrypt SignalR connections:

```csharp
app.UseHttpsRedirection();
```

### SignalR Authentication

Configure SignalR authentication for protected hubs:

```csharp
services.AddSignalR();
services.AddAuthentication(/* ... */);

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<TurboHub>("/turbo-stream", options =>
{
    options.Transports = HttpTransportType.WebSockets;
});
```

### CORS Configuration

If your client is on a different origin, configure CORS:

```csharp
services.AddCors(options =>
{
    options.AddPolicy("TurboStreams", builder =>
    {
        builder
            .WithOrigins("https://your-domain.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

app.UseCors("TurboStreams");
```

## Security Checklist

Before deploying to production:

- [ ] Implement `ITurboStreamAuthorization` with proper access control
- [ ] Sanitize all user-generated HTML content
- [ ] Use HTTPS in production
- [ ] Configure CORS appropriately
- [ ] Review stream naming conventions
- [ ] Test authorization bypass attempts
- [ ] Enable logging for security events
- [ ] Consider rate limiting for subscriptions

## Logging Security Events

The library logs security-relevant events:

```csharp
// Configure logging in appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Tombatron.Turbo": "Debug",
      "Tombatron.Turbo.Streams.TurboHub": "Warning"
    }
  }
}
```

Security events logged:
- Subscription denials (Warning level)
- Connection/disconnection with errors (Warning level)
- Successful subscriptions (Debug level)

## Reporting Security Issues

If you discover a security vulnerability, please report it privately:

1. Do NOT open a public issue
2. Email security concerns to the maintainer
3. Include steps to reproduce
4. Allow time for a fix before disclosure
