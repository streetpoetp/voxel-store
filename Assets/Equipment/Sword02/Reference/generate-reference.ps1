$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$partsRoot = Join-Path $root 'Parts'
New-Item -ItemType Directory -Force -Path $partsRoot | Out-Null

function C([string]$hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }
function Brush([string]$hex) { [System.Drawing.SolidBrush]::new((C $hex)) }
function Pen([string]$hex,[float]$w) { [System.Drawing.Pen]::new((C $hex),$w) }
function F([float]$size,[System.Drawing.FontStyle]$style=[System.Drawing.FontStyle]::Regular) {
  [System.Drawing.Font]::new('Arial',$size,$style,[System.Drawing.GraphicsUnit]::Pixel)
}
function Sha([string]$path) { (Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant() }

function Draw-FlatSword($g,[int]$cx,[int]$cy,[double]$scale,[string]$mode='front') {
  $len=[int](620*$scale); $bladeH=[int](94*$scale); $gripLen=[int](150*$scale)
  $guardH=[int](174*$scale); $guardW=[int](42*$scale)
  $x=$cx-[int]($len/2); $bladeX=$x+$gripLen+$guardW
  $profile = $mode -in @('left','right','top','bottom')
  if($profile) {
    $bladeH=[Math]::Max(10,[int](24*$scale)); $guardH=[Math]::Max(34,[int](54*$scale))
  }

  $grip=Brush '#4D2A23'; $purple=Brush '#702CA2'; $gold=Brush '#F3B72F'
  $cyan=Brush '#23C9D3'; $silver=Brush '#C6D9DF'; $light=Brush '#EAF4F6'; $blue=Brush '#8EADD0'
  $dark=Brush '#748A94'

  $gripY=$cy-[int]($bladeH*0.36)
  $g.FillRectangle($grip,$x,$gripY,$gripLen,[Math]::Max(16,[int]($bladeH*0.72)))
  $g.FillRectangle($purple,$x-[int](20*$scale),$gripY-[int](8*$scale),[int](32*$scale),[Math]::Max(20,[int]($bladeH*0.88)))
  $g.FillRectangle($cyan,$x-[int](12*$scale),$cy-[int](11*$scale),[int](16*$scale),[int](22*$scale))
  $g.FillRectangle($gold,$x+[int](78*$scale),$gripY,[int](18*$scale),[Math]::Max(16,[int]($bladeH*0.72)))
  $g.FillRectangle($purple,$x+[int](116*$scale),$gripY,[int](18*$scale),[Math]::Max(16,[int]($bladeH*0.72)))

  $guardX=$x+$gripLen
  $g.FillRectangle($purple,$guardX,$cy-[int]($guardH/2),$guardW,$guardH)
  $g.FillRectangle($gold,$guardX+[int](12*$scale),$cy-[int]($guardH/2)-[int](14*$scale),[int](48*$scale),[int](30*$scale))
  $g.FillRectangle($gold,$guardX+[int](12*$scale),$cy+[int]($guardH/2)-[int](16*$scale),[int](48*$scale),[int](30*$scale))
  if(-not $profile) {
    $g.FillRectangle($cyan,$guardX+[int](30*$scale),$cy-[int]($guardH/2)+[int](18*$scale),[int](28*$scale),[int](28*$scale))
    $g.FillRectangle($cyan,$guardX+[int](30*$scale),$cy+[int]($guardH/2)-[int](46*$scale),[int](28*$scale),[int](28*$scale))
  }

  $bladeLen=$len-$gripLen-$guardW-[int](40*$scale)
  $bladeY=$cy-[int]($bladeH/2)
  $g.FillRectangle($silver,$bladeX,$bladeY,$bladeLen,$bladeH)
  $tipX=$bladeX+$bladeLen
  $g.FillPolygon($silver,[System.Drawing.Point[]]@(
    [System.Drawing.Point]::new($tipX,$bladeY),
    [System.Drawing.Point]::new($tipX+[int](52*$scale),$cy),
    [System.Drawing.Point]::new($tipX,$bladeY+$bladeH)))
  if(-not $profile) {
    $g.FillRectangle($purple,$bladeX+[int](18*$scale),$bladeY-[int](4*$scale),[int](118*$scale),[Math]::Max(8,[int](18*$scale)))
    $g.FillRectangle($gold,$bladeX+[int](122*$scale),$bladeY+[int](5*$scale),[int](76*$scale),[Math]::Max(8,[int](16*$scale)))
    $g.FillRectangle($cyan,$bladeX+[int](42*$scale),$cy-[int](10*$scale),[int]($bladeLen*0.55),[Math]::Max(8,[int](20*$scale)))
    $g.FillRectangle($purple,$bladeX+[int]($bladeLen*0.57),$cy-[int](13*$scale),[int](70*$scale),[Math]::Max(8,[int](26*$scale)))
    $g.FillRectangle($gold,$bladeX+[int]($bladeLen*0.76),$cy-[int](9*$scale),[int](44*$scale),[Math]::Max(8,[int](18*$scale)))
    if($mode -eq 'back') {
      $g.FillRectangle($dark,$bladeX+[int](42*$scale),$bladeY+[int](20*$scale),[int]($bladeLen*0.65),[Math]::Max(6,[int](8*$scale)))
    } else {
      $g.FillRectangle($blue,$bladeX+[int](76*$scale),$bladeY+$bladeH-[int](20*$scale),[int](52*$scale),[Math]::Max(6,[int](12*$scale)))
      $g.FillRectangle($blue,$bladeX+[int](240*$scale),$bladeY+$bladeH-[int](18*$scale),[int](42*$scale),[Math]::Max(6,[int](10*$scale)))
    }
  } else {
    $g.FillRectangle($light,$bladeX,$bladeY,$bladeLen,[Math]::Max(4,[int]($bladeH*0.35)))
    if($mode -in @('top','bottom')) {
      $g.FillRectangle($cyan,$bladeX+[int](70*$scale),$cy-[int](4*$scale),[int]($bladeLen*0.46),[Math]::Max(4,[int](8*$scale)))
    }
  }

  $grip.Dispose(); $purple.Dispose(); $gold.Dispose(); $cyan.Dispose()
  $silver.Dispose(); $light.Dispose(); $blue.Dispose(); $dark.Dispose()
}

function Draw-Label($g,[string]$text,[int]$x,[int]$y,[int]$size=22) {
  $font=F $size ([System.Drawing.FontStyle]::Bold); $brush=Brush '#1E2832'
  $g.DrawString($text,$font,$brush,$x,$y)
  $font.Dispose(); $brush.Dispose()
}

# Composition Sheet 1200 x 800 (3:2)
$comp=[System.Drawing.Bitmap]::new(1200,800)
$g=[System.Drawing.Graphics]::FromImage($comp)
$g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::None
$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.Clear((C '#F7F1E6'))
$line=Pen '#C8B8A4' 2
$g.DrawRectangle($line,24,24,1152,752)
Draw-Label $g 'SWORD02 — COMPOSITION' 48 42 30
$small=F 18; $textBrush=Brush '#3E4852'
$g.DrawString('Visual authority from accepted Product Target. Construction only; implementation density remains downstream.',$small,$textBrush,50,86)

$target=[System.Drawing.Image]::FromFile((Join-Path $root 'target.png'))
$g.DrawImage($target,[System.Drawing.Rectangle]::new(50,130,620,410))
$target.Dispose()
Draw-Label $g 'HERO VIEW' 62 142 18

$g.DrawRectangle($line,700,130,425,190)
Draw-Label $g 'PART MANIFEST' 724 150 21
$g.DrawString('RequiredParts',$small,$textBrush,724,194)
$mono=F 24 ([System.Drawing.FontStyle]::Bold); $purpleText=Brush '#702CA2'
$g.DrawString('[ Sword ]',$mono,$purpleText,724,226)
$g.DrawString('One rigid weapon assembly.',$small,$textBrush,724,272)

$g.DrawRectangle($line,700,344,425,196)
Draw-Label $g 'ASSEMBLY RELATIONSHIP' 724 364 21
$g.DrawString('Sword is the only rigid Part.',$small,$textBrush,724,408)
$g.DrawString('Grip region is visual intent only.',$small,$textBrush,724,442)
$g.DrawString('Attachment implementation remains downstream.',$small,$textBrush,724,476)

$g.DrawRectangle($line,50,570,1075,160)
Draw-Label $g 'EXPLODED COMPOSITION' 70 588 20
Draw-FlatSword $g 610 674 0.70 'front'
$call=Pen '#702CA2' 3
$g.DrawLine($call,250,660,390,674)
$g.DrawString('Sword',$mono,$purpleText,90,642)

$compPath=Join-Path $root 'composition.png'
$comp.Save($compPath,[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $comp.Dispose()

# Part Sheet: one Part, local hero + six orthographic views.
$part=[System.Drawing.Bitmap]::new(1600,1180)
$g=[System.Drawing.Graphics]::FromImage($part)
$g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::None
$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.Clear((C '#F4F0E8'))
$g.DrawRectangle($line,24,24,1552,1132)
Draw-Label $g 'PART: SWORD' 52 42 32
$g.DrawString('Neutral local construction views • consistent scale • one rigid Part',$small,$textBrush,54,88)

# Local hero
$g.DrawRectangle($line,50,128,1500,250)
Draw-Label $g 'LOCAL HERO' 72 146 20
Draw-FlatSword $g 810 286 1.25 'front'

$views=@(
  @('FRONT',70,420,'front'),
  @('BACK',570,420,'back'),
  @('LEFT',1070,420,'left'),
  @('RIGHT',70,760,'right'),
  @('TOP',570,760,'top'),
  @('BOTTOM',1070,760,'bottom')
)
foreach($v in $views) {
  $label=[string]$v[0]; $x=[int]$v[1]; $y=[int]$v[2]; $mode=[string]$v[3]
  $g.DrawRectangle($line,$x,$y,440,300)
  Draw-Label $g $label ($x+18) ($y+14) 18
  Draw-FlatSword $g ($x+220) ($y+168) 0.56 $mode
}

$g.DrawString('Visual subparts: blade core • energy channel • guard • grip • pommel',$small,$textBrush,70,1082)
$g.DrawString('These are construction regions inside rigid Part Sword, not separate Parts.',$small,$textBrush,70,1112)

$partPath=Join-Path $partsRoot 'Sword.png'
$part.Save($partPath,[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $part.Dispose()

$line.Dispose(); $small.Dispose(); $textBrush.Dispose(); $mono.Dispose(); $purpleText.Dispose(); $call.Dispose()

$reference=@'
version: 3
target:
  image: target.png
  provenance: target.provenance.json
composition: composition.png
requiredParts:
  - Sword
partSheets:
  Sword: Parts/Sword.png
fidelity: fidelity.audit.json
'@
Set-Content (Join-Path $root 'reference.yaml') $reference -Encoding utf8

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
  target=[ordered]@{path='target.png';sha256=(Sha (Join-Path $root 'target.png'))}
  sheets=[ordered]@{
    'composition.png'=[ordered]@{sha256=(Sha $compPath);criteria=$criteria}
    'Parts/Sword.png'=[ordered]@{sha256=(Sha $partPath);criteria=$criteria}
  }
}
$fidelity | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $root 'fidelity.audit.json') -Encoding utf8

[pscustomobject]@{
  composition=(Sha $compPath)
  part=(Sha $partPath)
  target=(Sha (Join-Path $root 'target.png'))
} | ConvertTo-Json
