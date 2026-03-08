# Claude Code

[Claude Code](https://claude.ai/code) is an agentic coding tool from Anthropic. When you run it in a project directory, it reads a `CLAUDE.md` file from the project root to understand the codebase, conventions, and APIs available.

## Using CLAUDE.md with Your Project

The Tombatron.Turbo repository includes a [`CLAUDE.md`](https://github.com/tombatron/Tombatron.Turbo/blob/main/CLAUDE.md) file tailored for this library. To use it:

1. Copy the `CLAUDE.md` file from the [Tombatron.Turbo repo](https://github.com/tombatron/Tombatron.Turbo/blob/main/CLAUDE.md) into your own project root.
2. Run Claude Code in your project directory.
3. Claude Code will automatically pick up the file and use it as context when answering questions or generating code.

## What the CLAUDE.md Covers

The file gives Claude Code detailed knowledge of:

- **Setup** — `AddTurbo()`, `UseTurbo()`, `MapTurboHub()`, tag helper registration, and `<turbo-scripts />`
- **Turbo Frames** — Frame tag helpers, lazy loading, dynamic IDs, and `HttpContext` extensions
- **Turbo Streams** — Real-time updates via SignalR, the `ITurbo` interface, stream builder actions, and async partial rendering
- **Form Validation** — HTTP 422 pattern for inline validation within frames
- **Stimulus** — Controller discovery, naming conventions, and hot reload
- **Minimal API Support** — `TurboResults` factory methods and source-generated partial templates
- **Configuration** — All `TurboOptions` properties and authorization via signed stream names
- **Common Gotchas** — Frequent mistakes and how to avoid them
