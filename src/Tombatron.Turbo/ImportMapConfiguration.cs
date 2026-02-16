namespace Tombatron.Turbo;

/// <summary>
/// Configuration for JavaScript module import maps, modeled after Ruby's importmap-rails.
/// </summary>
public sealed class ImportMapConfiguration
{
    private readonly Dictionary<string, ImportMapEntry> _entries = new(StringComparer.Ordinal);

    /// <summary>Pin a module specifier to a URL.</summary>
    /// <param name="name">The module specifier (e.g. "@hotwired/turbo").</param>
    /// <param name="path">The URL or path the specifier resolves to.</param>
    /// <param name="preload">Whether to preload the module and auto-import it.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public ImportMapConfiguration Pin(string name, string path, bool preload = false)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(path);

        _entries[name] = new ImportMapEntry(path, preload);

        return this;
    }

    /// <summary>Remove a previously pinned module.</summary>
    /// <param name="name">The module specifier to remove.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public ImportMapConfiguration Unpin(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        _entries.Remove(name);

        return this;
    }

    /// <summary>All configured entries (for tag helper rendering).</summary>
    public IReadOnlyDictionary<string, ImportMapEntry> Entries => _entries;
}

/// <summary>
/// Represents a single entry in the import map.
/// </summary>
/// <param name="Path">The URL or path the module specifier resolves to.</param>
/// <param name="Preload">Whether to preload the module and auto-import it.</param>
public readonly record struct ImportMapEntry(string Path, bool Preload);
