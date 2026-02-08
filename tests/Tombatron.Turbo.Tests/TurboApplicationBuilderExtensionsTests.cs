using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Moq;
using Xunit;

namespace Tombatron.Turbo.Tests;

public class TurboApplicationBuilderExtensionsTests
{
    [Fact]
    public void UseTurbo_WithNullApp_ShouldThrowArgumentNullException()
    {
        // Arrange
        IApplicationBuilder app = null!;

        // Act
        Action act = () => app.UseTurbo();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("app");
    }

    [Fact]
    public void UseTurbo_WithValidApp_ShouldReturnSameInstance()
    {
        // Arrange
        var mockApp = new Mock<IApplicationBuilder>();
        mockApp.Setup(a => a.ApplicationServices)
            .Returns(Mock.Of<IServiceProvider>());

        // Act
        var result = mockApp.Object.UseTurbo();

        // Assert
        result.Should().BeSameAs(mockApp.Object);
    }
}
