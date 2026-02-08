# Tombatron.Turbo - Implementation Plan

## Project Overview
Hotwire Turbo for ASP.NET Core with compile-time frame optimization and SignalR-powered real-time streams.

**Core Rule:** Static frame IDs only, unless you provide a prefix. Dynamic IDs without prefix = compile error.

## Coding Standards

### C# Style Guidelines
- **Pure functions preferred:** Minimize side effects, make functions deterministic where possible
- **Always use braces:** Even for single-line if statements
  ```csharp
  // ✅ Correct
  if (condition)
  {
      DoSomething();
  }
  
  // ❌ Incorrect
  if (condition) DoSomething();
  ```
- **Comprehensive unit testing:** Aim for high coverage, test edge cases, prefer many small focused tests over few large tests
- **Explicit over implicit:** Be clear about types, avoid `var` when type isn't obvious
- **Immutability preferred:** Use `readonly` fields, prefer records for DTOs
- **Null safety:** Use nullable reference types, validate inputs

### Testing Standards
- Every public method should have unit tests
- Test happy path, error cases, and edge cases
- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- Arrange-Act-Assert pattern
- Mock external dependencies
- Integration tests for end-to-end scenarios

---

## Milestone 1: Foundation & Core Infrastructure
**Status:** 🟢 Complete

### Objectives
- Set up project structure
- Configure build and packaging
- Define core interfaces
- Basic configuration system

### Tasks
- [x] Create GitHub repository: `tombatron/Tombatron.Turbo`
- [x] Initialize solution structure:
  - [x] `src/Tombatron.Turbo/Tombatron.Turbo.csproj` (main library)
  - [x] `src/Tombatron.Turbo.SourceGenerator/Tombatron.Turbo.SourceGenerator.csproj`
  - [x] `src/Tombatron.Turbo.Analyzers/Tombatron.Turbo.Analyzers.csproj`
  - [x] `samples/Tombatron.Turbo.Sample/Tombatron.Turbo.Sample.csproj`
  - [x] `tests/Tombatron.Turbo.Tests/Tombatron.Turbo.Tests.csproj`
- [x] Configure NuGet package metadata in `.csproj` files
- [x] Set up GitHub Actions CI/CD:
  - [x] `.github/workflows/build.yml` - Build and test on push
  - [x] `.github/workflows/release.yml` - Pack and publish on tag
- [x] Create `.editorconfig` with coding standards:
  - [x] Enforce brace usage
  - [x] Configure nullable reference types
  - [x] Set formatting rules
- [x] Implement core interfaces:
  - [x] `src/Tombatron.Turbo/ITurbo.cs`
  - [x] `src/Tombatron.Turbo/ITurboStreamBuilder.cs`
- [x] Implement configuration:
  - [x] `src/Tombatron.Turbo/TurboOptions.cs`
  - [x] `src/Tombatron.Turbo/TurboServiceCollectionExtensions.cs`
  - [x] `AddTurbo()` extension method
  - [x] `UseTurbo()` middleware extension
- [x] Create test project with xUnit:
  - [x] Configure test framework
  - [x] Add common test utilities (FluentAssertions, Moq)
  - [x] Create test base classes if needed
- [x] Write initial unit tests:
  - [x] `tests/Tombatron.Turbo.Tests/TurboOptionsTests.cs`
  - [x] `tests/Tombatron.Turbo.Tests/TurboServiceCollectionExtensionsTests.cs`
  - [x] `tests/Tombatron.Turbo.Tests/TurboApplicationBuilderExtensionsTests.cs`
  - [x] Test default configuration
  - [x] Test custom configuration
- [x] Verify solution builds successfully
- [x] Verify NuGet pack creates valid package

### Acceptance Criteria
- ✅ Solution builds without errors
- ✅ `dotnet pack` creates NuGet package
- ✅ GitHub Actions workflow runs successfully
- ✅ Core interfaces are defined and documented
- ✅ `AddTurbo()` registers services without errors
- ✅ `.editorconfig` enforces coding standards
- ✅ All unit tests pass

### Files Created
```
Tombatron.Turbo/
├── .github/
│   └── workflows/
│       ├── build.yml
│       └── release.yml
├── .editorconfig
├── src/
│   ├── Tombatron.Turbo/
│   │   ├── Tombatron.Turbo.csproj
│   │   ├── ITurbo.cs
│   │   ├── ITurboStreamBuilder.cs
│   │   ├── TurboOptions.cs
│   │   └── TurboServiceCollectionExtensions.cs
│   ├── Tombatron.Turbo.SourceGenerator/
│   │   └── Tombatron.Turbo.SourceGenerator.csproj
│   └── Tombatron.Turbo.Analyzers/
│       └── Tombatron.Turbo.Analyzers.csproj
├── samples/
│   └── Tombatron.Turbo.Sample/
│       └── Tombatron.Turbo.Sample.csproj
├── tests/
│   └── Tombatron.Turbo.Tests/
│       ├── Tombatron.Turbo.Tests.csproj
│       └── TurboOptionsTests.cs
├── Tombatron.Turbo.sln
├── README.md
├── LICENSE
└── .gitignore
```

---

## Milestone 2: Source Generator - Frame Detection
**Status:** 🟢 Complete

