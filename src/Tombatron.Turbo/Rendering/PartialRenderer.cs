using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Tombatron.Turbo.Rendering;

/// <summary>
/// Implementation of <see cref="IPartialRenderer"/> that uses the composite view engine to render partials.
/// </summary>
public sealed class PartialRenderer : IPartialRenderer
{
    private readonly ICompositeViewEngine _viewEngine;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialRenderer"/> class.
    /// </summary>
    /// <param name="viewEngine">The composite view engine.</param>
    /// <param name="tempDataDictionaryFactory">The temp data provider.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public PartialRenderer(
        ICompositeViewEngine viewEngine,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        _tempDataDictionaryFactory = tempDataDictionaryFactory ?? throw new ArgumentNullException(nameof(tempDataDictionaryFactory));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public async Task<string> RenderAsync(string partialName, object? model = null) =>
        await RenderPartialAsync(partialName, model);

    /// <inheritdoc />
    public async Task<string> RenderAsync<TModel>(string partialName, TModel model) =>
        await RenderPartialAsync(partialName, model);

    private async Task<string> RenderPartialAsync(string partialName, object? model)
    {
        ArgumentException.ThrowIfNullOrEmpty(partialName);

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
        if (partialName.StartsWith('/') || partialName.StartsWith("~/"))
        {
            var absoluteResult = _viewEngine.GetView(executingFilePath: null, viewPath: partialName, isMainPage: false);
            if (absoluteResult.Success)
            {
                return absoluteResult.View;
            }

            throw new InvalidOperationException(
                $"Unable to find partial view '{partialName}'. Searched locations: {string.Join(", ", absoluteResult.SearchedLocations)}");
        }

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
