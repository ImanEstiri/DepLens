using ImanSoftware.DepLens.Cli.Commands.Analyze;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("deplens");
    config.SetApplicationVersion("1.0.0");

    config.AddCommand<AnalyzeCommand>("analyze")
    .WithDescription("Analyze a .NET solution and generate a dependency graph.")
    .WithExample("analyze")                                     
    .WithExample("analyze", @"G:\Projects\MyApp")
    .WithExample("analyze", @"G:\Projects\MyApp", "-o", @".\out");
});

try
{
    return await app.RunAsync(args);
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}
