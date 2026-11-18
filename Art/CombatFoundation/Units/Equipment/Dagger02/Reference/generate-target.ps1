$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = (Resolve-Path (Join-Path $root '../../../../../..')).Path
$styleDir = Join-Path $repo 'Art/Style/low-resolution-voxel.v1'
New-Item -ItemType Directory -Force -Path $root | Out-Null

function C([string]$hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }
function Brush([string]$hex) { [System.Drawing.SolidBrush]::new((C $hex)) }
function Pen([string]$hex,[float]$w) { [System.Drawing.Pen]::new((C $hex),$w) }
function Sha([string]$path) { (Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant() }

function Cuboid($g,[int]$x,[int]$y,[int]$w,[int]$h,[int]$d,[string]$front,[string]$top,[string]$side) {
    $bf=Brush $front; $bt=Brush $top; $bs=Brush $side
    $g.FillRectangle($bf,$x,$y,$w,$h)
    $g.FillPolygon($bt,[System.Drawing.Point[]]@(
        [System.Drawing.Point]::new($x,$y),
        [System.Drawing.Point]::new($x+$d,$y-$d),
        [System.Drawing.Point]::new($x+$w+$d,$y-$d),
        [System.Drawing.Point]::new($x+$w,$y)))
    $g.FillPolygon($bs,[System.Drawing.Point[]]@(
        [System.Drawing.Point]::new($x+$w,$y),
        [System.Drawing.Point]::new($x+$w+$d,$y-$d),
        [System.Drawing.Point]::new($x+$w+$d,$y+$h-$d),
        [System.Drawing.Point]::new($x+$w,$y+$h)))
    $bf.Dispose(); $bt.Dispose(); $bs.Dispose()
}

$bmp=[System.Drawing.Bitmap]::new(900,640)
$g=[System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::None
$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
$g.Clear((C '#F7F1E6'))

$shadow=Brush '#DED2C2'
$g.FillEllipse($shadow,135,448,640,54)
$shadow.Dispose()

# Chunky pommel and wrapped close-combat grip.
Cuboid $g 100 292 74 78 12 '#4D2A23' '#75483B' '#321B18'
Cuboid $g 76 306 42 50 10 '#5C238A' '#9149C2' '#3F175F'
Cuboid $g 86 318 20 26 7 '#35DCE4' '#85F4F5' '#1999A8'
Cuboid $g 172 298 112 66 10 '#5A3425' '#81503A' '#3D241B'
Cuboid $g 190 306 18 50 7 '#F3B72F' '#FFD96A' '#C17E12'
Cuboid $g 226 306 18 50 7 '#702CA2' '#A15AD0' '#4A1E70'

# Compact guard with upward/downward purple shoulders and cyan inset.
Cuboid $g 280 260 42 140 12 '#702CA2' '#A15AD0' '#4A1E70'
Cuboid $g 296 244 62 46 10 '#F3B72F' '#FFD96A' '#C17E12'
Cuboid $g 296 370 62 46 10 '#F3B72F' '#FFD96A' '#C17E12'
Cuboid $g 318 256 28 28 8 '#23C9D3' '#75EEF1' '#168C9A'
Cuboid $g 318 374 28 28 8 '#23C9D3' '#75EEF1' '#168C9A'

# Short broad blade: stepped leaf profile, clearly distinct from Sword.
Cuboid $g 326 274 176 112 12 '#D8E5EA' '#F6FBFC' '#9AABB3'
Cuboid $g 486 286 120 88 11 '#C6D9DF' '#EFF7F9' '#879AA3'
Cuboid $g 590 298 86 64 10 '#B7CED6' '#E6F1F4' '#748A94'

$tipFront=Brush '#C6D9DF'; $tipTop=Brush '#EFF7F9'; $tipSide=Brush '#879AA3'
$g.FillPolygon($tipFront,[System.Drawing.Point[]]@(
  [System.Drawing.Point]::new(676,298),
  [System.Drawing.Point]::new(744,330),
  [System.Drawing.Point]::new(676,362)))
$g.FillPolygon($tipTop,[System.Drawing.Point[]]@(
  [System.Drawing.Point]::new(676,298),
  [System.Drawing.Point]::new(688,286),
  [System.Drawing.Point]::new(756,318),
  [System.Drawing.Point]::new(744,330)))
$g.FillPolygon($tipSide,[System.Drawing.Point[]]@(
  [System.Drawing.Point]::new(744,330),
  [System.Drawing.Point]::new(756,318),
  [System.Drawing.Point]::new(688,350),
  [System.Drawing.Point]::new(676,362)))
$tipFront.Dispose(); $tipTop.Dispose(); $tipSide.Dispose()

# Distinct reinforced back edge, central fuller, and gold blade notch.
Cuboid $g 344 272 126 26 8 '#702CA2' '#A15AD0' '#4A1E70'
Cuboid $g 452 280 62 22 7 '#F3B72F' '#FFD96A' '#C17E12'
Cuboid $g 364 321 188 24 7 '#23C9D3' '#75EEF1' '#168C9A'
Cuboid $g 536 312 66 26 6 '#702CA2' '#A15AD0' '#4A1E70'
Cuboid $g 600 315 38 18 5 '#F3B72F' '#FFD96A' '#C17E12'
Cuboid $g 408 350 44 16 5 '#8EADD0' '#C5DCF3' '#617A9A'

$border=Pen '#C8B8A4' 3
$g.DrawRectangle($border,38,38,824,564)
$border.Dispose()

$targetPath=Join-Path $root 'target.png'
$bmp.Save($targetPath,[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

$traits=@(
'chunky low-resolution voxel forms',
'simplified readable geometry',
'chibi proportion language',
'restrained detail density',
'clear separated color regions',
'high silhouette readability',
'no realistic textures or painterly non-voxel forms',
'purposeful secondary construction detail',
'restrained tertiary accents only where they clarify form',
'readable taper, thickness variation, structural transitions, and grip regions'
)
$common=@('reference-02.png')
$kind=@('equipment-bow-source.png','equipment-general-reference.png')
$lf=[char]10
$builder='style=low-resolution-voxel.v1'+$lf+'kind=equipment'+$lf
foreach($trait in $traits){ $builder += 'trait='+$trait+$lf }
foreach($file in $common){ $builder += 'common='+$file+'|'+(Sha (Join-Path $styleDir $file))+$lf }
foreach($file in $kind){ $builder += 'kindref='+$file+'|'+(Sha (Join-Path $styleDir $file))+$lf }
$sha256=[System.Security.Cryptography.SHA256]::Create()
try { $authorityBytes=$sha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($builder)) }
finally { $sha256.Dispose() }
$authority=([System.BitConverter]::ToString($authorityBytes)).Replace('-','').ToLowerInvariant()
$styleHashes=[ordered]@{}
foreach($file in ($common+$kind)){
  $key='Art/Style/low-resolution-voxel.v1/'+$file
  $styleHashes[$key]=Sha (Join-Path $styleDir $file)
}
$provenance=[ordered]@{
  version=1
  origin='generated'
  targetSha256=(Sha $targetPath)
  targetStyleAuthoritySha256=$authority
  targetStyleSha256=$styleHashes
}
$provenance | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $root 'target.provenance.json') -Encoding utf8
[pscustomobject]@{target=(Sha $targetPath);authority=$authority;styleHashes=$styleHashes} | ConvertTo-Json -Depth 8
