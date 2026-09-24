
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record DiscoveredFile(
    string FullPath,
    FileType FileType,
    string RawContent);