### Objectives
- Implement Roslyn source generator
- Detect turbo-frame tags in Razor files
- Generate optimized sub-templates at compile time
- Generate metadata for runtime lookup

### Tasks
- [x] Set up source generator project structure:
  - [x] Reference `Microsoft.CodeAnalysis.CSharp`
  - [x] Reference `Microsoft.CodeAnalysis.Analyzers`
  - [x] Implement `IIncrementalGenerator`
- [x] Implement Razor file discovery:
  - [x] Via `AdditionalTextsProvider` in generator (no separate RazorFileProvider needed)
  - [x] Find all `.cshtml` files in project
  - [x] Read file contents
  - [x] Use pure functions for file parsing
- [x] Implement frame detection:
  - [x] `src/Tombatron.Turbo.SourceGenerator/FrameParser.cs`
  - [x] Parse `<turbo-frame>` tags (pure function)
  - [x] Extract `id` attribute
  - [x] Extract `asp-frame-prefix` attribute if present
  - [x] Detect static vs dynamic IDs
  - [x] Return immutable data structures (records)
- [x] Implement sub-template generation for static IDs:
  - [x] `src/Tombatron.Turbo.SourceGenerator/TemplateGenerator.cs` (combined static/dynamic)
  - [x] Generate template content (pure function)
  - [x] Extract frame content
  - [x] Set `Layout = null`
- [x] Implement sub-template generation for dynamic IDs:
  - [x] `src/Tombatron.Turbo.SourceGenerator/TemplateGenerator.cs` (combined)
  - [x] Generate template content (pure function)
  - [x] Include `var turboFrameId = ViewBag.TurboFrameId`
  - [x] Use `turboFrameId` in template
- [x] Generate runtime metadata:
  - [x] `src/Tombatron.Turbo.SourceGenerator/MetadataGenerator.cs`
  - [x] Create lookup dictionary: FrameId → Template (pure function)
  - [x] Create lookup dictionary: Prefix → Template
  - [x] Generate as C# source file with FrozenDictionary
- [x] Write comprehensive unit tests:
  - [x] `tests/Tombatron.Turbo.Tests/SourceGenerator/RazorFileInfoTests.cs`
    - [x] Test model properties and filtering
  - [x] `tests/Tombatron.Turbo.Tests/SourceGenerator/FrameParserTests.cs`
    - [x] Test parsing static frame IDs
    - [x] Test parsing dynamic frame IDs
    - [x] Test parsing with prefix
    - [x] Test parsing without prefix
    - [x] Test malformed HTML
    - [x] Test multiple frames
    - [x] Test nested frames
    - [x] Test frames with attributes
  - [x] `tests/Tombatron.Turbo.Tests/SourceGenerator/TemplateGeneratorTests.cs`
    - [x] Test static sub-template generation
    - [x] Test dynamic sub-template generation
    - [x] Test layout removal
    - [x] Test ViewBag.TurboFrameId inclusion
  - [x] `tests/Tombatron.Turbo.Tests/SourceGenerator/MetadataGeneratorTests.cs`
    - [x] Test metadata dictionary generation
    - [x] Test with static frames only
    - [x] Test with dynamic frames only
    - [x] Test with mixed frames
    - [x] Test with no frames

### Acceptance Criteria
- ✅ Generator discovers all `.cshtml` files
- ✅ Static frame IDs generate sub-templates
- ✅ Dynamic IDs with `asp-frame-prefix` generate prefix templates
- ✅ Generated files are syntactically valid Razor
- ✅ Metadata dictionary is generated correctly
- ✅ All parsing functions are pure
- ✅ All unit tests pass (aiming for >95% coverage)

### Files Created
```
src/Tombatron.Turbo.SourceGenerator/
├── TurboFrameGenerator.cs (IIncrementalGenerator)
├── RazorFileProvider.cs
├── FrameParser.cs
├── StaticFrameGenerator.cs
├── DynamicFrameGenerator.cs
└── MetadataGenerator.cs

tests/Tombatron.Turbo.Tests/SourceGenerator/
├── RazorFileProviderTests.cs
├── FrameParserTests.cs
├── StaticFrameGeneratorTests.cs
├── DynamicFrameGeneratorTests.cs
└── MetadataGeneratorTests.cs
```

---

## Milestone 3: Roslyn Analyzer - Compile-Time Validation
**Status:** 🟢 Complete

### Objectives
- Implement Roslyn analyzer
- Enforce "prefix required for dynamic IDs" rule at compile time
- Provide helpful diagnostics and code fixes

### Tasks
- [x] Set up analyzer project:
  - [x] Reference `Microsoft.CodeAnalysis.CSharp`
  - [x] Reference `Microsoft.CodeAnalysis.CSharp.Workspaces`
  - [x] Implement `DiagnosticAnalyzer`
- [x] Implement TURBO001: Dynamic ID without prefix (ERROR):
  - [x] `src/Tombatron.Turbo.Analyzers/TurboFrameAnalyzer.cs` (combined analyzer)
  - [x] Detect `id="@..."` or `id="text_@..."` patterns
  - [x] Check for missing `asp-frame-prefix`
  - [x] Report error with location
  - [x] Message: "Dynamic turbo-frame ID requires asp-frame-prefix attribute"
  - [x] Use pure functions for pattern detection (reuses FrameParser)
