

namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record ProjectDependencyReport(
    string ProjectFullPath,
    IReadOnlyList<string> SolutionPaths,
    IReadOnlyList<Dependency> Dependencies);
