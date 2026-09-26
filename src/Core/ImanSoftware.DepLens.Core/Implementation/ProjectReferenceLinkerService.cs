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

        var results = new List<ResolvedProjectReferences>();

        foreach (var parsedFile in parsedProjects)
        {
            if (parsedFile.Content is not ParsedProject parsedProject) continue;

            var projectDirectory = Path.GetDirectoryName(parsedFile.Source.FullPath)!;
            var links = new List<ProjectReferenceLink>();

            foreach (var raw in parsedProject.ProjectReferences)
            {
                var combined = Path.Combine(projectDirectory, raw.RelativeOrAbsolutePath);
                var normalized = NormalizePath(combined);

                links.Add(knownProjectPaths.Contains(normalized)
                    ? new InternalProjectReference(normalized)
                    : new ExternalProjectReference(raw.RelativeOrAbsolutePath));
            }

            results.Add(new ResolvedProjectReferences(parsedFile.Source.FullPath, links));
        }

        return Task.FromResult(Outcome.Successful(results));
    }

    private static string NormalizePath(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
