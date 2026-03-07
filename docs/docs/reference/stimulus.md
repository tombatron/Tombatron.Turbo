---
title: Stimulus
sidebar_label: Stimulus
sidebar_position: 3
description: Convention-based Stimulus controller discovery with import maps and hot reload.
---

`AddStimulus()` discovers controllers from `wwwroot/controllers/` (default) and registers them in the import map. No manual registration required.

## Setup

```csharp
using Tombatron.Turbo.Stimulus;

builder.Services.AddStimulus();
```

## Naming Conventions

| File | Identifier |
|---|---|
| `hello_controller.js` | `hello` |
| `hello-controller.js` | `hello` |
| `todo_form_controller.js` | `todo-form` |
| `todo-form-controller.js` | `todo-form` |
| `admin/users_controller.js` | `admin--users` |
| `admin/user_settings_controller.js` | `admin--user-settings` |

Both `_controller.js` and `-controller.js` suffixes are supported. Underscores become hyphens, the controller suffix is stripped, and directory separators become `--`.

## Options

```csharp
builder.Services.AddStimulus(options =>
{
    options.ControllersPath = "js/controllers";        // Default: "controllers"
    options.StimulusCdnUrl = "https://unpkg.com/...";  // Default: stimulus 3.2.2 from unpkg
    options.EnableHotReload = true;                     // Default: auto-detect from environment
});
```

## Hot Reload

Hot reload is enabled automatically in Development — save a controller file and the browser picks up the changes without a manual refresh.

## Usage in HTML

Connect a controller to an element using `data-controller`:

```html
<div data-controller="hello">
    <input data-hello-target="name" type="text" />
    <button data-action="click->hello#greet">Greet</button>
    <span data-hello-target="output"></span>
</div>
```

For a complete example, see [Step 8 of the Todo List tutorial](../tutorials/todo-list.md#step-8--add-a-stimulus-controller).
