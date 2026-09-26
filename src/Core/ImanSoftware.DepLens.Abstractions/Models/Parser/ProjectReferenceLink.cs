namespace ImanSoftware.DepLens.Abstractions.Models;

public abstract record ProjectReferenceLink;

public sealed record InternalProjectReference(string FullPath) : ProjectReferenceLink;

public sealed record ExternalProjectReference(string RawPath) : ProjectReferenceLink;
