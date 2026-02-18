# Form Validation with Turbo Frames

## Overview

Turbo provides built-in support for server-side form validation through HTTP status codes. When a form inside a `<turbo-frame>` is submitted and the server responds with **HTTP 422 (Unprocessable Entity)**, Turbo automatically replaces the frame's content with the response HTML — no full page reload needed. Validation errors appear inline, preserving the user's context.

## How It Works

1. A user submits a form inside a `<turbo-frame>`
2. The server validates the input
3. If validation fails, the server returns **HTTP 422** with HTML containing the form and error messages
4. Turbo intercepts the 422 response and replaces the frame content in-place
5. If validation passes, the server can return a success message (200) or redirect

The key insight is that Turbo treats 422 as a signal to render the response within the existing frame, rather than navigating away or showing an error page.

## Minimal API Pattern

For Minimal API endpoints, use the `TurboResults.ValidationFailure()` factory methods. These render a partial view with HTTP 422 status:

```csharp
app.MapPost("/contact", async (HttpContext context, string? name, string? email) =>
{
    var errors = new Dictionary<string, string>();

    if (string.IsNullOrWhiteSpace(name))
        errors["Name"] = "Name is required.";
    if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        errors["Email"] = "Valid email is required.";

    if (errors.Count > 0)
    {
        // Returns HTTP 422 — Turbo re-renders the frame with errors
        return TurboResults.ValidationFailure("_ContactForm", new { Name = name, Email = email, Errors = errors });
    }

    // Success — return 200 with success content
    return TurboResults.Partial("_ContactSuccess");
});
```

### Available Overloads

```csharp
// By partial name
TurboResults.ValidationFailure("_FormPartial");
TurboResults.ValidationFailure("_FormPartial", model);

// By PartialTemplate (from source generator)
TurboResults.ValidationFailure(Partials.FormPartial);
TurboResults.ValidationFailure(Partials.FormPartial, model);
```

## Razor Pages Pattern

In Razor Pages, set `Response.StatusCode = 422` before returning `Partial()`:

```csharp
[IgnoreAntiforgeryToken]
public class ContactModel : PageModel
{
    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
    public string? Email { get; set; }

    public Dictionary<string, string> Errors { get; set; } = new();

    public IActionResult OnPostSubmit()
    {
        if (string.IsNullOrWhiteSpace(Name))
            Errors["Name"] = "Name is required.";

        if (string.IsNullOrWhiteSpace(Email))
            Errors["Email"] = "Email is required.";

        if (Errors.Count > 0)
        {
            Response.StatusCode = 422;
            return Partial("_ContactForm", this);
        }

        ContactSubmitted = true;
        return Partial("_ContactForm", this);
    }
}
```

The partial should include the `<turbo-frame>` wrapper so Turbo can match and replace the content:

```html
<!-- Pages/Shared/_ContactForm.cshtml -->
@model ContactModel

<turbo-frame id="contact-form">
    <form method="post" asp-page-handler="Submit">
        <div>
            <label>Name</label>
            <input type="text" name="Name" value="@Model.Name" />
            @if (Model.Errors.ContainsKey("Name"))
            {
                <span class="field-error">@Model.Errors["Name"]</span>
            }
        </div>
        <button type="submit">Submit</button>
    </form>
</turbo-frame>
```

## Lazy Frame Pattern

Turbo frames support lazy loading with `loading="lazy"`. The frame content is fetched from the `src` URL when the frame becomes visible:

```html
<turbo-frame id="feedback-form" src="/Feedback?handler=Form" loading="lazy">
    <p>Loading form...</p>
</turbo-frame>
```

The server endpoint returns a partial containing the `<turbo-frame>` with matching ID:

```csharp
public IActionResult OnGetForm()
{
    return Partial("_FeedbackForm", this);
}
```

This pattern is useful for:
- Deferring the load of non-critical forms until they scroll into view
- Loading forms that require additional data fetching
- Reducing initial page weight

Lazy-loaded forms support the same 422 validation pattern once loaded.

## Complete Example

See the sample app's Validation page for a working demo with two forms:
- **Contact Form** — Standard form-in-frame with 422 validation
- **Feedback Form** — Lazy-loaded form that also uses 422 validation

Run the sample:
```bash
dotnet run --project samples/Tombatron.Turbo.Sample/
```
Then navigate to `/Validation`.

## Detecting Request Type

The library provides extension methods on `HttpContext` to detect Turbo request types:

```csharp
// Check if the request is a Turbo Stream request
// (Accept header contains "text/vnd.turbo-stream.html")
if (context.IsTurboStreamRequest())
{
    return Content(turboStreamHtml, "text/vnd.turbo-stream.html");
}

// Check if the request is a Turbo Frame request
if (context.IsTurboFrameRequest())
{
    return Partial("_FrameContent");
}

// Check for a specific frame ID
if (context.IsTurboFrameRequest("contact-form"))
{
    return Partial("_ContactForm");
}
```

These are useful when the same endpoint needs to return different responses based on how the request was initiated (full page navigation vs. frame request vs. stream request).
