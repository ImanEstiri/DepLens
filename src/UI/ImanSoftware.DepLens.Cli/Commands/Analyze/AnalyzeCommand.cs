using ImanSoftware.DepLens.Cli.Helpers;
using ImanSoftware.DepLens.Core.Factory;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ImanSoftware.DepLens.Cli.Commands.Analyze;

internal sealed class AnalyzeCommand : AsyncCommand<AnalyzeSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, AnalyzeSettings settings,CancellationToken _)
    {
        var effectivePath = settings.GetEffectivePath();
        var fullPath = System.IO.Path.GetFullPath(effectivePath);
        var outputDirectory = ResolveOutputDirectory(settings.Output, fullPath);

        ConsoleWriter.Header(fullPath, outputDirectory);

        // analyze 
        var analyzer = DepLensServiceFactory.Create();

        var analyzeOutcome = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Analyzing solution(s)...",
                _ => analyzer.Analyze(fullPath));

        if (analyzeOutcome.IsFailure)
        {
            ConsoleWriter.Error(analyzeOutcome.Error.Message);
            return 1;
        }

        var reports = analyzeOutcome.Data;

        if (reports is null || reports.Count == 0)
        {
            ConsoleWriter.Warning("No projects found.");
            return 1;
        }

        ConsoleWriter.Success($"Analyzed {reports.Count} project(s)");

        // generate report 
        var reportGenerator = DepLensServiceFactory.CreateHtmlReportGenerator();

        var reportOutcome = await reportGenerator.GenerateAsync(reports, outputDirectory);

        if (reportOutcome.IsFailure)
        {
            ConsoleWriter.Error(reportOutcome.Error.Message);
            return 1;
        }

        ConsoleWriter.Success($"Report: {reportOutcome.Data} \n\n");

        AnsiConsole.MarkupLine($"[grey]Open:[/] [link]{reportOutcome.Data}[/]");
        return 0;
    }

    private static string ResolveOutputDirectory(string? outputOption, string scannedPath)
    {
        if (!string.IsNullOrWhiteSpace(outputOption))
        {
            var outputFull = System.IO.Path.GetFullPath(outputOption);

            // اگر مسیر به یک فایل اشاره می‌کند (پسوند دارد و پوشه نیست)، پوشه‌اش را برگردان
            if (System.IO.Path.HasExtension(outputFull) && !Directory.Exists(outputFull))
                return System.IO.Path.GetDirectoryName(outputFull)!;

            return outputFull;
        }

        // پیش‌فرض: اگر مسیر اسکن یک فایل است، پوشه‌اش؛ اگر پوشه است، خودش
        return Directory.Exists(scannedPath)
            ? scannedPath
            : System.IO.Path.GetDirectoryName(scannedPath)!;
    }
}
