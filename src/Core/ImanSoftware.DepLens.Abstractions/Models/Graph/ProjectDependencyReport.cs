

namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Contains the full dependency information for a single project.
/// </summary>
/// <param name="ProjectFullPath">The full path to the project file.</param>
/// <param name="SolutionPaths">The solutions that contain this project.</param>
/// <param name="Dependencies">All direct and transitive dependencies of the project.</param>
public sealed record ProjectDependencyReport(
    string ProjectFullPath,
    IReadOnlyList<string> SolutionPaths,
    IReadOnlyList<Dependency> Dependencies);
