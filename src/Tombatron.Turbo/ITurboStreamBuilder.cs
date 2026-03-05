namespace Tombatron.Turbo;

/// <summary>
/// Builder interface for constructing Turbo Stream actions.
/// </summary>
/// <remarks>
/// This interface provides a fluent API for building Turbo Stream updates.
/// Multiple actions can be chained together and will be sent as a single message.
/// </remarks>
public interface ITurboStreamBuilder
{
    /// <summary>
    /// Appends content to the end of the target element.
    /// </summary>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="html">The HTML content to append.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when target or html is null.</exception>
    /// <exception cref="ArgumentException">Thrown when target is empty or whitespace.</exception>
    ITurboStreamBuilder Append(string target, string html);

    /// <summary>
    /// Prepends content to the beginning of the target element.
    /// </summary>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="html">The HTML content to prepend.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when target or html is null.</exception>
    /// <exception cref="ArgumentException">Thrown when target is empty or whitespace.</exception>
    ITurboStreamBuilder Prepend(string target, string html);

    /// <summary>
    /// Replaces the target element entirely with the provided content.
    /// </summary>
    /// <param name="target">The DOM ID of the target element to replace.</param>
    /// <param name="html">The HTML content to replace with.</param>
    /// <param name="morph">When true, uses morphing to update the element preserving DOM state.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when target or html is null.</exception>
    /// <exception cref="ArgumentException">Thrown when target is empty or whitespace.</exception>
    ITurboStreamBuilder Replace(string target, string html, bool morph = false);

    /// <summary>
    /// Updates the inner content of the target element without replacing the element itself.
    /// </summary>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="html">The HTML content to set as inner content.</param>
    /// <param name="morph">When true, uses morphing to update the element preserving DOM state.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when target or html is null.</exception>
    /// <exception cref="ArgumentException">Thrown when target is empty or whitespace.</exception>
    ITurboStreamBuilder Update(string target, string html, bool morph = false);

    /// <summary>
    /// Removes the target element from the DOM.
    /// </summary>
    /// <param name="target">The DOM ID of the target element to remove.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when target is null.</exception>
    /// <exception cref="ArgumentException">Thrown when target is empty or whitespace.</exception>
    ITurboStreamBuilder Remove(string target);

    /// <summary>
    /// Inserts content immediately before the target element.
    /// </summary>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="html">The HTML content to insert before.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when target or html is null.</exception>
    /// <exception cref="ArgumentException">Thrown when target is empty or whitespace.</exception>
    ITurboStreamBuilder Before(string target, string html);

    /// <summary>
    /// Inserts content immediately after the target element.
    /// </summary>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="html">The HTML content to insert after.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when target or html is null.</exception>
    /// <exception cref="ArgumentException">Thrown when target is empty or whitespace.</exception>
    ITurboStreamBuilder After(string target, string html);

    /// <summary>
    /// Tells clients to perform a page refresh. Optionally includes a request ID
    /// so the originating client can suppress the redundant refresh.
    /// </summary>
    /// <param name="requestId">The X-Turbo-Request-Id of the originating request, or null for no suppression.</param>
    /// <param name="morph">When true, uses morphing for the refresh instead of full page replacement.</param>
    /// <param name="preserveScroll">When true, preserves scroll position during the refresh.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ITurboStreamBuilder Refresh(string? requestId = null, bool morph = false, bool preserveScroll = false);

    /// <summary>
    /// Appends content to the end of all elements matching the CSS selector.
    /// </summary>
    /// <param name="targets">A CSS selector matching the target elements.</param>
    /// <param name="html">The HTML content to append.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ITurboStreamBuilder AppendAll(string targets, string html);

    /// <summary>
    /// Prepends content to the beginning of all elements matching the CSS selector.
    /// </summary>
    /// <param name="targets">A CSS selector matching the target elements.</param>
    /// <param name="html">The HTML content to prepend.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ITurboStreamBuilder PrependAll(string targets, string html);

    /// <summary>
    /// Replaces all elements matching the CSS selector with the provided content.
    /// </summary>
    /// <param name="targets">A CSS selector matching the target elements.</param>
    /// <param name="html">The HTML content to replace with.</param>
    /// <param name="morph">When true, uses morphing to update the elements preserving DOM state.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ITurboStreamBuilder ReplaceAll(string targets, string html, bool morph = false);

    /// <summary>
    /// Updates the inner content of all elements matching the CSS selector.
    /// </summary>
    /// <param name="targets">A CSS selector matching the target elements.</param>
    /// <param name="html">The HTML content to set as inner content.</param>
    /// <param name="morph">When true, uses morphing to update the elements preserving DOM state.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ITurboStreamBuilder UpdateAll(string targets, string html, bool morph = false);

    /// <summary>
    /// Removes all elements matching the CSS selector from the DOM.
    /// </summary>
    /// <param name="targets">A CSS selector matching the target elements.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ITurboStreamBuilder RemoveAll(string targets);

    /// <summary>
    /// Inserts content immediately before all elements matching the CSS selector.
    /// </summary>
    /// <param name="targets">A CSS selector matching the target elements.</param>
    /// <param name="html">The HTML content to insert before.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ITurboStreamBuilder BeforeAll(string targets, string html);

    /// <summary>
    /// Inserts content immediately after all elements matching the CSS selector.
    /// </summary>
    /// <param name="targets">A CSS selector matching the target elements.</param>
    /// <param name="html">The HTML content to insert after.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ITurboStreamBuilder AfterAll(string targets, string html);

    /// <summary>
    /// Builds the final Turbo Stream HTML containing all configured actions.
    /// </summary>
    /// <returns>The complete Turbo Stream HTML string.</returns>
    string Build();
}
