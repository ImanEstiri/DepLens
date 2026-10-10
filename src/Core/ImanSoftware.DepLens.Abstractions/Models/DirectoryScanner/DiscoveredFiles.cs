
namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Represents a file discovered during directory scanning.
/// </summary>
/// <param name="FullPath">The absolute path to the file.</param>
/// <param name="FileType">The classified type of the file.</param>
/// <param name="RawContent">The raw text content of the file.</param>
public sealed record DiscoveredFile(
    string FullPath,
    FileType FileType,
    string RawContent);
