// Services/IParserService.cs
using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IParserService : IService
{
    Task<Outcome<ParsedFile>> ParseAsync(DiscoveredFile file, ProjectContext? context = null);
}
