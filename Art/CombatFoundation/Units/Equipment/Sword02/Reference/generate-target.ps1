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

$bmp=[System.Drawing.Bitmap]::new(960,640)
$g=[System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::None
$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
$g.Clear((C '#F7F1E6'))

$shadow=Brush '#DED2C2'
$g.FillEllipse($shadow,115,455,725,58)
$shadow.Dispose()

Cuboid $g 105 286 74 86 12 '#4D2A23' '#75483B' '#321B18'
Cuboid $g 82 300 42 58 10 '#702CA2' '#A15AD0' '#4A1E70'
Cuboid $g 92 313 20 30 7 '#35DCE4' '#85F4F5' '#1999A8'
Cuboid $g 178 296 96 66 10 '#5A3425' '#81503A' '#3D241B'
Cuboid $g 194 304 20 50 8 '#F3B72F' '#FFD96A' '#C17E12'
Cuboid $g 232 304 20 50 8 '#702CA2' '#A15AD0' '#4A1E70'

Cuboid $g 270 248 38 160 12 '#5C238A' '#9149C2' '#3F175F'
Cuboid $g 286 226 50 60 10 '#F3B72F' '#FFD96A' '#C17E12'
Cuboid $g 286 370 50 60 10 '#F3B72F' '#FFD96A' '#C17E12'
Cuboid $g 310 240 30 42 8 '#35DCE4' '#85F4F5' '#1999A8'
Cuboid $g 310 374 30 42 8 '#35DCE4' '#85F4F5' '#1999A8'

Cuboid $g 326 272 210 112 12 '#D8E5EA' '#F6FBFC' '#9AABB3'
Cuboid $g 514 282 158 92 11 '#C6D9DF' '#EFF7F9' '#879AA3'
Cuboid $g 650 293 112 70 10 '#B7CED6' '#E6F1F4' '#748A94'

$tipFront=Brush '#C6D9DF'; $tipTop=Brush '#EFF7F9'; $tipSide=Brush '#879AA3'
$g.FillPolygon($tipFront,[System.Drawing.Point[]]@(
  [System.Drawing.Point]::new(762,293),
  [System.Drawing.Point]::new(842,328),
  [System.Drawing.Point]::new(762,363)))
$g.FillPolygon($tipTop,[System.Drawing.Point[]]@(
  [System.Drawing.Point]::new(762,293),
  [System.Drawing.Point]::new(774,281),
  [System.Drawing.Point]::new(854,316),
  [System.Drawing.Point]::new(842,328)))
$g.FillPolygon($tipSide,[System.Drawing.Point[]]@(
  [System.Drawing.Point]::new(842,328),
  [System.Drawing.Point]::new(854,316),
  [System.Drawing.Point]::new(774,351),
  [System.Drawing.Point]::new(762,363)))
$tipFront.Dispose(); $tipTop.Dispose(); $tipSide.Dispose()

Cuboid $g 346 270 108 24 8 '#702CA2' '#A15AD0' '#4A1E70'
Cuboid $g 448 280 88 22 7 '#F3B72F' '#FFD96A' '#C17E12'
Cuboid $g 365 321 244 24 7 '#23C9D3' '#75EEF1' '#168C9A'
Cuboid $g 598 312 74 22 6 '#702CA2' '#A15AD0' '#4A1E70'
Cuboid $g 675 315 44 18 5 '#F3B72F' '#FFD96A' '#C17E12'
Cuboid $g 395 349 52 18 5 '#8EADD0' '#C5DCF3' '#617A9A'
Cuboid $g 548 347 42 16 5 '#8EADD0' '#C5DCF3' '#617A9A'

$border=Pen '#C8B8A4' 3
$g.DrawRectangle($border,38,38,884,564)
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
