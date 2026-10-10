namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Contains the resolved references for a single project.
/// </summary>
/// <param name="ProjectFullPath">The full path to the project file.</param>
/// <param name="References">The resolved project references.</param>
public sealed record ResolvedProjectReferences(
    string ProjectFullPath,
    IReadOnlyList<ProjectReferenceLink> References);
