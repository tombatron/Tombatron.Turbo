using System.Collections.Immutable;

namespace Tombatron.Turbo.SourceGenerator.Models;

/// <summary>
/// Represents a Razor file and its parsed turbo-frame elements.
/// </summary>
/// <param name="FilePath">The path to the Razor file relative to the project.</param>
/// <param name="ViewName">The logical view name derived from the file path.</param>
/// <param name="Frames">The turbo-frame elements found in this file.</param>
public sealed record RazorFileInfo(
    string FilePath,
    string ViewName,
    ImmutableArray<TurboFrame> Frames)
{
    /// <summary>
    /// Gets whether this file contains any turbo-frame elements.
    /// </summary>
    public bool HasFrames => Frames.Length > 0;

    /// <summary>
    /// Gets the static frames (frames with non-dynamic IDs).
    /// </summary>
    public IEnumerable<TurboFrame> StaticFrames => Frames.Where(f => !f.IsDynamic);

    /// <summary>
    /// Gets the dynamic frames (frames with Razor expressions in IDs).
    /// </summary>
    public IEnumerable<TurboFrame> DynamicFrames => Frames.Where(f => f.IsDynamic);
}
