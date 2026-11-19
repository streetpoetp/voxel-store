$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $root '..\..')).Path
$tool = Join-Path $repoRoot 'Tool\VoxelStore.Tool\bin\Release\net8.0\Hero.Voxel.Tool.dll'
$source = Join-Path $root 'Source\WarHammerStyleTest.voxel.yaml'
$palette = Join-Path $root 'Source\WarHammerStyleTest.palette.yaml'
$generatedDir = Join-Path $root 'Generated'
$proofDir = Join-Path $root 'Proof'
$vox = Join-Path $generatedDir 'WarHammerStyleTest.generated.vox'

if (-not (Test-Path -LiteralPath $tool)) {
    throw "Hero.Voxel.Tool is not built at $tool"
}

New-Item -ItemType Directory -Force -Path $generatedDir, $proofDir | Out-Null

& dotnet $tool compile --spec $source --palette $palette --output $vox
if ($LASTEXITCODE -ne 0) {
    throw 'WarHammerStyleTest compile failed.'
}

Remove-Item (Join-Path $proofDir '*.png') -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $proofDir 'preview-poses.json') -Force -ErrorAction SilentlyContinue

& dotnet $tool preview --spec $source --palette $palette --output $proofDir --width 320 --height 320
if ($LASTEXITCODE -ne 0) {
    throw 'WarHammerStyleTest preview failed.'
}

Write-Output "Generated: $vox"
Write-Output "Proof: $proofDir"
