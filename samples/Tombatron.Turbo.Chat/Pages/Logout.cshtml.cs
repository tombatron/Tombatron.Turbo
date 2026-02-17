using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tombatron.Turbo.Streams;

namespace Tombatron.Turbo.Chat.Pages;

public class LogoutModel : PageModel
{
    private readonly ITurbo _turbo;

    public LogoutModel(ITurbo turbo)
    {
        _turbo = turbo;
    }

    public async Task<IActionResult> OnGet()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Remove the user from all connected clients' user lists
        if (userId is not null)
        {
            await _turbo.Broadcast(builder =>
            {
                builder.Remove($"user-entry-{userId}");
            });
        }

        return RedirectToPage("/Login");
    }
}
