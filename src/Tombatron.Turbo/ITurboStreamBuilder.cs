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
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when target or html is null.</exception>
    /// <exception cref="ArgumentException">Thrown when target is empty or whitespace.</exception>
    ITurboStreamBuilder Replace(string target, string html);

    /// <summary>
    /// Updates the inner content of the target element without replacing the element itself.
    /// </summary>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="html">The HTML content to set as inner content.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when target or html is null.</exception>
    /// <exception cref="ArgumentException">Thrown when target is empty or whitespace.</exception>
    ITurboStreamBuilder Update(string target, string html);

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
    /// Builds the final Turbo Stream HTML containing all configured actions.
    /// </summary>
    /// <returns>The complete Turbo Stream HTML string.</returns>
    string Build();
}
