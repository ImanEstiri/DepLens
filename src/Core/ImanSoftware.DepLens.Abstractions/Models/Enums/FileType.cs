
using System.ComponentModel;

namespace ImanSoftware.DepLens.Abstractions.Models;

public enum FileType
{
    None,

    [Description("*.sln")]
    SolutionClassic,      // .sln

    [Description("*.slnx")]
    SolutionXml,           // .slnx

    [Description("*.csproj")]
    Project,                // .csproj

    [Description("Directory.Packages.props")]
    DirectoryPackagesProps, // Directory.Packages.props

    [Description("Directory.Build.props")]
    DirectoryBuildProps     // Directory.Build.props 
}
