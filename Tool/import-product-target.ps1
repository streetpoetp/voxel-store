# Optional exact-byte transport helper for external Product Target PNGs.
# This script is not a required Product Target acquisition path or authority.

param(
    [Parameter(Mandatory=$true)]
    [string]$Source,
    [Parameter(Mandatory=$true)]
    [string]$Destination
)

$ErrorActionPreference='Stop'

function Sha256([string]$path) {
    (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
}

$sourcePath=(Resolve-Path -LiteralPath $Source).Path
if([System.IO.Path]::GetExtension($sourcePath).ToLowerInvariant() -ne '.png') {
    throw "Product Target import requires a PNG source: $sourcePath"
}

$destinationPath=$Destination
if(-not [System.IO.Path]::IsPathRooted($destinationPath)) {
    $destinationPath=Join-Path (Get-Location).Path $destinationPath
}
$destinationPath=[System.IO.Path]::GetFullPath($destinationPath)

$destinationDir=Split-Path -Parent $destinationPath
New-Item -ItemType Directory -Force -Path $destinationDir | Out-Null

$sourceHash=Sha256 $sourcePath
Copy-Item -LiteralPath $sourcePath -Destination $destinationPath -Force
$destinationHash=Sha256 $destinationPath

if($sourceHash -ne $destinationHash) {
    throw "Product Target exact-byte import failed: source=$sourceHash destination=$destinationHash"
}

[pscustomobject]@{
    Source=[System.IO.Path]::GetFileName($sourcePath)
    Destination=$destinationPath
    Sha256=$destinationHash
    Bytes=(Get-Item -LiteralPath $destinationPath).Length
} | ConvertTo-Json -Compress
