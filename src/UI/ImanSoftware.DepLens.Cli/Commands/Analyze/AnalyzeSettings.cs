using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ImanSoftware.DepLens.Cli.Commands.Analyze;

internal sealed class AnalyzeSettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    [Description("Path to a directory containing solutions. Defaults to the current directory.")]
    public string Path { get; init; } = string.Empty;

    // TODO
    //[CommandOption("-f|--format <FORMAT>")]
    //[Description("Output format (only 'html' supported in v0.1)")]
    //[DefaultValue("html")]
    //public string Format { get; init; } = "html";

    [CommandOption("-o|--output <PATH>")]
    [Description("Output directory for the report (defaults to the scanned path)")]
    public string? Output { get; init; }

    // TODO
    //[CommandOption("-v|--verbose")]
    //[Description("Show detailed progress output")]
    //public bool Verbose { get; init; }

    public override ValidationResult Validate()
    {
        var effectivePath = string.IsNullOrWhiteSpace(Path)
            ? Directory.GetCurrentDirectory()
            : Path;

        var fullPath = System.IO.Path.GetFullPath(effectivePath);

        if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
            return ValidationResult.Error($"Path not found: {fullPath}");

        //if (!string.Equals(Format, "html", StringComparison.OrdinalIgnoreCase))
        //    return ValidationResult.Error($"Unsupported format '{Format}'. Supported: html");

        return ValidationResult.Success();
    }

    public string GetEffectivePath()
        => string.IsNullOrWhiteSpace(Path)
            ? Directory.GetCurrentDirectory()
            : Path;
}