- [x] Implement TURBO002: Prefix doesn't match ID (ERROR):
  - [x] `src/Tombatron.Turbo.Analyzers/TurboFrameAnalyzer.cs` (combined analyzer)
  - [x] Extract prefix from `asp-frame-prefix`
  - [x] Extract static portion from `id`
  - [x] Validate they match (pure function)
  - [x] Report error on mismatch
- [x] Implement TURBO003: Unnecessary prefix (INFO):
  - [x] `src/Tombatron.Turbo.Analyzers/TurboFrameAnalyzer.cs` (combined analyzer)
  - [x] Detect static IDs with `asp-frame-prefix`
  - [x] Report informational diagnostic
- [x] Implement code fix provider for TURBO001:
  - [x] `src/Tombatron.Turbo.Analyzers/AddPrefixCodeFixProvider.cs`
  - [x] Infer prefix from dynamic ID (pure function)
  - [x] Add `asp-frame-prefix` attribute
  - [x] Register as quick action
- [x] Implement code fix provider for TURBO002:
  - [x] `src/Tombatron.Turbo.Analyzers/FixPrefixCodeFixProvider.cs`
  - [x] Correct prefix to match ID
  - [x] Register as quick action
- [x] Implement code fix provider for TURBO003:
  - [x] `src/Tombatron.Turbo.Analyzers/RemovePrefixCodeFixProvider.cs`
  - [x] Remove `asp-frame-prefix` attribute
  - [x] Register as quick action
- [x] Write comprehensive unit tests:
  - [x] `tests/Tombatron.Turbo.Tests/Analyzers/TurboFrameAnalyzerTests.cs`
    - [x] Test various dynamic ID patterns
    - [x] Test with prefix present (no error)
    - [x] Test without prefix (error)
    - [x] Test edge cases
    - [x] Test matching prefix (no error)
    - [x] Test mismatched prefix (error)
    - [x] Test various mismatch scenarios
    - [x] Test static ID without prefix (no diagnostic)
    - [x] Test static ID with prefix (info)
  - [x] `tests/Tombatron.Turbo.Tests/Analyzers/DiagnosticDescriptorsTests.cs`
    - [x] Test diagnostic descriptor properties
  - [x] `tests/Tombatron.Turbo.Tests/Analyzers/CodeFixProviderTests.cs`
    - [x] Test InferPrefix logic
    - [x] Test all code fix providers have correct diagnostic IDs
    - [x] Test all code fix providers provide FixAllProvider

### Acceptance Criteria
- ✅ Dynamic IDs without prefix produce compile error
- ✅ Mismatched prefix produces compile error
- ✅ Static IDs with prefix produce info diagnostic
- ✅ Code fixes work in IDE
- ✅ Errors prevent build from succeeding
- ✅ All analysis functions are pure where possible
- ✅ All unit tests pass (121 tests, up from 93)

### Notes
- Combined all three diagnostic checks into a single `TurboFrameAnalyzer.cs` for efficiency
- Analyzer reuses `FrameParser` from SourceGenerator project for consistent parsing
- Uses `AdditionalFileAction` to analyze .cshtml files registered as AdditionalFiles

### Files Created
```
src/Tombatron.Turbo.Analyzers/
├── DiagnosticDescriptors.cs
├── TurboFrameAnalyzer.cs (combined analyzer for all diagnostics)
├── AddPrefixCodeFixProvider.cs
├── FixPrefixCodeFixProvider.cs
└── RemovePrefixCodeFixProvider.cs

tests/Tombatron.Turbo.Tests/Analyzers/
├── TurboFrameAnalyzerTests.cs
├── DiagnosticDescriptorsTests.cs
└── CodeFixProviderTests.cs
```

---

## Milestone 4: Middleware & Tag Helper
**Status:** 🟢 Complete

### Objectives
- Implement turbo-frame tag helper
- Implement middleware for frame routing
- Route requests to correct sub-templates
- Return 422 for missing frames

### Tasks
- [x] Implement `<turbo-frame>` tag helper:
  - [x] `src/Tombatron.Turbo/TagHelpers/TurboFrameTagHelper.cs`
  - [x] Render standard `<turbo-frame>` element
  - [x] Support `id` attribute
  - [x] Support `asp-frame-prefix` attribute
  - [x] Pass through other attributes
  - [x] Use pure functions for attribute processing
- [x] Implement middleware:
  - [x] `src/Tombatron.Turbo/Middleware/TurboFrameMiddleware.cs`
  - [x] Detect `Turbo-Frame` request header
  - [x] Parse requested frame ID (pure function)
  - [x] Check for exact match in metadata
  - [x] Check for prefix match in metadata
  - [x] Route to appropriate sub-template
  - [x] Set `ViewBag.TurboFrameId` for prefix matches
  - [x] Return 422 if no match found
- [x] Implement response handling:
  - [x] Add `Vary: Turbo-Frame` header
  - [x] Set appropriate content type
  - [x] Handle both Razor Pages and MVC
- [x] Update `UseTurbo()` extension:
  - [x] `src/Tombatron.Turbo/TurboApplicationBuilderExtensions.cs`
  - [x] Register middleware
  - [x] Validate configuration
