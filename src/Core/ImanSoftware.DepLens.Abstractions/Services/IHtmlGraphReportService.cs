using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

/// <summary>
/// Defines a service for generating HTML dependency graph reports.
/// </summary>
public interface IHtmlGraphReportService : IService
{
    /// <summary>
    /// Generates an HTML report for the given dependency reports.
    /// </summary>
    /// <param name="reports">The dependency reports to visualize.</param>
    /// <param name="outputDirectory">The directory where the HTML file will be written.</param>
    /// <param name="fileName">The output file name.</param>
    /// <returns>An outcome containing the full path to the generated HTML file.</returns>
    Task<Outcome<string>> GenerateAsync(
        IReadOnlyList<ProjectDependencyReport> reports,
        string outputDirectory,
        string fileName = "dependency-graph.html");
}
