using System.Collections.Immutable;
using System.Composition;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Tombatron.Turbo.Analyzers;

/// <summary>
/// Provides a code fix for TURBO001: adds asp-frame-prefix attribute to dynamic turbo-frame IDs.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddPrefixCodeFixProvider))]
[Shared]
public class AddPrefixCodeFixProvider : CodeFixProvider
{
    private const string Title = "Add asp-frame-prefix attribute";

    // Pattern to match the turbo-frame opening tag and extract the id value
    private static readonly Regex TurboFramePattern = new(
        @"<turbo-frame\s+([^>]*id\s*=\s*[""']([^""']*)[""'][^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <inheritdoc />
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.DynamicIdWithoutPrefix.Id);

    /// <inheritdoc />
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // Get the additional text document
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

            // Find the turbo-frame tag on this line
            int lineNumber = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start).LineNumber;
            string lineText = sourceText.Lines[lineNumber].ToString();

            Match match = TurboFramePattern.Match(lineText);
            if (!match.Success)
            {
                continue;
            }

            string idValue = match.Groups[2].Value;
            string inferredPrefix = InferPrefix(idValue);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedSolution: c => AddPrefixAsync(
                        context.Document.Project.Solution,
                        document,
                        sourceText,
                        lineNumber,
                        match,
                        inferredPrefix,
                        c),
                    equivalenceKey: Title),
                diagnostic);
        }
    }

    /// <summary>
    /// Infers the prefix from a dynamic ID value.
    /// </summary>
    /// <param name="idValue">The ID value containing a Razor expression.</param>
    /// <returns>The inferred prefix (static portion before @).</returns>
    internal static string InferPrefix(string idValue)
    {
        int atIndex = idValue.IndexOf('@');
        if (atIndex <= 0)
        {
            // ID starts with @ or doesn't contain @
            // Use "frame_" as a default prefix
            return "frame_";
        }

        return idValue.Substring(0, atIndex);
    }

    private static Task<Solution> AddPrefixAsync(
        Solution solution,
        TextDocument document,
        SourceText sourceText,
        int lineNumber,
        Match match,
        string prefix,
        CancellationToken cancellationToken)
    {
        // Calculate where to insert the asp-frame-prefix attribute
        TextLine line = sourceText.Lines[lineNumber];
        string lineText = line.ToString();

        // Insert asp-frame-prefix after the id attribute
        int idEndIndex = match.Groups[2].Index + match.Groups[2].Length + 1; // +1 for closing quote
        string newAttribute = $" asp-frame-prefix=\"{prefix}\"";

        string newLineText = lineText.Insert(idEndIndex, newAttribute);

        // Create the new source text
        TextChange change = new(line.Span, newLineText);
        SourceText newSourceText = sourceText.WithChanges(change);

        // Update the solution
        Solution newSolution = solution.WithAdditionalDocumentText(document.Id, newSourceText);

        return Task.FromResult(newSolution);
    }
}
