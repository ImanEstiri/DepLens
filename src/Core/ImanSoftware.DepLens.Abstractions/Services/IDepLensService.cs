
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IDepLensService : IService
{
    Task<Outcome> Analyze(string path);
}
