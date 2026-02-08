using System.Text;
using Tombatron.Turbo.SourceGenerator.Models;

namespace Tombatron.Turbo.SourceGenerator;

/// <summary>
/// Generates Razor sub-templates for turbo-frame elements.
/// All methods are pure functions with no side effects.
/// </summary>
public static class TemplateGenerator
{
    /// <summary>
    /// Generates a sub-template for a static frame.
    /// </summary>
    /// <param name="frame">The static frame to generate a template for.</param>
    /// <returns>The generated Razor template content.</returns>
    public static string GenerateStaticTemplate(TurboFrame frame)
    {
        if (frame.IsDynamic)
        {
            throw new ArgumentException("Cannot generate static template for dynamic frame.", nameof(frame));
        }

        var sb = new StringBuilder();

        // Disable layout for partial rendering
        sb.AppendLine("@{");
        sb.AppendLine("    Layout = null;");
        sb.AppendLine("}");
        sb.AppendLine();

        // Wrap content in turbo-frame tag
        sb.AppendLine($"<turbo-frame id=\"{frame.Id}\">");
        sb.AppendLine(frame.Content);
        sb.AppendLine("</turbo-frame>");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a sub-template for a dynamic frame with a prefix.
    /// </summary>
    /// <param name="frame">The dynamic frame to generate a template for.</param>
    /// <returns>The generated Razor template content.</returns>
    public static string GenerateDynamicTemplate(TurboFrame frame)
    {
        if (!frame.IsDynamic)
        {
            throw new ArgumentException("Cannot generate dynamic template for static frame.", nameof(frame));
        }

        if (!frame.HasPrefix)
        {
            throw new ArgumentException("Dynamic frame must have a prefix.", nameof(frame));
        }

        var sb = new StringBuilder();

        // Disable layout and get the frame ID from ViewBag
        sb.AppendLine("@{");
        sb.AppendLine("    Layout = null;");
        sb.AppendLine("    var turboFrameId = ViewBag.TurboFrameId as string;");
        sb.AppendLine("}");
        sb.AppendLine();

        // Wrap content in turbo-frame tag with dynamic ID
        sb.AppendLine("<turbo-frame id=\"@turboFrameId\">");
        sb.AppendLine(TransformDynamicContent(frame.Content));
        sb.AppendLine("</turbo-frame>");

        return sb.ToString();
    }

    /// <summary>
    /// Transforms content that may reference the original dynamic ID to use turboFrameId.
    /// </summary>
    private static string TransformDynamicContent(string content)
    {
        // For now, return content as-is. In a more sophisticated implementation,
        // we could transform references to the original dynamic expression.
        return content;
    }

    /// <summary>
    /// Generates the file name for a static frame sub-template.
    /// </summary>
    /// <param name="viewName">The original view name.</param>
    /// <param name="frameId">The static frame ID.</param>
    /// <returns>The generated file name (without path).</returns>
    public static string GetStaticTemplateFileName(string viewName, string frameId)
    {
        // Sanitize the frame ID for use in a file name
        string sanitizedId = SanitizeForFileName(frameId);
        return $"{viewName}.{sanitizedId}.cshtml";
    }

    /// <summary>
    /// Generates the file name for a dynamic frame sub-template.
    /// </summary>
    /// <param name="viewName">The original view name.</param>
    /// <param name="prefix">The frame prefix.</param>
    /// <returns>The generated file name (without path).</returns>
    public static string GetDynamicTemplateFileName(string viewName, string prefix)
    {
        // Sanitize the prefix for use in a file name
        string sanitizedPrefix = SanitizeForFileName(prefix);
        return $"{viewName}.{sanitizedPrefix}_.cshtml";
    }

    /// <summary>
    /// Sanitizes a string for use in a file name.
    /// </summary>
    private static string SanitizeForFileName(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return "frame";
        }

        var sb = new StringBuilder();
        foreach (char c in input)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
            {
                sb.Append(c);
            }
            else if (c == ':' || c == '/')
            {
                sb.Append('_');
            }
        }

        return sb.Length > 0 ? sb.ToString() : "frame";
    }
}
