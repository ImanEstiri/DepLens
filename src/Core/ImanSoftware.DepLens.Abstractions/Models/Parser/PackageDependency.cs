// Models/Parser/PackageDependency.cs
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record PackageDependency(
    string Name,
    string? ResolvedVersion,   
    PackageVersionSourceType Source);
