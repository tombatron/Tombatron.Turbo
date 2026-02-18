using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Tombatron.Turbo.Sample.Pages.Validation;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    // Contact form fields
    [BindProperty]
    public string? ContactName { get; set; }

    [BindProperty]
    public string? ContactEmail { get; set; }

    [BindProperty]
    public string? ContactMessage { get; set; }

    public bool ContactSubmitted { get; set; }

    // Feedback form fields
    [BindProperty]
    public string? FeedbackRating { get; set; }

    [BindProperty]
    public string? FeedbackComment { get; set; }

    public bool FeedbackSubmitted { get; set; }

    // Validation error storage
    public Dictionary<string, string> ContactErrors { get; set; } = new();
    public Dictionary<string, string> FeedbackErrors { get; set; } = new();

    public void OnGet()
    {
    }

    public IActionResult OnGetFeedbackForm()
    {
        return Partial("_FeedbackForm", this);
    }

    public IActionResult OnPostSubmitContact()
    {
        ContactErrors.Clear();

        if (string.IsNullOrWhiteSpace(ContactName))
        {
            ContactErrors["ContactName"] = "Name is required.";
        }

        if (string.IsNullOrWhiteSpace(ContactEmail))
        {
            ContactErrors["ContactEmail"] = "Email is required.";
        }
        else if (!ContactEmail.Contains('@'))
        {
            ContactErrors["ContactEmail"] = "Please enter a valid email address.";
        }

        if (string.IsNullOrWhiteSpace(ContactMessage))
        {
            ContactErrors["ContactMessage"] = "Message is required.";
        }
        else if (ContactMessage.Length < 10)
        {
            ContactErrors["ContactMessage"] = "Message must be at least 10 characters.";
        }

        if (ContactErrors.Count > 0)
        {
            Response.StatusCode = 422;
            return Partial("_ContactForm", this);
        }

        ContactSubmitted = true;
        return Partial("_ContactForm", this);
    }

    public IActionResult OnPostSubmitFeedback()
    {
        FeedbackErrors.Clear();

        if (string.IsNullOrWhiteSpace(FeedbackRating))
        {
            FeedbackErrors["FeedbackRating"] = "Please select a rating.";
        }

        if (string.IsNullOrWhiteSpace(FeedbackComment))
        {
            FeedbackErrors["FeedbackComment"] = "Comment is required.";
        }
        else if (FeedbackComment.Length < 5)
        {
            FeedbackErrors["FeedbackComment"] = "Comment must be at least 5 characters.";
        }

        if (FeedbackErrors.Count > 0)
        {
            Response.StatusCode = 422;
            return Partial("_FeedbackForm", this);
        }

        FeedbackSubmitted = true;
        return Partial("_FeedbackForm", this);
    }
}
