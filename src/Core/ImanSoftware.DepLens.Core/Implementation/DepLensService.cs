using ImanSoftware.DepLens.Abstractions.Models;
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
