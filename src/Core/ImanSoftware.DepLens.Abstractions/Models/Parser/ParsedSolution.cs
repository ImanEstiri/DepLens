
namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Represents a parsed solution file (.sln or .slnx) containing project paths.
/// </summary>
/// <param name="ProjectPaths">The relative paths to projects included in the solution.</param>
public sealed record ParsedSolution(
    IReadOnlyList<string> ProjectPaths) : IParsedContent;
