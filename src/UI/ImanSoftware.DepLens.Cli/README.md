# DepLens CLI

**Analyze a .NET codebase — one or many solutions — and get an interactive dependency graph as a single HTML file.**

![DepLens interactive dependency report - graph sample 2](https://raw.githubusercontent.com/ImanEstiri/DepLens/main/docs/images/graph-sample-1.gif)

`deplens` is the command-line front end for [DepLens](https://github.com/ImanEstiri/DepLens), a dependency-analysis engine for .NET codebases. It scans a directory for `.sln` / `.slnx` files and `.csproj` projects, resolves Central Package Management versions, and produces a self-contained HTML report you can open in any browser — no server, no restore step.

## Installation and verify

Requires the **.NET 10** runtime.

```bash
dotnet tool install --global imansoftware.depLens.cli
```
Then
```bash
deplens --version
```
![DepLens interactive dependency report](https://raw.githubusercontent.com/ImanEstiri/DepLens/main/docs/images/installation-and-verify.gif)

## Usage

```bash
deplens analyze
```

Point it at a specific folder (it must be a directory, not a `.sln` or `.csproj` file), and optionally choose where the report is written:

```bash
deplens analyze "C:\Projects\MyApp" --output ".\reports"
```
![DepLens interactive dependency report - usage sample](https://raw.githubusercontent.com/ImanEstiri/DepLens/main/docs/images/usage.gif)
Then open the generated `dependency-graph.html` in a browser.
![DepLens interactive dependency report - graph sample 2](https://raw.githubusercontent.com/ImanEstiri/DepLens/main/docs/images/graph-sample-2.gif)

## What you get

- A package-level dependency graph and a solution-architecture view, in one report
- Solutions grouped visually, including projects shared across solutions
- Central Package Management resolved per project, with the version source (explicit, central, or override) shown
- A tree sidebar that stays in sync with the graph — click either one to explore

## Learn more

Full documentation, known limitations, the roadmap, and how to build from source or contribute all live in the main repository:

**[github.com/ImanEstiri/DepLens](https://github.com/ImanEstiri/DepLens)**

## License

MIT — see [LICENSE](https://github.com/ImanEstiri/DepLens/blob/main/LICENSE.txt).