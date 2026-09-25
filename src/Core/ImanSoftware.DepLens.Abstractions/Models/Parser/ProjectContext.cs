
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record ProjectContext(
    string ProjectFullPath,
    IReadOnlyList<string> SolutionPaths,      
    ParsedPackagesProps? CentralPackageVersions);
