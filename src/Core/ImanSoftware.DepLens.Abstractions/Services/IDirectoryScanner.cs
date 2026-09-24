
using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IDirectoryScanner : IService
{
    Task<Outcome<List<DiscoveredFile>>> InvestigateDirectoryAsync(string path);
}
