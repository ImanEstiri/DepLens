
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record ParsedFile(
    DiscoveredFile Source,
    IParsedContent Content);
