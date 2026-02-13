using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Tombatron.Turbo.Rendering;

/// <summary>
/// Implementation of <see cref="IPartialRenderer"/> that uses Razor view engine to render partials.
/// </summary>
public sealed class PartialRenderer : IPartialRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialRenderer"/> class.
    /// </summary>
    /// <param name="viewEngine">The Razor view engine.</param>
    /// <param name="tempDataDictionaryFactory">The temp data provider.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public PartialRenderer(
        IRazorViewEngine viewEngine,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        _tempDataDictionaryFactory = tempDataDictionaryFactory ?? throw new ArgumentNullException(nameof(tempDataDictionaryFactory));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public Task<string> RenderAsync(string partialName, object? model = null)
    {
        return RenderPartialAsync(partialName, model);
    }

    /// <inheritdoc />
    public Task<string> RenderAsync<TModel>(string partialName, TModel model)
    {
        return RenderPartialAsync(partialName, model);
    }

    private async Task<string> RenderPartialAsync(string partialName, object? model)
    {
        if (string.IsNullOrEmpty(partialName))
        {
            throw new ArgumentException("Partial name cannot be null or empty.", nameof(partialName));
        }

        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available. Ensure this is called within an HTTP request.");

        var actionContext = new ActionContext(
            httpContext,
            httpContext.GetRouteData(),
            new ActionDescriptor());

        var viewResult = FindView(actionContext, partialName);

        await using var writer = new StringWriter();

        var viewContext = new ViewContext(
            actionContext,
            viewResult,
            new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            },
            _tempDataDictionaryFactory.GetTempData(httpContext),
            writer,
            new HtmlHelperOptions());

        await viewResult.RenderAsync(viewContext);

        return writer.ToString();
    }

    private IView FindView(ActionContext actionContext, string partialName)
    {
        // Try to find as a partial view first
        var partialResult = _viewEngine.FindView(actionContext, partialName, isMainPage: false);
        if (partialResult.Success)
        {
            return partialResult.View;
        }

        // Try to get as a full path view
        var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: partialName, isMainPage: false);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        // Collect search locations for error message
        var searchedLocations = partialResult.SearchedLocations
            .Concat(getViewResult.SearchedLocations)
            .Distinct();

        throw new InvalidOperationException(
            $"Unable to find partial view '{partialName}'. Searched locations: {string.Join(", ", searchedLocations)}");
    }
}
