using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

/// <summary>
/// Defines a service for building dependency reports from parsed projects.
/// </summary>
public interface IDependencyGraphBuilderService : IService
{
    /// <summary>
    /// Builds dependency reports for all projects.
    /// </summary>
    /// <param name="parsedProjects">The parsed project files.</param>
    /// <param name="resolvedReferences">The resolved project references.</param>
    /// <param name="projectContexts">The project contexts.</param>
    /// <returns>An outcome containing a list of project dependency reports.</returns>
    Task<Outcome<List<ProjectDependencyReport>>> Build(
        IReadOnlyList<ParsedFile> parsedProjects,
        IReadOnlyList<ResolvedProjectReferences> resolvedReferences,
        IReadOnlyList<ProjectContext> projectContexts);
}
