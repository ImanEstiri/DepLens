using ImanSoftware.DepLens.Core.Factory;

namespace ImanSoftware.DepLens.Playground;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Enter target directory path: ");
        var path = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("Path cannot be empty.");
            return;
        }

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Directory not found: {path}");
            return;
        }

        var analyzer = DepLensServiceFactory.Create();
        var analyzeOutcome = await analyzer.Analyze(path);

        if (analyzeOutcome.IsSuccess && analyzeOutcome.Data is not null)
        {
            var reportGenerator = DepLensServiceFactory.CreateHtmlReportGenerator();
            var reportOutcome = await reportGenerator.GenerateAsync(analyzeOutcome.Data, path);

            if (reportOutcome.IsSuccess)
                Console.WriteLine($"Done: {reportOutcome.Data}");
        }
    }
}
