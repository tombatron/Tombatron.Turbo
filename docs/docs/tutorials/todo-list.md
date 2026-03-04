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

app.UseRouting();
app.UseTurbo();

app.MapRazorPages();
app.MapTurboHub();
app.MapStaticAssets();

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
        @@import url('https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600&display=swap');

        * { box-sizing: border-box; margin: 0; padding: 0; }

        body {
            font-family: 'DM Sans', system-ui, sans-serif;
            background: #faf9f7;
            color: #1a1a1a;
            max-width: 600px;
            margin: 3rem auto;
            padding: 0 1rem;
        }

        h1 {
            font-size: 1.75rem;
            font-weight: 600;
            margin-bottom: 0.25rem;
        }

        .subtitle {
            color: #6b6b6b;
            font-size: 0.95rem;
            margin-bottom: 1.5rem;
        }

        .card {
            background: #fff;
            border: 1px solid #e8e5e1;
            border-radius: 12px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.04);
            padding: 1.25rem;
        }

        .add-row {
            display: flex;
            gap: 0.5rem;
        }

        input[type="text"] {
            flex: 1;
            padding: 0.6rem 0.85rem;
            font-family: inherit;
            font-size: 0.95rem;
            border: 1px solid #d4d1cc;
            border-radius: 8px;
            outline: none;
            transition: border-color 0.15s, box-shadow 0.15s;
        }

        input[type="text"]:focus {
            border-color: #0d9488;
            box-shadow: 0 0 0 3px rgba(13,148,136,0.12);
        }

        .btn-add {
            padding: 0.6rem 1.1rem;
            font-family: inherit;
            font-size: 0.95rem;
            font-weight: 500;
            color: #fff;
            background: #0d9488;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            transition: background 0.15s;
        }

        .btn-add:hover { background: #0a7a70; }

        .error {
            display: inline-block;
            margin-top: 0.5rem;
            padding: 0.3rem 0.75rem;
            font-size: 0.85rem;
            color: #9f1b1b;
            background: #fde8e8;
            border-radius: 999px;
        }

        .todo-item {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.55rem 0.35rem;
            border-bottom: 1px solid #f0eeeb;
            transition: background 0.12s;
        }

        .todo-item:last-child { border-bottom: none; }
        .todo-item:hover { background: #faf9f7; border-radius: 6px; }

        .todo-list { margin-top: 0.75rem; }

        .btn-toggle {
            width: 22px;
            height: 22px;
            padding: 0;
            font-size: 0.75rem;
            line-height: 22px;
            text-align: center;
            border: 2px solid #ccc;
            border-radius: 50%;
            background: #fff;
            cursor: pointer;
            transition: border-color 0.15s, background 0.15s;
            flex-shrink: 0;
        }

        .btn-toggle:hover { border-color: #0d9488; }
        .btn-toggle.done { background: #0d9488; border-color: #0d9488; color: #fff; }

        .todo-title { flex: 1; font-size: 0.95rem; }
        .completed .todo-title { text-decoration: line-through; opacity: 0.45; }

        .btn-delete {
            padding: 0.15rem 0.45rem;
            font-size: 0.85rem;
            color: #aaa;
            background: none;
            border: 1px solid transparent;
            border-radius: 6px;
            cursor: pointer;
            opacity: 0;
            transition: opacity 0.12s, color 0.12s, border-color 0.12s;
        }

        .todo-item:hover .btn-delete { opacity: 1; }
        .btn-delete:hover { color: #c53030; border-color: #fde8e8; }
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
<p class="subtitle">A simple task tracker built with Turbo Frames</p>

<partial name="_TodoList" model="Model" />
```

The page renders the `_TodoList` partial, which wraps everything in a `<turbo-frame id="todo-list">`. When a form inside the frame submits, Turbo sends the request with a `Turbo-Frame: todo-list` header and replaces the frame with the partial response.

## Step 7 — Create the todo list partial

Create `Pages/Shared/_TodoList.cshtml`:

```html
@model TurboTodo.Pages.IndexModel

<turbo-frame id="todo-list">
    <div class="card">
        <form method="post" asp-page-handler="Add"
              data-controller="todo-form"
              data-action="turbo:submit-end->todo-form#reset">
            <div class="add-row">
                <input type="text" name="title" placeholder="What needs to be done?"
                       data-todo-form-target="input" />
                <button type="submit" class="btn-add">Add</button>
            </div>
            @if (Model.Error is not null)
            {
                <p class="error">@Model.Error</p>
            }
        </form>

        <div class="todo-list">
            @foreach (var todo in Model.Todos)
            {
                <div class="todo-item @(todo.IsComplete ? "completed" : "")">
                    <form method="post" asp-page-handler="Toggle">
                        <input type="hidden" name="id" value="@todo.Id" />
                        <button type="submit"
                                class="btn-toggle @(todo.IsComplete ? "done" : "")">
                            @(todo.IsComplete ? "✓" : "")
                        </button>
                    </form>
                    <span class="todo-title">@todo.Title</span>
                    <form method="post" asp-page-handler="Delete">
                        <input type="hidden" name="id" value="@todo.Id" />
                        <button type="submit" class="btn-delete">✕</button>
                    </form>
                </div>
            }
        </div>
    </div>
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
