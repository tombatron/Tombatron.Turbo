using System.Text;

namespace Tombatron.Turbo.Streams;

/// <summary>
/// Implementation of <see cref="ITurboStreamBuilder"/> for constructing Turbo Stream actions.
/// </summary>
/// <remarks>
/// This class generates valid Turbo Stream HTML that can be processed by Hotwire Turbo.js.
/// All generation methods are pure functions with no side effects.
/// </remarks>
public sealed class TurboStreamBuilder : ITurboStreamBuilder
{
    private readonly List<string> _actions = new();

    /// <inheritdoc />
    public ITurboStreamBuilder Append(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        _actions.Add(GenerateAction("append", target, html));
        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Prepend(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        _actions.Add(GenerateAction("prepend", target, html));
        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Replace(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        _actions.Add(GenerateAction("replace", target, html));
        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Update(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        _actions.Add(GenerateAction("update", target, html));
        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Remove(string target)
    {
        ValidateTarget(target);

        _actions.Add(GenerateRemoveAction(target));
        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Before(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        _actions.Add(GenerateAction("before", target, html));
        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder After(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        _actions.Add(GenerateAction("after", target, html));
        return this;
    }

    /// <inheritdoc />
    public string Build()
    {
        if (_actions.Count == 0)
        {
            return string.Empty;
        }

        if (_actions.Count == 1)
        {
            return _actions[0];
        }

        var sb = new StringBuilder();
        foreach (string action in _actions)
        {
            sb.Append(action);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates a Turbo Stream action element with content.
    /// </summary>
    /// <param name="action">The action type (append, prepend, replace, update, before, after).</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="html">The HTML content.</param>
    /// <returns>The Turbo Stream action HTML.</returns>
    internal static string GenerateAction(string action, string target, string html)
    {
        // Note: We don't HTML-escape the target ID because it should be a valid DOM ID
        // The html content is wrapped in a template element and is not escaped
        // (it's the caller's responsibility to ensure safe HTML)
        return $"<turbo-stream action=\"{action}\" target=\"{EscapeAttribute(target)}\"><template>{html}</template></turbo-stream>";
    }

    /// <summary>
    /// Generates a Turbo Stream remove action element (no content needed).
    /// </summary>
    /// <param name="target">The DOM ID of the target element to remove.</param>
    /// <returns>The Turbo Stream remove action HTML.</returns>
    internal static string GenerateRemoveAction(string target)
    {
        return $"<turbo-stream action=\"remove\" target=\"{EscapeAttribute(target)}\"></turbo-stream>";
    }

    /// <summary>
    /// Escapes a value for use in an HTML attribute.
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <returns>The escaped value.</returns>
    internal static string EscapeAttribute(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private static void ValidateTarget(string target)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ArgumentException("Target cannot be empty or whitespace.", nameof(target));
        }
    }

    private static void ValidateHtml(string html)
    {
        if (html == null)
        {
            throw new ArgumentNullException(nameof(html));
        }
    }
}