- [x] Write comprehensive unit tests:
  - [x] `tests/Tombatron.Turbo.Tests/TagHelpers/TurboFrameTagHelperTests.cs`
    - [x] Test rendering with static ID
    - [x] Test rendering with dynamic ID and prefix
    - [x] Test attribute pass-through
    - [x] Test various attribute combinations
  - [x] `tests/Tombatron.Turbo.Tests/Middleware/TurboFrameMiddlewareTests.cs`
    - [x] Test header detection
    - [x] Test frame ID parsing
    - [x] Test exact match routing
    - [x] Test prefix match routing
    - [x] Test ViewBag.TurboFrameId is set
    - [x] Test 422 response for missing frames
    - [x] Test Vary header is added
    - [x] Test without Turbo-Frame header (passthrough)
  - [x] `tests/Tombatron.Turbo.Tests/TurboHttpContextExtensionsTests.cs`
    - [x] Test IsTurboFrameRequest extensions
    - [x] Test GetTurboFrameId extensions
    - [x] Test prefix matching extensions
- [x] Create sample application:
  - [x] `samples/Tombatron.Turbo.Sample/Pages/Cart/Index.cshtml`
  - [x] Static frames example
  - [x] Dynamic frames with prefix example
  - [x] Navigation between frames

### Notes
- Added `TurboFrameResultFilter` for automatic sub-template routing in MVC/Razor Pages
- Added `TurboHttpContextExtensions` for easy access to turbo-frame request info in views/controllers
- Integration tests deferred to Milestone 8 (Polish & Testing) for end-to-end testing with full infrastructure

### Acceptance Criteria
- ✅ Tag helper renders correct HTML
- ✅ Middleware detects Turbo-Frame header
- ✅ Static frames route to correct template
- ✅ Dynamic frames with prefix route correctly
- ✅ ViewBag.TurboFrameId is set for dynamic frames
- ✅ Missing frames return 422
- ✅ Full page renders without Turbo-Frame header
- ✅ Sample app demonstrates all features
- ✅ All unit tests pass (177 tests, up from 121)

### Files Created
```
src/Tombatron.Turbo/
├── TagHelpers/
│   └── TurboFrameTagHelper.cs
├── Middleware/
│   ├── TurboFrameMiddleware.cs
│   └── TurboFrameResultFilter.cs
├── TurboHttpContextExtensions.cs
└── TurboApplicationBuilderExtensions.cs (updated)

tests/Tombatron.Turbo.Tests/
├── TagHelpers/
│   └── TurboFrameTagHelperTests.cs
├── Middleware/
│   └── TurboFrameMiddlewareTests.cs
└── TurboHttpContextExtensionsTests.cs

samples/Tombatron.Turbo.Sample/
├── Pages/
│   ├── _ViewImports.cshtml
│   ├── Shared/
│   │   └── _Layout.cshtml
│   ├── Index.cshtml
│   ├── Index.cshtml.cs
│   └── Cart/
│       ├── Index.cshtml
│       └── Index.cshtml.cs
└── Program.cs
```

---

## Milestone 5: Turbo Streams - Server Side
**Status:** 🔴 Not Started

### Objectives
- Implement SignalR hub for streaming
- Implement ITurbo service
- Implement stream builder
- Add authorization hooks

### Tasks
- [ ] Implement TurboHub:
  - [ ] `src/Tombatron.Turbo/Streams/TurboHub.cs`
  - [ ] `Subscribe(string streamName)` method
  - [ ] `Unsubscribe(string streamName)` method
  - [ ] Add connection to SignalR group on subscribe
  - [ ] Remove from group on unsubscribe
  - [ ] Handle connection lifecycle events
  - [ ] Validate stream names (pure function)
- [ ] Implement authorization interface:
  - [ ] `src/Tombatron.Turbo/Streams/ITurboStreamAuthorization.cs`
  - [ ] `CanSubscribe(ClaimsPrincipal? user, string streamName)` method
  - [ ] Default implementation that allows all authenticated users
  - [ ] Hook into TurboHub.Subscribe
- [ ] Implement ITurbo service:
  - [ ] `src/Tombatron.Turbo/Streams/TurboService.cs`
  - [ ] Inject `IHubContext<TurboHub>`
  - [ ] `Stream(string streamName, Action<ITurboStreamBuilder> build)`
  - [ ] `Stream (overload)(IEnumerable<string> streamNames, Action<ITurboStreamBuilder> build)`
  - [ ] `Broadcast(Action<ITurboStreamBuilder> build)`
  - [ ] Validate inputs
- [ ] Implement ITurboStreamBuilder:
  - [ ] `src/Tombatron.Turbo/Streams/TurboStreamBuilder.cs`
  - [ ] `Append(string target, string html)` method
  - [ ] `Prepend(string target, string html)` method
  - [ ] `Replace(string target, string html)` method
  - [ ] `Update(string target, string html)` method
  - [ ] `Remove(string target)` method
  - [ ] `Before(string target, string html)` method
  - [ ] `After(string target, string html)` method
  - [ ] Generate proper Turbo Stream HTML (pure function)
  - [ ] Support multiple actions in one call
  - [ ] Validate target and html parameters
  - [ ] Use immutable list for building
- [ ] Update service registration:
  - [ ] Register TurboHub in `AddTurbo()`
  - [ ] Register ITurbo as singleton
  - [ ] Register default ITurboStreamAuthorization
  - [ ] Allow custom authorization via options
