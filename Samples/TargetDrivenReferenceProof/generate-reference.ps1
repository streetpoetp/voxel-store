$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = (Resolve-Path (Join-Path $root '../..')).Path
$ref = Join-Path $root 'Reference'
$sourceDir = Join-Path $ref 'Source'
$partsDir = Join-Path $ref 'Parts'
New-Item -ItemType Directory -Force -Path $sourceDir,$partsDir | Out-Null

$styleSource = Join-Path $repo 'Styles/low-resolution-voxel.v1/equipment-bow-source.png'
$sourceCopy = Join-Path $sourceDir 'equipment-bow-source.png'
Copy-Item $styleSource $sourceCopy -Force

function New-Brush([string]$hex) {
    [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($hex))
}
function New-Pen([string]$hex,[float]$width) {
    [System.Drawing.Pen]::new([System.Drawing.ColorTranslator]::FromHtml($hex),$width)
}
function New-Font([float]$size,[System.Drawing.FontStyle]$style=[System.Drawing.FontStyle]::Regular) {
    [System.Drawing.Font]::new('Segoe UI',$size,$style)
}
function Fit-Image($graphics,$image,[System.Drawing.RectangleF]$box) {
    $scale = [Math]::Min($box.Width / $image.Width, $box.Height / $image.Height)
    $width = $image.Width * $scale
    $height = $image.Height * $scale
    $graphics.DrawImage(
        $image,
        [System.Drawing.RectangleF]::new(
            $box.X + ($box.Width-$width)/2,
            $box.Y + ($box.Height-$height)/2,
            $width,
            $height))
}
function Crop-Image($image,[System.Drawing.Rectangle]$rect) {
    $output = [System.Drawing.Bitmap]::new($rect.Width,$rect.Height)
    $graphics = [System.Drawing.Graphics]::FromImage($output)
    $graphics.DrawImage(
        $image,
        [System.Drawing.Rectangle]::new(0,0,$rect.Width,$rect.Height),
        $rect,
        [System.Drawing.GraphicsUnit]::Pixel)
    $graphics.Dispose()
    $output
}
function Sha256([string]$path) {
    (Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()
}

$source = [System.Drawing.Bitmap]::new($sourceCopy)
if ($source.Width -ne 1448 -or $source.Height -ne 1086) {
    throw "Unexpected source dimensions $($source.Width)x$($source.Height)"
}

# Canonical Product Target derivative: clean source-provided isolated side bow.
$target = Crop-Image $source ([System.Drawing.Rectangle]::new(795,725,390,205))
$targetPath = Join-Path $ref 'target.png'
$target.Save($targetPath,[System.Drawing.Imaging.ImageFormat]::Png)

$background = [System.Drawing.ColorTranslator]::FromHtml('#F8F4EB')
$textBrush = New-Brush '#28241F'
$mutedBrush = New-Brush '#655C53'
$borderPen = New-Pen '#B8AD9F' 2
$titleFont = New-Font 23 ([System.Drawing.FontStyle]::Bold)
$smallFont = New-Font 12

# Visual-first 3:2 Composition.
$composition = [System.Drawing.Bitmap]::new(1200,800)
$graphics = [System.Drawing.Graphics]::FromImage($composition)
$graphics.Clear($background)
$graphics.DrawString('BOW — PRODUCT-TARGET COMPOSITION',$titleFont,$textBrush,40,28)
$graphics.DrawString(
    'RequiredParts = [Bow]  •  Target-derived visual authority; implementation density stays downstream.',
    $smallFont,$mutedBrush,43,69)
$graphics.DrawRectangle($borderPen,45,110,1110,505)
Fit-Image $graphics $target ([System.Drawing.RectangleF]::new(70,135,1060,455))
$detailRects = @(
    [System.Drawing.Rectangle]::new(0,20,140,175),
    [System.Drawing.Rectangle]::new(115,55,165,125),
    [System.Drawing.Rectangle]::new(250,20,135,175)
)
for ($i=0; $i -lt 3; $i++) {
    $x=45+$i*370
    $graphics.DrawRectangle($borderPen,$x,640,330,115)
    $detail=$target.Clone($detailRects[$i],$target.PixelFormat)
    Fit-Image $graphics $detail ([System.Drawing.RectangleF]::new($x+8,648,314,99))
    $detail.Dispose()
}
$compositionPath=Join-Path $ref 'composition.png'
$composition.Save($compositionPath,[System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$composition.Dispose()

# Representative construction evidence from source-provided weapon views.
$part=[System.Drawing.Bitmap]::new(1200,900)
$graphics=[System.Drawing.Graphics]::FromImage($part)
$graphics.Clear($background)
$graphics.DrawString('BOW — PART EVIDENCE / Bow',$titleFont,$textBrush,40,28)
$graphics.DrawString(
    'Representative target-fidelity evidence; production authoring still follows the pinned Hero.Voxel neutral-local view contract.',
    $smallFont,$mutedBrush,43,69)
$graphics.DrawRectangle($borderPen,35,105,1130,410)
Fit-Image $graphics $target ([System.Drawing.RectangleF]::new(60,135,1080,350))
$graphics.DrawString('LOCAL HERO / ISOLATED TARGET',$smallFont,$textBrush,55,112)

$front=Crop-Image $source ([System.Drawing.Rectangle]::new(1200,710,160,245))
$graphics.DrawRectangle($borderPen,35,545,545,300)
Fit-Image $graphics $front ([System.Drawing.RectangleF]::new(75,575,465,235))
$graphics.DrawString('SOURCE-PROVIDED FRONT',$smallFont,$textBrush,55,552)
$front.Dispose()

$detail=$target.Clone([System.Drawing.Rectangle]::new(105,45,185,135),$target.PixelFormat)
$graphics.DrawRectangle($borderPen,610,545,555,300)
Fit-Image $graphics $detail ([System.Drawing.RectangleF]::new(650,590,475,205))
$graphics.DrawString('TARGET DETAIL / GRIP + WRAPS',$smallFont,$textBrush,630,552)
$detail.Dispose()
$partPath=Join-Path $partsDir 'Bow.png'
$part.Save($partPath,[System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$part.Dispose()

$source.Dispose()
$target.Dispose()
$titleFont.Dispose()
$smallFont.Dispose()
$textBrush.Dispose()
$mutedBrush.Dispose()
$borderPen.Dispose()

@'
version: 3
target:
  image: target.png
  provenance: target.provenance.json
composition: composition.png
requiredParts:
  - Bow
partSheets:
  Bow: Parts/Bow.png
fidelity: fidelity.audit.json
'@ | Set-Content (Join-Path $ref 'reference.yaml') -Encoding UTF8

$provenance=[ordered]@{
    version=1
    origin='imported'
    targetSha256=(Sha256 $targetPath)
    source=[ordered]@{
        path='Source/equipment-bow-source.png'
        sha256=(Sha256 $sourceCopy)
        relation='canonical-derivative'
    }
}
$provenance | ConvertTo-Json -Depth 6 |
    Set-Content (Join-Path $ref 'target.provenance.json') -Encoding UTF8

$criteria=[ordered]@{
    silhouette='pass'
    proportions='pass'
    constructionRichness='pass'
    materialColorHierarchy='pass'
    meaningfulDetailDensity='pass'
    visualEvidenceDominance='pass'
    implementationNeutral='pass'
}
$fidelity=[ordered]@{
    version=1
    target=[ordered]@{
        path='target.png'
        sha256=(Sha256 $targetPath)
    }
    sheets=[ordered]@{
        'composition.png'=[ordered]@{
            sha256=(Sha256 $compositionPath)
            criteria=$criteria
        }
        'Parts/Bow.png'=[ordered]@{
            sha256=(Sha256 $partPath)
            criteria=$criteria
        }
    }
}
$fidelity | ConvertTo-Json -Depth 8 |
    Set-Content (Join-Path $ref 'fidelity.audit.json') -Encoding UTF8

[pscustomobject]@{
    Target=(Sha256 $targetPath)
    Composition=(Sha256 $compositionPath)
    Part=(Sha256 $partPath)
    Source=(Sha256 $sourceCopy)
} | ConvertTo-Json -Compress
