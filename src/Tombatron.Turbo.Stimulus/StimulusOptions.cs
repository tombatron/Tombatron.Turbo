namespace Tombatron.Turbo.Stimulus;

/// <summary>
/// Configuration options for Stimulus controller discovery and serving.
/// </summary>
public sealed class StimulusOptions
{
    /// <summary>
    /// Gets or sets the path to the controllers directory, relative to wwwroot.
    /// Default is "controllers".
    /// </summary>
    public string ControllersPath { get; set; } = "controllers";

    /// <summary>
    /// Gets or sets the endpoint path for the generated controller index module.
    /// Default is "/_stimulus/controllers/index.js".
    /// </summary>
    public string IndexEndpointPath { get; set; } = "/_stimulus/controllers/index.js";

    /// <summary>
    /// Gets or sets the CDN URL for the Stimulus library.
    /// Default is the unpkg URL for @hotwired/stimulus 3.2.2.
    /// </summary>
    public string StimulusCdnUrl { get; set; } = "https://unpkg.com/@hotwired/stimulus@3.2.2/dist/stimulus.js";

    /// <summary>
    /// Gets or sets whether to enable hot reload of controllers in development.
    /// When null, automatically enabled if the environment is Development.
    /// </summary>
    public bool? EnableHotReload { get; set; }

    /// <summary>
    /// Validates the current options configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ControllersPath))
        {
            throw new InvalidOperationException("ControllersPath cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(IndexEndpointPath))
        {
            throw new InvalidOperationException("IndexEndpointPath cannot be null or empty.");
        }

        if (!IndexEndpointPath.StartsWith('/'))
        {
            throw new InvalidOperationException("IndexEndpointPath must start with a forward slash.");
        }

        if (string.IsNullOrWhiteSpace(StimulusCdnUrl))
        {
            throw new InvalidOperationException("StimulusCdnUrl cannot be null or empty.");
        }
    }
}
