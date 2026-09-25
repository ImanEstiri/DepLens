using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Core.Implementation;

internal sealed class ProjectOrienterService : IProjectOrienterService
{
    public Task<Outcome<List<ProjectContext>>> Orient(
        IReadOnlyList<DiscoveredFile> projectFiles,
        IReadOnlyList<ParsedFile> parsedSolutions,
        IReadOnlyList<ParsedFile> parsedPackagesProps)
    {
        var solutionProjectMap = BuildSolutionProjectMap(parsedSolutions);
        var packagesPropsByDirectory = parsedPackagesProps
            .Where(f => f.Content is ParsedPackagesProps)
            .ToDictionary(
                f => NormalizeDirectory(Path.GetDirectoryName(f.Source.FullPath)!),
                f => (ParsedPackagesProps)f.Content);

        var contexts = new List<ProjectContext>();

        foreach (var project in projectFiles)
        {
            var normalizedProjectPath = NormalizePath(project.FullPath);

            var owningSolutions = solutionProjectMap
                .Where(kvp => kvp.Value.Contains(normalizedProjectPath))
                .Select(kvp => kvp.Key)
                .ToList();

            var nearestDpp = FindNearestPackagesProps(project.FullPath, packagesPropsByDirectory);

            contexts.Add(new ProjectContext(project.FullPath, owningSolutions, nearestDpp));
        }

        return Task.FromResult(Outcome.Successful(contexts));
    }

    private static Dictionary<string, HashSet<string>> BuildSolutionProjectMap(
        IReadOnlyList<ParsedFile> parsedSolutions)
    {
        var map = new Dictionary<string, HashSet<string>>();

        foreach (var solutionFile in parsedSolutions)
        {
            if (solutionFile.Content is not ParsedSolution parsedSolution) continue;

            var solutionDirectory = Path.GetDirectoryName(solutionFile.Source.FullPath)!;
            var resolvedPaths = new HashSet<string>();

            foreach (var relativePath in parsedSolution.ProjectPaths)
            {
                var combined = Path.Combine(solutionDirectory, relativePath);
                resolvedPaths.Add(NormalizePath(combined));
            }

            map[solutionFile.Source.FullPath] = resolvedPaths;
        }

        return map;
    }

    private static ParsedPackagesProps? FindNearestPackagesProps(
        string projectFullPath,
        Dictionary<string, ParsedPackagesProps> packagesPropsByDirectory)
    {
        var currentDirectory = Path.GetDirectoryName(projectFullPath);

        while (!string.IsNullOrEmpty(currentDirectory))
        {
            if (packagesPropsByDirectory.TryGetValue(NormalizeDirectory(currentDirectory), out var props))
                return props;

            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
        }

        return null;
    }

    private static string NormalizePath(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static string NormalizeDirectory(string path) => NormalizePath(path);
}
