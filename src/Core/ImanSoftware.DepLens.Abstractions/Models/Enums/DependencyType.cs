
namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Specifies the type of a dependency between projects or packages.
/// </summary>
public enum DependencyType
{
    /// <summary>No dependency type specified.</summary>
    None,

    /// <summary>A dependency on another project within the same solution.</summary>
    Project,

    /// <summary>A dependency on a NuGet package.</summary>
    Package
}
