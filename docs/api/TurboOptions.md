# TurboOptions Class

Configuration options for Tombatron.Turbo.

## Namespace

```csharp
Tombatron.Turbo
```

## Usage

Configure options in `Program.cs`:

```csharp
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/my-turbo-hub";
    options.UseSignedStreamNames = true;
    options.AddVaryHeader = true;
});
```

## Properties

### HubPath

Gets or sets the path for the SignalR hub endpoint.

- **Type:** `string`
- **Default:** `"/turbo-hub"`

```csharp
options.HubPath = "/realtime";
```

> **Note:** Must start with a forward slash (`/`).

### UseSignedStreamNames

Gets or sets whether to use signed stream names for subscription security.

- **Type:** `bool`
- **Default:** `true`

When enabled, clients can only subscribe to streams that have been cryptographically signed by the server. This uses ASP.NET Core's Data Protection API.

```csharp
options.UseSignedStreamNames = true;
```

**Security Model:** If the server rendered the stream subscription tag, the client is implicitly authorized to subscribe. This provides security without requiring authentication.

### SignedStreamNameExpiration

Gets or sets the expiration time for signed stream name tokens.

- **Type:** `TimeSpan?`
- **Default:** `TimeSpan.FromHours(24)`

After this duration, clients will need to refresh the page to get new tokens. Set to `null` for no expiration.

```csharp
options.SignedStreamNameExpiration = TimeSpan.FromHours(8);
```

### AddVaryHeader

Gets or sets whether to automatically add the `Vary: Turbo-Frame` header to responses.

- **Type:** `bool`
- **Default:** `true`

This header tells caches that responses vary based on the `Turbo-Frame` request header, preventing incorrect cached responses.

```csharp
options.AddVaryHeader = true;
```

### DefaultUserStreamPattern

Gets or sets the default stream name pattern for authenticated users.

- **Type:** `string`
- **Default:** `"user:{0}"`

Use `{0}` as a placeholder for the user identifier.

```csharp
options.DefaultUserStreamPattern = "account:{0}";
```

### DefaultSessionStreamPattern

Gets or sets the default stream name pattern for anonymous sessions.

- **Type:** `string`
- **Default:** `"session:{0}"`

Use `{0}` as a placeholder for the session ID.

```csharp
options.DefaultSessionStreamPattern = "visitor:{0}";
```

### EnableAutoReconnect

Gets or sets whether to enable automatic reconnection for SignalR clients.

- **Type:** `bool`
- **Default:** `true`

```csharp
options.EnableAutoReconnect = true;
```

### MaxReconnectAttempts

Gets or sets the maximum number of reconnection attempts before giving up.

- **Type:** `int`
- **Default:** `5`

```csharp
options.MaxReconnectAttempts = 10;
```

## Validation

The `Validate()` method is called automatically during service registration and throws `InvalidOperationException` if:

- `HubPath` is null, empty, or doesn't start with `/`
- `DefaultUserStreamPattern` is null or empty
- `DefaultSessionStreamPattern` is null or empty
- `MaxReconnectAttempts` is negative

## Example Configuration

```csharp
builder.Services.AddTurbo(options =>
{
    // Custom hub path
    options.HubPath = "/turbo";

    // Enable signed stream names with 8-hour expiration
    options.UseSignedStreamNames = true;
    options.SignedStreamNameExpiration = TimeSpan.FromHours(8);

    // Custom stream patterns
    options.DefaultUserStreamPattern = "user:{0}:updates";
    options.DefaultSessionStreamPattern = "guest:{0}";

    // Reconnection settings
    options.EnableAutoReconnect = true;
    options.MaxReconnectAttempts = 10;
});
```

## See Also

- [ITurbo](ITurbo.md) - Main service interface
- [Authorization Guide](../guides/authorization.md) - Stream security
