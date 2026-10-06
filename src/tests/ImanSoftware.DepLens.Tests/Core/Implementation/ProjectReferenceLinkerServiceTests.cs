using FluentAssertions;
using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Core.Implementation;

namespace ImanSoftware.DepLens.Tests.Core.Implementation;

public class ProjectReferenceLinkerServiceTests
{
    [Fact]
    public async Task Link_ShouldResolveInternalReference_WhenCaseDiffers()
    {
        // Arrange
        var projectPath = @"C:\Repo\MyLib\MyLib.csproj";
        var referencePath = @"..\MyLib\mylib.csproj"; // حروف کوچک

        var parsedProject = new ParsedProject(
            TargetFrameworks: [],
            ProjectReferences: [new RawProjectReference(referencePath)],
            PackageReferences: [],
            ManagePackageVersionsCentrallyOverride: null,
            EffectiveManagePackageVersionsCentrally: false);

        var parsedFile = new ParsedFile(
            Source: new DiscoveredFile(projectPath, FileType.Project, ""),
            Content: parsedProject);

        var targetProjectPath = @"C:\Repo\MyLib\MyLib.csproj";
        var targetParsedFile = new ParsedFile(
            Source: new DiscoveredFile(targetProjectPath, FileType.Project, ""),
            Content: parsedProject);

        var service = new ProjectReferenceLinkerService();

        // Act
        var outcome = await service.Link([parsedFile, targetParsedFile]);

        // Assert
        outcome.IsSuccess.Should().BeTrue();
        var resolved = outcome.Data!.First(r => r.ProjectFullPath == projectPath);
        resolved.References.Should().ContainSingle()
            .Which.Should().BeOfType<InternalProjectReference>();
    }
}
