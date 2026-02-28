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
    private readonly StringWriter _outputActions = new();

    /// <summary>
    /// Gets or sets the partial renderer for async partial rendering operations.
    /// </summary>
    /// <remarks>
    /// This property is set internally by TurboService when using async Stream overloads.
    /// </remarks>
    internal IPartialRenderer? Renderer { get; set; } // ???

    /// <inheritdoc />
    public ITurboStreamBuilder Append(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("append", target, html, _outputActions);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Prepend(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("prepend", target, html, _outputActions);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Replace(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("replace", target, html, _outputActions);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Update(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("update", target, html,  _outputActions);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Remove(string target)
    {
        ValidateTarget(target);

        GenerateRemoveAction(target, _outputActions);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Refresh(string? requestId = null)
    {
        GenerateRefreshAction(requestId, _outputActions);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder Before(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("before", target, html, _outputActions);

        return this;
    }

    /// <inheritdoc />
    public ITurboStreamBuilder After(string target, string html)
    {
        ValidateTarget(target);
        ValidateHtml(html);

        GenerateAction("after", target, html, _outputActions);

        return this;
    }

    /// <inheritdoc />
    public string Build() => _outputActions.ToString();

    /// <summary>
    /// Appends a Turbo Stream action element to the provided string writer.
    /// </summary>
    /// <param name="action">The action type (append, prepend, replace, update, before after).</param>
    /// <param name="target">The DOM ID of the target element.</param>
    /// <param name="html">The HTML content.</param>
    /// <param name="outputWriter">The string writer used to produce the payload for the stream.</param>
    internal static void GenerateAction(string action, string target, string html, StringWriter outputWriter)
    {
        outputWriter.Write("<turbo-stream action=\"");
        outputWriter.Write(action);
        outputWriter.Write("\" target=\"");
        outputWriter.Write(EscapeAttribute(target));
        outputWriter.Write("\"><template>");
        outputWriter.Write(html);
        outputWriter.Write("</template></turbo-stream>");
    }

    /// <summary>
    /// Appends a Turbo Stream remove action element to the provided string writer.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="outputWriter"></param>
    internal static void GenerateRemoveAction(string target, StringWriter outputWriter)
    {
        outputWriter.Write("<turbo-stream action=\"remove\" target=\"");
        outputWriter.Write(EscapeAttribute(target));
        outputWriter.Write("></turbo-stream>");
    }

    /// <summary>
    /// Appends a Turbo Stream refresh action element to the provided string writer.
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="outputWriter"></param>
    internal static void GenerateRefreshAction(string? requestId, StringWriter outputWriter)
    {
        if (string.IsNullOrEmpty(requestId))
        {
            outputWriter.Write("<turbo-stream action=\"refresh\"></turbo-stream>");
        }
        else
        {
            outputWriter.Write("<turbo-stream action=\"refresh\" request-id=\"");
            outputWriter.Write("EscapeAttribute(requestId)");
            outputWriter.Write("></turbo-stream>");
        }
    }

    /// <summary>
    /// Escapes a value for use in an HTML attribute.
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <returns>The escaped value.</returns>
    internal static string EscapeAttribute(string value) => HtmlEncoder.Default.Encode(value);

    private static void ValidateTarget(string target) => ArgumentException.ThrowIfNullOrEmpty(target);

    private static void ValidateHtml(string html) => ArgumentNullException.ThrowIfNull(html);
}
