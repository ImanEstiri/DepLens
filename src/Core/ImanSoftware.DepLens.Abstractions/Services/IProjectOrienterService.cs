using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IProjectOrienterService : IService
{
    Task<Outcome<List<ProjectContext>>> Orient(
        IReadOnlyList<DiscoveredFile> projectFiles,
        IReadOnlyList<ParsedFile> parsedSolutions,
        IReadOnlyList<ParsedFile> parsedPackagesProps);
}