- [ ] Write comprehensive unit tests:
  - [ ] `tests/Tombatron.Turbo.Tests/Streams/TurboStreamBuilderTests.cs`
    - [ ] Test each action generates correct HTML
    - [ ] Test append action
    - [ ] Test prepend action
    - [ ] Test replace action
    - [ ] Test update action
    - [ ] Test remove action
    - [ ] Test before action
    - [ ] Test after action
    - [ ] Test multiple actions in sequence
    - [ ] Test with empty target (should throw)
    - [ ] Test with null html (should throw)
    - [ ] Test HTML escaping
  - [ ] `tests/Tombatron.Turbo.Tests/Streams/TurboServiceTests.cs`
    - [ ] Test Stream() with valid stream name
    - [ ] Test Stream() with invalid stream name
    - [ ] Test Stream (overload)() with multiple streams
    - [ ] Test Stream (overload)() with empty list
    - [ ] Test Broadcast()
    - [ ] Test null builder (should throw)
  - [ ] `tests/Tombatron.Turbo.Tests/Streams/TurboHubTests.cs`
    - [ ] Test Subscribe adds to group
    - [ ] Test Unsubscribe removes from group
    - [ ] Test authorization check
    - [ ] Test unauthorized subscription fails
  - [ ] `tests/Tombatron.Turbo.Tests/Streams/AuthorizationTests.cs`
    - [ ] Test default authorization (authenticated users)
    - [ ] Test custom authorization implementation
    - [ ] Test with various stream name patterns
- [ ] Write integration tests:
  - [ ] `tests/Tombatron.Turbo.Tests/Integration/StreamingTests.cs`
    - [ ] Test hub connection
    - [ ] Test subscribe/unsubscribe flow
    - [ ] Test message delivery to correct group
    - [ ] Test message not delivered to other groups
    - [ ] Test authorization integration
    - [ ] Test multiple clients
- [ ] Update sample app:
  - [ ] Add streaming examples
  - [ ] Cart updates via streams
  - [ ] Notification examples

### Acceptance Criteria
- ✅ TurboHub accepts connections
- ✅ Subscribe adds connection to group
- ✅ Authorization checks work
- ✅ ITurbo.Stream() broadcasts to group
- ✅ ITurbo.Stream (overload)() broadcasts to multiple groups
- ✅ ITurbo.Broadcast() sends to all clients
- ✅ Stream builder generates valid Turbo Stream HTML
- ✅ HTML generation functions are pure
- ✅ All inputs are validated
- ✅ All unit tests pass (aiming for >95% coverage)
- ✅ Integration tests pass

### Files Created
```
src/Tombatron.Turbo/Streams/
├── TurboHub.cs
├── TurboService.cs
├── TurboStreamBuilder.cs
├── ITurboStreamAuthorization.cs
└── DefaultTurboStreamAuthorization.cs

tests/Tombatron.Turbo.Tests/Streams/
├── TurboStreamBuilderTests.cs
├── TurboServiceTests.cs
├── TurboHubTests.cs
└── AuthorizationTests.cs

tests/Tombatron.Turbo.Tests/Integration/
└── StreamingTests.cs
```

---

## Milestone 6: Turbo Streams - Client Side
**Status:** 🔴 Not Started

### Objectives
- Implement browser-side SignalR adapter
- Create custom element for stream sources
- Implement `<turbo>` tag helper
- Build and bundle JavaScript

### Tasks
- [ ] Set up JavaScript build:
  - [ ] `src/Tombatron.Turbo/wwwroot/package.json`
  - [ ] Add `@microsoft/signalr` dependency
  - [ ] Configure build scripts
  - [ ] Output to `wwwroot/dist/turbo-signalr.js`
  - [ ] Configure ESLint with strict rules
- [ ] Implement custom element:
  - [ ] `src/Tombatron.Turbo/wwwroot/src/turbo-stream-source-signalr.js`
  - [ ] Create `turbo-stream-source-signalr` custom element
  - [ ] Singleton SignalR connection
  - [ ] Reference counting for streams
  - [ ] Subscribe on `connectedCallback()`
  - [ ] Unsubscribe on `disconnectedCallback()`
  - [ ] Handle reconnection
  - [ ] Resubscribe to active streams on reconnect
  - [ ] Validate stream names
  - [ ] Handle connection errors gracefully
- [ ] Implement Turbo.js integration:
  - [ ] Listen for "TurboStream" SignalR messages
  - [ ] Call `window.Turbo.renderStreamMessage(html)`
  - [ ] Dispatch connection status events
  - [ ] Handle missing Turbo.js gracefully
- [ ] Implement `<turbo>` tag helper:
  - [ ] `src/Tombatron.Turbo/TagHelpers/TurboTagHelper.cs`
  - [ ] Inject `IHttpContextAccessor`
  - [ ] Support `id` attribute
  - [ ] Support `stream` attribute
  - [ ] Default to `user:{username}` if authenticated
  - [ ] Default to `session:{sessionId}` if anonymous
  - [ ] Render `<turbo-stream-source-signalr>` element
  - [ ] Validate inputs
  - [ ] Use pure functions for stream name generation
