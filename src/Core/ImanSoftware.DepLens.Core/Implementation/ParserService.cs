using System.Text.RegularExpressions;
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

    public Task<Outcome<ParsedFile>> ParseAsync(DiscoveredFile file, ProjectContext? context = null)
    {
        try
        {
            IParsedContent content = file.FileType switch
            {
                FileType.SolutionClassic => ParseSolutionClassic(file.RawContent),
                FileType.SolutionXml => ParseSolutionXml(file.RawContent),
                FileType.Project => ParseProject(file.RawContent, context),
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

    private static ParsedProject ParseProject(string raw, ProjectContext? context)
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

        var localOverride = ExtractManagePackageVersionsCentrallyOverride(root);
        var dpp = context?.CentralPackageVersions;
        var effectiveCpm = dpp is not null && dpp.ManagePackageVersionsCentrally && localOverride != false;

        var packageReferences = root
            .Descendants("PackageReference")
            .Select(e => ResolvePackageDependency(
                name: e.Attribute("Include")?.Value,
                explicitVersion: e.Attribute("Version")?.Value,
                versionOverride: e.Attribute("VersionOverride")?.Value,
                effectiveCpm: effectiveCpm,
                dpp: dpp))
            .Where(pd => pd is not null)
            .Select(pd => pd!)
            .ToList();

        return new ParsedProject(targetFrameworks, projectReferences, packageReferences, localOverride);
    }

    private static PackageDependency? ResolvePackageDependency(
        string? name,
        string? explicitVersion,
        string? versionOverride,
        bool effectiveCpm,
        ParsedPackagesProps? dpp)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        if (!effectiveCpm)
            return new PackageDependency(name, explicitVersion, PackageVersionSourceType.Explicit);

        if (!string.IsNullOrWhiteSpace(versionOverride))
            return new PackageDependency(name, versionOverride, PackageVersionSourceType.VersionOverride);

        if (dpp is not null && dpp.CentralPackageVersions.TryGetValue(name, out var centralVersion))
            return new PackageDependency(name, centralVersion, PackageVersionSourceType.CentralPackageManagement);

        // پیدا نشد: نه در csproj (چون CPM فعاله و انتظار Version نداریم) نه در DPP
        return new PackageDependency(name, null, PackageVersionSourceType.CentralPackageManagement);
    }

    private static bool? ExtractManagePackageVersionsCentrallyOverride(XElement root)
    {
        var raw = root.Descendants("ManagePackageVersionsCentrally").Select(e => e.Value).FirstOrDefault();
        return raw is not null && bool.TryParse(raw, out var parsed) ? parsed : null;
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

        var managesCentrally = root
            .Descendants("ManagePackageVersionsCentrally")
            .Select(e => e.Value)
            .Any(v => bool.TryParse(v, out var result) && result);

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

        return new ParsedPackagesProps(managesCentrally, versions);
    }
}
