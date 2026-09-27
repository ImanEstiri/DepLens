# DepLens CLI

**DepLens** is a .NET CLI tool for analyzing project and NuGet dependencies and generating an interactive HTML dependency report.

It is designed to give you a quick visual overview of how projects and packages are connected inside a .NET codebase.

## Installation

Install DepLens globally as a .NET tool:

```bash
dotnet tool install --global ImanSoftware.DepLens.Cli
```

Verify the installation:

```bash
deplens --version
```

## Usage

Analyze the current directory:

```bash
deplens analyze
```

When no path is provided, DepLens analyzes the directory where the terminal is currently open.

You can also provide a directory explicitly:

```bash
deplens analyze "C:\Projects\MyApp"
```

The path should point to a **directory**, not directly to a `.sln` or `.csproj` file.

### Output Directory

By default, the generated report is placed in the directory being analyzed.

To specify a different output directory:

```bash
deplens analyze "C:\Projects\MyApp" --output ".\reports"
```

Or use the short option:

```bash
deplens analyze "C:\Projects\MyApp" -o ".\reports"
```

The generated report can then be opened in any modern web browser.

## Currently Available

### 🔍 Project Analysis

- Analyze .NET projects inside a directory
- Detect `ProjectReference` relationships
- Resolve project dependencies
- Detect external project references

### 📦 NuGet Analysis

- Detect NuGet `PackageReference` dependencies
- Identify direct package dependencies
- Identify transitive package dependencies
- Display package versions
- Support Central Package Management
- Detect package version sources

### 🌐 Interactive HTML Report

DepLens generates an interactive HTML report containing:

- Project dependency graph
- NuGet package dependency graph
- Solution-based project grouping
- Project and package details
- Dependency highlighting
- Search and navigation
- Dependency relationships and versions

The generated report is completely self-contained and can be opened directly in a browser.

## Example

```bash
deplens analyze
```

Example output:

```text
────────────────────────────────────────────── DepLens
Scanning : C:\Projects\MyApp
Output   : C:\Projects\MyApp

✓ Analyzed 12 project(s)
✓ Report: C:\Projects\MyApp\dependency-graph.html

Open: C:\Projects\MyApp\dependency-graph.html
```

## Coming Soon

The following features are planned for future releases:

- ⏳ Dependency cycle detection
- ⏳ NuGet version conflict detection
- ⏳ JSON output
- ⏳ Additional report formats
- ⏳ Dependency path analysis
- ⏳ Architecture rules
- ⏳ CI/CD integration
- ⏳ Advanced graph filtering
- ⏳ More dependency types

## Development

Clone the repository:

```bash
git clone https://github.com/ImanEstiri/DepLens.git
cd DepLens
```

Build the solution:

```bash
dotnet build
```

Run the CLI directly from source:

```bash
dotnet run --project src/UI/ImanSoftware.DepLens.Cli -- analyze
```

Or analyze a specific directory:

```bash
dotnet run --project src/UI/ImanSoftware.DepLens.Cli -- analyze "C:\Projects\MyApp"
```

## Project

DepLens is open source and developed for the .NET ecosystem.

Repository:

https://github.com/ImanEstiri/DepLens

## License

See the [`LICENSE`](https://github.com/ImanEstiri/DepLens/blob/main/LICENSE) file for license information.