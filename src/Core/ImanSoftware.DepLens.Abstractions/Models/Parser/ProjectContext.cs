
namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Contains the context for a project, including its solutions and central package versions.
/// </summary>
/// <param name="ProjectFullPath">The full path to the project file.</param>
/// <param name="SolutionPaths">The solutions that contain this project.</param>
/// <param name="CentralPackageVersions">The central package versions applicable to this project.</param>
public sealed record ProjectContext(
    string ProjectFullPath,
    IReadOnlyList<string> SolutionPaths,
    ParsedPackagesProps? CentralPackageVersions);
