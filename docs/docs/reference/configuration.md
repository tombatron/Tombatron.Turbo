---
title: Configuration
sidebar_label: Configuration
sidebar_position: 1
description: Configure Tombatron.Turbo services and options.
---

Configure Tombatron.Turbo in `Program.cs` via the `AddTurbo()` options callback:

```csharp
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/turbo-hub";                             // Default: "/turbo-hub"
    options.AddVaryHeader = true;                               // Default: true
    options.UseSignedStreamNames = true;                        // Default: true
    options.SignedStreamNameExpiration = TimeSpan.FromHours(24); // Default: 24 hours
    options.EnableAutoReconnect = true;                         // Default: true
    options.MaxReconnectAttempts = 5;                           // Default: 5
    options.DefaultUserStreamPattern = "user:{0}";              // Default: "user:{0}"
    options.DefaultSessionStreamPattern = "session:{0}";        // Default: "session:{0}"
});
```

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `HubPath` | `string` | `"/turbo-hub"` | SignalR hub endpoint path. Must start with `/`. |
| `AddVaryHeader` | `bool` | `true` | Add `Vary: Turbo-Frame` header to prevent caching issues. |
| `UseSignedStreamNames` | `bool` | `true` | Cryptographically sign stream names for subscription security. |
| `SignedStreamNameExpiration` | `TimeSpan?` | `24 hours` | How long signed tokens remain valid. `null` for no expiration. |
| `EnableAutoReconnect` | `bool` | `true` | Automatically reconnect SignalR clients on disconnect. |
| `MaxReconnectAttempts` | `int` | `5` | Maximum reconnection attempts before giving up. |
| `DefaultUserStreamPattern` | `string` | `"user:{0}"` | Stream name pattern for authenticated users. `{0}` is the user ID. |
| `DefaultSessionStreamPattern` | `string` | `"session:{0}"` | Stream name pattern for anonymous sessions. `{0}` is the session ID. |

For the full API, see [TurboOptions](../api/TurboOptions.md).
