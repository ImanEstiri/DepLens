namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Represents a parsed .csproj file containing project references, package references, and metadata.
/// </summary>
/// <param name="TargetFrameworks">The target frameworks specified in the project.</param>
/// <param name="ProjectReferences">References to other projects.</param>
/// <param name="PackageReferences">NuGet package references.</param>
/// <param name="ManagePackageVersionsCentrallyOverride">Local override for CPM.</param>
/// <param name="EffectiveManagePackageVersionsCentrally">Whether CPM is effectively enabled.</param>
public sealed record ParsedProject(
    IReadOnlyList<string> TargetFrameworks,
    IReadOnlyList<RawProjectReference> ProjectReferences,
    IReadOnlyList<PackageDependency> PackageReferences,
    bool? ManagePackageVersionsCentrallyOverride,
    bool EffectiveManagePackageVersionsCentrally) : IParsedContent;
