using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Tombatron.Turbo.Stimulus;

/// <summary>
/// Represents a discovered Stimulus controller.
/// </summary>
/// <param name="StimulusIdentifier">The derived Stimulus identifier (e.g. "hello").</param>
/// <param name="ImportPath">The URL path to import the controller from (e.g. "/controllers/hello_controller.js").</param>
public record ControllerEntry(string StimulusIdentifier, string ImportPath);

/// <summary>
/// Maintains an in-memory registry of discovered Stimulus controllers.
/// </summary>
public sealed class StimulusControllerRegistry
{
    private IReadOnlyList<ControllerEntry> _controllers = Array.Empty<ControllerEntry>();
    private string _etag = "";
    private string _generatedIndexModule = "";

    /// <summary>
    /// Gets the list of discovered controllers.
    /// </summary>
    public IReadOnlyList<ControllerEntry> Controllers => _controllers;

    /// <summary>
    /// Gets the ETag for the current controller list (for HTTP 304 support).
    /// </summary>
    public string ETag => _etag;

    /// <summary>
    /// Gets the cached generated JavaScript index module source.
    /// </summary>
    public string GeneratedIndexModule => _generatedIndexModule;

    /// <summary>
    /// Scans the controllers directory and rebuilds the registry.
    /// </summary>
    /// <param name="webRootFileProvider">The web root file provider to scan.</param>
    /// <param name="controllersPath">The relative path to the controllers directory.</param>
    /// <param name="logger">Logger for reporting discovery results.</param>
    public void Rebuild(IFileProvider webRootFileProvider, string controllersPath, ILogger logger)
    {
        var entries = new List<ControllerEntry>();
        ScanDirectory(webRootFileProvider, controllersPath, "", entries, logger);

        _controllers = entries.AsReadOnly();
        _generatedIndexModule = BuildIndexModule(entries);
        _etag = ComputeETag(_generatedIndexModule);
    }

    private static void ScanDirectory(
        IFileProvider fileProvider,
        string controllersPath,
        string relativePath,
        List<ControllerEntry> entries,
        ILogger logger)
    {
        var directoryPath = string.IsNullOrEmpty(relativePath)
            ? controllersPath
            : $"{controllersPath}/{relativePath}";

        var contents = fileProvider.GetDirectoryContents(directoryPath);

        foreach (var item in contents)
        {
            var itemRelativePath = string.IsNullOrEmpty(relativePath)
                ? item.Name
                : $"{relativePath}/{item.Name}";

            if (item.IsDirectory)
            {
                ScanDirectory(fileProvider, controllersPath, itemRelativePath, entries, logger);
            }
            else if (item.Name.EndsWith("_controller.js", StringComparison.OrdinalIgnoreCase))
            {
                var identifier = DeriveIdentifier(itemRelativePath);

                if (identifier != null)
                {
                    var importPath = $"/{controllersPath}/{itemRelativePath}";
                    entries.Add(new ControllerEntry(identifier, importPath));

                    if (itemRelativePath.Contains('/') || itemRelativePath.Contains('_'))
                    {
                        logger.LogInformation(
                            "Stimulus: {FilePath} → \"{Identifier}\"",
                            itemRelativePath,
                            identifier);
                    }
                    else
                    {
                        logger.LogDebug(
                            "Stimulus: {FilePath} → \"{Identifier}\"",
                            itemRelativePath,
                            identifier);
                    }
                }
                else
                {
                    logger.LogWarning(
                        "Stimulus: Could not derive identifier from {FilePath}, skipping.",
                        itemRelativePath);
                }
            }
        }
    }

    /// <summary>
    /// Derives a Stimulus identifier from a controller file path.
    /// </summary>
    /// <param name="relativePath">Path relative to the controllers directory (e.g. "admin/user_profile_controller.js").</param>
    /// <returns>The Stimulus identifier, or null if the path is invalid.</returns>
    internal static string? DeriveIdentifier(string relativePath)
    {
        // Normalize to forward slashes
        var normalized = relativePath.Replace('\\', '/');

        // Must end with _controller.js
        if (!normalized.EndsWith("_controller.js", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Strip the _controller.js suffix
        var withoutSuffix = normalized[..^"_controller.js".Length];

        if (string.IsNullOrEmpty(withoutSuffix))
        {
            return null;
        }

        // Split into directory parts and filename
        var parts = withoutSuffix.Split('/');

        // Replace underscores with hyphens in each segment, then join with --
        var transformed = parts.Select(part => part.Replace('_', '-'));
        var identifier = string.Join("--", transformed);

        return string.IsNullOrEmpty(identifier) ? null : identifier;
    }

    private static string BuildIndexModule(IReadOnlyList<ControllerEntry> entries)
    {
        if (entries.Count == 0)
        {
            return "// No Stimulus controllers discovered.\n";
        }

        var sb = new StringBuilder();

        sb.AppendLine("import { application } from \"/_content/Tombatron.Turbo.Stimulus/_stimulus/application.js\";");

        // Generate import statements
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var className = GenerateClassName(entry.StimulusIdentifier, i);
            sb.AppendLine($"import {className} from \"{entry.ImportPath}\";");
        }

        sb.AppendLine();

        // Generate registration calls
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var className = GenerateClassName(entry.StimulusIdentifier, i);
            sb.AppendLine($"application.register(\"{entry.StimulusIdentifier}\", {className});");
        }

        return sb.ToString();
    }

    private static string GenerateClassName(string identifier, int index)
    {
        // Convert identifier like "hello" to "HelloController"
        // Convert "admin--users" to "AdminUsersController"
        var parts = identifier.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        var pascal = string.Concat(parts.Select(p =>
            char.ToUpperInvariant(p[0]) + p[1..]));

        return $"{pascal}Controller";
    }

    private static string ComputeETag(string content)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return $"\"{Convert.ToHexStringLower(hash[..8])}\"";
    }
}
