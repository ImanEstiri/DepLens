
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record ParsedPackagesProps(
    bool ManagePackageVersionsCentrally,
    IReadOnlyDictionary<string, string> CentralPackageVersions) : IParsedContent;
