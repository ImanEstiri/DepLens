// Services/IParserService.cs
using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

/// <summary>
/// Defines a service for parsing discovered files into structured content.
/// </summary>
public interface IParserService : IService
{
    /// <summary>
    /// Parses a discovered file into its structured content representation.
    /// </summary>
    /// <param name="file">The file to parse.</param>
    /// <param name="context">Optional project context for CPM resolution.</param>
    /// <returns>An outcome containing the parsed file.</returns>
    Task<Outcome<ParsedFile>> ParseAsync(DiscoveredFile file, ProjectContext? context = null);
}
