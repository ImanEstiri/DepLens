using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IDependencyGraphBuilderService : IService
{
    Task<Outcome<List<ProjectDependencyReport>>> Build(
        IReadOnlyList<ParsedFile> parsedProjects,
        IReadOnlyList<ResolvedProjectReferences> resolvedReferences,
        IReadOnlyList<ProjectContext> projectContexts);
}
