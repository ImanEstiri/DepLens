param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectName,

    [switch]$SkipPush
)

$ErrorActionPreference = "Stop"

$solutionRoot  = $PSScriptRoot
$srcRoot       = Join-Path $solutionRoot "src"
$artifactsPath = Join-Path $solutionRoot "artifacts\$ProjectName"

# ===================================================================
# Logging helpers
#   Write-Step   -> yellow : status / decision messages
#   Write-Action -> blue   : "about to run a command" announcements
#   (actual command output is left uncolored / default terminal color)
#   Add-Summary  -> records a line for the final summary block
# ===================================================================

$script:summary = @()

function Write-Step {
    param([Parameter(Mandatory = $true)][string]$Message)
    Write-Host $Message -ForegroundColor Yellow
}

function Write-Action {
    param([Parameter(Mandatory = $true)][string]$Message)
    Write-Host $Message -ForegroundColor Blue
}

function Add-Summary {
    param([Parameter(Mandatory = $true)][string]$Message)
    $script:summary += $Message
}

function Show-Summary {
    Write-Host ""
    Write-Host "===================== Summary =====================" -ForegroundColor Cyan
    if ($script:summary.Count -eq 0) {
        Write-Host " (nothing was recorded)" -ForegroundColor Cyan
    } else {
        foreach ($line in $script:summary) {
            Write-Host " - $line" -ForegroundColor Cyan
        }
    }
    Write-Host "=====================================================" -ForegroundColor Cyan
}

function Bump-PatchVersion {
    param([Parameter(Mandatory = $true)][string]$Version)

    if ($Version -notmatch '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?<suffix>[-+].*)?$') {
        throw "Version '$Version' is not in a recognizable Major.Minor.Patch format."
    }

    $major  = $Matches['major']
    $minor  = $Matches['minor']
    $patch  = [int]$Matches['patch'] + 1
    $suffix = $Matches['suffix']

    return "$major.$minor.$patch$suffix"
}

function Get-CsprojVersion {
    param([Parameter(Mandatory = $true)][string]$Path)
    [xml]$xml = Get-Content $Path
    $value = $xml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    # Explicit cast: the pipeline above can hand back a wrapped value that
    # later fails when assigned to an XmlAttribute, even though it prints fine elsewhere.
    return [string]$value
}

function Set-CsprojVersion {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$NewVersion
    )
    [xml]$xml = Get-Content $Path
    $propertyGroup = $xml.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1

    if (-not $propertyGroup) {
        throw "Could not find a <PropertyGroup> with a <Version> element in $Path"
    }

    $propertyGroup.Version = [string]$NewVersion
    $xml.Save($Path)
}

# Runs an external process under a hard, PowerShell-enforced wall-clock timeout.
# This exists because "dotnet nuget push --timeout N" only bounds the actual
# upload request — the preceding service-index lookup (GET .../v3/index.json)
# has its own internal ~100s timeout that --timeout does NOT control. If the
# process doesn't exit in time we kill the whole process tree (dotnet can spawn
# child processes) via taskkill /T /F so nothing is left running in the background.
#
# Uses System.Diagnostics.Process directly (not the Start-Process cmdlet) because
# Start-Process -PassThru combined with output redirection can unreliably report
# ExitCode even after the process has exited successfully.
function ConvertTo-ArgumentString {
    param([string[]]$Arguments)
    return ($Arguments | ForEach-Object {
        if ($_ -match '[\s"]') {
            '"' + ($_ -replace '"', '\"') + '"'
        } else {
            $_
        }
    }) -join ' '
}

