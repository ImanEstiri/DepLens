
namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Specifies whether a dependency is direct or transitive.
/// </summary>
public enum DependencyScope
{
    /// <summary>Scope is not specified.</summary>
    None,

    /// <summary>The dependency is explicitly referenced in the project file.</summary>
    Direct,

    /// <summary>The dependency is pulled in indirectly through another dependency.</summary>
    Transitive
}
