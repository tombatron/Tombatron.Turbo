namespace Tombatron.Turbo;

/// <summary>
/// Main service interface for broadcasting Turbo Stream updates to connected clients.
/// </summary>
/// <remarks>
/// Use this interface to send real-time updates to clients subscribed to specific streams.
/// The service manages SignalR connections and groups automatically.
/// </remarks>
public interface ITurbo
{
    /// <summary>
    /// Broadcasts Turbo Stream updates to all clients subscribed to the specified stream.
    /// </summary>
    /// <param name="streamName">The name of the stream to broadcast to.</param>
    /// <param name="build">An action that configures the stream updates to send.</param>
    /// <returns>A task that completes when the broadcast has been sent.</returns>
    /// <exception cref="ArgumentNullException">Thrown when streamName or build is null.</exception>
    /// <exception cref="ArgumentException">Thrown when streamName is empty or whitespace.</exception>
    /// <example>
    /// <code>
    /// await turbo.Stream("cart:123", builder =>
    /// {
    ///     builder.Replace("cart-total", "&lt;span&gt;$99.99&lt;/span&gt;");
    /// });
    /// </code>
    /// </example>
    Task Stream(string streamName, Action<ITurboStreamBuilder> build);

    /// <summary>
    /// Broadcasts Turbo Stream updates to all clients subscribed to any of the specified streams.
    /// </summary>
    /// <param name="streamNames">The names of the streams to broadcast to.</param>
    /// <param name="build">An action that configures the stream updates to send.</param>
    /// <returns>A task that completes when all broadcasts have been sent.</returns>
    /// <exception cref="ArgumentNullException">Thrown when streamNames or build is null.</exception>
    /// <example>
    /// <code>
    /// await turbo.Stream(new[] { "user:alice", "user:bob" }, builder =>
    /// {
    ///     builder.Append("notifications", "&lt;div&gt;New message!&lt;/div&gt;");
    /// });
    /// </code>
    /// </example>
    Task Stream(IEnumerable<string> streamNames, Action<ITurboStreamBuilder> build);

    /// <summary>
    /// Broadcasts Turbo Stream updates to all connected clients regardless of their subscriptions.
    /// </summary>
    /// <param name="build">An action that configures the stream updates to send.</param>
    /// <returns>A task that completes when the broadcast has been sent.</returns>
    /// <exception cref="ArgumentNullException">Thrown when build is null.</exception>
    /// <example>
    /// <code>
    /// await turbo.Broadcast(builder =>
    /// {
    ///     builder.Update("announcement", "&lt;p&gt;System maintenance at midnight&lt;/p&gt;");
    /// });
    /// </code>
    /// </example>
    Task Broadcast(Action<ITurboStreamBuilder> build);

    /// <summary>
    /// Broadcasts Turbo Stream updates to all clients subscribed to the specified stream,
    /// with support for async operations like partial rendering.
    /// </summary>
    /// <param name="streamName">The name of the stream to broadcast to.</param>
    /// <param name="buildAsync">An async function that configures the stream updates to send.</param>
    /// <returns>A task that completes when the broadcast has been sent.</returns>
    /// <exception cref="ArgumentNullException">Thrown when streamName or buildAsync is null.</exception>
    /// <exception cref="ArgumentException">Thrown when streamName is empty or whitespace.</exception>
    /// <example>
    /// <code>
    /// await turbo.Stream("room:123", async builder =>
    /// {
    ///     await builder.AppendAsync("messages", Partials.Message, message);
    /// });
    /// </code>
    /// </example>
    Task Stream(string streamName, Func<ITurboStreamBuilder, Task> buildAsync);

    /// <summary>
    /// Broadcasts Turbo Stream updates to all clients subscribed to any of the specified streams,
    /// with support for async operations like partial rendering.
    /// </summary>
    /// <param name="streamNames">The names of the streams to broadcast to.</param>
    /// <param name="buildAsync">An async function that configures the stream updates to send.</param>
    /// <returns>A task that completes when all broadcasts have been sent.</returns>
    /// <exception cref="ArgumentNullException">Thrown when streamNames or buildAsync is null.</exception>
    /// <example>
    /// <code>
    /// await turbo.Stream(new[] { "user:alice", "user:bob" }, async builder =>
    /// {
    ///     await builder.AppendAsync("notifications", Partials.Notification, notification);
    /// });
    /// </code>
    /// </example>
    Task Stream(IEnumerable<string> streamNames, Func<ITurboStreamBuilder, Task> buildAsync);

    /// <summary>
    /// Broadcasts Turbo Stream updates to all connected clients regardless of their subscriptions,
    /// with support for async operations like partial rendering.
    /// </summary>
    /// <param name="buildAsync">An async function that configures the stream updates to send.</param>
    /// <returns>A task that completes when the broadcast has been sent.</returns>
    /// <exception cref="ArgumentNullException">Thrown when buildAsync is null.</exception>
    /// <example>
    /// <code>
    /// await turbo.Broadcast(async builder =>
    /// {
    ///     await builder.UpdateAsync("announcement", Partials.Announcement, announcement);
    /// });
    /// </code>
    /// </example>
    Task Broadcast(Func<ITurboStreamBuilder, Task> buildAsync);
}
