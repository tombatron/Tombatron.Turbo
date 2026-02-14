using Microsoft.AspNetCore.Http;
using Tombatron.Turbo.Rendering;

namespace Tombatron.Turbo.Results;

/// <summary>
/// Factory methods for creating <see cref="IResult"/> instances that render Razor partial views.
/// </summary>
public static class TurboResults
{
    /// <summary>
    /// Creates an <see cref="IResult"/> that renders the specified partial view.
    /// </summary>
    public static IResult Partial(string partialName) =>
        new TurboPartialResult(partialName);

    /// <summary>
    /// Creates an <see cref="IResult"/> that renders the specified partial view with the given model.
    /// </summary>
    public static IResult Partial(string partialName, object model) =>
        new TurboPartialResult(partialName, model);

    /// <summary>
    /// Creates an <see cref="IResult"/> that renders the specified <see cref="PartialTemplate"/>.
    /// </summary>
    public static IResult Partial(PartialTemplate template) =>
        new TurboPartialResult(template.ViewPath);

    /// <summary>
    /// Creates an <see cref="IResult"/> that renders the specified <see cref="PartialTemplate{TModel}"/> with the given model.
    /// </summary>
    public static IResult Partial<TModel>(PartialTemplate<TModel> template, TModel model) =>
        new TurboPartialResult(template.ViewPath, model);
}
