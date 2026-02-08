using System.Collections.Immutable;
using System.Composition;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Tombatron.Turbo.Analyzers;

/// <summary>
/// Provides a code fix for TURBO003: removes unnecessary asp-frame-prefix attribute from static IDs.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemovePrefixCodeFixProvider))]
[Shared]
public class RemovePrefixCodeFixProvider : CodeFixProvider
{
    private const string Title = "Remove unnecessary asp-frame-prefix attribute";

    // Pattern to match the asp-frame-prefix attribute with optional surrounding whitespace
    private static readonly Regex PrefixAttributePattern = new(
        @"\s*asp-frame-prefix\s*=\s*[""'][^""']*[""']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <inheritdoc />
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.UnnecessaryPrefix.Id);

    /// <inheritdoc />
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        TextDocument? document = context.Document.Project.AdditionalDocuments
            .FirstOrDefault(d => d.FilePath == context.Document.FilePath);

        if (document == null)
        {
            return;
        }

        SourceText? sourceText = await document.GetTextAsync(context.CancellationToken);
        if (sourceText == null)
        {
            return;
        }

        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            int lineNumber = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start).LineNumber;
            string lineText = sourceText.Lines[lineNumber].ToString();

            Match prefixMatch = PrefixAttributePattern.Match(lineText);
            if (!prefixMatch.Success)
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedSolution: c => RemovePrefixAsync(
                        context.Document.Project.Solution,
                        document,
                        sourceText,
                        lineNumber,
                        prefixMatch,
                        c),
                    equivalenceKey: Title),
                diagnostic);
        }
    }

    private static Task<Solution> RemovePrefixAsync(
        Solution solution,
        TextDocument document,
        SourceText sourceText,
        int lineNumber,
        Match prefixMatch,
        CancellationToken cancellationToken)
    {
        TextLine line = sourceText.Lines[lineNumber];
        string lineText = line.ToString();

        // Remove the asp-frame-prefix attribute
        string newLineText = lineText.Remove(prefixMatch.Index, prefixMatch.Length);

        TextChange change = new(line.Span, newLineText);
        SourceText newSourceText = sourceText.WithChanges(change);

        Solution newSolution = solution.WithAdditionalDocumentText(document.Id, newSourceText);

        return Task.FromResult(newSolution);
    }
}
