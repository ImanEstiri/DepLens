### nuget 

```xml 
<?xml version="1.0" encoding="utf-8"?>

<configuration>
	<packageSources>
		<clear />
		 <add key="nuget" value="https://api.nuget.org/v3/index.json" protocolVersion="3" /> 
	</packageSources>
	<packageSourceMapping>
		<packageSource key="nuget">
			<package pattern="*" />
		</packageSource>
	</packageSourceMapping>
</configuration>
``` 


### src > Core > ImanSoftware.DepLens.Abstractions > ImanSoftware.DepLens.Abstractions 

```xml 
﻿<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ImanSoftware.OutCome" />
    <PackageReference Include="ImanSoftware.FileStorage" />
    <PackageReference Include="ImanSoftware.Extensions" />
  </ItemGroup>

</Project>

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > DirectoryScanner > DiscoveredFiles 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record DiscoveredFile(
    string FullPath,
    FileType FileType,
    string RawContent);

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > Enums > DependencyScope 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Models;

public enum DependencyScope
{
    None,
    Direct,
    Transitive
}

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > Enums > DependencyType 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Models;

public enum DependencyType
{
    None,
    Project,
    Package,
    DLL
}

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > Enums > FileType 

```csharp 
﻿
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

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > Enums > PackageVersionSourceType 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Models;

public enum PackageVersionSourceType { Explicit, CentralPackageManagement, VersionOverride }

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > Parser > IParsedContent 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Models;

public interface IParsedContent { }

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > Parser > ParsedFile 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record ParsedFile(
    DiscoveredFile Source,
    IParsedContent Content);

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > Parser > ParsedPackagesProps 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record ParsedPackagesProps(
    IReadOnlyDictionary<string, string> CentralPackageVersions) : IParsedContent;

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > Parser > ParsedProject 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record ParsedProject(
    IReadOnlyList<string> TargetFrameworks,
    IReadOnlyList<RawProjectReference> ProjectReferences,
    IReadOnlyList<RawPackageReference> PackageReferences,
    bool ManagePackageVersionsCentrally) : IParsedContent;



``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > Parser > ParsedSolution 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record ParsedSolution(
    IReadOnlyList<string> ProjectPaths) : IParsedContent;

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > Parser > RawPackageReference 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record RawPackageReference(string Name, string? Version);

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Models > Parser > RawProjectReference 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Models;

public sealed record RawProjectReference(string RelativeOrAbsolutePath);

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Services > IDepLensService 

```csharp 
﻿
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IDepLensService : IService
{
    Task<Outcome> Analyze(string path);
}

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Services > IDirectoryScanner 

```csharp 
﻿
using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IDirectoryScanner : IService
{
    Task<Outcome<List<DiscoveredFile>>> InvestigateDirectoryAsync(string path);
}

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Services > IParserService 

```csharp 
﻿
using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IParserService : IService
{
    Task<Outcome<ParsedFile>> ParseAsync(DiscoveredFile file);
}

``` 


### src > Core > ImanSoftware.DepLens.Abstractions > Services > IService 

```csharp 
﻿
namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IService
{
}

``` 


### src > Core > ImanSoftware.DepLens.Core > ImanSoftware.DepLens.Core 

```xml 
﻿<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\ImanSoftware.DepLens.Abstractions\ImanSoftware.DepLens.Abstractions.csproj" />
  </ItemGroup>

</Project>

``` 


### src > Core > ImanSoftware.DepLens.Core > Factory > DepLensServiceFactory 

```csharp 
﻿using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.DepLens.Core.Implementation;

namespace ImanSoftware.DepLens.Core.Factory;

public static class DepLensServiceFactory
{
    public static IDepLensService Create() => new DepLensService();
}

``` 


### src > Core > ImanSoftware.DepLens.Core > Implementation > DepLensService 

