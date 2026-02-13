namespace Tombatron.Turbo.Rendering;

/// <summary>
/// Represents a partial view template without a model type.
/// </summary>
public readonly struct PartialTemplate
{
    /// <summary>
    /// Gets the view path used for rendering.
    /// </summary>
    public string ViewPath { get; }

    /// <summary>
    /// Gets the partial name (without underscore prefix).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialTemplate"/> struct.
    /// </summary>
    /// <param name="viewPath">The view path for rendering.</param>
    /// <param name="name">The partial name.</param>
    public PartialTemplate(string viewPath, string name)
    {
        ViewPath = viewPath;
        Name = name;
    }

    /// <summary>
    /// Returns the view path.
    /// </summary>
    public override string ToString() => ViewPath;
}

/// <summary>
/// Represents a partial view template with a strongly-typed model.
/// </summary>
/// <typeparam name="TModel">The model type for the partial.</typeparam>
public readonly struct PartialTemplate<TModel>
{
    /// <summary>
    /// Gets the view path used for rendering.
    /// </summary>
    public string ViewPath { get; }

    /// <summary>
    /// Gets the partial name (without underscore prefix).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialTemplate{TModel}"/> struct.
    /// </summary>
    /// <param name="viewPath">The view path for rendering.</param>
    /// <param name="name">The partial name.</param>
    public PartialTemplate(string viewPath, string name)
    {
        ViewPath = viewPath;
        Name = name;
    }

    /// <summary>
    /// Returns the view path.
    /// </summary>
    public override string ToString() => ViewPath;
}
