
using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IDepLensService : IService
{
    Task<Outcome<List<ProjectDependencyReport>>> Analyze(string path);
}
