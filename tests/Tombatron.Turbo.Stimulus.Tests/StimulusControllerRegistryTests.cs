using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Tombatron.Turbo.Stimulus.Tests;

public class StimulusControllerRegistryTests
{
    private readonly ILogger _logger = NullLogger.Instance;

    #region DeriveIdentifier Tests

    [Theory]
    [InlineData("hello_controller.js", "hello")]
    [InlineData("user_profile_controller.js", "user-profile")]
    [InlineData("admin/users_controller.js", "admin--users")]
    [InlineData("admin/user_profile_controller.js", "admin--user-profile")]
    [InlineData("deeply/nested/thing_controller.js", "deeply--nested--thing")]
    public void DeriveIdentifier_ProducesCorrectIdentifiers(string input, string expected)
    {
        var result = StimulusControllerRegistry.DeriveIdentifier(input);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("hello.js")]
    [InlineData("styles.css")]
    [InlineData("utils/helper.js")]
    [InlineData("README.md")]
    public void DeriveIdentifier_ReturnsNull_ForNonControllerFiles(string input)
    {
        var result = StimulusControllerRegistry.DeriveIdentifier(input);

        result.Should().BeNull();
    }

    [Fact]
    public void DeriveIdentifier_ReturnsNull_ForEmptyPrefix()
    {
        var result = StimulusControllerRegistry.DeriveIdentifier("_controller.js");

        result.Should().BeNull();
    }

    [Fact]
    public void DeriveIdentifier_HandlesBackslashes()
    {
        var result = StimulusControllerRegistry.DeriveIdentifier("admin\\users_controller.js");

        result.Should().Be("admin--users");
    }

    #endregion

    #region Rebuild Tests

    [Fact]
    public void Rebuild_DiscoversControllers()
    {
        var registry = new StimulusControllerRegistry();
        var fileProvider = CreateMockFileProvider(new[]
        {
            "hello_controller.js",
            "goodbye_controller.js"
        });

        registry.Rebuild(fileProvider, "controllers", _logger);

        registry.Controllers.Should().HaveCount(2);
        registry.Controllers.Should().Contain(c => c.StimulusIdentifier == "hello");
        registry.Controllers.Should().Contain(c => c.StimulusIdentifier == "goodbye");
    }

    [Fact]
    public void Rebuild_SkipsNonControllerFiles()
    {
        var registry = new StimulusControllerRegistry();
        var fileProvider = CreateMockFileProvider(new[]
        {
            "hello_controller.js",
            "utils.js",
            "styles.css"
        });

        registry.Rebuild(fileProvider, "controllers", _logger);

        registry.Controllers.Should().HaveCount(1);
        registry.Controllers[0].StimulusIdentifier.Should().Be("hello");
    }

    [Fact]
    public void Rebuild_SetsImportPaths()
    {
        var registry = new StimulusControllerRegistry();
        var fileProvider = CreateMockFileProvider(new[]
        {
            "hello_controller.js"
        });

        registry.Rebuild(fileProvider, "controllers", _logger);

        registry.Controllers[0].ImportPath.Should().Be("/controllers/hello_controller.js");
    }

    [Fact]
    public void Rebuild_GeneratesETag()
    {
        var registry = new StimulusControllerRegistry();
        var fileProvider = CreateMockFileProvider(new[]
        {
            "hello_controller.js"
        });

        registry.Rebuild(fileProvider, "controllers", _logger);

        registry.ETag.Should().NotBeNullOrEmpty();
        registry.ETag.Should().StartWith("\"").And.EndWith("\"");
    }

    [Fact]
    public void Rebuild_ChangesETag_WhenControllersChange()
    {
        var registry = new StimulusControllerRegistry();

        var fileProvider1 = CreateMockFileProvider(new[] { "hello_controller.js" });
        registry.Rebuild(fileProvider1, "controllers", _logger);
        var etag1 = registry.ETag;

        var fileProvider2 = CreateMockFileProvider(new[] { "hello_controller.js", "goodbye_controller.js" });
        registry.Rebuild(fileProvider2, "controllers", _logger);
        var etag2 = registry.ETag;

        etag2.Should().NotBe(etag1);
    }

