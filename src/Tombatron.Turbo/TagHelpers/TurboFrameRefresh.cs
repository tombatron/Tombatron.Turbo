namespace Tombatron.Turbo.TagHelpers;

/// <summary>
/// Specifies the refresh behavior for a turbo-frame element during page refreshes (Turbo 8+).
/// </summary>
public enum TurboFrameRefresh
{
    /// <summary>
    /// Replace the frame content entirely on refresh (default behavior).
    /// </summary>
    Replace,

    /// <summary>
    /// Morph the frame content on refresh, preserving DOM state where possible.
    /// </summary>
    Morph
}
