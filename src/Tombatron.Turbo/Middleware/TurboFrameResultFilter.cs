using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tombatron.Turbo.Middleware;

/// <summary>
/// Result filter that automatically routes Turbo Frame requests to their sub-templates.
/// </summary>
public class TurboFrameResultFilter : IAsyncResultFilter
{
    /// <inheritdoc />
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (next == null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        // Check if this is a Turbo Frame request with a resolved template
        if (context.HttpContext.Items.TryGetValue(TurboFrameMiddleware.TemplateNameKey, out var templateObj) &&
            templateObj is string templateName)
        {
            // Handle ViewResult (MVC)
            if (context.Result is ViewResult viewResult)
            {
                // Store the original view name and swap to the sub-template
                viewResult.ViewName = templateName;

                // Set ViewBag.TurboFrameId for dynamic frames
                if (context.HttpContext.Items.TryGetValue(TurboFrameMiddleware.FrameIdKey, out var frameIdObj) &&
                    frameIdObj is string frameId)
                {
                    if (viewResult.ViewData != null)
                    {
                        viewResult.ViewData["TurboFrameId"] = frameId;
                    }
                }
            }
            // Handle PageResult (Razor Pages)
            else if (context.Result is PageResult pageResult)
            {
                // For Razor Pages, we need to render a partial view instead
                // This is more complex and may require a custom approach
                // For now, set the ViewBag data
                if (context.HttpContext.Items.TryGetValue(TurboFrameMiddleware.FrameIdKey, out var frameIdObj) &&
                    frameIdObj is string frameId)
                {
                    if (pageResult.ViewData != null)
                    {
                        pageResult.ViewData["TurboFrameId"] = frameId;
                    }
                }
            }
        }

        // Check if this is a Turbo Frame request without a resolved template
        // In this case, we might want to return a 422 Unprocessable Entity
        if (context.HttpContext.Items.TryGetValue(TurboFrameMiddleware.IsTurboFrameRequestKey, out var isTurboFrame) &&
            isTurboFrame is true &&
            !context.HttpContext.Items.ContainsKey(TurboFrameMiddleware.TemplateNameKey))
        {
            // The frame was requested but no template was found
            // Controllers/Pages can handle this manually, or we return 422
            // Only return 422 if the result is a ViewResult or PageResult
            // (not if it's already a ContentResult, JsonResult, etc.)
            if (context.Result is ViewResult or PageResult)
            {
                string? frameId = context.HttpContext.Items.TryGetValue(TurboFrameMiddleware.FrameIdKey, out var fid)
                    ? fid as string
                    : "unknown";

                // Set 422 status and return error message
                context.Result = new ContentResult
                {
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                    ContentType = "text/html",
                    Content = $"<turbo-frame id=\"{System.Web.HttpUtility.HtmlEncode(frameId)}\">Frame not found</turbo-frame>"
                };
            }
        }

        await next();
    }
}
