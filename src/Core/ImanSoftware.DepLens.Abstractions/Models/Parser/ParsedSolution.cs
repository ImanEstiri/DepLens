
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record ParsedSolution(
    IReadOnlyList<string> ProjectPaths) : IParsedContent;