- [ ] Write JavaScript tests:
  - [ ] `src/Tombatron.Turbo/wwwroot/tests/turbo-stream-source-signalr.test.js`
  - [ ] Test custom element lifecycle
  - [ ] Test subscription management
  - [ ] Test reference counting
    - [ ] Test multiple elements same stream
    - [ ] Test unsubscribe only on last removal
  - [ ] Test reconnection behavior
  - [ ] Test Turbo.js integration
  - [ ] Test error handling
  - [ ] Mock SignalR connection
- [ ] Write C# unit tests for tag helper:
  - [ ] `tests/Tombatron.Turbo.Tests/TagHelpers/TurboTagHelperTests.cs`
    - [ ] Test default stream name for authenticated user
    - [ ] Test default stream name for anonymous user
    - [ ] Test explicit id attribute
    - [ ] Test explicit stream attribute
    - [ ] Test stream + id combination
    - [ ] Test with no user context
    - [ ] Test various edge cases
- [ ] Write integration tests:
  - [ ] `tests/Tombatron.Turbo.Tests/Integration/ClientStreamingTests.cs`
    - [ ] Test end-to-end streaming
    - [ ] Test multiple streams on same page
    - [ ] Test reconnection
    - [ ] Test message delivery
    - [ ] Use Playwright for browser automation
- [ ] Update sample app:
  - [ ] Include turbo-signalr.js
  - [ ] Add `<turbo>` tags
  - [ ] Demonstrate real-time updates
  - [ ] Show connection status
  - [ ] Add error handling examples

### Acceptance Criteria
- ✅ JavaScript builds successfully
- ✅ Custom element registers and works
- ✅ Single SignalR connection is shared
- ✅ Reference counting works correctly
- ✅ Reconnection resubscribes automatically
- ✅ Turbo.js renders streamed HTML
- ✅ `<turbo>` tag helper generates correct element
- ✅ Default stream names work
- ✅ All inputs are validated
- ✅ JavaScript tests pass
- ✅ C# unit tests pass (aiming for >95% coverage)
- ✅ Integration tests pass
- ✅ Sample app demonstrates streaming

### Files Created
```
src/Tombatron.Turbo/wwwroot/
├── package.json
├── .eslintrc.json
├── rollup.config.js (or webpack)
├── src/
│   └── turbo-stream-source-signalr.js
├── tests/
│   └── turbo-stream-source-signalr.test.js
└── dist/
    └── turbo-signalr.js (built)

src/Tombatron.Turbo/TagHelpers/
└── TurboTagHelper.cs

tests/Tombatron.Turbo.Tests/TagHelpers/
└── TurboTagHelperTests.cs

tests/Tombatron.Turbo.Tests/Integration/
└── ClientStreamingTests.cs
```

---

## Milestone 7: Documentation & Samples
**Status:** 🔴 Not Started

### Objectives
- Write comprehensive documentation
- Create multiple sample applications
- Document all public APIs
- Write migration guides

### Tasks
- [ ] Update README.md:
  - [ ] Project overview
  - [ ] Installation instructions
  - [ ] Quick start guide
  - [ ] Basic examples (frames and streams)
  - [ ] Link to full documentation
  - [ ] Badge for NuGet version
  - [ ] Badge for build status
  - [ ] Coding standards reference
- [ ] Write API documentation:
  - [ ] `docs/api/ITurbo.md`
  - [ ] `docs/api/ITurboStreamBuilder.md`
  - [ ] `docs/api/TurboOptions.md`
  - [ ] `docs/api/TagHelpers.md`
  - [ ] Document all public APIs with examples
- [ ] Write feature guides:
  - [ ] `docs/guides/turbo-frames.md`
    - [ ] Static frames
    - [ ] Dynamic frames with prefixes
    - [ ] Navigation patterns
    - [ ] Lazy loading
    - [ ] Best practices
  - [ ] `docs/guides/turbo-streams.md`
    - [ ] Setting up streaming
    - [ ] Stream naming conventions
    - [ ] Broadcasting patterns
    - [ ] Authorization
    - [ ] Best practices
  - [ ] `docs/guides/authorization.md`
    - [ ] Implementing ITurboStreamAuthorization
    - [ ] Stream security patterns
    - [ ] User vs resource streams
    - [ ] Testing authorization
  - [ ] `docs/guides/troubleshooting.md`
    - [ ] Common issues
    - [ ] Debugging tips
    - [ ] Performance optimization
    - [ ] Error messages explained
  - [ ] `docs/guides/testing.md`
    - [ ] Testing frames
    - [ ] Testing streams
    - [ ] Integration testing
    - [ ] Mocking strategies
- [ ] Write migration guides:
  - [ ] `docs/migration/from-turbo-rails.md`
  - [ ] `docs/migration/from-blazor-server.md`
  - [ ] `docs/migration/from-htmx.md`
- [ ] Create sample applications:
  - [ ] E-commerce cart (existing, enhance):
    - [ ] Product listing with frames
    - [ ] Add to cart with streams
    - [ ] Live cart total updates
    - [ ] Checkout flow
    - [ ] README with setup
  - [ ] `samples/Tombatron.Turbo.Chat/` - Real-time chat:
    - [ ] Multiple chat rooms
    - [ ] User-specific streams
    - [ ] Typing indicators
    - [ ] Message history with frames
    - [ ] README with setup
  - [ ] `samples/Tombatron.Turbo.Dashboard/` - Live dashboard:
    - [ ] Real-time metrics
    - [ ] Multiple widgets with frames
    - [ ] Broadcast updates
    - [ ] Auto-refresh data
    - [ ] README with setup
