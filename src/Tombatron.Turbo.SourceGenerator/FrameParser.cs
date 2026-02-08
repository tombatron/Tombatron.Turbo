using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Tombatron.Turbo.SourceGenerator.Models;

namespace Tombatron.Turbo.SourceGenerator;

/// <summary>
/// Parses turbo-frame elements from Razor file content.
/// All methods are pure functions with no side effects.
/// </summary>
public static class FrameParser
{
    // Regex to match turbo-frame opening tags with attributes
    // Captures: full match, attributes section
    private static readonly Regex OpeningTagPattern = new(
        @"<turbo-frame\s+([^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    // Regex to extract id attribute value (handles both single and double quotes, and Razor expressions)
    private static readonly Regex IdAttributePattern = new(
        @"(?:^|\s)id\s*=\s*(?:""([^""]*)""|'([^']*)')",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Regex to extract asp-frame-prefix attribute value
    private static readonly Regex PrefixAttributePattern = new(
        @"(?:^|\s)asp-frame-prefix\s*=\s*(?:""([^""]*)""|'([^']*)')",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Parses all turbo-frame elements from the given Razor content.
    /// </summary>
    /// <param name="content">The Razor file content to parse.</param>
    /// <returns>An immutable array of parsed turbo-frame elements.</returns>
    public static ImmutableArray<TurboFrame> Parse(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return ImmutableArray<TurboFrame>.Empty;
        }

        var frames = ImmutableArray.CreateBuilder<TurboFrame>();
        var matches = OpeningTagPattern.Matches(content);

        foreach (Match match in matches)
        {
            TurboFrame? frame = ParseFrame(content, match);
            if (frame != null)
            {
                frames.Add(frame);
            }
        }

        return frames.ToImmutable();
    }

    /// <summary>
    /// Parses a single turbo-frame element from a regex match.
    /// </summary>
    private static TurboFrame? ParseFrame(string content, Match openingTagMatch)
    {
        string attributes = openingTagMatch.Groups[1].Value;

        // Extract id attribute
        string? id = ExtractAttributeValue(attributes, IdAttributePattern);
        if (string.IsNullOrEmpty(id))
        {
            // Frame without id is invalid, skip it
            return null;
        }

        // Extract asp-frame-prefix attribute
        string? prefix = ExtractAttributeValue(attributes, PrefixAttributePattern);

        // Determine if ID is dynamic (contains Razor expression)
        // id is guaranteed non-null here due to the check above
        bool isDynamic = ContainsRazorExpression(id!);

        // Extract frame content
        string frameContent = ExtractFrameContent(content, openingTagMatch);

        // Calculate line number
        int startLine = CalculateLineNumber(content, openingTagMatch.Index);

        return new TurboFrame(id!, prefix, frameContent, startLine, isDynamic);
    }

    /// <summary>
    /// Extracts an attribute value using the given regex pattern.
    /// </summary>
    private static string? ExtractAttributeValue(string attributes, Regex pattern)
    {
        Match match = pattern.Match(attributes);
        if (!match.Success)
        {
            return null;
        }

        // Return whichever group matched (double or single quotes)
        return match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
    }

    /// <summary>
    /// Determines if a string contains a Razor expression.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string contains a Razor expression (@).</returns>
    public static bool ContainsRazorExpression(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        // Look for @ that's not escaped (@@)
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == '@')
            {
                // Check if it's escaped
                if (i + 1 < value.Length && value[i + 1] == '@')
                {
                    i++; // Skip the escaped @
                    continue;
                }
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Extracts the content between the opening and closing turbo-frame tags.
    /// </summary>
    private static string ExtractFrameContent(string content, Match openingTagMatch)
    {
        int contentStart = openingTagMatch.Index + openingTagMatch.Length;

        // Find the matching closing tag, handling nested frames
        int closingTagIndex = FindMatchingClosingTag(content, contentStart);

        if (closingTagIndex == -1)
        {
            // No closing tag found, return empty content
            return string.Empty;
        }

        return content.Substring(contentStart, closingTagIndex - contentStart).Trim();
    }

    /// <summary>
    /// Finds the matching closing tag, properly handling nested turbo-frame elements.
    /// </summary>
    private static int FindMatchingClosingTag(string content, int startIndex)
    {
        const string openTag = "<turbo-frame";
        const string closeTag = "</turbo-frame>";

        int depth = 1;
        int currentIndex = startIndex;

        while (depth > 0 && currentIndex < content.Length)
        {
            int nextOpen = content.IndexOf(openTag, currentIndex, StringComparison.OrdinalIgnoreCase);
            int nextClose = content.IndexOf(closeTag, currentIndex, StringComparison.OrdinalIgnoreCase);

            if (nextClose == -1)
            {
                // No closing tag found
                return -1;
            }

            if (nextOpen != -1 && nextOpen < nextClose)
            {
                // Found another opening tag before the closing tag
                depth++;
                currentIndex = nextOpen + openTag.Length;
            }
            else
            {
                // Found a closing tag
                depth--;
                if (depth == 0)
                {
                    return nextClose;
                }
                currentIndex = nextClose + closeTag.Length;
            }
        }

        return -1;
    }

    /// <summary>
    /// Calculates the line number (1-based) for a given character index.
    /// </summary>
    private static int CalculateLineNumber(string content, int charIndex)
    {
        int lineNumber = 1;
        for (int i = 0; i < charIndex && i < content.Length; i++)
        {
            if (content[i] == '\n')
            {
                lineNumber++;
            }
        }
        return lineNumber;
    }

    /// <summary>
    /// Extracts the static portion of a frame ID (the part before any Razor expression).
    /// </summary>
    /// <param name="id">The frame ID.</param>
    /// <returns>The static portion of the ID, or the full ID if it's not dynamic.</returns>
    public static string GetStaticIdPortion(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return string.Empty;
        }

        int atIndex = id.IndexOf('@');
        if (atIndex == -1)
        {
            return id;
        }

        if (atIndex == 0)
        {
            return string.Empty;
        }

        return id.Substring(0, atIndex);
    }

    /// <summary>
    /// Validates that a dynamic frame has a matching prefix.
    /// </summary>
    /// <param name="frame">The frame to validate.</param>
    /// <returns>True if the frame is valid (static, or dynamic with matching prefix).</returns>
    public static bool IsValidFrame(TurboFrame frame)
    {
        if (!frame.IsDynamic)
        {
            return true;
        }

        // Dynamic frames must have a prefix
        if (!frame.HasPrefix)
        {
            return false;
        }

        // Prefix must match the static portion of the ID
        string staticPortion = frame.StaticIdPortion;
        return staticPortion.StartsWith(frame.Prefix!, StringComparison.Ordinal) ||
               frame.Prefix!.StartsWith(staticPortion, StringComparison.Ordinal);
    }
}
