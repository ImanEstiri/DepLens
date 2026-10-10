namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Base type for a project reference link, which can be internal or external.
/// </summary>
public abstract record ProjectReferenceLink;

/// <summary>
/// A project reference that resolves to a project within the scanned directory.
/// </summary>
/// <param name="FullPath">The full path to the referenced project.</param>
public sealed record InternalProjectReference(string FullPath) : ProjectReferenceLink;

/// <summary>
/// A project reference that points outside the scanned directory.
/// </summary>
/// <param name="RawPath">The raw relative or absolute path from the project file.</param>
public sealed record ExternalProjectReference(string RawPath) : ProjectReferenceLink;
