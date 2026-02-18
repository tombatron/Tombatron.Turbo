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

    /// <summary>
    /// Creates an <see cref="IResult"/> that renders the specified partial view with HTTP 422 status code
    /// for form validation failures. Turbo will replace the frame content in-place when it receives a 422.
    /// </summary>
    public static IResult ValidationFailure(string partialName) =>
        new TurboPartialResult(partialName, statusCode: 422);

    /// <summary>
    /// Creates an <see cref="IResult"/> that renders the specified partial view with the given model
    /// and HTTP 422 status code for form validation failures.
    /// </summary>
    public static IResult ValidationFailure(string partialName, object model) =>
        new TurboPartialResult(partialName, model, statusCode: 422);

    /// <summary>
    /// Creates an <see cref="IResult"/> that renders the specified <see cref="PartialTemplate"/>
    /// with HTTP 422 status code for form validation failures.
    /// </summary>
    public static IResult ValidationFailure(PartialTemplate template) =>
        new TurboPartialResult(template.ViewPath, statusCode: 422);

    /// <summary>
    /// Creates an <see cref="IResult"/> that renders the specified <see cref="PartialTemplate{TModel}"/>
    /// with the given model and HTTP 422 status code for form validation failures.
    /// </summary>
    public static IResult ValidationFailure<TModel>(PartialTemplate<TModel> template, TModel model) =>
        new TurboPartialResult(template.ViewPath, model, statusCode: 422);
}
