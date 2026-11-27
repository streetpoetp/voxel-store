param(
    [switch]$VerifyOnly
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$heroRoot = Join-Path $repoRoot 'Dependencies\hero-studio'
$lockPath = Join-Path $repoRoot 'toolchain\hero-voxel.lock.yaml'

function Read-LockScalar([string]$name) {
    $line = Get-Content $lockPath |
        Where-Object { $_ -match ("^" + [regex]::Escape($name) + ":\s*(.+)$") } |
        Select-Object -First 1
    if (-not $line) {
        throw "Missing '$name' in $lockPath"
    }
    return ($line -replace ("^" + [regex]::Escape($name) + ":\s*"), '').Trim()
}

if (-not (Test-Path $lockPath)) {
    throw "Hero.Voxel lock not found: $lockPath"
}

$providerIdentity = Read-LockScalar 'heroStudioRevision'
$expectedWorkflowHash = Read-LockScalar 'workflowSha256'
$workflowRelative = Read-LockScalar 'workflow'

if (-not (Test-Path $heroRoot)) {
    throw 'Hero Studio provider is missing at Dependencies/hero-studio.'
}

$workflowPath = Join-Path $heroRoot (
    $workflowRelative.Replace('/', [IO.Path]::DirectorySeparatorChar))
if (-not (Test-Path $workflowPath)) {
    throw "Pinned Hero.Voxel workflow is missing: $workflowPath"
}
$actualWorkflowHash =
    (Get-FileHash $workflowPath -Algorithm SHA256).Hash.ToLowerInvariant()
if (-not [string]::Equals(
    $actualWorkflowHash,
    $expectedWorkflowHash,
    [StringComparison]::OrdinalIgnoreCase)) {
    throw "Hero.Voxel workflow fingerprint mismatch. expected=$expectedWorkflowHash actual=$actualWorkflowHash"
}

$packageSource = Join-Path $heroRoot 'src\client\unity\Packages\hero.studio'
$packageJson = Join-Path $packageSource 'package.json'
if (-not (Test-Path $packageJson)) {
    throw "Pinned Hero Studio Unity package is missing: $packageJson"
}

$packageLink = Join-Path $PSScriptRoot 'Unity\Packages\hero.studio'
if (Test-Path $packageLink) {
    $item = Get-Item $packageLink -Force
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -eq 0) {
        throw "Refusing to replace non-link Unity package path: $packageLink"
    }

    $actualTarget = [IO.Path]::GetFullPath([string]$item.Target)
    $expectedTarget = [IO.Path]::GetFullPath($packageSource)
    if (-not [string]::Equals(
        $actualTarget,
        $expectedTarget,
        [StringComparison]::OrdinalIgnoreCase)) {
        if ($VerifyOnly) {
            throw "Hero Studio package junction target mismatch. expected=$expectedTarget actual=$actualTarget"
        }
        Remove-Item $packageLink -Force
    }
}

if (-not $VerifyOnly -and -not (Test-Path $packageLink)) {
    New-Item -ItemType Junction -Path $packageLink -Target $packageSource | Out-Null
}

if ($VerifyOnly -and -not (Test-Path $packageLink)) {
    throw "Hero Studio embedded package junction is missing: $packageLink"
}

Write-Output "verified Hero Studio provider contract $providerIdentity"
Write-Output "embedded full Hero Studio Unity package from $packageSource"
