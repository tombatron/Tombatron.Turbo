using System.Text;
using System.Text.Encodings.Web;
using Tombatron.Turbo.Rendering;

namespace Tombatron.Turbo.Streams;

/// <summary>
/// Implementation of <see cref="ITurboStreamBuilder"/> for constructing Turbo Stream actions.
/// </summary>
/// <remarks>
/// This class generates valid Turbo Stream HTML that can be processed by Hotwire Turbo.js.
/// </remarks>
public sealed class TurboStreamBuilder : ITurboStreamBuilder
{
    private readonly StringBuilder _outputBuilder = new();

    /// <summary>
    /// Gets or sets the partial renderer for async partial rendering operations.
    /// </summary>
    /// <remarks>
    /// This property is set internally by TurboService when using async Stream overloads.
    /// </remarks>
    internal IPartialRenderer? Renderer { get; init; }

    /// <inheritdoc />
    public ITurboStreamBuilder Append(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("append", target, html, _outputBuilder);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Prepend(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("prepend", target, html, _outputBuilder);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Replace(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("replace", target, html, _outputBuilder);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Update(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("update", target, html,  _outputBuilder);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Remove(string target)
    {
        ValidateTarget(target);

        GenerateRemoveAction(target, _outputBuilder);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Refresh(string? requestId = null)
    {
        GenerateRefreshAction(requestId, _outputBuilder);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Before(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("before", target, html, _outputBuilder);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder After(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("after", target, html, _outputBuilder);

        return this;
    }

    /// <inheritdoc />
    public string Build() => _outputBuilder.ToString();

    /// <summary>
    /// Appends a Turbo Stream action element to the provided string writer.
    /// </summary>
    /// <param name="action">The action type (append, prepend, replace, update, before after).</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="html">The HTML content.</param>
    /// <param name="outputBuilder">The string builder used to produce the payload for the stream.</param>
    internal static void GenerateAction(string action, string target, string html, StringBuilder outputBuilder)
    {
        outputBuilder.Append("<turbo-stream action=\"");
        outputBuilder.Append(action);
        outputBuilder.Append("\" target=\"");
        outputBuilder.Append(EscapeAttribute(target));
        outputBuilder.Append("\"><template>");
        outputBuilder.Append(html);
        outputBuilder.Append("</template></turbo-stream>");
    }

    /// <summary>
    /// Appends a Turbo Stream remove action element to the provided string writer.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="outputBuilder"></param>
    internal static void GenerateRemoveAction(string target, StringBuilder outputBuilder)
    {
        outputBuilder.Append("<turbo-stream action=\"remove\" target=\"");
        outputBuilder.Append(EscapeAttribute(target));
        outputBuilder.Append("\"></turbo-stream>");
    }

    /// <summary>
    /// Appends a Turbo Stream refresh action element to the provided string writer.
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="outputBuilder"></param>
    internal static void GenerateRefreshAction(string? requestId, StringBuilder outputBuilder)
    {
        if (string.IsNullOrEmpty(requestId))
        {
            outputBuilder.Append("<turbo-stream action=\"refresh\"></turbo-stream>");
        }
        else
        {
            outputBuilder.Append("<turbo-stream action=\"refresh\" request-id=\"");
            outputBuilder.Append(EscapeAttribute(requestId));
            outputBuilder.Append("\"></turbo-stream>");
        }
    }

    /// <summary>
    /// Escapes a value for use in an HTML attribute.
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <returns>The escaped value.</returns>
    internal static string? EscapeAttribute(string? value) =>
        value is null ? value : HtmlEncoder.Default.Encode(value);

    private static void ValidateTarget(string target) =>
        ArgumentException.ThrowIfNullOrWhiteSpace(target);

    private static void ValidateHtml(string html) =>
        ArgumentNullException.ThrowIfNull(html);
}
