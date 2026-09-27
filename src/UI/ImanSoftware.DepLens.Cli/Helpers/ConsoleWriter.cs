using Spectre.Console;

namespace ImanSoftware.DepLens.Cli.Helpers;

internal static class ConsoleWriter
{
    public static void Header(string targetPath, string outputDirectory)
    {
        AnsiConsole.Write(new Rule("[cyan bold]DepLens[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.MarkupLine($"[grey]Scanning :[/] [white]{targetPath.EscapeMarkup()}[/]");
        AnsiConsole.MarkupLine($"[grey]Output   :[/] [white]{outputDirectory.EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();
    }

    public static void Success(string message)
        => AnsiConsole.MarkupLine($"[green]✓[/] {message.EscapeMarkup()}");

    public static void Warning(string message)
        => AnsiConsole.MarkupLine($"[yellow]⚠[/] {message.EscapeMarkup()}");

    public static void Error(string message)
        => AnsiConsole.MarkupLine($"[red]✗[/] {message.EscapeMarkup()}");

    public static void Info(string message)
        => AnsiConsole.MarkupLine($"[grey]ℹ[/] {message.EscapeMarkup()}");
}
