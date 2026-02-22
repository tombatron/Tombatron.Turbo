using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Tombatron.Turbo.Rendering;

namespace Tombatron.Turbo.Streams;

/// <summary>
/// Implementation of <see cref="ITurbo"/> for broadcasting Turbo Stream updates via SignalR.
/// </summary>
public sealed class TurboService : ITurbo
{
    private readonly IHubContext<TurboHub> _hubContext;
    private readonly ILogger<TurboService> _logger;
    private readonly IPartialRenderer _partialRenderer;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TurboService"/> class.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context for TurboHub.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="partialRenderer">The partial renderer for async partial rendering operations.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor for reading request headers.</param>
    public TurboService(IHubContext<TurboHub> hubContext, ILogger<TurboService> logger, IPartialRenderer partialRenderer, IHttpContextAccessor httpContextAccessor)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _partialRenderer = partialRenderer ?? throw new ArgumentNullException(nameof(partialRenderer));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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

    /// <inheritdoc />
    public async Task Stream(string streamName, Func<ITurboStreamBuilder, Task> buildAsync)
    {
        if (streamName == null)
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentException("Stream name cannot be empty or whitespace.", nameof(streamName));
        }

        if (buildAsync == null)
        {
            throw new ArgumentNullException(nameof(buildAsync));
        }

        string html = await BuildStreamHtmlAsync(buildAsync);

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
    public async Task Stream(IEnumerable<string> streamNames, Func<ITurboStreamBuilder, Task> buildAsync)
    {
        if (streamNames == null)
        {
            throw new ArgumentNullException(nameof(streamNames));
        }

        if (buildAsync == null)
        {
            throw new ArgumentNullException(nameof(buildAsync));
        }

        string html = await BuildStreamHtmlAsync(buildAsync);

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
    public async Task Broadcast(Func<ITurboStreamBuilder, Task> buildAsync)
    {
        if (buildAsync == null)
        {
            throw new ArgumentNullException(nameof(buildAsync));
        }

        string html = await BuildStreamHtmlAsync(buildAsync);

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

    /// <inheritdoc />
    public Task StreamRefresh(string streamName)
    {
        var requestId = _httpContextAccessor.HttpContext?.GetTurboRequestId();
        return Stream(streamName, builder => builder.Refresh(requestId));
    }

    /// <inheritdoc />
    public Task StreamRefresh(IEnumerable<string> streamNames)
    {
        var requestId = _httpContextAccessor.HttpContext?.GetTurboRequestId();
        return Stream(streamNames, builder => builder.Refresh(requestId));
    }

    /// <inheritdoc />
    public Task BroadcastRefresh()
    {
        var requestId = _httpContextAccessor.HttpContext?.GetTurboRequestId();
        return Broadcast(builder => builder.Refresh(requestId));
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

    /// <summary>
    /// Builds the Turbo Stream HTML from an async builder function.
    /// </summary>
    /// <param name="buildAsync">The async function that configures the builder.</param>
    /// <returns>The built HTML string.</returns>
    private async Task<string> BuildStreamHtmlAsync(Func<ITurboStreamBuilder, Task> buildAsync)
    {
        var builder = new TurboStreamBuilder { Renderer = _partialRenderer };
        await buildAsync(builder);
        return builder.Build();
    }
}
