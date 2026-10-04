using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Core.Implementation;

internal sealed class ProjectReferenceLinkerService : IProjectReferenceLinkerService
{
    public Task<Outcome<List<ResolvedProjectReferences>>> Link(IReadOnlyList<ParsedFile> parsedProjects)
    {
        var knownProjectPaths = parsedProjects
            .Select(p => NormalizePath(p.Source.FullPath))
            .ToHashSet();

        var projectsByDirectory = parsedProjects
            .Select(p => NormalizePath(p.Source.FullPath))
            .GroupBy(p => NormalizeDirectory(Path.GetDirectoryName(p)!))
            .ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<ResolvedProjectReferences>();

        foreach (var parsedFile in parsedProjects.Where(file => file.Content is ParsedProject))
        {
            var parsedProject = (ParsedProject)parsedFile.Content;

            var projectDirectory = Path.GetDirectoryName(parsedFile.Source.FullPath)!;
            var links = new List<ProjectReferenceLink>();

            foreach (var raw in parsedProject.ProjectReferences)
            {
                var combined = Path.Combine(projectDirectory, raw.RelativeOrAbsolutePath);
                var normalized = NormalizePath(combined);

                if (knownProjectPaths.Contains(normalized))
                {
                    links.Add(new InternalProjectReference(normalized));
                    continue;
                }

                var targetDirectory = NormalizeDirectory(Path.GetDirectoryName(normalized)!);
                if (projectsByDirectory.TryGetValue(targetDirectory, out var candidates) && candidates.Count == 1)
                {
                    links.Add(new InternalProjectReference(candidates[0]));
                    continue;
                }

                links.Add(new ExternalProjectReference(raw.RelativeOrAbsolutePath));
            }

            results.Add(new ResolvedProjectReferences(parsedFile.Source.FullPath, links));
        }

        return Task.FromResult(Outcome.Successful(results));
    }

    private static string NormalizePath(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static string NormalizeDirectory(string path) => NormalizePath(path);
}
