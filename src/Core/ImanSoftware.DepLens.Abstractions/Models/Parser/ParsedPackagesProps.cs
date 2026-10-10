
namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Represents a parsed Directory.Packages.props file with central package version information.
/// </summary>
/// <param name="ManagePackageVersionsCentrally">Whether CPM is enabled.</param>
/// <param name="CentralPackageVersions">Mapping of package names to their central versions.</param>
public sealed record ParsedPackagesProps(
    bool ManagePackageVersionsCentrally,
    IReadOnlyDictionary<string, string> CentralPackageVersions) : IParsedContent;
