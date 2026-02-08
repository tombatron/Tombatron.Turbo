using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Tombatron.Turbo.SourceGenerator;
using Tombatron.Turbo.SourceGenerator.Models;

namespace Tombatron.Turbo.Analyzers;

/// <summary>
/// Analyzes Razor files for turbo-frame issues and reports diagnostics.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TurboFrameAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticDescriptors.DynamicIdWithoutPrefix,
            DiagnosticDescriptors.PrefixMismatch,
            DiagnosticDescriptors.UnnecessaryPrefix);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            compilationContext.RegisterAdditionalFileAction(AnalyzeAdditionalFile);
        });
    }

    private static void AnalyzeAdditionalFile(AdditionalFileAnalysisContext context)
    {
        // Only analyze .cshtml files
        if (!context.AdditionalFile.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SourceText? sourceText = context.AdditionalFile.GetText(context.CancellationToken);
        if (sourceText == null)
        {
            return;
        }

        string content = sourceText.ToString();
        ImmutableArray<TurboFrame> frames = FrameParser.Parse(content);

        foreach (TurboFrame frame in frames)
        {
            AnalyzeFrame(context, sourceText, frame);
        }
    }

    private static void AnalyzeFrame(
        AdditionalFileAnalysisContext context,
        SourceText sourceText,
        TurboFrame frame)
    {
        // Get the location for the diagnostic
        Location location = CreateLocation(context.AdditionalFile.Path, sourceText, frame.StartLine);

        if (frame.IsDynamic)
        {
            if (!frame.HasPrefix)
            {
                // TURBO001: Dynamic ID without prefix
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.DynamicIdWithoutPrefix,
                    location));
            }
            else if (!IsPrefixValid(frame))
            {
                // TURBO002: Prefix mismatch
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.PrefixMismatch,
                    location,
                    frame.Prefix,
                    frame.StaticIdPortion));
            }
        }
        else if (frame.HasPrefix)
        {
            // TURBO003: Unnecessary prefix on static ID
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.UnnecessaryPrefix,
                location));
        }
    }

    /// <summary>
    /// Validates that the prefix matches the static portion of a dynamic ID.
    /// </summary>
    private static bool IsPrefixValid(TurboFrame frame)
    {
        if (!frame.IsDynamic || !frame.HasPrefix)
        {
            return true;
        }

        string staticPortion = frame.StaticIdPortion;
        string prefix = frame.Prefix!;

        // The prefix should match the static portion of the ID
        // e.g., id="item_@Model.Id" with prefix="item_" is valid
        // e.g., id="item_@Model.Id" with prefix="product_" is invalid

        // If the ID starts with just @, there's no static portion to match
        if (string.IsNullOrEmpty(staticPortion))
        {
            // Any prefix is technically valid for a fully dynamic ID like "@Model.Id"
            // The prefix just defines how we'll match at runtime
            return true;
        }

        // The static portion should start with the prefix, or they should be equal
        return staticPortion.Equals(prefix, StringComparison.Ordinal) ||
               staticPortion.StartsWith(prefix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a location for a diagnostic in an additional file.
    /// </summary>
    private static Location CreateLocation(string filePath, SourceText sourceText, int lineNumber)
    {
        // Convert 1-based line number to 0-based
        int lineIndex = Math.Max(0, lineNumber - 1);

        if (lineIndex >= sourceText.Lines.Count)
        {
            lineIndex = sourceText.Lines.Count - 1;
        }

        TextLine line = sourceText.Lines[lineIndex];
        TextSpan span = line.Span;

        return Location.Create(
            filePath,
            span,
            new LinePositionSpan(
                new LinePosition(lineIndex, 0),
                new LinePosition(lineIndex, line.End - line.Start)));
    }
}
