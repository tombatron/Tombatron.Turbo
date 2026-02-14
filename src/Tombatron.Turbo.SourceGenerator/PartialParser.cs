using System.Text.RegularExpressions;
using Tombatron.Turbo.SourceGenerator.Models;

namespace Tombatron.Turbo.SourceGenerator;

/// <summary>
/// Parses Razor partial views and extracts metadata.
/// All methods are pure functions with no side effects.
/// </summary>
public static class PartialParser
{
    // Files to exclude from partial generation
    private static readonly HashSet<string> ExcludedFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "_Layout",
        "_ViewStart",
        "_ViewImports"
    };

    // Regex to extract @model directive type name
    // Matches everything after @model to end-of-line, supporting simple types,
    // generics, tuples, nullable types, and fully qualified names.
    private static readonly Regex ModelDirectivePattern = new(
        @"^\s*@model\s+(?<type>.+)$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    /// Determines if a file is a partial view that should be processed.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>True if the file is a valid partial view.</returns>
    public static bool IsPartialFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }

        // Must be a .cshtml file
        if (!filePath.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string fileName = Path.GetFileNameWithoutExtension(filePath);

        // Must start with underscore
        if (!fileName.StartsWith("_", StringComparison.Ordinal))
        {
            return false;
        }

        // Must not be an excluded file
        return !ExcludedFileNames.Contains(fileName);
    }

    /// <summary>
    /// Extracts the model type from a Razor partial view's @model directive.
    /// </summary>
    /// <param name="content">The content of the Razor file.</param>
    /// <returns>The model type name if found, otherwise null.</returns>
    public static string? ExtractModelType(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        Match match = ModelDirectivePattern.Match(content);
        if (!match.Success)
        {
            return null;
        }

        return match.Groups["type"].Value.Trim();
    }

    /// <summary>
    /// Gets the application-relative view path for a partial.
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    /// <returns>The application-relative view path (e.g., "/Pages/Shared/_Message.cshtml").</returns>
    public static string GetViewPath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return string.Empty;
        }

        string normalizedPath = filePath.Replace('\\', '/');

        int pagesIndex = normalizedPath.IndexOf("/Pages/", StringComparison.OrdinalIgnoreCase);
        int viewsIndex = normalizedPath.IndexOf("/Views/", StringComparison.OrdinalIgnoreCase);

        int startIndex = Math.Max(pagesIndex, viewsIndex);
        if (startIndex == -1)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        // Return absolute path from /Pages/ or /Views/ onward (with .cshtml extension)
        return normalizedPath.Substring(startIndex);
    }

    /// <summary>
    /// Gets the partial name from a file path (without underscore prefix).
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    /// <returns>The partial name (e.g., "Message" for "_Message.cshtml").</returns>
    public static string GetPartialName(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return string.Empty;
        }

        string fileName = Path.GetFileNameWithoutExtension(filePath);

        // Remove underscore prefix
        if (fileName.StartsWith("_", StringComparison.Ordinal))
        {
            return fileName.Substring(1);
        }

        return fileName;
    }

    /// <summary>
    /// Parses a Razor partial file and returns its metadata.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="content">The content of the file.</param>
    /// <returns>The partial info, or null if not a valid partial.</returns>
    public static PartialInfo? Parse(string filePath, string content)
    {
        if (!IsPartialFile(filePath))
        {
            return null;
        }

        string partialName = GetPartialName(filePath);
        string viewPath = GetViewPath(filePath);
        string? modelType = ExtractModelType(content);

        return new PartialInfo(filePath, partialName, viewPath, modelType);
    }

}
