
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IParserService : IService
{
    Task<Outcome> ParseSolution();
    Task<Outcome> ParseProject();
}
