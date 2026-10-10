using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

/// <summary>
/// Defines a service for resolving project references as internal or external.
/// </summary>
public interface IProjectReferenceLinkerService : IService
{
    /// <summary>
    /// Links project references by resolving them against known projects.
    /// </summary>
    /// <param name="parsedProjects">The parsed project files.</param>
    /// <returns>An outcome containing resolved project references.</returns>
    Task<Outcome<List<ResolvedProjectReferences>>> Link(IReadOnlyList<ParsedFile> parsedProjects);
}
