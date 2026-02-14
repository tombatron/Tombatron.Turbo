using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Tombatron.Turbo.Rendering;

namespace Tombatron.Turbo.Results;

/// <summary>
/// An <see cref="IResult"/> that renders a Razor partial view and writes the HTML to the response.
/// </summary>
public sealed class TurboPartialResult : IResult
{
    private readonly string _partialName;
    private readonly object? _model;

    internal TurboPartialResult(string partialName, object? model = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(partialName);

        _partialName = partialName;
        _model = model;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var renderer = httpContext.RequestServices.GetRequiredService<IPartialRenderer>();
        var html = await renderer.RenderAsync(_partialName, _model);

        httpContext.Response.ContentType = "text/html; charset=utf-8";
        await httpContext.Response.WriteAsync(html);
    }
}
