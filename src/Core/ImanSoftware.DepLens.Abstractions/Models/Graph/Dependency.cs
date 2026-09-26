namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record Dependency(
    string Name,
    string? Version,
    DependencyType Type,
    DependencyScope Scope,
    PackageVersionSourceType? VersionSource,
    bool IsResolved,
    string? TargetFullPath = null);
