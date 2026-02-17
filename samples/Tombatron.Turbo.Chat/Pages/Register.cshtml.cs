using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tombatron.Turbo.Generated;
using Tombatron.Turbo.Streams;

namespace Tombatron.Turbo.Chat.Pages;

public class RegisterModel : PageModel
{
    private readonly ChatService _chatService;
    private readonly ITurbo _turbo;

    public RegisterModel(ChatService chatService, ITurbo turbo)
    {
        _chatService = chatService;
        _turbo = turbo;
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

    public async Task<IActionResult> OnPost(string username, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Username and password are required.";
            Username = username;
            return Page();
        }

        if (password != confirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            Username = username;
            return Page();
        }

        var existing = await _chatService.GetUserByUsername(username.Trim());
        if (existing is not null)
        {
            ErrorMessage = "Username is already taken.";
            Username = username;
            return Page();
        }

        var user = await _chatService.RegisterUser(username.Trim(), password);

        // Auto-join default rooms
        await _chatService.JoinRoom(1, user.Id);
        await _chatService.JoinRoom(2, user.Id);
        await _chatService.JoinRoom(3, user.Id);

        // Broadcast new user to all connected clients' user lists
        await _turbo.Broadcast(async builder =>
        {
            await builder.AppendAsync("user-list", Partials.UserEntry, user);
        });

        // Sign in
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
