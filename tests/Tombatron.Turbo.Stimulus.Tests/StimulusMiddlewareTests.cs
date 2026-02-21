using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Tombatron.Turbo.Stimulus.Tests;

public class StimulusMiddlewareTests
{
    private readonly StimulusOptions _options = new();
    private readonly StimulusControllerRegistry _registry = new();

    public StimulusMiddlewareTests()
    {
        // Build a registry with one controller
        var fileProvider = CreateMockFileProvider(new[] { "hello_controller.js" });
        _registry.Rebuild(fileProvider, "controllers", NullLogger.Instance);
    }

    [Fact]
    public async Task Returns200_WithGeneratedModule_ForIndexEndpoint()
    {
        var nextCalled = false;
        var middleware = new StimulusMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            _registry,
            _options);

        var context = CreateHttpContext("GET", _options.IndexEndpointPath);

        await middleware.Invoke(context);

        context.Response.StatusCode.Should().Be(200);
        context.Response.ContentType.Should().Be("application/javascript");
        context.Response.Headers.ETag.ToString().Should().Be(_registry.ETag);
        nextCalled.Should().BeFalse();

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        body.Should().Contain("application.register(\"hello\", HelloController)");
    }

    [Fact]
    public async Task Returns304_WhenIfNoneMatchMatches()
    {
        var middleware = new StimulusMiddleware(
            _ => Task.CompletedTask,
            _registry,
            _options);

        var context = CreateHttpContext("GET", _options.IndexEndpointPath);
        context.Request.Headers.IfNoneMatch = _registry.ETag;

        await middleware.Invoke(context);

        context.Response.StatusCode.Should().Be(304);
    }

    [Fact]
    public async Task PassesThrough_ForNonMatchingPath()
    {
        var nextCalled = false;
        var middleware = new StimulusMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            _registry,
            _options);

        var context = CreateHttpContext("GET", "/some/other/path");

        await middleware.Invoke(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task PassesThrough_ForPostRequest()
    {
        var nextCalled = false;
        var middleware = new StimulusMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            _registry,
            _options);

        var context = CreateHttpContext("POST", _options.IndexEndpointPath);

        await middleware.Invoke(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Returns200_WithStaleETag()
    {
        var middleware = new StimulusMiddleware(
            _ => Task.CompletedTask,
            _registry,
            _options);

        var context = CreateHttpContext("GET", _options.IndexEndpointPath);
        context.Request.Headers.IfNoneMatch = "\"stale-etag\"";

        await middleware.Invoke(context);

        context.Response.StatusCode.Should().Be(200);
    }

    private static HttpContext CreateHttpContext(string method, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static IFileProvider CreateMockFileProvider(string[] fileNames)
    {
        var mockProvider = new Mock<IFileProvider>();
        var files = fileNames.Select(name =>
        {
            var mockFile = new Mock<IFileInfo>();
            mockFile.Setup(f => f.Name).Returns(name);
            mockFile.Setup(f => f.IsDirectory).Returns(false);
            mockFile.Setup(f => f.Exists).Returns(true);
            return mockFile.Object;
        }).ToList();

        var mockContents = new Mock<IDirectoryContents>();
        mockContents.Setup(d => d.Exists).Returns(true);
        mockContents.Setup(d => d.GetEnumerator()).Returns(() => files.GetEnumerator());

        mockProvider.Setup(p => p.GetDirectoryContents("controllers"))
            .Returns(mockContents.Object);

        return mockProvider.Object;
    }
}