- [ ] Add XML documentation comments:
  - [ ] All public interfaces
  - [ ] All public classes
  - [ ] All public methods
  - [ ] Include code examples in docs
  - [ ] Enable XML doc generation in build
  - [ ] Treat doc warnings as errors

### Acceptance Criteria
- ✅ README is complete and clear
- ✅ All public APIs are documented
- ✅ Feature guides cover common scenarios
- ✅ Migration guides help users transition
- ✅ Sample apps build and run
- ✅ Sample apps demonstrate key features
- ✅ XML docs generate without warnings
- ✅ Coding standards are documented

### Files Created
```
README.md (updated)
LICENSE

docs/
├── api/
│   ├── ITurbo.md
│   ├── ITurboStreamBuilder.md
│   ├── TurboOptions.md
│   └── TagHelpers.md
├── guides/
│   ├── turbo-frames.md
│   ├── turbo-streams.md
│   ├── authorization.md
│   ├── troubleshooting.md
│   └── testing.md
└── migration/
    ├── from-turbo-rails.md
    ├── from-blazor-server.md
    └── from-htmx.md

samples/
├── Tombatron.Turbo.Sample/ (enhanced)
│   └── README.md
├── Tombatron.Turbo.Chat/
│   └── README.md
└── Tombatron.Turbo.Dashboard/
    └── README.md
```

---

## Milestone 8: Polish & Testing
**Status:** 🔴 Not Started

### Objectives
- Performance optimization
- Comprehensive testing
- Security review
- Error handling improvements

### Tasks
- [ ] Performance optimization:
  - [ ] `tests/Tombatron.Turbo.Benchmarks/` - BenchmarkDotNet project
  - [ ] Benchmark frame routing performance
  - [ ] Benchmark stream broadcasting performance
  - [ ] Benchmark source generator performance
  - [ ] Profile memory usage
  - [ ] Optimize hot paths (while maintaining pure functions)
  - [ ] Document performance characteristics
- [ ] Comprehensive testing:
  - [ ] Review test coverage (aim for >95%)
  - [ ] Add missing unit tests
  - [ ] Add edge case tests:
    - [ ] Nested frames (multiple levels)
    - [ ] Very large HTML documents (>1MB)
    - [ ] Malformed Razor syntax
    - [ ] Concurrent stream updates (1000+ clients)
    - [ ] Rapid connect/disconnect cycles
    - [ ] Unicode in stream names and targets
    - [ ] Special characters in frame IDs
    - [ ] Empty frames
    - [ ] Frames with no content
  - [ ] Add stress tests:
    - [ ] 1000+ concurrent SignalR connections
    - [ ] High-frequency stream updates (100+ per second)
    - [ ] Large number of frames per page (50+)
    - [ ] Memory usage under load
- [ ] Error handling improvements:
  - [ ] Review all exception paths
  - [ ] Add helpful error messages
  - [ ] Log important events with structured logging
  - [ ] Implement graceful degradation
  - [ ] Handle network failures
  - [ ] Add retry logic where appropriate
  - [ ] Document error scenarios
- [ ] Security review:
  - [ ] Review for XSS vulnerabilities in stream HTML
    - [ ] Test with malicious HTML in streams
    - [ ] Verify proper escaping
  - [ ] Test authorization bypass attempts
    - [ ] Try to subscribe to unauthorized streams
    - [ ] Test with manipulated tokens
  - [ ] Validate user input sanitization
    - [ ] Stream names
    - [ ] Target IDs
    - [ ] Frame IDs
  - [ ] Check for injection attacks
    - [ ] HTML injection
    - [ ] JavaScript injection
  - [ ] Review connection security
    - [ ] HTTPS requirements
    - [ ] Token handling
  - [ ] Add security documentation
- [ ] Memory leak testing:
  - [ ] Long-running SignalR connections (24+ hours)
  - [ ] Subscription cleanup verification
  - [ ] Generator memory usage during large builds
  - [ ] JavaScript memory leaks (use Chrome DevTools)
  - [ ] Fix any leaks found
  - [ ] Document memory management
- [ ] Browser compatibility testing:
  - [ ] Test in Chrome (latest)
  - [ ] Test in Firefox (latest)
  - [ ] Test in Safari (latest)
  - [ ] Test in Edge (latest)
  - [ ] Test in Chrome mobile
  - [ ] Test in Safari mobile
  - [ ] Document any issues or limitations
- [ ] Logging and diagnostics:
  - [ ] Add diagnostic logging throughout
  - [ ] Integrate with ILogger
  - [ ] Add structured logging
  - [ ] Add performance counters
  - [ ] Document logging configuration
  - [ ] Add troubleshooting guide

### Acceptance Criteria
- ✅ Benchmarks show acceptable performance
- ✅ Test coverage >95%
- ✅ All edge cases are handled
- ✅ Stress tests pass
- ✅ No security vulnerabilities found
- ✅ No memory leaks detected
- ✅ Works in all major browsers
- ✅ Error messages are helpful
- ✅ Logging is comprehensive
- ✅ All pure functions remain pure

