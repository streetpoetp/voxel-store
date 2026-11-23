[CmdletBinding()]
param(
    [string]$Scenario = '',
    [string]$UnityExe = '',
    [ValidateRange(1,86400)][int]$TimeoutSeconds = 900
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$heroTools = Join-Path $repoRoot 'Dependencies/hero-studio/consumer/game-client/tools'
$bootstrap = Join-Path $repoRoot 'Preview/Bootstrap.ps1'

try {
    if (-not (Test-Path -LiteralPath $bootstrap -PathType Leaf)) {
        throw "Preview bootstrap not found: $bootstrap"
    }

    if (-not (Get-Command Get-FileHash -ErrorAction SilentlyContinue)) {
        function Get-FileHash {
            param(
                [Parameter(Mandatory = $true)][string]$Path,
                [string]$Algorithm = 'SHA256'
            )

            if (-not [string]::Equals(
                $Algorithm,
                'SHA256',
                [StringComparison]::OrdinalIgnoreCase)) {
                throw "Fallback Get-FileHash supports only SHA256."
            }

            $stream = [IO.File]::OpenRead($Path)
            try {
                $sha = [Security.Cryptography.SHA256]::Create()
                try {
                    $bytes = $sha.ComputeHash($stream)
                }
                finally {
                    $sha.Dispose()
                }
            }
            finally {
                $stream.Dispose()
            }

            $hash = [BitConverter]::ToString($bytes).Replace('-', '')
            return [pscustomobject]@{
                Algorithm = 'SHA256'
                Hash = $hash
                Path = [IO.Path]::GetFullPath($Path)
            }
        }
    }

    & $bootstrap | ForEach-Object { Write-Host $_ }

    if (-not (Get-Variable -Name IsWindows -Scope Global -ErrorAction SilentlyContinue)) {
        $global:IsWindows = [string]::Equals(
            [string]$env:OS,
            'Windows_NT',
            [StringComparison]::OrdinalIgnoreCase)
    }

    . (Join-Path $heroTools 'shared/unity-project.ps1')
    . (Join-Path $heroTools 'shared/game-view-capture.ps1')

    $project = Resolve-HeroUnityProject -RepoRoot $repoRoot -DefaultRelativeProject 'Preview/Unity'
    $exe = Resolve-HeroUnityExecutable -UnityProject $project -Requested $UnityExe
    if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
        throw "Unity executable not found: $exe"
    }

    Assert-HeroUnityProjectAvailable -UnityProject $project

    $registryPath = Join-Path $project 'Assets/Preview/Capture/CaptureRegistry.json'
    $selected = Resolve-HeroGameViewCaptureScenario -RegistryPath $registryPath -UnityProject $project -Scenario $Scenario

    $runner = [string]$selected.Runner
    $output = [string]$selected.OutputDirectory
    $scenarioName = [string]$selected.Name

    $library = [IO.Path]::GetFullPath((Join-Path $project 'Library'))
    New-Item -ItemType Directory -Force -Path $output | Out-Null
    $log = Join-Path $library ('voxel-store-game-view-' + [Guid]::NewGuid().ToString('N') + '.log')

    $baseline = @{}
    foreach ($file in @(Get-ChildItem -LiteralPath $output -File -Recurse -ErrorAction SilentlyContinue)) {
        $baseline[$file.FullName] = [pscustomobject]@{
            Time = $file.LastWriteTimeUtc
            Length = $file.Length
        }
    }

    $started = [DateTime]::UtcNow
    $arguments = @(
        '-projectPath',('"' + $project + '"'),
        '-executeMethod',$runner,
        '-logFile',('"' + $log + '"')
    )

    $child = Start-Process -FilePath $exe -ArgumentList $arguments -PassThru

    try {
        $completed = $child.WaitForExit($TimeoutSeconds * 1000)
        if (-not $completed) {
            throw "Graphical Unity Editor capture timed out after $TimeoutSeconds seconds; log: $log"
        }

        $child.Refresh()
        if ($child.ExitCode -ne 0) {
            throw "Unity Editor exited with code $($child.ExitCode); log: $log"
        }
    }
    finally {
        if (-not $child.HasExited) {
            Stop-Process -Id $child.Id -Force -ErrorAction SilentlyContinue
            $child.WaitForExit(5000) | Out-Null
        }
        $child.Dispose()
    }

    function Test-CurrentCapture([string]$Path) {
        if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
            return $false
        }

        $file = Get-Item -LiteralPath $Path
        if ($file.LastWriteTimeUtc -lt $started.AddSeconds(-2)) {
            return $false
        }

        if ($baseline.ContainsKey($file.FullName)) {
            $old = $baseline[$file.FullName]
            if ($old.Time -eq $file.LastWriteTimeUtc -and $old.Length -eq $file.Length) {
                return $false
            }
        }

        return $true
    }

    $capture = Confirm-HeroGameViewCaptureAttestation -OutputDirectory $output -IsCurrentCapture {
        param($candidate)
        Test-CurrentCapture $candidate
    }

    Write-Host ('VOXEL_STORE_GAME_VIEW_SCENARIO=' + $scenarioName)
    Write-Host ('VOXEL_STORE_GAME_VIEW_OUTPUT_DIR=' + $output)
    Write-Host ('VOXEL_STORE_GAME_VIEW_CAPTURE_JSON=' + $capture.Path)
    Write-Host ('VOXEL_STORE_GAME_VIEW_PNG_COUNT=' + $capture.PngPaths.Count)
    Write-Host ('VOXEL_STORE_GAME_VIEW_LOG=' + $log)
    Write-Host 'VOXEL_STORE_GAME_VIEW_RESULT=CAPTURE_ATTESTED'
}
catch {
    [Console]::Error.WriteLine('Voxel Store Game View capture failed: ' + $_.Exception.Message)
    exit 1
}
