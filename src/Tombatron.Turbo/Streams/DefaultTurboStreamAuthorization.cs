using System.Security.Claims;

namespace Tombatron.Turbo.Streams;

/// <summary>
/// Default implementation of <see cref="ITurboStreamAuthorization"/> that allows all subscriptions.
/// </summary>
/// <remarks>
/// This implementation allows any client to subscribe to any stream. For production use,
/// consider implementing custom authorization logic based on your application's security requirements.
///
/// When <see cref="TurboOptions.UseSignedStreamNames"/> is enabled, security is provided by
/// cryptographically signing stream names, similar to how Rails handles Turbo Stream subscriptions.
/// </remarks>
public sealed class DefaultTurboStreamAuthorization : ITurboStreamAuthorization
{
    /// <inheritdoc />
    /// <remarks>
    /// The default implementation always returns true, allowing all subscriptions.
    /// Security can be enforced via signed stream names instead of explicit authorization checks.
    /// </remarks>
    public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
    {
        // Default implementation allows all subscriptions.
        // Security is typically provided via signed stream names.
        return true;
    }
}
