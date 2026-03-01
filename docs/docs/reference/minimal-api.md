---
title: Minimal API
sidebar_label: Minimal API
sidebar_position: 5
description: Return partials from Minimal API endpoints with TurboResults.
---

Return partials from Minimal API endpoints using `TurboResults`:

```csharp
app.MapGet("/items", (HttpContext ctx) =>
{
    if (ctx.IsTurboFrameRequest())
    {
        return TurboResults.Partial("_Items", model);
    }
    return Results.Redirect("/");
});
```

## Validation Failures

Return HTTP 422 for inline form validation errors:

```csharp
app.MapPost("/contact", async (HttpContext context, string? name, string? email) =>
{
    var errors = new Dictionary<string, string>();

    if (string.IsNullOrWhiteSpace(name))
    {
        errors["Name"] = "Name is required.";
    }

    if (errors.Count > 0)
    {
        return TurboResults.ValidationFailure("_ContactForm", new { Name = name, Errors = errors });
    }

    return TurboResults.Partial("_ContactSuccess");
});
```

## Available Methods

```csharp
// Render a partial
TurboResults.Partial("_PartialName");
TurboResults.Partial("_PartialName", model);

// Render a partial with 422 status
TurboResults.ValidationFailure("_FormPartial");
TurboResults.ValidationFailure("_FormPartial", model);

// Using source-generated PartialTemplate references
TurboResults.Partial(Partials.Items, model);
TurboResults.ValidationFailure(Partials.FormPartial, model);
```

For more details, see the [Form Validation Guide](../guides/form-validation.md).
