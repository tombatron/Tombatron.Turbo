---
title: "Build a Todo List"
sidebar_label: "Build a Todo List"
sidebar_position: 1
description: "Step-by-step tutorial building a todo list with Turbo Frames, form validation, and Stimulus."
---

This walkthrough creates a todo list app from scratch using Turbo Frames for partial page updates and Stimulus for client-side behavior. Each step builds on the previous one.

## Step 1 — Create the project and install packages

```bash
dotnet new webapp -n TurboTodo
cd TurboTodo
dotnet add package Tombatron.Turbo
dotnet add package Tombatron.Turbo.Stimulus
```

## Step 2 — Configure services

Replace the contents of `Program.cs`:

```csharp
using Tombatron.Turbo;
using Tombatron.Turbo.Stimulus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTurbo();
builder.Services.AddStimulus();
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseTurbo();

app.MapRazorPages();
app.MapTurboHub();

app.Run();
```

`AddTurbo()` registers the Turbo services and tag helpers. `AddStimulus()` sets up automatic controller discovery from `wwwroot/controllers/`. `UseTurbo()` adds middleware that sets the `Vary` header on Turbo Frame responses. `MapTurboHub()` exposes the SignalR hub for Turbo Streams.

## Step 3 — Register tag helpers

Add to `Pages/_ViewImports.cshtml`:

```razor
@addTagHelper *, Tombatron.Turbo
```

## Step 4 — Set up the layout

Replace `Pages/Shared/_Layout.cshtml`:

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Todo List</title>
    <turbo-scripts mode="Importmap" />
    <style>
        body { font-family: system-ui, sans-serif; max-width: 600px; margin: 2rem auto; padding: 0 1rem; }
        .todo-item { display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 0; }
        .completed { text-decoration: line-through; opacity: 0.6; }
        .error { color: red; font-size: 0.875rem; }
        input[type="text"] { flex: 1; padding: 0.5rem; font-size: 1rem; }
        button { padding: 0.5rem 1rem; cursor: pointer; }
    </style>
</head>
<body>
    @RenderBody()
</body>
</html>
```

The `<turbo-scripts mode="Importmap" />` tag helper renders Turbo.js, the SignalR bridge, Stimulus, and any discovered controllers via an import map.

## Step 5 — Create the page model

Replace `Pages/Index.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tombatron.Turbo;

namespace TurboTodo.Pages;

public record TodoItem(int Id, string Title, bool IsComplete);

public class IndexModel : PageModel
{
    private static readonly List<TodoItem> _todos = new()
    {
        new(1, "Learn Turbo Frames", false),
        new(2, "Add Stimulus controllers", false)
    };

    private static int _nextId = 3;

    public List<TodoItem> Todos => _todos;
    public string? Error { get; set; }

    public void OnGet() { }

    public IActionResult OnPostAdd(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Error = "Title is required.";
            Response.StatusCode = 422;
            return Partial("_TodoList", this);
        }

        _todos.Add(new TodoItem(_nextId++, title.Trim(), false));

        if (HttpContext.IsTurboFrameRequest())
        {
            return Partial("_TodoList", this);
        }

        return RedirectToPage();
    }

    public IActionResult OnPostToggle(int id)
    {
        var index = _todos.FindIndex(t => t.Id == id);

        if (index >= 0)
        {
            var todo = _todos[index];
            _todos[index] = todo with { IsComplete = !todo.IsComplete };
        }

        if (HttpContext.IsTurboFrameRequest())
        {
            return Partial("_TodoList", this);
        }

        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        _todos.RemoveAll(t => t.Id == id);

        if (HttpContext.IsTurboFrameRequest())
        {
            return Partial("_TodoList", this);
        }

        return RedirectToPage();
    }
}
```

The pattern is straightforward: check `IsTurboFrameRequest()` and return just the partial, or redirect for regular requests. When validation fails, set HTTP 422 so Turbo replaces the frame content in-place.

## Step 6 — Create the page view

Replace `Pages/Index.cshtml`:

```html
@page
@model TurboTodo.Pages.IndexModel

<h1>Todo List</h1>

<partial name="_TodoList" model="Model" />
```

The page renders the `_TodoList` partial, which wraps everything in a `<turbo-frame id="todo-list">`. When a form inside the frame submits, Turbo sends the request with a `Turbo-Frame: todo-list` header and replaces the frame with the partial response.

## Step 7 — Create the todo list partial

Create `Pages/Shared/_TodoList.cshtml`:

```html
@model TurboTodo.Pages.IndexModel

<turbo-frame id="todo-list">
    <form method="post" asp-page-handler="Add"
          data-controller="todo-form"
          data-action="turbo:submit-end->todo-form#reset">
        <div style="display: flex; gap: 0.5rem;">
            <input type="text" name="title" placeholder="What needs to be done?"
                   data-todo-form-target="input" />
            <button type="submit">Add</button>
        </div>
        @if (Model.Error is not null)
        {
            <p class="error">@Model.Error</p>
        }
    </form>

    @foreach (var todo in Model.Todos)
    {
        <div class="todo-item">
            <form method="post" asp-page-handler="Toggle">
                <input type="hidden" name="id" value="@todo.Id" />
                <button type="submit">@(todo.IsComplete ? "✓" : "○")</button>
            </form>
            <span class="@(todo.IsComplete ? "completed" : "")">@todo.Title</span>
            <form method="post" asp-page-handler="Delete">
                <input type="hidden" name="id" value="@todo.Id" />
                <button type="submit">×</button>
            </form>
        </div>
    }
</turbo-frame>
```

The partial wraps everything in a `<turbo-frame>` with the same `id` as the page. When Turbo receives the response, it matches the frame by ID and swaps the content.

If validation fails (HTTP 422), Turbo replaces the frame content with the error markup instead of navigating away.

## Step 8 — Add a Stimulus controller

Create `wwwroot/controllers/todo_form_controller.js`:

```javascript
import { Controller } from "@hotwired/stimulus";

export default class extends Controller {
    static targets = ["input"];

    reset(event) {
        if (event.detail.success) {
            this.inputTarget.value = "";
        }
    }
}
```

This controller clears the input field after a successful submission. The naming convention maps the filename to an identifier: `todo_form_controller.js` becomes `todo-form` (underscores become hyphens, the `_controller.js` suffix is stripped).

The `data-controller="todo-form"` attribute on the form connects it, and `data-action="turbo:submit-end->todo-form#reset"` calls the `reset` method when Turbo finishes the submission.

No manual registration is needed. `AddStimulus()` automatically discovers controllers in `wwwroot/controllers/` and generates the import map entries.

## Step 9 — Run it

```bash
dotnet run
```

Open `https://localhost:5001` (or the port shown in the console). You should be able to add, toggle, and delete todos without full page reloads. The form clears automatically on successful submission thanks to the Stimulus controller. If you submit an empty title, the validation error appears inline.
