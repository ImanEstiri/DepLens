using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

/// <summary>
/// Defines a service for determining project contexts (solutions and CPM) for each project.
/// </summary>
public interface IProjectOrienterService : IService
{
    /// <summary>
    /// Orients projects by determining their owning solutions and applicable central package versions.
    /// </summary>
    /// <param name="projectFiles">The discovered project files.</param>
    /// <param name="parsedSolutions">The parsed solution files.</param>
    /// <param name="parsedPackagesProps">The parsed Directory.Packages.props files.</param>
    /// <returns>An outcome containing a list of project contexts.</returns>
    Task<Outcome<List<ProjectContext>>> Orient(
        IReadOnlyList<DiscoveredFile> projectFiles,
        IReadOnlyList<ParsedFile> parsedSolutions,
        IReadOnlyList<ParsedFile> parsedPackagesProps);
}
