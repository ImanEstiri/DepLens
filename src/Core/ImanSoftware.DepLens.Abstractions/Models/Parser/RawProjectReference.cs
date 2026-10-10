
namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Represents a raw project reference path as it appears in a project file.
/// </summary>
/// <param name="RelativeOrAbsolutePath">The raw path string.</param>
public sealed record RawProjectReference(string RelativeOrAbsolutePath);
