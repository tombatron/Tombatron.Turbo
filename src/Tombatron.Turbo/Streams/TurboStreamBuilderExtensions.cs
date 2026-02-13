using Tombatron.Turbo.Rendering;

namespace Tombatron.Turbo.Streams;

/// <summary>
/// Extension methods for <see cref="ITurboStreamBuilder"/> that provide async partial rendering support.
/// </summary>
public static class TurboStreamBuilderExtensions
{
    /// <summary>
    /// Appends rendered partial content to the end of the target element.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="renderer">The partial renderer service.</param>
    /// <param name="partialName">The name or path of the partial view.</param>
    /// <param name="model">Optional model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    public static async Task<ITurboStreamBuilder> AppendAsync(
        this ITurboStreamBuilder builder,
        string target,
        IPartialRenderer renderer,
        string partialName,
        object? model = null)
    {
        string html = await renderer.RenderAsync(partialName, model);
        return builder.Append(target, html);
    }

    /// <summary>
    /// Prepends rendered partial content to the beginning of the target element.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="renderer">The partial renderer service.</param>
    /// <param name="partialName">The name or path of the partial view.</param>
    /// <param name="model">Optional model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    public static async Task<ITurboStreamBuilder> PrependAsync(
        this ITurboStreamBuilder builder,
        string target,
        IPartialRenderer renderer,
        string partialName,
        object? model = null)
    {
        string html = await renderer.RenderAsync(partialName, model);
        return builder.Prepend(target, html);
    }

    /// <summary>
    /// Replaces the target element entirely with rendered partial content.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element to replace.</param>
    /// <param name="renderer">The partial renderer service.</param>
    /// <param name="partialName">The name or path of the partial view.</param>
    /// <param name="model">Optional model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    public static async Task<ITurboStreamBuilder> ReplaceAsync(
        this ITurboStreamBuilder builder,
        string target,
        IPartialRenderer renderer,
        string partialName,
        object? model = null)
    {
        string html = await renderer.RenderAsync(partialName, model);
        return builder.Replace(target, html);
    }

    /// <summary>
    /// Updates the inner content of the target element with rendered partial content.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="renderer">The partial renderer service.</param>
    /// <param name="partialName">The name or path of the partial view.</param>
    /// <param name="model">Optional model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    public static async Task<ITurboStreamBuilder> UpdateAsync(
        this ITurboStreamBuilder builder,
        string target,
        IPartialRenderer renderer,
        string partialName,
        object? model = null)
    {
        string html = await renderer.RenderAsync(partialName, model);
        return builder.Update(target, html);
    }

    /// <summary>
    /// Inserts rendered partial content immediately before the target element.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="renderer">The partial renderer service.</param>
    /// <param name="partialName">The name or path of the partial view.</param>
    /// <param name="model">Optional model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    public static async Task<ITurboStreamBuilder> BeforeAsync(
        this ITurboStreamBuilder builder,
        string target,
        IPartialRenderer renderer,
        string partialName,
        object? model = null)
    {
        string html = await renderer.RenderAsync(partialName, model);
        return builder.Before(target, html);
    }

    /// <summary>
    /// Inserts rendered partial content immediately after the target element.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="renderer">The partial renderer service.</param>
    /// <param name="partialName">The name or path of the partial view.</param>
    /// <param name="model">Optional model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    public static async Task<ITurboStreamBuilder> AfterAsync(
        this ITurboStreamBuilder builder,
        string target,
        IPartialRenderer renderer,
        string partialName,
        object? model = null)
    {
        string html = await renderer.RenderAsync(partialName, model);
        return builder.After(target, html);
    }

    // PartialTemplate<TModel> overloads that use the injected renderer

    /// <summary>
    /// Appends rendered partial content to the end of the target element using a PartialTemplate.
    /// </summary>
    /// <typeparam name="TModel">The model type for the partial.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="template">The partial template.</param>
    /// <param name="model">The model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when IPartialRenderer is not available. Use the async Stream() overload.</exception>
    public static async Task<ITurboStreamBuilder> AppendAsync<TModel>(
        this ITurboStreamBuilder builder,
        string target,
        PartialTemplate<TModel> template,
        TModel model)
    {
        var renderer = GetRenderer(builder);
        string html = await renderer.RenderAsync<TModel>(template.ViewPath, model);
        return builder.Append(target, html);
    }

    /// <summary>
    /// Prepends rendered partial content to the beginning of the target element using a PartialTemplate.
    /// </summary>
    /// <typeparam name="TModel">The model type for the partial.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="template">The partial template.</param>
    /// <param name="model">The model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when IPartialRenderer is not available. Use the async Stream() overload.</exception>
    public static async Task<ITurboStreamBuilder> PrependAsync<TModel>(
        this ITurboStreamBuilder builder,
        string target,
        PartialTemplate<TModel> template,
        TModel model)
    {
        var renderer = GetRenderer(builder);
        string html = await renderer.RenderAsync<TModel>(template.ViewPath, model);
        return builder.Prepend(target, html);
    }

    /// <summary>
    /// Replaces the target element entirely with rendered partial content using a PartialTemplate.
    /// </summary>
    /// <typeparam name="TModel">The model type for the partial.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element to replace.</param>
    /// <param name="template">The partial template.</param>
    /// <param name="model">The model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when IPartialRenderer is not available. Use the async Stream() overload.</exception>
    public static async Task<ITurboStreamBuilder> ReplaceAsync<TModel>(
        this ITurboStreamBuilder builder,
        string target,
        PartialTemplate<TModel> template,
        TModel model)
    {
        var renderer = GetRenderer(builder);
        string html = await renderer.RenderAsync<TModel>(template.ViewPath, model);
        return builder.Replace(target, html);
    }

    /// <summary>
    /// Updates the inner content of the target element with rendered partial content using a PartialTemplate.
    /// </summary>
    /// <typeparam name="TModel">The model type for the partial.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="template">The partial template.</param>
    /// <param name="model">The model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when IPartialRenderer is not available. Use the async Stream() overload.</exception>
    public static async Task<ITurboStreamBuilder> UpdateAsync<TModel>(
        this ITurboStreamBuilder builder,
        string target,
        PartialTemplate<TModel> template,
        TModel model)
    {
        var renderer = GetRenderer(builder);
        string html = await renderer.RenderAsync<TModel>(template.ViewPath, model);
        return builder.Update(target, html);
    }

    /// <summary>
    /// Inserts rendered partial content immediately before the target element using a PartialTemplate.
    /// </summary>
    /// <typeparam name="TModel">The model type for the partial.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="template">The partial template.</param>
    /// <param name="model">The model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when IPartialRenderer is not available. Use the async Stream() overload.</exception>
    public static async Task<ITurboStreamBuilder> BeforeAsync<TModel>(
        this ITurboStreamBuilder builder,
        string target,
        PartialTemplate<TModel> template,
        TModel model)
    {
        var renderer = GetRenderer(builder);
        string html = await renderer.RenderAsync<TModel>(template.ViewPath, model);
        return builder.Before(target, html);
    }

    /// <summary>
    /// Inserts rendered partial content immediately after the target element using a PartialTemplate.
    /// </summary>
    /// <typeparam name="TModel">The model type for the partial.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="template">The partial template.</param>
    /// <param name="model">The model to pass to the partial view.</param>
    /// <returns>A task that resolves to the builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when IPartialRenderer is not available. Use the async Stream() overload.</exception>
    public static async Task<ITurboStreamBuilder> AfterAsync<TModel>(
        this ITurboStreamBuilder builder,
        string target,
        PartialTemplate<TModel> template,
        TModel model)
    {
        var renderer = GetRenderer(builder);
        string html = await renderer.RenderAsync<TModel>(template.ViewPath, model);
        return builder.After(target, html);
    }

    /// <summary>
    /// Gets the IPartialRenderer from the builder instance.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The IPartialRenderer instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when IPartialRenderer is not available.</exception>
    private static IPartialRenderer GetRenderer(ITurboStreamBuilder builder)
    {
        if (builder is TurboStreamBuilder tsb && tsb.Renderer != null)
        {
            return tsb.Renderer;
        }

        throw new InvalidOperationException(
            "IPartialRenderer not available. Use the async Stream() overload that accepts Func<ITurboStreamBuilder, Task>.");
    }
}
