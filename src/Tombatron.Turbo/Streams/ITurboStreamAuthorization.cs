using System.Security.Claims;

namespace Tombatron.Turbo.Streams;

/// <summary>
/// Interface for authorizing Turbo Stream subscriptions.
/// </summary>
/// <remarks>
/// Implement this interface to provide custom authorization logic for stream subscriptions.
/// The authorization check is performed when a client attempts to subscribe to a stream.
/// </remarks>
public interface ITurboStreamAuthorization
{
    /// <summary>
    /// Determines whether the specified user can subscribe to the given stream.
    /// </summary>
    /// <param name="user">The user attempting to subscribe, or null for anonymous users.</param>
    /// <param name="streamName">The name of the stream to subscribe to.</param>
    /// <returns>True if the user is authorized to subscribe; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// public class CustomAuthorization : ITurboStreamAuthorization
    /// {
    ///     public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
    ///     {
    ///         // Only allow users to subscribe to their own stream
    ///         if (streamName.StartsWith("user:"))
    ///         {
    ///             string userId = streamName.Substring(5);
    ///             return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value == userId;
    ///         }
    ///         return true;
    ///     }
    /// }
    /// </code>
    /// </example>
    bool CanSubscribe(ClaimsPrincipal? user, string streamName);
}
