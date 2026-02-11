using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tombatron.Turbo.Chat.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        // If user already has a username, redirect to chat
        var username = HttpContext.Session.GetString("Username");
        if (!string.IsNullOrEmpty(username))
        {
            return RedirectToPage("/Room", new { id = "general" });
        }

        return Page();
    }

    public IActionResult OnPost(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Page();
        }

        // Store username in session
        HttpContext.Session.SetString("Username", username.Trim());

        return RedirectToPage("/Room", new { id = "general" });
    }
}
