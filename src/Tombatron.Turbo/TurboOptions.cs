namespace Tombatron.Turbo;

/// <summary>
/// Configuration options for Tombatron.Turbo.
/// </summary>
public sealed class TurboOptions
{
    /// <summary>
    /// Gets or sets the path for the SignalR hub endpoint.
    /// Default is "/turbo-hub".
    /// </summary>
    public string HubPath { get; set; } = "/turbo-hub";

    /// <summary>
    /// Gets or sets whether to use signed stream names for subscription security.
    /// When enabled, clients can only subscribe to streams that have been cryptographically
    /// signed by the server (similar to Rails' Turbo Streams approach).
    /// Default is true.
    /// </summary>
    /// <remarks>
    /// Signed stream names use ASP.NET Core's Data Protection API for signing.
    /// This provides security without requiring authentication - if the server rendered
    /// the stream subscription tag, the client is implicitly authorized to subscribe.
    /// </remarks>
    public bool UseSignedStreamNames { get; set; } = true;

    /// <summary>
    /// Gets or sets the expiration time for signed stream name tokens.
    /// After this duration, clients will need to refresh the page to get new tokens.
    /// Default is 24 hours. Set to null for no expiration.
    /// </summary>
    public TimeSpan? SignedStreamNameExpiration { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Gets or sets whether to automatically add the Vary: Turbo-Frame header to responses.
    /// Default is true.
    /// </summary>
    public bool AddVaryHeader { get; set; } = true;

    /// <summary>
    /// Gets or sets the default stream name pattern for authenticated users.
    /// Use {0} as placeholder for the user identifier.
    /// Default is "user:{0}".
    /// </summary>
    public string DefaultUserStreamPattern { get; set; } = "user:{0}";

    /// <summary>
    /// Gets or sets the default stream name pattern for anonymous sessions.
    /// Use {0} as placeholder for the session ID.
    /// Default is "session:{0}".
    /// </summary>
    public string DefaultSessionStreamPattern { get; set; } = "session:{0}";

    /// <summary>
    /// Gets or sets whether to enable automatic reconnection for SignalR clients.
    /// Default is true.
    /// </summary>
    public bool EnableAutoReconnect { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of reconnection attempts before giving up.
    /// Default is 5.
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 5;

    /// <summary>
    /// Gets the import map configuration for JavaScript module imports.
    /// Use <see cref="ImportMapConfiguration.Pin"/> to add module entries.
    /// </summary>
    public ImportMapConfiguration ImportMap { get; } = new();

    /// <summary>
    /// Validates the current options configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(HubPath))
        {
            throw new InvalidOperationException("HubPath cannot be null or empty.");
        }

        if (!HubPath.StartsWith('/'))
        {
            throw new InvalidOperationException("HubPath must start with a forward slash.");
        }

        if (string.IsNullOrWhiteSpace(DefaultUserStreamPattern))
        {
            throw new InvalidOperationException("DefaultUserStreamPattern cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(DefaultSessionStreamPattern))
        {
            throw new InvalidOperationException("DefaultSessionStreamPattern cannot be null or empty.");
        }

        if (MaxReconnectAttempts < 0)
        {
            throw new InvalidOperationException("MaxReconnectAttempts cannot be negative.");
        }
    }
}