```csharp 
﻿using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Core.Implementation;

internal sealed class DepLensService : IDepLensService
{
    private readonly IDirectoryScanner _directoryScanner;
    private readonly IParserService _parserService;
    public DepLensService()
        : this(new DirectoryScannerService(), new ParserService()) { }

    internal DepLensService(IDirectoryScanner directoryScanner, IParserService parserService)
    {
        _directoryScanner = directoryScanner;
        _parserService = parserService;
    }

    public async Task<Outcome> Analyze(string path)
    {
        // scan (path most be pointing to a directory not a file)
        var scanOutcome = await _directoryScanner.InvestigateDirectoryAsync(path);

        if (!scanOutcome.IsSuccess || scanOutcome.Data is null)
            return Outcome.Failure(scanOutcome.Error);

        // parse 
        var parsed = new List<ParsedFile>();
        foreach (var entry in scanOutcome.Data)
        {
            var parseOutCome = await _parserService.ParseAsync(entry);
            if (parseOutCome.IsSuccess && parseOutCome.Data is not null)
                parsed.Add(parseOutCome.Data);
        }


        // resolve parsed data to find references



        throw new NotImplementedException();
    }
}

``` 


### src > Core > ImanSoftware.DepLens.Core > Implementation > DirectoryScannerService 

```csharp 
﻿using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.Extensions;
using ImanSoftware.FileStorage;
using ImanSoftware.Outcomes;
using System.Collections.Immutable;

namespace ImanSoftware.DepLens.Core.Implementation;

internal sealed class DirectoryScannerService : IDirectoryScanner
{
    private readonly IFileStorage _fileStorage;
    private readonly ImmutableArray<string> patterns;

    public DirectoryScannerService()
    {
        _fileStorage = ImanFileStorageFactory.CreateStorage();

        patterns = Enum.GetValues<FileType>()
            .Where(x => x != FileType.None)
            .Select(x => x.GetDescription())
            .ToImmutableArray();
    }

    public async Task<Outcome<List<DiscoveredFile>>> InvestigateDirectoryAsync(string path)
    {
        var searchOutCome = await _fileStorage.SearchFilesAsync(path, patterns, SearchOption.AllDirectories);
        if (!searchOutCome.IsSuccess || searchOutCome.Data is null)
            return Outcome.Failure<List<DiscoveredFile>>(searchOutCome.Error);

        var discovered = new List<DiscoveredFile>();
        foreach (var item in searchOutCome.Data)
        {
            var fileType = DetermineFileType(item);
            if (fileType is FileType.None) continue;

            var readDataOutCome = await ReadFileContent(item.FullPath);

            if (readDataOutCome.IsSuccess && readDataOutCome.Data is not null)
                discovered.Add(
                    new DiscoveredFile(
                        item.FullPath,
                        fileType,
                        readDataOutCome.Data
                        )
                    );
        }

        return Outcome.Successful(discovered);
    }

    private async Task<Outcome<string>> ReadFileContent(string path)
    {
        if (!_fileStorage.FileExists(path))
            return Outcome.Failure<string>(new OutcomeError(
                $"File not found: {path}",
                "FILE_NOT_FOUND",
                OutcomeErrorType.NotFound));

        return await _fileStorage.ReadAllTextAsync(path);
    }

    private static FileType DetermineFileType(FileMetadata metadata)
    {
        if (metadata.FileName.Equals("Directory.Packages.props", StringComparison.OrdinalIgnoreCase))
            return FileType.DirectoryPackagesProps;

        if (metadata.FileName.Equals("Directory.Build.props", StringComparison.OrdinalIgnoreCase))
            return FileType.DirectoryBuildProps;

        return metadata.Extension switch
        {
            ".sln" => FileType.SolutionClassic,
            ".slnx" => FileType.SolutionXml,
            ".csproj" => FileType.Project,
            _ => FileType.None
        };
    }

}

``` 


### src > Core > ImanSoftware.DepLens.Core > Implementation > ParserService 

