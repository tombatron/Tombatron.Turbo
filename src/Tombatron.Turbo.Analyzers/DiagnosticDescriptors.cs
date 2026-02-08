using Microsoft.CodeAnalysis;

namespace Tombatron.Turbo.Analyzers;

/// <summary>
/// Contains diagnostic descriptors for Turbo analyzers.
/// </summary>
public static class DiagnosticDescriptors
{
    /// <summary>
    /// Category for all Turbo diagnostics.
    /// </summary>
    public const string Category = "Tombatron.Turbo";

    /// <summary>
    /// TURBO001: Dynamic turbo-frame ID requires asp-frame-prefix attribute.
    /// </summary>
    public static readonly DiagnosticDescriptor DynamicIdWithoutPrefix = new(
        id: "TURBO001",
        title: "Dynamic ID without prefix",
        messageFormat: "Dynamic turbo-frame ID requires asp-frame-prefix attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "When using a dynamic ID in a turbo-frame element, you must provide an asp-frame-prefix attribute to enable compile-time optimization.");

    /// <summary>
    /// TURBO002: The asp-frame-prefix value does not match the static portion of the ID.
    /// </summary>
    public static readonly DiagnosticDescriptor PrefixMismatch = new(
        id: "TURBO002",
        title: "Prefix mismatch",
        messageFormat: "The asp-frame-prefix '{0}' does not match the static portion of the ID '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The asp-frame-prefix attribute must match the static portion of the turbo-frame ID.");

    /// <summary>
    /// TURBO003: Static turbo-frame ID does not need asp-frame-prefix.
    /// </summary>
    public static readonly DiagnosticDescriptor UnnecessaryPrefix = new(
        id: "TURBO003",
        title: "Unnecessary prefix",
        messageFormat: "Static turbo-frame ID does not need asp-frame-prefix attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The asp-frame-prefix attribute is only needed for dynamic IDs. Static IDs are automatically optimized.");
}
