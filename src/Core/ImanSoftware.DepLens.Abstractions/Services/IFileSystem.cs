
namespace ImanSoftware.DepLens.Abstractions.Services;

public interface IFileSystem : IService
{ 
    bool FileExists(string path);
    bool DirectoryExists(string path);
    Task<string> ReadAllTextAsync(string path, CancellationToken ct = default);
    IEnumerable<string> EnumerateFiles(string rootPath, string searchPattern);
}
