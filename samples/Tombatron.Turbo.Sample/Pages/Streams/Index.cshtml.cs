using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tombatron.Turbo;

namespace Tombatron.Turbo.Sample.Pages.Streams;

public class IndexModel : PageModel
{
    private readonly ITurbo _turbo;

    public IndexModel(ITurbo turbo)
    {
        _turbo = turbo;
    }

    public void OnGet()
    {
    }

    /// <summary>
    /// Send a notification to the demo stream.
    /// </summary>
    public async Task<IActionResult> OnPostSendNotification(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "Hello from the server!";
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        await _turbo.Stream("demo-notifications", builder =>
        {
            builder.Append("notification-list",
                $"<div class=\"stream-notification\"><strong>{timestamp}</strong>: {System.Net.WebUtility.HtmlEncode(message)}</div>");
        });

        // Return 204 No Content to tell Turbo not to navigate
        return new NoContentResult();
    }

    /// <summary>
    /// Update the counter.
    /// </summary>
    public async Task<IActionResult> OnPostIncrementCounter()
    {
        // In a real app, this would be stored in a database or cache
        int counter = HttpContext.Session.GetInt32("counter") ?? 0;
        counter++;
        HttpContext.Session.SetInt32("counter", counter);

        await _turbo.Stream("demo-notifications", builder =>
        {
            builder.Update("counter-value", $"<strong>{counter}</strong>");
        });

        // Return 204 No Content to tell Turbo not to navigate
        return new NoContentResult();
    }

    /// <summary>
    /// Broadcast to all connected clients.
    /// </summary>
    public async Task<IActionResult> OnPostBroadcast(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "Broadcast message to all clients!";
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss");

        await _turbo.Broadcast(builder =>
        {
            builder.Append("broadcast-list",
                $"<div class=\"stream-notification\"><strong>[{timestamp}] Broadcast:</strong> {System.Net.WebUtility.HtmlEncode(message)}</div>");
        });

        // Return 204 No Content to tell Turbo not to navigate
        return new NoContentResult();
    }

    /// <summary>
    /// Clear notifications.
    /// </summary>
    public async Task<IActionResult> OnPostClearNotifications()
    {
        await _turbo.Stream("demo-notifications", builder =>
        {
            builder.Update("notification-list", "");
        });

        // Return 204 No Content to tell Turbo not to navigate
        return new NoContentResult();
    }
}
