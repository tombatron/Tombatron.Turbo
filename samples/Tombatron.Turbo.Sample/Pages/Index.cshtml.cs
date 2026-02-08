using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tombatron.Turbo.Sample.Pages;

public class IndexModel : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnGetRefreshWelcome()
    {
        // This handler is called when the "Refresh Frame" button is clicked
        // Turbo will only update the welcome-message frame
        return Page();
    }
}
