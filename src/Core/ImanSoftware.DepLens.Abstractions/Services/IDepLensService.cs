
using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

/// <summary>
/// Defines the main entry point for analyzing .NET solutions.
/// </summary>
public interface IDepLensService : IService
{
    /// <summary>
    /// Analyzes the solution or directory at the specified path and returns dependency reports.
    /// </summary>
    /// <param name="path">The path to a .sln/.slnx file or a directory containing solutions.</param>
    /// <returns>An outcome containing a list of project dependency reports.</returns>
    Task<Outcome<List<ProjectDependencyReport>>> Analyze(string path);
}
