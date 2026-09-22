using ImanSoftware.DepLens.Abstractions.Services;

namespace ImanSoftware.DepLens.Core.Implementation;

internal sealed class PhysicalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public Task<string> ReadAllTextAsync(string path, CancellationToken ct)
        => File.ReadAllTextAsync(path, ct);
    public IEnumerable<string> EnumerateFiles(string rootPath, string pattern)
        => Directory.EnumerateFiles(rootPath, pattern, SearchOption.AllDirectories);
}
