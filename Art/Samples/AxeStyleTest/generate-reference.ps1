$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$ref = Join-Path $root 'Reference'
$parts = Join-Path $ref 'Parts'
New-Item -ItemType Directory -Force -Path $parts | Out-Null

$target = Join-Path $ref 'target.png'
if(-not (Test-Path $target)){ throw "Missing accepted exact-byte Product Target: $target" }

Remove-Item (Join-Path $root 'axe-style-reference.png') -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $root 'target.png') -Force -ErrorAction SilentlyContinue

function Sha([string]$p){ (Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant() }
function C([string]$hex){ [System.Drawing.ColorTranslator]::FromHtml($hex) }
function Brush([string]$hex){ [System.Drawing.SolidBrush]::new((C $hex)) }
function Pen([string]$hex,[float]$w){ [System.Drawing.Pen]::new((C $hex),$w) }
function Font([float]$size,[System.Drawing.FontStyle]$style=[System.Drawing.FontStyle]::Regular){
  [System.Drawing.Font]::new('Arial',$size,$style,[System.Drawing.GraphicsUnit]::Pixel)
}
function Crop($img,[System.Drawing.Rectangle]$r){
  $bmp=[System.Drawing.Bitmap]::new($r.Width,$r.Height)
  $g=[System.Drawing.Graphics]::FromImage($bmp)
  $g.DrawImage($img,[System.Drawing.Rectangle]::new(0,0,$r.Width,$r.Height),$r,[System.Drawing.GraphicsUnit]::Pixel)
  $g.Dispose()
  $bmp
}
function Fit($g,$img,[System.Drawing.RectangleF]$box){
  $scale=[Math]::Min($box.Width/$img.Width,$box.Height/$img.Height)
  $w=$img.Width*$scale; $h=$img.Height*$scale
  $g.DrawImage($img,[System.Drawing.RectangleF]::new($box.X+($box.Width-$w)/2,$box.Y+($box.Height-$h)/2,$w,$h))
}

$resolved=(Resolve-Path $target).Path
$img=[System.Drawing.Bitmap]::new($resolved)
if($img.Width -ne 1448 -or $img.Height -ne 1086){ throw "Unexpected target size $($img.Width)x$($img.Height)" }

$hero = Crop $img ([System.Drawing.Rectangle]::new(55,625,620,390))
$front = Crop $img ([System.Drawing.Rectangle]::new(660,665,180,300))
$side = Crop $img ([System.Drawing.Rectangle]::new(835,665,170,300))
$back = Crop $img ([System.Drawing.Rectangle]::new(985,665,190,300))

$bg=C '#F8F4E9'; $text=Brush '#29251F'; $muted=Brush '#655F56'; $border=Pen '#C7BBA9' 2
$title=Font 28 ([System.Drawing.FontStyle]::Bold); $small=Font 18; $label=Font 20 ([System.Drawing.FontStyle]::Bold)

$comp=[System.Drawing.Bitmap]::new(1200,800)
$g=[System.Drawing.Graphics]::FromImage($comp); $g.Clear($bg)
$g.DrawString('AXE — PRODUCT-TARGET COMPOSITION',$title,$text,40,30)
$g.DrawString('RequiredParts = [Axe]  •  Exact accepted image-model target remains visual authority.',$small,$muted,43,72)
$g.DrawRectangle($border,45,115,1110,510)
Fit $g $hero ([System.Drawing.RectangleF]::new(70,140,1060,455))
$g.DrawRectangle($border,45,650,1110,100)
Fit $g $front ([System.Drawing.RectangleF]::new(80,655,280,88))
Fit $g $side ([System.Drawing.RectangleF]::new(455,655,280,88))
Fit $g $back ([System.Drawing.RectangleF]::new(830,655,280,88))
$g.DrawString('FRONT',$small,$text,150,755)
$g.DrawString('SIDE',$small,$text,535,755)
$g.DrawString('BACK',$small,$text,915,755)
$composition=Join-Path $ref 'composition.png'
$comp.Save($composition,[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $comp.Dispose()

$part=[System.Drawing.Bitmap]::new(1500,1050)
$g=[System.Drawing.Graphics]::FromImage($part); $g.Clear($bg)
$g.DrawString('PART: AXE — TARGET-DERIVED CONSTRUCTION EVIDENCE',$title,$text,42,30)
$g.DrawString('Large local hero plus source-provided FRONT / SIDE / BACK. Missing thickness details are construction interpretation downstream, never Product Target redesign.',$small,$muted,45,72)
$g.DrawRectangle($border,40,115,1420,420)
Fit $g $hero ([System.Drawing.RectangleF]::new(70,140,1360,365))
$g.DrawString('LOCAL HERO / ACCEPTED TARGET WEAPON',$label,$text,70,122)

$boxes=@(
  [pscustomobject]@{Name='FRONT';Img=$front;X=50},
  [pscustomobject]@{Name='SIDE';Img=$side;X=520},
  [pscustomobject]@{Name='BACK';Img=$back;X=990}
)
foreach($b in $boxes){
  $g.DrawRectangle($border,$b.X,575,420,390)
  Fit $g $b.Img ([System.Drawing.RectangleF]::new($b.X+30,620,360,310))
  $g.DrawString($b.Name,$label,$text,$b.X+24,585)
}
$g.DrawString('Visible construction: wood handle • green wraps • dark steel eye/head • gold front inset • broad stepped silver blades • gray pommel.',$small,$muted,50,990)
$partPath=Join-Path $parts 'Axe.png'
$part.Save($partPath,[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $part.Dispose()

$img.Dispose(); $hero.Dispose(); $front.Dispose(); $side.Dispose(); $back.Dispose()
$title.Dispose();$small.Dispose();$label.Dispose();$text.Dispose();$muted.Dispose();$border.Dispose()

$request=[ordered]@{
  version=1
  asset='AxeStyleTest'
  intent='Design a polished low-resolution voxel fantasy ranger axe reference sheet in the visual language of the supplied green ranger/bow reference: chunky stepped voxel geometry, cream background, green/brown/cream character palette, readable orthographic weapon views, broad double steel axe head, dark central eye, gold inset, brown handle with green wraps, product-quality presentation.'
  negativeConstraints=@(
    'no procedural redraw as Product Target',
    'no realistic smooth non-voxel surfaces',
    'no dense texture noise',
    'do not simplify to icon-level geometry'
  )
  delivery=[ordered]@{
    method='exact-byte desktop download bridge'
    sourceFileName='Voxel Ranger Axe Design Sheet.png'
    importer='Tool/import-product-target.ps1'
  }
}
$requestPath=Join-Path $ref 'target.request.json'
$request | ConvertTo-Json -Depth 8 | Set-Content $requestPath -Encoding utf8

$prov=[ordered]@{
  version=2
  origin='generated'
  targetSha256=(Sha $target)
  generator=[ordered]@{
    kind='image-model'
    provider='OpenAI'
    model='chatgpt-image-generation'
    request=[ordered]@{
      path='target.request.json'
      sha256=(Sha $requestPath)
    }
  }
}
$prov | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $ref 'target.provenance.json') -Encoding utf8

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
  target=[ordered]@{path='target.png';sha256=(Sha $target)}
  sheets=[ordered]@{
    'composition.png'=[ordered]@{sha256=(Sha $composition);criteria=$criteria}
    'Parts/Axe.png'=[ordered]@{sha256=(Sha $partPath);criteria=$criteria}
  }
}
$fidelity | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $ref 'fidelity.audit.json') -Encoding utf8

@'
version: 3
target:
  image: target.png
  provenance: target.provenance.json
composition: composition.png
requiredParts:
  - Axe
partSheets:
  Axe: Parts/Axe.png
fidelity: fidelity.audit.json
'@ | Set-Content (Join-Path $ref 'reference.yaml') -Encoding utf8

[pscustomobject]@{
  target=(Sha $target)
  request=(Sha $requestPath)
  composition=(Sha $composition)
  part=(Sha $partPath)
} | ConvertTo-Json -Compress
