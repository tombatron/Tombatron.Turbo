using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tombatron.Turbo.Chat.Pages;

public class LoginModel : PageModel
{
    private readonly ChatService _chatService;

    public LoginModel(ChatService chatService)
    {
        _chatService = chatService;
    }

    public string? ErrorMessage { get; set; }
    public string? Username { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Room", new { id = 1 });
        }

        return Page();
    }

    public async Task<IActionResult> OnPost(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Username and password are required.";
            Username = username;
            return Page();
        }

        var user = await _chatService.ValidateCredentials(username.Trim(), password);

        if (user is null)
        {
            ErrorMessage = "Invalid username or password.";
            Username = username;
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToPage("/Room", new { id = 1 });
    }
}
