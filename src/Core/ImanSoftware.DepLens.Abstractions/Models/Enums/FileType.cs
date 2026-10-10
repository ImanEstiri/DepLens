
using System.ComponentModel;

namespace ImanSoftware.DepLens.Abstractions.Models;

/// <summary>
/// Specifies the type of a discovered file in a .NET solution.
/// </summary>
public enum FileType
{
    /// <summary>Unknown or unsupported file type.</summary>
    None,

    /// <summary>A classic Visual Studio solution file (.sln).</summary>
    [Description("*.sln")]
    SolutionClassic,

    /// <summary>An XML-based solution file (.slnx).</summary>
    [Description("*.slnx")]
    SolutionXml,

    /// <summary>A C# project file (.csproj).</summary>
    [Description("*.csproj")]
    Project,

    /// <summary>Central Package Management file (Directory.Packages.props).</summary>
    [Description("Directory.Packages.props")]
    DirectoryPackagesProps,

    /// <summary>Directory.Build.props file for shared build properties.</summary>
    [Description("Directory.Build.props")]
    DirectoryBuildProps
}
