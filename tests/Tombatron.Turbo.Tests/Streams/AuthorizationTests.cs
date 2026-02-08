using FluentAssertions;
using System.Security.Claims;
using Tombatron.Turbo.Streams;
using Xunit;

namespace Tombatron.Turbo.Tests.Streams;

/// <summary>
/// Tests for stream authorization implementations.
/// </summary>
public class AuthorizationTests
{
    #region DefaultTurboStreamAuthorization Tests

    [Fact]
    public void DefaultAuthorization_WithAuthenticatedUser_ReturnsTrue()
    {
        // Arrange
        var authorization = new DefaultTurboStreamAuthorization();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);

        // Act
        bool result = authorization.CanSubscribe(user, "any-stream");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DefaultAuthorization_WithNullUser_ReturnsTrue()
    {
        // Arrange
        var authorization = new DefaultTurboStreamAuthorization();

        // Act
        bool result = authorization.CanSubscribe(null, "any-stream");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DefaultAuthorization_WithAnonymousUser_ReturnsTrue()
    {
        // Arrange
        var authorization = new DefaultTurboStreamAuthorization();
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // No authentication type = anonymous

        // Act
        bool result = authorization.CanSubscribe(user, "any-stream");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DefaultAuthorization_WithAnyStreamName_ReturnsTrue()
    {
        // Arrange
        var authorization = new DefaultTurboStreamAuthorization();
        var user = new ClaimsPrincipal();

        // Act & Assert
        authorization.CanSubscribe(user, "public-stream").Should().BeTrue();
        authorization.CanSubscribe(user, "private-stream").Should().BeTrue();
        authorization.CanSubscribe(user, "user:123").Should().BeTrue();
        authorization.CanSubscribe(user, "room:456").Should().BeTrue();
    }

    #endregion

    #region Custom Authorization Implementation Tests

    [Fact]
    public void CustomAuthorization_CanDenyAnonymousUsers()
    {
        // Arrange
        var authorization = new RequireAuthenticationAuthorization();

        // Act
        bool anonymousResult = authorization.CanSubscribe(null, "any-stream");
        bool authenticatedResult = authorization.CanSubscribe(CreateAuthenticatedUser("user-1"), "any-stream");

        // Assert
        anonymousResult.Should().BeFalse();
        authenticatedResult.Should().BeTrue();
    }

    [Fact]
    public void CustomAuthorization_CanRestrictToUserStreams()
    {
        // Arrange
        var authorization = new UserStreamAuthorization();
        var user = CreateAuthenticatedUser("user-123");

        // Act
        bool ownStreamResult = authorization.CanSubscribe(user, "user:user-123");
        bool otherStreamResult = authorization.CanSubscribe(user, "user:user-456");
        bool publicStreamResult = authorization.CanSubscribe(user, "public-stream");

        // Assert
        ownStreamResult.Should().BeTrue();
        otherStreamResult.Should().BeFalse();
        publicStreamResult.Should().BeTrue();
    }

    [Fact]
    public void CustomAuthorization_CanRestrictByRole()
    {
        // Arrange
        var authorization = new RoleBasedAuthorization("admin");
        var adminUser = CreateUserWithRole("admin");
        var regularUser = CreateUserWithRole("user");

        // Act
        bool adminResult = authorization.CanSubscribe(adminUser, "admin-stream");
        bool userResult = authorization.CanSubscribe(regularUser, "admin-stream");

        // Assert
        adminResult.Should().BeTrue();
        userResult.Should().BeFalse();
    }

    [Fact]
    public void CustomAuthorization_CanAllowPublicStreams()
    {
        // Arrange
        var authorization = new PublicStreamAuthorization();

        // Act
        bool publicResult = authorization.CanSubscribe(null, "public:announcements");
        bool privateResult = authorization.CanSubscribe(null, "private:secret");

        // Assert
        publicResult.Should().BeTrue();
        privateResult.Should().BeFalse();
    }

    #endregion

    #region Helper Methods and Custom Implementations

    private static ClaimsPrincipal CreateAuthenticatedUser(string userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "test");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateUserWithRole(string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id"),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "test");
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Sample authorization that requires authentication.
    /// </summary>
    private class RequireAuthenticationAuthorization : ITurboStreamAuthorization
    {
        public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
        {
            return user?.Identity?.IsAuthenticated == true;
        }
    }

    /// <summary>
    /// Sample authorization that restricts user: streams to the owning user.
    /// </summary>
    private class UserStreamAuthorization : ITurboStreamAuthorization
    {
        public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
        {
            if (streamName.StartsWith("user:"))
            {
                string userId = streamName.Substring(5);
                return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value == userId;
            }

            return true;
        }
    }

    /// <summary>
    /// Sample authorization that restricts streams based on role.
    /// </summary>
    private class RoleBasedAuthorization : ITurboStreamAuthorization
    {
        private readonly string _requiredRole;

        public RoleBasedAuthorization(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
        {
            return user?.IsInRole(_requiredRole) == true;
        }
    }

    /// <summary>
    /// Sample authorization that allows public: streams for everyone.
    /// </summary>
    private class PublicStreamAuthorization : ITurboStreamAuthorization
    {
        public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
        {
            return streamName.StartsWith("public:");
        }
    }

    #endregion
}
