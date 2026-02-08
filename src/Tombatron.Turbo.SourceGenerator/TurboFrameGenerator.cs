using Microsoft.CodeAnalysis;

namespace Tombatron.Turbo.SourceGenerator;

/// <summary>
/// Source generator that detects turbo-frame elements in Razor files and generates
/// optimized sub-templates at compile time.
/// </summary>
[Generator]
public class TurboFrameGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Implementation will be added in Milestone 2
    }
}
