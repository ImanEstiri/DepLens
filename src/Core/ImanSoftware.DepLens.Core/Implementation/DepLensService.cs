using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.Outcomes;

namespace ImanSoftware.DepLens.Core.Implementation;

internal sealed class DepLensService : IDepLensService
{
    private readonly IDirectoryScanner _directoryScanner;
    private readonly IParserService _parserService;
    private readonly IProjectOrienterService _orienterService;

    public DepLensService()
        : this(new DirectoryScannerService(), new ParserService(), new ProjectOrienterService()) { }

    internal DepLensService(
        IDirectoryScanner directoryScanner,
        IParserService parserService,
        IProjectOrienterService orienterService)
    {
        _directoryScanner = directoryScanner;
        _parserService = parserService;
        _orienterService = orienterService;
    }

    public async Task<Outcome> Analyze(string path)
    {
        // scan (path most be pointing to a directory not a file)
        var scanOutcome = await _directoryScanner.InvestigateDirectoryAsync(path);

        if (!scanOutcome.IsSuccess || scanOutcome.Data is null)
            return Outcome.Failure(scanOutcome.Error);

        var discoveredFiles = scanOutcome.Data;
        
        // parse 
        var solutionFiles = discoveredFiles
        .Where(f => f.FileType is FileType.SolutionClassic or FileType.SolutionXml)
        .ToList();

        var parsedSolutions = new List<ParsedFile>();
        foreach (var file in solutionFiles)
        {
            var parseOutcome = await _parserService.ParseAsync(file);
            if (parseOutcome.IsSuccess && parseOutcome.Data is not null)
                parsedSolutions.Add(parseOutcome.Data);
        }


        var packagesPropsFiles = discoveredFiles
            .Where(f => f.FileType is FileType.DirectoryPackagesProps)
            .ToList();

        var parsedPackagesProps = new List<ParsedFile>();
        foreach (var file in packagesPropsFiles)
        {
            var parseOutcome = await _parserService.ParseAsync(file);
            if (parseOutcome.IsSuccess && parseOutcome.Data is not null)
                parsedPackagesProps.Add(parseOutcome.Data);
        }


        // orient
        var projectFiles = discoveredFiles
            .Where(f => f.FileType is FileType.Project)
            .ToList();

        var orientOutcome = await _orienterService.Orient(projectFiles, parsedSolutions, parsedPackagesProps);
        if (!orientOutcome.IsSuccess || orientOutcome.Data is null)
            return Outcome.Failure(orientOutcome.Error);


        var contextsByPath = orientOutcome.Data.ToDictionary(c => c.ProjectFullPath);

        var parsedProjects = new List<ParsedFile>();
        foreach (var file in projectFiles)
        {
            var context = contextsByPath.GetValueOrDefault(file.FullPath);
            var parseOutcome = await _parserService.ParseAsync(file, context);
            if (parseOutcome.IsSuccess && parseOutcome.Data is not null)
                parsedProjects.Add(parseOutcome.Data);
        }

        // resolve parsed data to find references



        throw new NotImplementedException();
    }
}
