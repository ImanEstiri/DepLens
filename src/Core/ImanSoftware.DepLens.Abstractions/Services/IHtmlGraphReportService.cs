using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IHtmlGraphReportService : IService
{
    Task<Outcome<string>> GenerateAsync(
        IReadOnlyList<ProjectDependencyReport> reports,
        string outputDirectory,
        string fileName = "dependency-graph.html");
}
