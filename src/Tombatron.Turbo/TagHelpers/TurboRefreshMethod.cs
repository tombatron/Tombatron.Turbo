namespace Tombatron.Turbo.TagHelpers;

/// <summary>
/// Specifies the method used for Turbo page refreshes.
/// </summary>
public enum TurboRefreshMethod
{
    /// <summary>
    /// Uses morphing to update the page, preserving DOM state where possible.
    /// </summary>
    Morph,

    /// <summary>
    /// Replaces the entire page content (default Turbo behavior).
    /// </summary>
    Replace
}
