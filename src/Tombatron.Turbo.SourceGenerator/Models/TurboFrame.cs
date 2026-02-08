namespace Tombatron.Turbo.SourceGenerator.Models;

/// <summary>
/// Represents a parsed turbo-frame element from a Razor file.
/// </summary>
/// <param name="Id">The frame ID (may contain Razor expressions for dynamic IDs).</param>
/// <param name="Prefix">The asp-frame-prefix value if specified, null otherwise.</param>
/// <param name="Content">The inner HTML/Razor content of the frame.</param>
/// <param name="StartLine">The line number where the frame starts (1-based).</param>
/// <param name="IsDynamic">True if the ID contains Razor expressions (@).</param>
public sealed record TurboFrame(
    string Id,
    string? Prefix,
    string Content,
    int StartLine,
    bool IsDynamic)
{
    /// <summary>
    /// Gets whether this frame has a valid prefix for dynamic ID handling.
    /// </summary>
    public bool HasPrefix => !string.IsNullOrEmpty(Prefix);

    /// <summary>
    /// Gets the static portion of the ID (before any Razor expression).
    /// </summary>
    public string StaticIdPortion
    {
        get
        {
            if (!IsDynamic)
            {
                return Id;
            }

            int atIndex = Id.IndexOf('@');
            if (atIndex <= 0)
            {
                return string.Empty;
            }

            return Id.Substring(0, atIndex);
        }
    }
}
