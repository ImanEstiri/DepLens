// Models/Parser/PackageDependency.cs
namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Represents a NuGet package dependency with its resolved version and source.
/// </summary>
/// <param name="Name">The package identifier.</param>
/// <param name="ResolvedVersion">The resolved version, or null if unknown.</param>
/// <param name="Source">How the version was determined.</param>
public sealed record PackageDependency(
    string Name,
    string? ResolvedVersion,
    PackageVersionSourceType Source);
