using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Core.Implementation;

internal sealed class DepLensService : IDepLensService
{
    private readonly IDirectoryScanner _directoryScanner;
    private readonly IParserService _parserService;
    private readonly IProjectOrienterService _orienterService;
    private readonly IProjectReferenceLinkerService _linkerService;
    private readonly IDependencyGraphBuilderService _graphBuilderService;

    public DepLensService()
        : this(
            new DirectoryScannerService(),
            new ParserService(),
            new ProjectOrienterService(),
            new ProjectReferenceLinkerService(),
            new DependencyGraphBuilderService())
    { }

    internal DepLensService(
        IDirectoryScanner directoryScanner,
        IParserService parserService,
        IProjectOrienterService orienterService,
        IProjectReferenceLinkerService linkerService,
        IDependencyGraphBuilderService graphBuilderService)
    {
        _directoryScanner = directoryScanner;
        _parserService = parserService;
        _orienterService = orienterService;
        _linkerService = linkerService;
        _graphBuilderService = graphBuilderService;
    }

    public async Task<Outcome<List<ProjectDependencyReport>>> Analyze(string path)
    {
        // --- Step 1: Scan ---
        var scanOutcome = await _directoryScanner.InvestigateDirectoryAsync(path);
        if (!scanOutcome.IsSuccess || scanOutcome.Data is null)
            return Outcome.Failure<List<ProjectDependencyReport>>(scanOutcome.Error);

        var discoveredFiles = scanOutcome.Data;

        var solutionFiles = discoveredFiles
            .Where(f => f.FileType is FileType.SolutionClassic or FileType.SolutionXml)
            .ToList();

        var packagesPropsFiles = discoveredFiles
            .Where(f => f.FileType is FileType.DirectoryPackagesProps)
            .ToList();

        var projectFiles = discoveredFiles
            .Where(f => f.FileType is FileType.Project)
            .ToList();

        // --- Step 2: Parse Solutions + Directory.Packages.props ---
        var parsedSolutions = new List<ParsedFile>();
        foreach (var file in solutionFiles)
        {
            var parseOutcome = await _parserService.ParseAsync(file);
            if (parseOutcome.IsSuccess && parseOutcome.Data is not null)
                parsedSolutions.Add(parseOutcome.Data);
        }

        var parsedPackagesProps = new List<ParsedFile>();
        foreach (var file in packagesPropsFiles)
        {
            var parseOutcome = await _parserService.ParseAsync(file);
            if (parseOutcome.IsSuccess && parseOutcome.Data is not null)
                parsedPackagesProps.Add(parseOutcome.Data);
        }

        // --- Step 3: Orient ---
        var orientOutcome = await _orienterService.Orient(projectFiles, parsedSolutions, parsedPackagesProps);
        if (!orientOutcome.IsSuccess || orientOutcome.Data is null)
            return Outcome.Failure<List<ProjectDependencyReport>>(orientOutcome.Error);

        var projectContexts = orientOutcome.Data;
        var contextsByPath = projectContexts.ToDictionary(c => c.ProjectFullPath);

        // --- Step 4: Parse Projects ---
        var parsedProjects = new List<ParsedFile>();
        foreach (var file in projectFiles)
        {
            var context = contextsByPath.GetValueOrDefault(file.FullPath);
            var parseOutcome = await _parserService.ParseAsync(file, context);
            if (parseOutcome.IsSuccess && parseOutcome.Data is not null)
                parsedProjects.Add(parseOutcome.Data);
        }

        // --- Step 5: Link ProjectReferences (Internal/External) ---
        var linkOutcome = await _linkerService.Link(parsedProjects);
        if (!linkOutcome.IsSuccess || linkOutcome.Data is null)
            return Outcome.Failure<List<ProjectDependencyReport>>(linkOutcome.Error);

        var resolvedReferences = linkOutcome.Data;

        // --- Step 6: Build dependency graph ---
        var graphOutcome = await _graphBuilderService.Build(parsedProjects, resolvedReferences, projectContexts);
        if (!graphOutcome.IsSuccess || graphOutcome.Data is null)
            return Outcome.Failure<List<ProjectDependencyReport>>(graphOutcome.Error);

        return Outcome.Successful(graphOutcome.Data);
    }
}
