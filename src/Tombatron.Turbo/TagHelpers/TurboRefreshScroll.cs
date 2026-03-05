namespace Tombatron.Turbo.TagHelpers;

/// <summary>
/// Specifies the scroll behavior during Turbo page refreshes.
/// </summary>
public enum TurboRefreshScroll
{
    /// <summary>
    /// Preserves scroll position during the refresh.
    /// </summary>
    Preserve,

    /// <summary>
    /// Resets scroll position to the top after the refresh (default Turbo behavior).
    /// </summary>
    Reset
}
