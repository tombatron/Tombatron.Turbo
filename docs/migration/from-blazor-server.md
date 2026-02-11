# Migrating from Blazor Server

This guide helps you migrate from Blazor Server to Tombatron.Turbo while preserving real-time interactivity.

## Overview

| Blazor Server | Tombatron.Turbo |
|---------------|-----------------|
| C# components with SignalR | HTML + Turbo.js with SignalR |
| Full component re-render | Targeted DOM updates |
| Stateful connections | Stateless HTTP + optional streams |
| ~200KB+ initial payload | ~30KB (Turbo + SignalR) |

## Key Differences

### Component Model

**Blazor Server:**
```razor
@page "/counter"

<h1>Counter</h1>
<p>Current count: @currentCount</p>
<button @onclick="IncrementCount">Click me</button>

@code {
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}
```

**Tombatron.Turbo (Form-based):**
```html
@page
@model CounterModel

<h1>Counter</h1>
<turbo-frame id="counter">
    <p>Current count: @Model.Count</p>
    <form method="post" asp-page-handler="Increment">
        <button type="submit">Click me</button>
    </form>
</turbo-frame>
```

```csharp
public class CounterModel : PageModel
{
    private static int _count = 0; // Use session/database in real apps

    public int Count => _count;

    public IActionResult OnPostIncrement()
    {
        _count++;

        if (HttpContext.IsTurboFrameRequest())
        {
            return Partial("_Counter", this);
        }

        return RedirectToPage();
    }
}
```

### Real-Time Updates

**Blazor Server:**
```csharp
// Updates automatically sync via SignalR circuit
@inject NotificationService Notifications

@foreach (var notification in Notifications.Items)
{
    <div>@notification.Message</div>
}

@code {
    protected override void OnInitialized()
    {
        Notifications.OnChange += StateHasChanged;
    }
}
```

**Tombatron.Turbo:**
```html
<!-- Subscribe to updates -->
<turbo stream="notifications"></turbo>

<!-- Target for updates -->
<div id="notification-list">
    @foreach (var notification in Model.Notifications)
    {
        <div>@notification.Message</div>
    }
</div>
```

```csharp
// Server-side broadcast
public class NotificationService
{
    private readonly ITurbo _turbo;

    public async Task AddNotification(string message)
    {
        // Save notification...

        await _turbo.Stream("notifications", builder =>
        {
            builder.Append("notification-list", $"<div>{message}</div>");
        });
    }
}
```

## Migration Strategies

### Strategy 1: Gradual Migration

Keep Blazor for complex interactive components, use Turbo for simpler pages.

```csharp
// Program.cs
builder.Services.AddServerSideBlazor();
builder.Services.AddTurbo();

app.MapBlazorHub();
app.MapTurboHub();
app.MapRazorPages();
```

### Strategy 2: Full Migration

Replace Blazor components with Razor Pages + Turbo.

1. Convert Blazor components to Razor Pages
2. Use Turbo Frames for partial updates
3. Use Turbo Streams for real-time features

## Common Patterns

### Two-Way Binding

**Blazor:**
```razor
<input @bind="searchText" @bind:event="oninput" />
```

**Turbo (debounced form submission):**
```html
<turbo-frame id="search-results">
    <form method="get" data-turbo-frame="search-results">
        <input type="search" name="q" value="@Model.Query"
               data-turbo-submit-delay="300" />
    </form>
    @foreach (var result in Model.Results)
    {
        <div>@result.Name</div>
    }
</turbo-frame>
```

### Loading States

**Blazor:**
```razor
@if (isLoading)
{
    <div>Loading...</div>
}
else
{
    <div>@content</div>
}
```

**Turbo:**
```html
<turbo-frame id="content" src="/content" loading="lazy">
    <div>Loading...</div>
</turbo-frame>
```

### Error Handling

**Blazor:**
```razor
<ErrorBoundary>
    <ChildContent>@content</ChildContent>
    <ErrorContent>Something went wrong</ErrorContent>
</ErrorBoundary>
```

**Turbo:**
```html
<!-- Turbo shows response content on errors -->
<!-- Return error message in the frame -->
<turbo-frame id="form-frame">
    <div class="error">Something went wrong</div>
</turbo-frame>
```

### Cascading Values

**Blazor:**
```razor
<CascadingValue Value="@theme">
    <ChildComponent />
</CascadingValue>
```

**Turbo:** Use ViewData, shared layouts, or CSS variables:
```html
<div data-theme="@Model.Theme">
    <!-- Child content inherits theme via CSS -->
</div>
```

## What You Gain

1. **Simpler Mental Model** - Standard HTTP request/response
2. **Better Performance** - Smaller payloads, no persistent connections required
3. **Easier Debugging** - Standard browser DevTools work fully
4. **Progressive Enhancement** - Works without JavaScript (degrades to full page loads)
5. **Horizontal Scaling** - Stateless servers are easier to scale

## What You Lose

1. **Fine-grained reactivity** - No automatic DOM diffing
2. **C# in browser** - Logic moves to server or JavaScript
3. **Component lifecycle** - No `OnInitialized`, `OnParametersSet`, etc.
4. **Built-in validation** - Use standard ASP.NET validation

## Step-by-Step Migration

### 1. Add Turbo to Your Project

```bash
dotnet add package Tombatron.Turbo
```

### 2. Configure Services

```csharp
builder.Services.AddTurbo();

app.UseTurbo();
app.MapTurboHub();
```

### 3. Add Turbo.js

```html
<script type="module" src="https://cdn.jsdelivr.net/npm/@hotwired/turbo@8/dist/turbo.es2017-esm.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8/dist/browser/signalr.min.js"></script>
```

### 4. Convert Components to Pages

For each Blazor component:

1. Create a Razor Page
2. Move logic to PageModel
3. Wrap interactive sections in `<turbo-frame>`
4. Create handler methods for interactions
5. Add stream subscriptions for real-time features

### 5. Update Navigation

Replace `NavLink` with standard links:

```html
<!-- Blazor -->
<NavLink href="/products">Products</NavLink>

<!-- Turbo - just works -->
<a href="/products">Products</a>
```

### 6. Handle State

Move from component state to:
- Session state
- Database
- Distributed cache
- TempData for flash messages

## See Also

- [Turbo Frames Guide](../guides/turbo-frames.md)
- [Turbo Streams Guide](../guides/turbo-streams.md)
- [From HTMX](from-htmx.md)
