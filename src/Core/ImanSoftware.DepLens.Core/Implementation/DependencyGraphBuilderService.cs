using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Core.Implementation;

internal sealed class DependencyGraphBuilderService : IDependencyGraphBuilderService
{
    public Task<Outcome<List<ProjectDependencyReport>>> Build(
        IReadOnlyList<ParsedFile> parsedProjects,
        IReadOnlyList<ResolvedProjectReferences> resolvedReferences,
        IReadOnlyList<ProjectContext> projectContexts)
    {
        var projectsByPath = parsedProjects
            .Where(p => p.Content is ParsedProject)
            .ToDictionary(p => p.Source.FullPath, p => (ParsedProject)p.Content);

        var linksByPath = resolvedReferences
            .ToDictionary(r => r.ProjectFullPath, r => r.References);

        var contextsByPath = projectContexts
            .ToDictionary(c => c.ProjectFullPath);

        var reports = new List<ProjectDependencyReport>();

        foreach (var (projectPath, parsedProject) in projectsByPath)
        {
            var dependencies = new List<Dependency>();

            // --- Direct package dependencies ---
            foreach (var pkg in parsedProject.PackageReferences)
            {
                dependencies.Add(new Dependency(
                    pkg.Name, pkg.ResolvedVersion, DependencyType.Package,
                    DependencyScope.Direct, pkg.Source, IsResolved: true));
            }

            // --- Direct project dependencies (Internal + External) ---
            var directLinks = linksByPath.GetValueOrDefault(projectPath, []);
            foreach (var link in directLinks)
            {
                dependencies.Add(link switch
                {
                    InternalProjectReference internalRef => new Dependency(
                        Path.GetFileNameWithoutExtension(internalRef.FullPath), null,
                        DependencyType.Project, DependencyScope.Direct, null,
                        IsResolved: true, TargetFullPath: internalRef.FullPath),
                    ExternalProjectReference externalRef => new Dependency(
                        externalRef.RawPath, null,
                        DependencyType.Project, DependencyScope.Direct, null, IsResolved: false),
                    _ => throw new NotSupportedException()
                });
            }

            // --- Transitive dependencies via internal ProjectReference graph ---
            var transitive = CollectTransitiveDependencies(
                projectPath, projectsByPath, linksByPath,
                directPackageNames: parsedProject.PackageReferences.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase),
                directProjectPaths: directLinks.OfType<InternalProjectReference>().Select(l => l.FullPath).ToHashSet());

            dependencies.AddRange(transitive);

            var solutionPaths = contextsByPath.GetValueOrDefault(projectPath)?.SolutionPaths ?? [];
            reports.Add(new ProjectDependencyReport(projectPath, solutionPaths, dependencies));
        }

        return Task.FromResult(Outcome.Successful(reports));
    }

    private static List<Dependency> CollectTransitiveDependencies(
        string rootProjectPath,
        Dictionary<string, ParsedProject> projectsByPath,
        Dictionary<string, IReadOnlyList<ProjectReferenceLink>> linksByPath,
        HashSet<string> directPackageNames,
        HashSet<string> directProjectPaths)
    {
        var result = new List<Dependency>();
        var visitedProjects = new HashSet<string> { rootProjectPath };
        var seenPackageNames = new HashSet<string>(directPackageNames, StringComparer.OrdinalIgnoreCase);

        var queue = new Queue<string>(directProjectPaths);
        foreach (var p in directProjectPaths) visitedProjects.Add(p);

        while (queue.Count > 0)
        {
            var currentPath = queue.Dequeue();

            if (!projectsByPath.TryGetValue(currentPath, out var currentProject))
                continue; 

            foreach (var pkg in currentProject.PackageReferences)
            {
                if (!seenPackageNames.Add(pkg.Name)) continue; 

                result.Add(new Dependency(
                    pkg.Name, pkg.ResolvedVersion, DependencyType.Package,
                    DependencyScope.Transitive, pkg.Source, IsResolved: true));
            }

            var nextLinks = linksByPath.GetValueOrDefault(currentPath, []);
            foreach (var link in nextLinks)
            {
                if (link is not InternalProjectReference internalRef) continue;
                if (!visitedProjects.Add(internalRef.FullPath)) continue;

                result.Add(new Dependency(
                    Path.GetFileNameWithoutExtension(internalRef.FullPath), null,
                    DependencyType.Project, DependencyScope.Transitive, null,
                    IsResolved: true, TargetFullPath: internalRef.FullPath));

                queue.Enqueue(internalRef.FullPath);
            }
        }

        return result;
    }
}
