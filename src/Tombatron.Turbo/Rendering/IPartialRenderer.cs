namespace Tombatron.Turbo.Rendering;

/// <summary>
/// Service for rendering Razor partial views to strings.
/// </summary>
/// <remarks>
/// Use this interface to render partials for use with Turbo Stream updates.
/// The rendered HTML can be passed directly to ITurboStreamBuilder methods.
/// </remarks>
public interface IPartialRenderer
{
    /// <summary>
    /// Renders a partial view to a string.
    /// </summary>
    /// <param name="partialName">The name or path of the partial view.</param>
    /// <param name="model">Optional model to pass to the partial view.</param>
    /// <returns>The rendered HTML string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the partial view cannot be found.</exception>
    Task<string> RenderAsync(string partialName, object? model = null);

    /// <summary>
    /// Renders a partial view to a string with a strongly-typed model.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="partialName">The name or path of the partial view.</param>
    /// <param name="model">The model to pass to the partial view.</param>
    /// <returns>The rendered HTML string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the partial view cannot be found.</exception>
    Task<string> RenderAsync<TModel>(string partialName, TModel model);
}