function Invoke-ProcessWithTimeout {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$ArgumentList,
        [Parameter(Mandatory = $true)][int]$TimeoutSeconds
    )

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName               = $FilePath
    $psi.Arguments              = ConvertTo-ArgumentString -Arguments $ArgumentList
    $psi.UseShellExecute        = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError  = $true
    $psi.CreateNoWindow         = $true

    $proc = New-Object System.Diagnostics.Process
    $proc.StartInfo = $psi

    $stdOutBuilder = New-Object System.Text.StringBuilder
    $stdErrBuilder = New-Object System.Text.StringBuilder

    $outEvent = Register-ObjectEvent -InputObject $proc -EventName OutputDataReceived -Action {
        if ($EventArgs.Data) { $Event.MessageData.AppendLine($EventArgs.Data) | Out-Null }
    } -MessageData $stdOutBuilder

    $errEvent = Register-ObjectEvent -InputObject $proc -EventName ErrorDataReceived -Action {
        if ($EventArgs.Data) { $Event.MessageData.AppendLine($EventArgs.Data) | Out-Null }
    } -MessageData $stdErrBuilder

    try {
        $proc.Start() | Out-Null
        $proc.BeginOutputReadLine()
        $proc.BeginErrorReadLine()

        $exited = $proc.WaitForExit($TimeoutSeconds * 1000)

        if (-not $exited) {
            try {
                Start-Process -FilePath "taskkill.exe" -ArgumentList "/PID $($proc.Id) /T /F" -NoNewWindow -Wait -ErrorAction SilentlyContinue | Out-Null
            } catch {}
            Start-Sleep -Milliseconds 300
        } else {
            # Blocks only until the already-exited process's streams finish flushing.
            $proc.WaitForExit()
        }
    }
    finally {
        Unregister-Event -SourceIdentifier $outEvent.Name -ErrorAction SilentlyContinue
        Unregister-Event -SourceIdentifier $errEvent.Name -ErrorAction SilentlyContinue
        Remove-Job $outEvent, $errEvent -Force -ErrorAction SilentlyContinue
    }

    if ($stdOutBuilder.Length -gt 0) { Write-Host $stdOutBuilder.ToString().TrimEnd() }
    if ($stdErrBuilder.Length -gt 0) { Write-Host $stdErrBuilder.ToString().TrimEnd() }

    if (-not $exited) {
        return [PSCustomObject]@{ TimedOut = $true; ExitCode = $null }
    }

    return [PSCustomObject]@{ TimedOut = $false; ExitCode = $proc.ExitCode }
}

# ===================================================================
# Main
# ===================================================================

