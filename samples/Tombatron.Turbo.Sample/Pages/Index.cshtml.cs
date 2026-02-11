using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tombatron.Turbo.Middleware;

namespace Tombatron.Turbo.Sample.Pages;

public class IndexModel : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnGetRefreshWelcome()
    {
        // For Turbo-Frame requests, return just the partial
        if (Request.Headers.ContainsKey(TurboFrameMiddleware.TurboFrameHeader))
        {
            return Partial("_WelcomeMessage");
        }

        // For regular requests, redirect to the full page
        return RedirectToPage();
    }
}
