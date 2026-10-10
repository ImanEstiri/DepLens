
namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Represents a file that has been parsed into a structured content model.
/// </summary>
/// <param name="Source">The originally discovered file.</param>
/// <param name="Content">The parsed content (solution, project, or packages props).</param>
public sealed record ParsedFile(
    DiscoveredFile Source,
    IParsedContent Content);