try {
    # DepLens nests projects under src\Core, src\UI, src\Playground, ... (unlike Platform's
    # flat src\<ProjectName>), so we locate the .csproj by name instead of assuming a fixed path.
    Write-Step "looking for $ProjectName.csproj under $srcRoot ..."

    $csprojFile = Get-ChildItem -Path $srcRoot -Filter "$ProjectName.csproj" -Recurse -ErrorAction SilentlyContinue |
        Select-Object -First 1

    if (-not $csprojFile) {
        Write-Error "No project named '$ProjectName' found under $srcRoot (searched recursively)."
    }

    $projectPath = $csprojFile.DirectoryName
    $csprojPath  = $csprojFile.FullName
    Write-Step "found project at $projectPath"

    # ===== API Key =====
    # Recommendation: do NOT hardcode the key in this file. Set it once, outside of git:
    #   setx NUGET_API_KEY "your-real-key-here"
    # Then close and reopen your terminal / Visual Studio so the new value is loaded.
    $ApiKey = $env:NUGET_API_KEY

    if ([string]::IsNullOrWhiteSpace($ApiKey)) {
        Write-Error "Environment variable NUGET_API_KEY is not set. Run 'setx NUGET_API_KEY your-key' and restart your terminal."
    }

    # Note: previous builds are kept intentionally, so you have a local history
    # of every version ever packed. We don't delete anything here.
    if (-not (Test-Path $artifactsPath)) {
        Write-Step "artifacts folder for $ProjectName does not exist yet; creating $artifactsPath"
        New-Item -Path $artifactsPath -ItemType Directory | Out-Null
    }

    # ---------------------------------------------------------------
    # Pack (auto-bumping the patch version if it was already packed)
    # ---------------------------------------------------------------

    $version = Get-CsprojVersion -Path $csprojPath

    if (-not $version) {
        Write-Error "Could not read <Version> from $csprojPath. Make sure it's set explicitly in the project file."
    }

    $originalVersion = $version

    Write-Step "checking artifacts/$ProjectName for any $version output."
    $expectedNupkg = Join-Path $artifactsPath "$ProjectName.$version.nupkg"

    while (Test-Path $expectedNupkg) {
        $previous = $version
        $version  = Bump-PatchVersion -Version $version
        Write-Step "auto bumping version for $ProjectName from $previous to $version"
        Add-Summary "Version auto-bumped: $previous -> $version (an artifact for $previous already existed)"
        Write-Step "checking artifacts/$ProjectName for any $version output."
        $expectedNupkg = Join-Path $artifactsPath "$ProjectName.$version.nupkg"
    }

    Write-Step "version $version is free; no existing artifact found."

    if ($version -ne $originalVersion) {
        Set-CsprojVersion -Path $csprojPath -NewVersion $version
        Write-Step "updated <Version> in $csprojPath : $originalVersion -> $version"
        Add-Summary "csproj updated: $ProjectName Version $originalVersion -> $version"
    } else {
        Add-Summary "Version used: $ProjectName $version (no bump needed)"
    }

    Write-Action "packing $ProjectName v$version ..."
    dotnet pack $projectPath -c Release --output $artifactsPath
    if ($LASTEXITCODE -ne 0) {
        Add-Summary "FAILED: dotnet pack for $ProjectName v$version"
        Write-Error "Pack failed."
    }
    Add-Summary "Packed: $ProjectName v$version -> $artifactsPath"

    # Multiple versions may already sit in this folder from previous runs.
    # The one we just built is always the most recently modified file, so we
    # sort by LastWriteTime and take the newest — nothing else gets touched.
    $nupkg = Get-ChildItem -Path $artifactsPath -Filter "*.nupkg" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $nupkg) {
        Write-Error "No .nupkg file was found after packing."
    }

    $snupkgPath = Join-Path $artifactsPath "$ProjectName.$version.snupkg"

    Write-Step "package built: $($nupkg.Name)"

    if ($SkipPush) {
        Write-Step "SkipPush is set; packed only, not pushed."
        Add-Summary "Push skipped (-SkipPush): $($nupkg.Name) left in $artifactsPath"
        return
    }

    # ---------------------------------------------------------------
    # Push (15s timeout; on failure, remove the artifacts we just built
    # so the next run doesn't see a "used" version and bump for no reason)
    # ---------------------------------------------------------------

    Write-Action "pushing package to nuget ..."

    $pushTimeoutSeconds = 15
    $pushArgs = @(
        'nuget', 'push', $nupkg.FullName,
        '--source', 'https://api.nuget.org/v3/index.json',
        '--api-key', $ApiKey,
        '--skip-duplicate',
        '--timeout', "$pushTimeoutSeconds"
    )

    $pushResult = Invoke-ProcessWithTimeout -FilePath 'dotnet' -ArgumentList $pushArgs -TimeoutSeconds $pushTimeoutSeconds

    if ($pushResult.TimedOut) {
        Write-Step "push did not finish within ${pushTimeoutSeconds}s; the dotnet process was forcefully terminated."
        Add-Summary "Push timed out after ${pushTimeoutSeconds}s (process killed): $ProjectName v$version"
    } elseif ($pushResult.ExitCode -ne 0) {
        Write-Step "push failed (exit code $($pushResult.ExitCode))."
        Add-Summary "Push failed (exit code $($pushResult.ExitCode)): $ProjectName v$version"
    }

    if ($pushResult.TimedOut -or $pushResult.ExitCode -ne 0) {
        Write-Step "removing the artifacts just built for v$version so the version isn't wasted."

        if (Test-Path $nupkg.FullName) {
            Remove-Item $nupkg.FullName -Force
            Write-Step "removed $($nupkg.Name)"
            Add-Summary "Removed after failed push: $($nupkg.Name)"
        }
        if (Test-Path $snupkgPath) {
            Remove-Item $snupkgPath -Force
            Write-Step "removed $(Split-Path $snupkgPath -Leaf)"
            Add-Summary "Removed after failed push: $(Split-Path $snupkgPath -Leaf)"
        }

        Add-Summary "FAILED: dotnet nuget push for $ProjectName v$version"
        Write-Error "Push failed."
    }

    Write-Step "$($nupkg.Name) was successfully published to NuGet.org."
    Add-Summary "Pushed to NuGet.org: $($nupkg.Name)"

    # NOTE: unlike the Platform script, there is intentionally no "sync Directory.Packages.props"
    # or "bump dependent projects" step here. Nothing in this repo currently consumes DepLens
    # packages via <PackageReference> — Core references Abstractions, and Cli references Core,
    # both via <ProjectReference>. If that ever changes (e.g. Abstractions/Core get published
    # too and something else in this repo takes a PackageReference on them), port that logic
    # back in from publish.ps1 in ImanSoftware.Platform.
}
finally {
    Show-Summary
}
