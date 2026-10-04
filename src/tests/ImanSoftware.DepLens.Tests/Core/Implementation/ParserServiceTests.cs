using ImanSoftware.DepLens.Abstractions.Models;
using ImanSoftware.DepLens.Core.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImanSoftware.DepLens.Tests.Core.Implementation;

public class ParserServiceTests
{
    [Fact]
    public async Task ParseAsync_MalformedXml_ReturnsFailure()
    {
        var parser = new ParserService();
        var file = new DiscoveredFile("Broken.csproj", FileType.Project, "<Project>");

        var result = await parser.ParseAsync(file);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ParseAsync_UnsupportedFileType_ReturnsFailure()
    {
        var parser = new ParserService();
        var file = new DiscoveredFile("Unknown", (FileType)int.MaxValue, "");

        var result = await parser.ParseAsync(file);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ParseAsync_ClassicSolutionWithTwoCSharpProjects_ReturnsBothProjectPaths()
    {
        // Arrange
        var parser = new ParserService();

        var rawContent = """
    Microsoft Visual Studio Solution File, Format Version 12.00
    Project("{GUID}") = "App", "src\App\App.csproj", "{GUID}"
    EndProject
    Project("{GUID}") = "Domain", "src\Domain\Domain.csproj", "{GUID}"
    EndProject
    """;

        var file = new DiscoveredFile(
            "Test.sln",
            FileType.SolutionClassic,
            rawContent);

        // Act
        var result = await parser.ParseAsync(file);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(file, result.Data.Source);
        var parsedSolution = Assert.IsType<ParsedSolution>(result.Data.Content);
        Assert.Equal(2, parsedSolution.ProjectPaths.Count);
        Assert.Contains(@"src\Domain\Domain.csproj", parsedSolution.ProjectPaths);
        Assert.Contains(@"src\App\App.csproj", parsedSolution.ProjectPaths);
    }

    [Fact]
    public async Task ParseAsync_ProjectWithVersionOverride_UsesOverrideInsteadOfCentralVersion()
    {
        // Arrange
        var parser = new ParserService();

        var centralPackages = new ParsedPackagesProps(
            true,
            new Dictionary<string, string>
            {
                ["Serilog"] = "4.2.0"
            });

        var context = new ProjectContext(
            "Test.csproj",
            Array.Empty<string>(),
            centralPackages);

        var rawContent = """
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Serilog" VersionOverride="4.3.0" />
    </ItemGroup>
</Project>
""";

        var file = new DiscoveredFile(
            "Test.csproj",
            FileType.Project,
            rawContent);

        // Act
        var result = await parser.ParseAsync(file, context);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var parsedProject = Assert.IsType<ParsedProject>(result.Data.Content);

        var package = Assert.Single(parsedProject.PackageReferences);

        Assert.Equal("Serilog", package.Name);
        Assert.Equal("4.3.0", package.ResolvedVersion);
        Assert.Equal(PackageVersionSourceType.VersionOverride, package.Source);
    }

    [Fact]
    public async Task ParseAsync_ProjectWithMultipleTargetFrameworks_ReturnsAllFrameworks()
    {
        // Arrange
        var parser = new ParserService();

        var rawContent = """
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
    </PropertyGroup>
</Project>
""";

        var file = new DiscoveredFile(
            "Test.csproj",
            FileType.Project,
            rawContent);

        // Act
        var result = await parser.ParseAsync(file);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var parsedProject = Assert.IsType<ParsedProject>(result.Data.Content);

        Assert.Equal(3, parsedProject.TargetFrameworks.Count);
        Assert.Contains("net8.0", parsedProject.TargetFrameworks);
        Assert.Contains("net9.0", parsedProject.TargetFrameworks);
        Assert.Contains("net10.0", parsedProject.TargetFrameworks);
    }

    [Fact]
    public async Task ParseAsync_PackagesPropsWithIncompletePackageVersions_ExcludesInvalidEntries()
    {
        // Arrange
        var parser = new ParserService();

        var rawContent = """
<Project>
    <PropertyGroup>
        <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    </PropertyGroup>

    <ItemGroup>
        <PackageVersion Include="Serilog" Version="4.2.0" />
        <PackageVersion Include="FluentValidation" />
        <PackageVersion Version="8.0.0" />
    </ItemGroup>
</Project>
""";

        var file = new DiscoveredFile(
            "Directory.Packages.props",
            FileType.DirectoryPackagesProps,
            rawContent);

        // Act
        var result = await parser.ParseAsync(file);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var parsedPackages = Assert.IsType<ParsedPackagesProps>(result.Data.Content);

        Assert.True(parsedPackages.ManagePackageVersionsCentrally);
        Assert.Single(parsedPackages.CentralPackageVersions);
        Assert.Equal("4.2.0", parsedPackages.CentralPackageVersions["Serilog"]);
    }

    [Fact]
    public async Task ParseAsync_ProjectWithMissingCentralPackage_ReturnsNullVersionWithCentralSource()
    {
        // Arrange
        var parser = new ParserService();

        var centralPackages = new ParsedPackagesProps(
            true,
            new Dictionary<string, string>
            {
                ["Serilog"] = "4.2.0"
            });

        var context = new ProjectContext(
            "Test.csproj",
            Array.Empty<string>(),
            centralPackages);

        var rawContent = """
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="FluentValidation" />
    </ItemGroup>
</Project>
""";

        var file = new DiscoveredFile(
            "Test.csproj",
            FileType.Project,
            rawContent);

        // Act
        var result = await parser.ParseAsync(file, context);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var parsedProject = Assert.IsType<ParsedProject>(result.Data.Content);
        var package = Assert.Single(parsedProject.PackageReferences);

        Assert.Equal("FluentValidation", package.Name);
        Assert.Null(package.ResolvedVersion);
        Assert.Equal(
            PackageVersionSourceType.CentralPackageManagement,
            package.Source);
    }

    [Fact]
    public async Task ParseAsync_SolutionXmlWithNestedProjects_ReturnsOnlyCSharpProjects()
    {
        // Arrange
        var parser = new ParserService();

        var rawContent = """
<Solution>
    <Folder Name="/src/">
        <Project Path="src/App/App.csproj" />
        <Project Path="src/Domain/Domain.csproj" />
    </Folder>

    <Project Path="tools/Legacy.vbproj" />
</Solution>
""";

        var file = new DiscoveredFile(
            "Test.slnx",
            FileType.SolutionXml,
            rawContent);

        // Act
        var result = await parser.ParseAsync(file);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var parsedSolution = Assert.IsType<ParsedSolution>(result.Data.Content);

        Assert.Equal(2, parsedSolution.ProjectPaths.Count);
        Assert.Contains("src/App/App.csproj", parsedSolution.ProjectPaths);
        Assert.Contains("src/Domain/Domain.csproj", parsedSolution.ProjectPaths);
        Assert.DoesNotContain("tools/Legacy.vbproj", parsedSolution.ProjectPaths);
    }
}
