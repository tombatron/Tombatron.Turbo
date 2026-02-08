using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Tombatron.Turbo.Streams;

/// <summary>
/// SignalR hub for managing Turbo Stream subscriptions and broadcasting updates.
/// </summary>
/// <remarks>
/// Clients subscribe to named streams and receive Turbo Stream HTML when updates are broadcast.
/// The hub uses SignalR groups to efficiently route messages to subscribed clients.
/// </remarks>
public class TurboHub : Hub
{
    /// <summary>
    /// The method name used to send Turbo Stream messages to clients.
    /// </summary>
    public const string TurboStreamMethod = "TurboStream";

    private readonly ITurboStreamAuthorization _authorization;
    private readonly TurboOptions _options;
    private readonly ILogger<TurboHub> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TurboHub"/> class.
    /// </summary>
    /// <param name="authorization">The authorization service for stream subscriptions.</param>
    /// <param name="options">The Turbo configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public TurboHub(
        ITurboStreamAuthorization authorization,
        TurboOptions options,
        ILogger<TurboHub> logger)
    {
        _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Subscribes the current connection to a named stream.
    /// </summary>
    /// <param name="streamName">The name of the stream to subscribe to.</param>
    /// <returns>True if the subscription was successful; false if unauthorized.</returns>
    /// <exception cref="ArgumentNullException">Thrown when streamName is null.</exception>
    /// <exception cref="ArgumentException">Thrown when streamName is empty or whitespace.</exception>
    public async Task<bool> Subscribe(string streamName)
    {
        ValidateStreamName(streamName);

        // Check authorization
        if (!_authorization.CanSubscribe(Context.User, streamName))
        {
            _logger.LogWarning(
                "Subscription denied for connection {ConnectionId} to stream {StreamName}",
                Context.ConnectionId,
                streamName);
            return false;
        }

        // Add to SignalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, streamName);

        _logger.LogDebug(
            "Connection {ConnectionId} subscribed to stream {StreamName}",
            Context.ConnectionId,
            streamName);

        return true;
    }

    /// <summary>
    /// Unsubscribes the current connection from a named stream.
    /// </summary>
    /// <param name="streamName">The name of the stream to unsubscribe from.</param>
    /// <exception cref="ArgumentNullException">Thrown when streamName is null.</exception>
    /// <exception cref="ArgumentException">Thrown when streamName is empty or whitespace.</exception>
    public async Task Unsubscribe(string streamName)
    {
        ValidateStreamName(streamName);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, streamName);

        _logger.LogDebug(
            "Connection {ConnectionId} unsubscribed from stream {StreamName}",
            Context.ConnectionId,
            streamName);
    }

    /// <summary>
    /// Called when a new connection is established.
    /// </summary>
    public override Task OnConnectedAsync()
    {
        _logger.LogDebug("Connection {ConnectionId} connected to TurboHub", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a connection is terminated.
    /// </summary>
    /// <param name="exception">The exception that caused the disconnection, if any.</param>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(
                exception,
                "Connection {ConnectionId} disconnected from TurboHub with error",
                Context.ConnectionId);
        }
        else
        {
            _logger.LogDebug(
                "Connection {ConnectionId} disconnected from TurboHub",
                Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Validates a stream name.
    /// </summary>
    /// <param name="streamName">The stream name to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when streamName is null.</exception>
    /// <exception cref="ArgumentException">Thrown when streamName is empty or whitespace.</exception>
    internal static void ValidateStreamName(string streamName)
    {
        if (streamName == null)
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentException("Stream name cannot be empty or whitespace.", nameof(streamName));
        }
    }
}
