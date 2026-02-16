using FluentAssertions;
using Xunit;

namespace Tombatron.Turbo.Tests;

public class ImportMapConfigurationTests
{
    #region Pin Tests

    [Fact]
    public void Pin_AddsEntryWithCorrectValues()
    {
        // Arrange
        var config = new ImportMapConfiguration();

        // Act
        config.Pin("@hotwired/turbo", "https://unpkg.com/@hotwired/turbo@8.0.4/dist/turbo.es2017-esm.js", preload: true);

        // Assert
        config.Entries.Should().ContainKey("@hotwired/turbo");
        var entry = config.Entries["@hotwired/turbo"];
        entry.Path.Should().Be("https://unpkg.com/@hotwired/turbo@8.0.4/dist/turbo.es2017-esm.js");
        entry.Preload.Should().BeTrue();
    }

    [Fact]
    public void Pin_OverwritesExistingEntry()
    {
        // Arrange
        var config = new ImportMapConfiguration();
        config.Pin("my-module", "/old-path.js");

        // Act
        config.Pin("my-module", "/new-path.js", preload: true);

        // Assert
        config.Entries.Should().HaveCount(1);
        config.Entries["my-module"].Path.Should().Be("/new-path.js");
        config.Entries["my-module"].Preload.Should().BeTrue();
    }

    [Fact]
    public void Pin_DefaultPreloadIsFalse()
    {
        // Arrange
        var config = new ImportMapConfiguration();

        // Act
        config.Pin("my-module", "/module.js");

        // Assert
        config.Entries["my-module"].Preload.Should().BeFalse();
    }

    [Fact]
    public void Pin_ReturnsThisForFluentChaining()
    {
        // Arrange
        var config = new ImportMapConfiguration();

        // Act
        var result = config.Pin("a", "/a.js").Pin("b", "/b.js").Pin("c", "/c.js");

        // Assert
        result.Should().BeSameAs(config);
        config.Entries.Should().HaveCount(3);
    }

    [Fact]
    public void Pin_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new ImportMapConfiguration();

        // Act
        Action act = () => config.Pin(null!, "/module.js");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("name");
    }

    [Fact]
    public void Pin_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new ImportMapConfiguration();

        // Act
        Action act = () => config.Pin("my-module", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("path");
    }

    #endregion

    #region Unpin Tests

    [Fact]
    public void Unpin_RemovesEntry()
    {
        // Arrange
        var config = new ImportMapConfiguration();
        config.Pin("my-module", "/module.js");

        // Act
        config.Unpin("my-module");

        // Assert
        config.Entries.Should().NotContainKey("my-module");
    }

    [Fact]
    public void Unpin_NonExistentName_DoesNotThrow()
    {
        // Arrange
        var config = new ImportMapConfiguration();

        // Act
        Action act = () => config.Unpin("nonexistent");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Unpin_ReturnsThisForFluentChaining()
    {
        // Arrange
        var config = new ImportMapConfiguration();
        config.Pin("a", "/a.js").Pin("b", "/b.js");

        // Act
        var result = config.Unpin("a");

        // Assert
        result.Should().BeSameAs(config);
    }

    [Fact]
    public void Unpin_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new ImportMapConfiguration();

        // Act
        Action act = () => config.Unpin(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("name");
    }

    #endregion

    #region Entries Tests

    [Fact]
    public void Entries_IsReadOnlyView()
    {
        // Arrange
        var config = new ImportMapConfiguration();
        config.Pin("a", "/a.js");

        // Act
        var entries = config.Entries;

        // Assert
        entries.Should().BeAssignableTo<IReadOnlyDictionary<string, ImportMapEntry>>();
        entries.Should().HaveCount(1);
    }

    [Fact]
    public void Entries_ReflectsMutationsViaPin()
    {
        // Arrange
        var config = new ImportMapConfiguration();
        var entries = config.Entries;

        // Act
        config.Pin("a", "/a.js");

        // Assert
        entries.Should().HaveCount(1);
    }

    #endregion
}
