using System.Collections.Immutable;
using System.Composition;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Tombatron.Turbo.Analyzers;

/// <summary>
/// Provides a code fix for TURBO002: corrects the asp-frame-prefix attribute to match the ID.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FixPrefixCodeFixProvider))]
[Shared]
public class FixPrefixCodeFixProvider : CodeFixProvider
{
    private const string Title = "Fix asp-frame-prefix to match ID";

    // Pattern to match the asp-frame-prefix attribute
    private static readonly Regex PrefixAttributePattern = new(
        @"asp-frame-prefix\s*=\s*[""']([^""']*)[""']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Pattern to match the id attribute
    private static readonly Regex IdAttributePattern = new(
        @"id\s*=\s*[""']([^""']*)[""']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <inheritdoc />
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.PrefixMismatch.Id);

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

            // Extract the ID to get the correct prefix
            Match idMatch = IdAttributePattern.Match(lineText);
            if (!idMatch.Success)
            {
                continue;
            }

            string correctPrefix = AddPrefixCodeFixProvider.InferPrefix(idMatch.Groups[1].Value);

            Match prefixMatch = PrefixAttributePattern.Match(lineText);
            if (!prefixMatch.Success)
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedSolution: c => FixPrefixAsync(
                        context.Document.Project.Solution,
                        document,
                        sourceText,
                        lineNumber,
                        prefixMatch,
                        correctPrefix,
                        c),
                    equivalenceKey: Title),
                diagnostic);
        }
    }

    private static Task<Solution> FixPrefixAsync(
        Solution solution,
        TextDocument document,
        SourceText sourceText,
        int lineNumber,
        Match prefixMatch,
        string correctPrefix,
        CancellationToken cancellationToken)
    {
        TextLine line = sourceText.Lines[lineNumber];
        string lineText = line.ToString();

        // Replace the prefix value
        string oldAttribute = prefixMatch.Value;
        string newAttribute = $"asp-frame-prefix=\"{correctPrefix}\"";
        string newLineText = lineText.Replace(oldAttribute, newAttribute);

        TextChange change = new(line.Span, newLineText);
        SourceText newSourceText = sourceText.WithChanges(change);

        Solution newSolution = solution.WithAdditionalDocumentText(document.Id, newSourceText);

        return Task.FromResult(newSolution);
    }
}