### Files Created
```
tests/Tombatron.Turbo.Benchmarks/
├── Tombatron.Turbo.Benchmarks.csproj
├── FrameRoutingBenchmarks.cs
├── StreamBroadcastBenchmarks.cs
└── SourceGeneratorBenchmarks.cs

tests/Tombatron.Turbo.Tests/
├── EdgeCases/
│   ├── NestedFramesTests.cs
│   ├── LargeDocumentsTests.cs
│   ├── MalformedRazorTests.cs
│   ├── ConcurrentUpdatesTests.cs
│   └── SpecialCharactersTests.cs
└── Security/
    ├── XssTests.cs
    ├── AuthorizationBypassTests.cs
    ├── InjectionTests.cs
    └── InputValidationTests.cs

docs/
├── security.md
├── performance.md
└── browser-compatibility.md
```

---

## Milestone 9: Release
**Status:** 🔴 Not Started

### Objectives
- Beta testing
- NuGet package publishing
- Public announcement
- Community setup

### Tasks
- [ ] Beta testing:
  - [ ] Recruit 3-5 beta testers
  - [ ] Create beta release (1.0.0-beta.1)
  - [ ] Publish to NuGet as prerelease
  - [ ] Collect feedback
  - [ ] Fix critical issues
  - [ ] Create RC release (1.0.0-rc.1)
  - [ ] Final testing round
- [ ] Prepare for release:
  - [ ] Review all documentation
  - [ ] Verify all examples work
  - [ ] Update CHANGELOG.md
  - [ ] Tag version 1.0.0
  - [ ] Build release packages
  - [ ] Sign assemblies (optional)
- [ ] NuGet publishing:
  - [ ] Verify package metadata
  - [ ] Test package installation
  - [ ] Publish Tombatron.Turbo 1.0.0
  - [ ] Verify it appears on NuGet.org
  - [ ] Test installation from NuGet
  - [ ] Verify symbol package works
- [ ] Announcements:
  - [ ] Write blog post announcing release
  - [ ] Post to Reddit:
    - [ ] r/dotnet
    - [ ] r/csharp
    - [ ] r/aspnetcore
  - [ ] Post to Twitter/X
  - [ ] Post to LinkedIn
  - [ ] Consider Hacker News (if appropriate)
  - [ ] Share in .NET Discord servers
  - [ ] Post to dev.to
- [ ] Community setup:
  - [ ] Enable GitHub Discussions
  - [ ] Create issue templates:
    - [ ] Bug report
    - [ ] Feature request
    - [ ] Question
  - [ ] Create CONTRIBUTING.md
    - [ ] Include coding standards
    - [ ] Include testing requirements
  - [ ] Create CODE_OF_CONDUCT.md
  - [ ] Set up GitHub project board
  - [ ] Configure issue labels
  - [ ] Set up sponsor page (optional)
- [ ] Marketing materials:
  - [ ] Create demo GIF/video
  - [ ] Create comparison table (vs Blazor, HTMX, etc.)
  - [ ] Design logo (optional)
  - [ ] Create social media graphics

### Acceptance Criteria
- ✅ Beta testing complete with positive feedback
- ✅ Version 1.0.0 published to NuGet
- ✅ Package installs correctly
- ✅ Announcements posted
- ✅ Community infrastructure in place
- ✅ Documentation is final
- ✅ Coding standards are documented for contributors

### Files Created
```
CHANGELOG.md
CONTRIBUTING.md
CODE_OF_CONDUCT.md

.github/
├── ISSUE_TEMPLATE/
│   ├── bug_report.md
│   ├── feature_request.md
│   └── question.md
└── PULL_REQUEST_TEMPLATE.md

docs/
└── comparison.md
```

---

## Progress Tracking

### Overall Status
- **Milestones Completed:** 4 / 9
- **Current Phase:** Development

### Milestone Summary
1. 🟢 Foundation & Core Infrastructure
2. 🟢 Source Generator
3. 🟢 Roslyn Analyzer
4. 🟢 Middleware & Tag Helper
5. ⚪ Turbo Streams - Server
6. ⚪ Turbo Streams - Client
7. ⚪ Documentation & Samples
8. ⚪ Polish & Testing
9. ⚪ Release

### Legend
- 🔴 Not Started
- 🟡 In Progress
- 🟢 Complete
- ⚪ Future

---

## Notes for Claude Code

**When implementing a milestone:**
1. Update the milestone status from 🔴 to 🟡
2. Check off tasks as they are completed
3. Add any additional files created
4. Note any deviations from the plan
5. When complete, update status to 🟢
6. Update the Progress Tracking section

**When encountering issues:**
- Document the issue in the milestone's "Notes" section (add if needed)
- Consider adding new tasks if needed
- Flag for discussion if needed

**Coding standards reminders:**
- Always use braces for if statements
- Prefer pure functions where possible
- Write comprehensive unit tests for every feature
- Follow .editorconfig rules
- Use nullable reference types
- Validate all inputs

**Testing reminders:**
- Test happy path, error cases, and edge cases
- Use descriptive test names
- Aim for >95% coverage
- Write tests before or alongside implementation
- Mock external dependencies

**Best practices:**
- Keep this document up-to-date as the single source of truth
- Reference task checkboxes in commit messages
- Create GitHub issues for tasks that need discussion
