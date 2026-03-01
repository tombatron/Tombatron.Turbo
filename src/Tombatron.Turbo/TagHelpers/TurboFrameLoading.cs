namespace Tombatron.Turbo.TagHelpers;

/// <summary>
/// Specifies the loading behavior for a turbo-frame element.
/// </summary>
public enum TurboFrameLoading
{
    /// <summary>
    /// Load the frame content immediately (default browser behavior).
    /// </summary>
    Eager,

    /// <summary>
    /// Load the frame content when it scrolls into view.
    /// </summary>
    Lazy
}
