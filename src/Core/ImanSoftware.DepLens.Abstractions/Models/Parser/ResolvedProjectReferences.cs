namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record ResolvedProjectReferences(
    string ProjectFullPath,
    IReadOnlyList<ProjectReferenceLink> References);
