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
    // Matches: @model TypeName or @model Namespace.TypeName
    private static readonly Regex ModelDirectivePattern = new(
        @"^\s*@model\s+(?<type>[^\s<>]+(?:<[^>]+>)?)",
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

        return match.Groups["type"].Value;
    }

    /// <summary>
    /// Gets the view path for a partial relative to Pages or Views folder.
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    /// <returns>The view path (e.g., "Shared/_Message" for "Pages/Shared/_Message.cshtml").</returns>
    public static string GetViewPath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return string.Empty;
        }

        // Normalize path separators
        string normalizedPath = filePath.Replace('\\', '/');

        // Find Pages or Views folder
        int pagesIndex = FindFolderIndex(normalizedPath, "/Pages/");
        int viewsIndex = FindFolderIndex(normalizedPath, "/Views/");

        int startIndex = Math.Max(pagesIndex, viewsIndex);
        if (startIndex == -1)
        {
            // Fallback: use file name
            return Path.GetFileNameWithoutExtension(filePath);
        }

        // Extract path after Pages/ or Views/
        string relativePath = normalizedPath.Substring(startIndex);

        // Remove .cshtml extension
        if (relativePath.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
        {
            relativePath = relativePath.Substring(0, relativePath.Length - 7);
        }

        return relativePath;
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

    private static int FindFolderIndex(string path, string folder)
    {
        int index = path.IndexOf(folder, StringComparison.OrdinalIgnoreCase);
        if (index == -1)
        {
            return -1;
        }

        // Return the position after the folder name
        return index + folder.Length;
    }
}
