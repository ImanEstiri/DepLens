using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Abstractions.Services;
using ImanSoftware.Extensions;
using ImanSoftware.FileStorage;
using ImanSoftware.Outcomes;
using System.Collections.Immutable;

namespace ImanSoftware.DepLens.Core.Implementation;

internal sealed class DirectoryScannerService : IDirectoryScanner
{
    private readonly IFileStorage _fileStorage;
    private readonly ImmutableArray<string> patterns;

    public DirectoryScannerService()
    {
        _fileStorage = ImanFileStorageFactory.CreateStorage();

        patterns = Enum.GetValues<FileType>()
            .Where(x => x != FileType.None)
            .Select(x => x.GetDescription())
            .ToImmutableArray();
    }

    public async Task<Outcome<List<DiscoveredFile>>> InvestigateDirectoryAsync(string path)
    {
        var searchOutCome = await _fileStorage.SearchFilesAsync(path, patterns, SearchOption.AllDirectories);
        if (!searchOutCome.IsSuccess || searchOutCome.Data is null)
            return Outcome.Failure<List<DiscoveredFile>>(searchOutCome.Error);

        var discovered = new List<DiscoveredFile>();
        foreach (var item in searchOutCome.Data)
        {
            var fileType = DetermineFileType(item);
            if (fileType is FileType.None) continue;

            var readDataOutCome = await ReadFileContent(item.FullPath);

            if (readDataOutCome.IsSuccess && readDataOutCome.Data is not null)
                discovered.Add(
                    new DiscoveredFile(
                        item.FullPath,
                        fileType,
                        readDataOutCome.Data
                        )
                    );
        }

        return Outcome.Successful(discovered);
    }

    private async Task<Outcome<string>> ReadFileContent(string path)
    {
        if (!_fileStorage.FileExists(path))
            return Outcome.Failure<string>(new OutcomeError(
                $"File not found: {path}",
                "FILE_NOT_FOUND",
                OutcomeErrorType.NotFound));

        return await _fileStorage.ReadAllTextAsync(path);
    }

    private static FileType DetermineFileType(FileMetadata metadata)
    {
        if (metadata.FileName.Equals("Directory.Packages.props", StringComparison.OrdinalIgnoreCase))
            return FileType.DirectoryPackagesProps;

        if (metadata.FileName.Equals("Directory.Build.props", StringComparison.OrdinalIgnoreCase))
            return FileType.DirectoryBuildProps;

        return metadata.Extension switch
        {
            ".sln" => FileType.SolutionClassic,
            ".slnx" => FileType.SolutionXml,
            ".csproj" => FileType.Project,
            _ => FileType.None
        };
    }

}
