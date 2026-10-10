
using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

/// <summary>
/// Defines a service for discovering relevant files within a directory.
/// </summary>
public interface IDirectoryScanner : IService
{
    /// <summary>
    /// Scans the specified directory and returns all discovered solution, project, and props files.
    /// </summary>
    /// <param name="path">The directory path to scan.</param>
    /// <returns>An outcome containing a list of discovered files.</returns>
    Task<Outcome<List<DiscoveredFile>>> InvestigateDirectoryAsync(string path);
}