    [Fact]
    public void Rebuild_GeneratesIndexModule_WithImportsAndRegistrations()
    {
        var registry = new StimulusControllerRegistry();
        var fileProvider = CreateMockFileProvider(new[]
        {
            "hello_controller.js"
        });

        registry.Rebuild(fileProvider, "controllers", _logger);

        var module = registry.GeneratedIndexModule;
        module.Should().Contain("import { application }");
        module.Should().Contain("import HelloController from \"/controllers/hello_controller.js\"");
        module.Should().Contain("application.register(\"hello\", HelloController)");
    }

    [Fact]
    public void Rebuild_GeneratesIndexModule_ForNamespacedControllers()
    {
        var registry = new StimulusControllerRegistry();
        var fileProvider = CreateMockFileProviderWithSubdirectory(
            "controllers",
            "admin",
            new[] { "users_controller.js" });

        registry.Rebuild(fileProvider, "controllers", _logger);

        var module = registry.GeneratedIndexModule;
        module.Should().Contain("import AdminUsersController from \"/controllers/admin/users_controller.js\"");
        module.Should().Contain("application.register(\"admin--users\", AdminUsersController)");
    }

    [Fact]
    public void Rebuild_EmptyDirectory_GeneratesCommentOnlyModule()
    {
        var registry = new StimulusControllerRegistry();
        var fileProvider = CreateMockFileProvider(Array.Empty<string>());

        registry.Rebuild(fileProvider, "controllers", _logger);

        registry.Controllers.Should().BeEmpty();
        registry.GeneratedIndexModule.Should().Contain("No Stimulus controllers discovered");
    }

    #endregion

    #region Helpers

    private static IFileProvider CreateMockFileProvider(string[] fileNames)
    {
        var mockProvider = new Mock<IFileProvider>();
        var files = fileNames
            .Where(f => !f.Contains('/'))
            .Select(name =>
            {
                var mockFile = new Mock<IFileInfo>();
                mockFile.Setup(f => f.Name).Returns(name);
                mockFile.Setup(f => f.IsDirectory).Returns(false);
                mockFile.Setup(f => f.Exists).Returns(true);
                return mockFile.Object;
            })
            .ToList();

        var mockContents = new Mock<IDirectoryContents>();
        mockContents.Setup(d => d.Exists).Returns(true);
        mockContents.Setup(d => d.GetEnumerator()).Returns(() => files.GetEnumerator());

        mockProvider.Setup(p => p.GetDirectoryContents("controllers"))
            .Returns(mockContents.Object);

        return mockProvider.Object;
    }

    private static IFileProvider CreateMockFileProviderWithSubdirectory(
        string basePath,
        string subdirectory,
        string[] fileNames)
    {
        var mockProvider = new Mock<IFileProvider>();

        // Root directory contains just the subdirectory
        var subDirInfo = new Mock<IFileInfo>();
        subDirInfo.Setup(f => f.Name).Returns(subdirectory);
        subDirInfo.Setup(f => f.IsDirectory).Returns(true);
        subDirInfo.Setup(f => f.Exists).Returns(true);

        var rootContents = new Mock<IDirectoryContents>();
        rootContents.Setup(d => d.Exists).Returns(true);
        var rootItems = new List<IFileInfo> { subDirInfo.Object };
        rootContents.Setup(d => d.GetEnumerator()).Returns(() => rootItems.GetEnumerator());

        mockProvider.Setup(p => p.GetDirectoryContents(basePath))
            .Returns(rootContents.Object);

        // Subdirectory contains the files
        var files = fileNames.Select(name =>
        {
            var mockFile = new Mock<IFileInfo>();
            mockFile.Setup(f => f.Name).Returns(name);
            mockFile.Setup(f => f.IsDirectory).Returns(false);
            mockFile.Setup(f => f.Exists).Returns(true);
            return mockFile.Object;
        }).ToList();

        var subContents = new Mock<IDirectoryContents>();
        subContents.Setup(d => d.Exists).Returns(true);
        subContents.Setup(d => d.GetEnumerator()).Returns(() => files.GetEnumerator());

        mockProvider.Setup(p => p.GetDirectoryContents($"{basePath}/{subdirectory}"))
            .Returns(subContents.Object);

        return mockProvider.Object;
    }

    #endregion
}