```csharp 
﻿using System.Text.RegularExpressions;
using System.Xml.Linq;
using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Core.Implementation;

internal sealed class ParserService : IParserService
{
    private static readonly Regex ProjectLineRegex = new(
        @"^Project\(""\{[^}]+\}""\)\s*=\s*""[^""]+"",\s*""(?<path>[^""]+)"",\s*""\{[^}]+\}""",
        RegexOptions.Compiled);

    public Task<Outcome<ParsedFile>> ParseAsync(DiscoveredFile file)
    {
        try
        {
            IParsedContent content = file.FileType switch
            {
                FileType.SolutionClassic => ParseSolutionClassic(file.RawContent),
                FileType.SolutionXml => ParseSolutionXml(file.RawContent),
                FileType.Project => ParseProject(file.RawContent),
                FileType.DirectoryPackagesProps => ParsePackagesProps(file.RawContent),
                _ => throw new NotSupportedException($"Unsupported file type: {file.FileType}")
            };

            return Task.FromResult(Outcome.Successful(new ParsedFile(file, content)));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Outcome.Failure<ParsedFile>(new OutcomeError(
                $"Failed to parse '{file.FullPath}': {ex.Message}",
                "PARSE_FAILED",
                OutcomeErrorType.Failure))); 
        }
    }

    private static ParsedSolution ParseSolutionClassic(string raw)
    {
        var projectPaths = new List<string>();

        using var reader = new StringReader(raw);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            var match = ProjectLineRegex.Match(line);
            if (!match.Success) continue;

            var relativePath = match.Groups["path"].Value;
            if (relativePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                projectPaths.Add(relativePath);
        }

        return new ParsedSolution(projectPaths);
    }

    private static ParsedSolution ParseSolutionXml(string raw)
    {
        var doc = XDocument.Parse(raw);

        var projectPaths = doc
            .Descendants("Project")
            .Select(e => e.Attribute("Path")?.Value)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!)
            .Where(path => path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new ParsedSolution(projectPaths);
    }

    private static ParsedProject ParseProject(string raw)
    {
        var doc = XDocument.Parse(raw);
        var root = doc.Root ?? throw new InvalidOperationException("Missing root <Project> element.");

        var targetFrameworks = ExtractTargetFrameworks(root);

        var projectReferences = root
            .Descendants("ProjectReference")
            .Select(e => e.Attribute("Include")?.Value)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => new RawProjectReference(path!))
            .ToList();

        var packageReferences = root
            .Descendants("PackageReference")
            .Select(e => new RawPackageReference(
                e.Attribute("Include")?.Value ?? string.Empty,
                e.Attribute("Version")?.Value))
            .Where(pr => !string.IsNullOrWhiteSpace(pr.Name))
            .ToList();

        var managesCentrally = root
            .Descendants("ManagePackageVersionsCentrally")
            .Select(e => e.Value)
            .Any(v => bool.TryParse(v, out var result) && result);

        return new ParsedProject(targetFrameworks, projectReferences, packageReferences, managesCentrally);
    }

    private static List<string> ExtractTargetFrameworks(XElement root)
    {
        var multi = root.Descendants("TargetFrameworks").FirstOrDefault()?.Value;
        if (!string.IsNullOrWhiteSpace(multi))
            return multi.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var single = root.Descendants("TargetFramework").FirstOrDefault()?.Value;
        return string.IsNullOrWhiteSpace(single) ? [] : [single];
    }

    private static ParsedPackagesProps ParsePackagesProps(string raw)
    {
        var doc = XDocument.Parse(raw);
        var root = doc.Root ?? throw new InvalidOperationException("Missing root <Project> element.");

        var versions = root
            .Descendants("PackageVersion")
            .Select(e => new
            {
                Name = e.Attribute("Include")?.Value,
                Version = e.Attribute("Version")?.Value
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.Version))
            .GroupBy(x => x.Name!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Version!, StringComparer.OrdinalIgnoreCase);

        return new ParsedPackagesProps(versions);
    }
}

``` 


