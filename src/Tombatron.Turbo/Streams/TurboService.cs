using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Tombatron.Turbo.Streams;

/// <summary>
/// Implementation of <see cref="ITurbo"/> for broadcasting Turbo Stream updates via SignalR.
/// </summary>
public sealed class TurboService : ITurbo
{
    private readonly IHubContext<TurboHub> _hubContext;
    private readonly ILogger<TurboService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TurboService"/> class.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context for TurboHub.</param>
    /// <param name="logger">The logger instance.</param>
    public TurboService(IHubContext<TurboHub> hubContext, ILogger<TurboService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task Stream(string streamName, Action<ITurboStreamBuilder> build)
    {
        if (streamName == null)
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentException("Stream name cannot be empty or whitespace.", nameof(streamName));
        }

        if (build == null)
        {
            throw new ArgumentNullException(nameof(build));
        }

        string html = BuildStreamHtml(build);

        if (string.IsNullOrEmpty(html))
        {
            _logger.LogDebug("No actions configured for stream {StreamName}, skipping broadcast", streamName);
            return;
        }

        await _hubContext.Clients
            .Group(streamName)
            .SendAsync(TurboHub.TurboStreamMethod, html);

        _logger.LogDebug("Broadcast Turbo Stream to {StreamName}", streamName);
    }

    /// <inheritdoc />
    public async Task Stream(IEnumerable<string> streamNames, Action<ITurboStreamBuilder> build)
    {
        if (streamNames == null)
        {
            throw new ArgumentNullException(nameof(streamNames));
        }

        if (build == null)
        {
            throw new ArgumentNullException(nameof(build));
        }

        string html = BuildStreamHtml(build);

        if (string.IsNullOrEmpty(html))
        {
            _logger.LogDebug("No actions configured for streams, skipping broadcast");
            return;
        }

        // Collect stream names and validate
        List<string> streams = new();
        foreach (string streamName in streamNames)
        {
            if (string.IsNullOrWhiteSpace(streamName))
            {
                throw new ArgumentException("Stream names cannot contain null, empty, or whitespace values.", nameof(streamNames));
            }

            streams.Add(streamName);
        }

        if (streams.Count == 0)
        {
            _logger.LogDebug("No stream names provided, skipping broadcast");
            return;
        }

        // Send to all groups
        var tasks = streams.Select(streamName =>
            _hubContext.Clients
                .Group(streamName)
                .SendAsync(TurboHub.TurboStreamMethod, html));

        await Task.WhenAll(tasks);

        _logger.LogDebug("Broadcast Turbo Stream to {Count} streams", streams.Count);
    }

    /// <inheritdoc />
    public async Task Broadcast(Action<ITurboStreamBuilder> build)
    {
        if (build == null)
        {
            throw new ArgumentNullException(nameof(build));
        }

        string html = BuildStreamHtml(build);

        if (string.IsNullOrEmpty(html))
        {
            _logger.LogDebug("No actions configured for broadcast, skipping");
            return;
        }

        await _hubContext.Clients
            .All
            .SendAsync(TurboHub.TurboStreamMethod, html);

        _logger.LogDebug("Broadcast Turbo Stream to all clients");
    }

    /// <summary>
    /// Builds the Turbo Stream HTML from the builder action.
    /// </summary>
    /// <param name="build">The action that configures the builder.</param>
    /// <returns>The built HTML string.</returns>
    private static string BuildStreamHtml(Action<ITurboStreamBuilder> build)
    {
        var builder = new TurboStreamBuilder();
        build(builder);
        return builder.Build();
    }
}
