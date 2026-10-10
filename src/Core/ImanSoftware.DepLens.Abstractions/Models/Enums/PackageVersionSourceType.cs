
namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Specifies where the version of a NuGet package reference comes from.
/// </summary>
public enum PackageVersionSourceType
{
    /// <summary>The version is explicitly specified on the PackageReference.</summary>
    Explicit,

    /// <summary>The version comes from Central Package Management.</summary>
    CentralPackageManagement,

    /// <summary>The version is overridden using VersionOverride.</summary>
    VersionOverride
}
