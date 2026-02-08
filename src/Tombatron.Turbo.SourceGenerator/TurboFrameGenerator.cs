using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Tombatron.Turbo.SourceGenerator.Models;

namespace Tombatron.Turbo.SourceGenerator;

/// <summary>
/// Source generator that detects turbo-frame elements in Razor files and generates
/// optimized sub-templates and metadata at compile time.
/// </summary>
[Generator]
public class TurboFrameGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all .cshtml files provided as additional files
        IncrementalValuesProvider<AdditionalText> razorFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase));

        // Parse each Razor file for turbo-frame elements
        IncrementalValuesProvider<RazorFileInfo?> parsedFiles = razorFiles
            .Select(static (file, cancellationToken) =>
            {
                SourceText? sourceText = file.GetText(cancellationToken);
                if (sourceText == null)
                {
                    return null;
                }

                string content = sourceText.ToString();
                ImmutableArray<TurboFrame> frames = FrameParser.Parse(content);

                if (frames.Length == 0)
                {
                    return null;
                }

                string viewName = GetViewName(file.Path);
                return new RazorFileInfo(file.Path, viewName, frames);
            })
            .Where(static info => info != null);

        // Collect all parsed files
        IncrementalValueProvider<ImmutableArray<RazorFileInfo>> collectedFiles = parsedFiles
            .Select(static (info, _) => info!)
            .Collect();

        // Generate the metadata source file
        context.RegisterSourceOutput(collectedFiles, static (context, razorFiles) =>
        {
            if (razorFiles.Length == 0)
            {
                return;
            }

            string metadataSource = MetadataGenerator.GenerateMetadataSource(razorFiles);
            context.AddSource("TurboFrameMetadata.g.cs", SourceText.From(metadataSource, Encoding.UTF8));

            // Generate diagnostic for any invalid frames (dynamic without prefix)
            foreach (RazorFileInfo file in razorFiles)
            {
                foreach (TurboFrame frame in file.Frames)
                {
                    if (!FrameParser.IsValidFrame(frame))
                    {
                        // Report diagnostic - this will be handled by the analyzer project
                        // For now, we just skip invalid frames
                    }
                }
            }
        });
    }

    /// <summary>
    /// Extracts the logical view name from a file path.
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    /// <returns>The logical view name.</returns>
    private static string GetViewName(string filePath)
    {
        // Extract just the file name without extension
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        // Try to extract a more meaningful name from the path
        // e.g., "Pages/Cart/Index.cshtml" -> "Cart_Index"
        string[] parts = filePath.Replace('\\', '/').Split('/');

        var relevantParts = new List<string>();
        bool foundPagesOrViews = false;

        foreach (string part in parts)
        {
            if (part.Equals("Pages", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("Views", StringComparison.OrdinalIgnoreCase))
            {
                foundPagesOrViews = true;
                continue;
            }

            if (foundPagesOrViews && !string.IsNullOrEmpty(part))
            {
                // Remove .cshtml extension if present
                string cleanPart = part.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                    ? part.Substring(0, part.Length - 7)
                    : part;

                if (!string.IsNullOrEmpty(cleanPart))
                {
                    relevantParts.Add(cleanPart);
                }
            }
        }

        if (relevantParts.Count > 0)
        {
            return string.Join("_", relevantParts);
        }

        return fileName;
    }
}
