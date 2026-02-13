namespace Tombatron.Turbo.SourceGenerator.Models;

/// <summary>
/// Represents metadata about a Razor partial view discovered at compile time.
/// </summary>
/// <param name="FilePath">The full file path to the partial view.</param>
/// <param name="PartialName">The name of the partial without the underscore prefix (e.g., "Message" for "_Message.cshtml").</param>
/// <param name="ViewPath">The application-relative view path (e.g., "/Pages/Shared/_Message.cshtml").</param>
/// <param name="ModelTypeName">The fully qualified model type name if @model directive is present, otherwise null.</param>
public sealed record PartialInfo(
    string FilePath,
    string PartialName,
    string ViewPath,
    string? ModelTypeName);
