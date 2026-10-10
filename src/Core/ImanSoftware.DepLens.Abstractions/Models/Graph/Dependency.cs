namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Represents a single dependency of a project, which can be a project, package, or assembly.
/// </summary>
/// <param name="Name">The name of the dependency.</param>
/// <param name="Version">The resolved version of the dependency, if applicable.</param>
/// <param name="Type">The type of dependency (project, package, etc.).</param>
/// <param name="Scope">Whether the dependency is direct or transitive.</param>
/// <param name="VersionSource">The source of the version information.</param>
/// <param name="IsResolved">Whether the dependency was successfully resolved.</param>
/// <param name="TargetFullPath">The full path to the target project, if resolved internally.</param>
public sealed record Dependency(
    string Name,
    string? Version,
    DependencyType Type,
    DependencyScope Scope,
    PackageVersionSourceType? VersionSource,
    bool IsResolved,
    string? TargetFullPath = null);
