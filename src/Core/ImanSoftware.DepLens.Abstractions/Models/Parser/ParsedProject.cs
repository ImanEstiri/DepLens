namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record ParsedProject(
    IReadOnlyList<string> TargetFrameworks,
    IReadOnlyList<RawProjectReference> ProjectReferences,
    IReadOnlyList<PackageDependency> PackageReferences,
    bool? ManagePackageVersionsCentrallyOverride,
    bool EffectiveManagePackageVersionsCentrally) : IParsedContent;
