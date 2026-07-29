[CmdletBinding()]
param(
    [string]$UnityPath,
    [string]$OutputPath = "Builds\Windows\ClashOfPantheons",
    [switch]$SkipArchive
)

$ErrorActionPreference = "Stop"

$projectRoot = [System.IO.Path]::GetFullPath($PSScriptRoot)
$buildsRoot = [System.IO.Path]::GetFullPath((Join-Path $projectRoot "Builds"))
$logsRoot = Join-Path $projectRoot "Logs\Builds"
$projectVersionPath = Join-Path $projectRoot "ProjectSettings\ProjectVersion.txt"

function Resolve-ProjectPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $projectRoot $Path))
}

function Assert-SafeBuildOutput {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $buildsPrefix = $buildsRoot + [System.IO.Path]::DirectorySeparatorChar
    if (-not $Path.StartsWith($buildsPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Build output must be a subdirectory of '$buildsRoot'. Received '$Path'."
    }

    if ($Path.Equals($buildsRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "The Builds root itself cannot be used as an output directory."
    }
}

function Move-WithRollback {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Source,
        [Parameter(Mandatory = $true)]
        [string]$Destination
    )

    $backup = $Destination + ".backup-" + [System.Guid]::NewGuid().ToString("N")
    $hadPreviousDestination = Test-Path -LiteralPath $Destination

    try {
        if ($hadPreviousDestination) {
            Move-Item -LiteralPath $Destination -Destination $backup
        }

        Move-Item -LiteralPath $Source -Destination $Destination
    }
    catch {
        $promotionError = $_

        if ($hadPreviousDestination -and
            (Test-Path -LiteralPath $backup) -and
            -not (Test-Path -LiteralPath $Destination)) {
            try {
                Move-Item -LiteralPath $backup -Destination $Destination
            }
            catch {
                throw "Promotion failed and the previous output could not be restored from '$backup'. Promotion error: $($promotionError.Exception.Message) Rollback error: $($_.Exception.Message)"
            }
        }

        throw $promotionError
    }

    if (Test-Path -LiteralPath $backup) {
        try {
            Remove-Item -LiteralPath $backup -Recurse -Force
        }
        catch {
            Write-Warning "The new output is valid, but the previous-output backup could not be removed: '$backup'. $($_.Exception.Message)"
        }
    }
}

if (-not (Test-Path -LiteralPath $projectVersionPath -PathType Leaf)) {
    throw "Unity project version file was not found at '$projectVersionPath'."
}

$versionFile = Get-Content -LiteralPath $projectVersionPath -Raw
$versionMatch = [System.Text.RegularExpressions.Regex]::Match(
    $versionFile,
    "m_EditorVersion:\s*(\S+)"
)

if (-not $versionMatch.Success) {
    throw "Could not determine the required Unity version from '$projectVersionPath'."
}

$projectUnityVersion = $versionMatch.Groups[1].Value
if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    $UnityPath = $env:UNITY_EDITOR_PATH
}

if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    $UnityPath = Join-Path ${env:ProgramFiles} "Unity\Hub\Editor\$projectUnityVersion\Editor\Unity.exe"
}

$resolvedUnityPath = Resolve-ProjectPath -Path $UnityPath
if (-not (Test-Path -LiteralPath $resolvedUnityPath -PathType Leaf)) {
    throw @"
Unity $projectUnityVersion was not found at '$resolvedUnityPath'.
Install that editor version with Windows Build Support, set UNITY_EDITOR_PATH,
or pass -UnityPath 'C:\path\to\Unity.exe'.
"@
}

$outputFullPath = Resolve-ProjectPath -Path $OutputPath
Assert-SafeBuildOutput -Path $outputFullPath

$unityLockPath = Join-Path $projectRoot "Temp\UnityLockfile"
if (Test-Path -LiteralPath $unityLockPath) {
    throw "This project appears to be open in Unity. Close the Unity Editor, then run this command again. The project lock is '$unityLockPath'."
}

New-Item -ItemType Directory -Path $buildsRoot -Force | Out-Null
New-Item -ItemType Directory -Path $logsRoot -Force | Out-Null

$runId = (Get-Date -Format "yyyyMMdd-HHmmss") + "-" + [System.Guid]::NewGuid().ToString("N").Substring(0, 8)
$stagingPath = Join-Path $buildsRoot (".windows-x64-staging-" + $runId)
$logPath = Join-Path $logsRoot ("Windows-x64-" + $runId + ".log")
$expectedExecutable = Join-Path $stagingPath "ClashOfPantheons.exe"
$expectedDataDirectory = Join-Path $stagingPath "ClashOfPantheons_Data"
$expectedUnityPlayer = Join-Path $stagingPath "UnityPlayer.dll"
$archiveStagingPath = $null

New-Item -ItemType Directory -Path $stagingPath | Out-Null

try {
    Write-Host "Building Clash of Pantheons for Windows x64 with Unity $projectUnityVersion..."
    Write-Host "Unity log: $logPath"

    $unityArguments = @(
        "-batchmode",
        "-quit",
        "-buildTarget",
        "StandaloneWindows64",
        "-projectPath",
        "`"$projectRoot`"",
        "-executeMethod",
        "ClashWindowsBuild.BuildWindows64",
        "-clashBuildExecutable",
        "`"$expectedExecutable`"",
        "-logFile",
        "`"$logPath`""
    )

    $unityProcess = Start-Process `
        -FilePath $resolvedUnityPath `
        -ArgumentList $unityArguments `
        -Wait `
        -PassThru `
        -WindowStyle Hidden

    $unityExitCode = $unityProcess.ExitCode
    if ($unityExitCode -ne 0) {
        throw "Unity player build failed with exit code $unityExitCode. See '$logPath'."
    }

    if (-not (Test-Path -LiteralPath $expectedExecutable -PathType Leaf)) {
        throw "Unity reported success but did not create '$expectedExecutable'. See '$logPath'."
    }

    if (-not (Test-Path -LiteralPath $expectedDataDirectory -PathType Container)) {
        throw "Unity reported success but did not create '$expectedDataDirectory'. See '$logPath'."
    }

    if (-not (Get-ChildItem -LiteralPath $expectedDataDirectory -Force | Select-Object -First 1)) {
        throw "Unity reported success but '$expectedDataDirectory' is empty. See '$logPath'."
    }

    if (-not (Test-Path -LiteralPath $expectedUnityPlayer -PathType Leaf)) {
        throw "Unity reported success but did not create '$expectedUnityPlayer'. See '$logPath'."
    }

    $outputParent = Split-Path -Parent $outputFullPath
    New-Item -ItemType Directory -Path $outputParent -Force | Out-Null
    Move-WithRollback -Source $stagingPath -Destination $outputFullPath

    Write-Host "Windows player folder: $outputFullPath"

    if (-not $SkipArchive) {
        $archiveName = (Split-Path -Leaf $outputFullPath) + "-Windows-x64.zip"
        $archivePath = Join-Path $outputParent $archiveName
        $archiveStagingPath = Join-Path $outputParent (".archive-staging-" + $runId + ".zip")
        Compress-Archive -LiteralPath $outputFullPath -DestinationPath $archiveStagingPath -CompressionLevel Optimal
        Move-WithRollback -Source $archiveStagingPath -Destination $archivePath
        Write-Host "Shareable archive: $archivePath"
    }
}
finally {
    if (Test-Path -LiteralPath $stagingPath) {
        try {
            Remove-Item -LiteralPath $stagingPath -Recurse -Force
        }
        catch {
            Write-Warning "Could not remove staging directory '$stagingPath': $($_.Exception.Message)"
        }
    }

    if ($null -ne $archiveStagingPath -and (Test-Path -LiteralPath $archiveStagingPath)) {
        try {
            Remove-Item -LiteralPath $archiveStagingPath -Force
        }
        catch {
            Write-Warning "Could not remove archive staging file '$archiveStagingPath': $($_.Exception.Message)"
        }
    }
}
