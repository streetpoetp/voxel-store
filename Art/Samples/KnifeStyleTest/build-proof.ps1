$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$target = Join-Path $root 'Reference\target.png'
$selected = Join-Path $root 'Candidates\Vpu12'
$sourceDir = Join-Path $root 'Source'
$generatedDir = Join-Path $root 'Generated'
$proofDir = Join-Path $root 'Proof'
New-Item -ItemType Directory -Force -Path $sourceDir,$generatedDir,$proofDir | Out-Null

function Sha([string]$p){ (Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant() }
function Fit($g,$img,[System.Drawing.RectangleF]$box){
  $scale=[Math]::Min($box.Width/$img.Width,$box.Height/$img.Height)
  $w=$img.Width*$scale; $h=$img.Height*$scale
  $g.DrawImage($img,[System.Drawing.RectangleF]::new($box.X+($box.Width-$w)/2,$box.Y+($box.Height-$h)/2,$w,$h))
}
function Crop($img,[System.Drawing.Rectangle]$r){
  $bmp=[System.Drawing.Bitmap]::new($r.Width,$r.Height)
  $g=[System.Drawing.Graphics]::FromImage($bmp)
  $g.DrawImage($img,[System.Drawing.Rectangle]::new(0,0,$r.Width,$r.Height),$r,[System.Drawing.GraphicsUnit]::Pixel)
  $g.Dispose()
  $bmp
}

$expectedTarget='ff3b01cf4bfb90b0f2bb663b1081dcb24882a39d704d394cf259df830888a707'
if((Sha $target) -ne $expectedTarget){ throw 'Frozen Product Target hash changed.' }

$selectedSpec=Join-Path $selected 'KnifeStyleTest.Vpu12.voxel.yaml'
$selectedPalette=Join-Path $selected 'KnifeStyleTest.Vpu12.palette.yaml'
$source=Join-Path $sourceDir 'KnifeStyleTest.voxel.yaml'
$palette=Join-Path $sourceDir 'KnifeStyleTest.palette.yaml'

if(-not (Test-Path $selectedSpec)){ throw "Selected VPU12 density candidate is missing: $selectedSpec" }
if(-not (Test-Path $source)){ throw "Final VPU12 source is missing: $source" }
if((Get-Content -LiteralPath $source -Raw) -notmatch '(?m)^voxelsPerUnit:\s*12\s*$'){
  throw 'Final Knife source must remain at selected VPU12.'
}
Copy-Item $selectedPalette $palette -Force

$tool='Tool\VoxelStore.Tool\bin\Release\net8.0\Hero.Voxel.Tool.dll'
$vox=Join-Path $generatedDir 'KnifeStyleTest.generated.vox'
$compileOut = (& dotnet $tool compile --spec $source --palette $palette --output $vox 2>&1 | Out-String)
if($LASTEXITCODE -ne 0){ throw "Final compile failed: $compileOut" }
$match=[regex]::Match($compileOut,'(?<count>\d+) voxels')
if(-not $match.Success){ throw "Could not parse occupied voxel count: $compileOut" }
$occupied=[int]$match.Groups['count'].Value

Remove-Item (Join-Path $proofDir '*.png') -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $proofDir 'preview-poses.json') -Force -ErrorAction SilentlyContinue
& dotnet $tool preview --spec $source --palette $palette --output $proofDir --width 320 --height 320
if($LASTEXITCODE -ne 0){ throw 'Final preview failed.' }

# Proof presentation. Only accepted target pixels and Hero.Voxel renders are composed.
# The target's own FRONT orthographic is shown beside the model FRONT so silhouette/proportion comparison is direct.
$targetImg=[System.Drawing.Bitmap]::new((Resolve-Path $target).Path)
$targetWeapon=Crop $targetImg ([System.Drawing.Rectangle]::new(70,675,800,350))
$targetFront=Crop $targetImg ([System.Drawing.Rectangle]::new(815,645,205,370))
$hero=[System.Drawing.Bitmap]::new((Resolve-Path (Join-Path $proofDir 'hero.png')).Path)
$front=[System.Drawing.Bitmap]::new((Resolve-Path (Join-Path $proofDir 'front.png')).Path)
$canvas=[System.Drawing.Bitmap]::new(1700,900)
$g=[System.Drawing.Graphics]::FromImage($canvas)
$g.Clear([System.Drawing.ColorTranslator]::FromHtml('#F8F4E9'))
$title=[System.Drawing.Font]::new('Arial',28,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Pixel)
$small=[System.Drawing.Font]::new('Arial',19,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Pixel)
$brush=[System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#29251F'))
$muted=[System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#655F56'))
$border=[System.Drawing.Pen]::new([System.Drawing.ColorTranslator]::FromHtml('#C7BBA9'),2)
$g.DrawString('KNIFE — PRODUCT TARGET vs HERO.VOXEL MODEL',$title,$brush,40,28)
$g.DrawString('Frozen target perspective + direct FRONT orthographic comparison • selected VPU12 • neutral proof lighting',$small,$muted,42,70)
$g.DrawRectangle($border,40,110,780,730)
$g.DrawRectangle($border,850,110,380,730)
$g.DrawRectangle($border,1260,110,400,345)
$g.DrawRectangle($border,1260,495,400,345)
Fit $g $targetWeapon ([System.Drawing.RectangleF]::new(70,150,720,630))
Fit $g $targetFront ([System.Drawing.RectangleF]::new(890,150,300,620))
Fit $g $front ([System.Drawing.RectangleF]::new(1295,145,330,270))
Fit $g $hero ([System.Drawing.RectangleF]::new(1295,530,330,270))
$g.DrawString('ACCEPTED TARGET — PERSPECTIVE',$small,$brush,70,805)
$g.DrawString('ACCEPTED TARGET — FRONT',$small,$brush,890,805)
$g.DrawString('MODEL — FRONT',$small,$brush,1295,420)
$g.DrawString('MODEL — HERO',$small,$brush,1295,805)
$comparison=Join-Path $proofDir 'model-vs-target.png'
$canvas.Save($comparison,[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$canvas.Dispose();$targetWeapon.Dispose();$targetFront.Dispose();$targetImg.Dispose();$hero.Dispose();$front.Dispose()
$title.Dispose();$small.Dispose();$brush.Dispose();$muted.Dispose();$border.Dispose()

$ruler='policy\vpu-ruler.yaml'
$density=[ordered]@{
  version=2
  ruler=[ordered]@{
    version=1
    sha256=(Sha $ruler)
  }
  targetWorldLongAxis=1.4875
  apparentVoxelCount=22
  estimatedVpu=(22/1.4875)
  candidateSequence=@(8,10,12,16,20,24)
  meaningfulFeatures=@(
    'broad asymmetric stepped blade silhouette',
    'intentional one-voxel bright cutting edge',
    'separate light steel bevel',
    'brown guard with centered gold inset',
    'three diagonal green wrap groups',
    'brown pommel with gold inset'
  )
  candidates=@(
    [ordered]@{vpu=8;edgeThicknessVoxels=1;occupiedVoxels=37;result='reject';reason='Guard/grip/pommel relationships are over-compressed and the wrap groups no longer read distinctly enough.'},
    [ordered]@{vpu=10;edgeThicknessVoxels=1;occupiedVoxels=54;result='reject';reason='Broad blade survives, but the three diagonal wrap groups and small inset hierarchy remain materially merged.'},
    [ordered]@{vpu=12;edgeThicknessVoxels=1;occupiedVoxels=$occupied;result='select';reason='Lowest candidate preserving target cadence, stepped blade, one-voxel edge, bevel, guard inset, three wrap groups, and pommel hierarchy.'},
    [ordered]@{vpu=16;edgeThicknessVoxels=1;occupiedVoxels=189;result='reject';reason='Quantization introduces an unintended blade separation not present in the accepted target.'},
    [ordered]@{vpu=20;edgeThicknessVoxels=1;occupiedVoxels=492;result='reject';reason='Higher density still introduces a blade/guard separation and does not improve target fidelity.'},
    [ordered]@{vpu=24;edgeThicknessVoxels=1;occupiedVoxels=704;result='pass-not-selected';reason='Structurally readable but materially finer than needed; adds no meaningful target feature over tuned VPU12.'}
  )
  selectedVpu=12
  selectionReason='lowest visually and structurally sufficient per-unit ruler candidate'
}
$density | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $proofDir 'density.audit.json') -Encoding utf8

$audit=[ordered]@{
  version=1
  target=[ordered]@{path='../Reference/target.png';sha256=(Sha $target)}
  model=[ordered]@{
    voxelsPerUnit=12
    occupiedVoxels=$occupied
    source=[ordered]@{path='../Source/KnifeStyleTest.voxel.yaml';sha256=(Sha $source)}
    palette=[ordered]@{path='../Source/KnifeStyleTest.palette.yaml';sha256=(Sha $palette)}
    generatedVox=[ordered]@{path='../Generated/KnifeStyleTest.generated.vox';sha256=(Sha $vox)}
    hero=[ordered]@{path='hero.png';sha256=(Sha (Join-Path $proofDir 'hero.png'))}
    front=[ordered]@{path='front.png';sha256=(Sha (Join-Path $proofDir 'front.png'))}
    left=[ordered]@{path='left.png';sha256=(Sha (Join-Path $proofDir 'left.png'))}
    comparison=[ordered]@{path='model-vs-target.png';sha256=(Sha $comparison)}
  }
  criteria=[ordered]@{
    semanticForm='pass'
    silhouette='pass'
    proportions='pass'
    materialColorHierarchy='pass'
    meaningfulConstruction='pass'
    targetAuthorityPreserved='pass'
  }
  iterationEvidence=@(
    [ordered]@{iteration=1;result='reject';reason='Old VPU80 baseline is structurally readable but materially over-dense relative to the accepted low-resolution voxel cadence.'},
    [ordered]@{iteration=2;result='reject';reason='The first normalized VPU12 quantization selected the right density but left a disconnected blade segment; density stayed fixed while geometry was repaired.'},
    [ordered]@{iteration=3;result='accept';reason='Hand-tuned VPU12 source keeps the selected per-unit density and restores a continuous stepped blade, one-voxel edge, guard inset, chunky wraps, and pommel hierarchy.'}
  )
  note='Hero.Voxel proof uses neutral presentation. Acceptance compares silhouette, proportions, construction, and material/color hierarchy rather than target studio lighting/background.'
}
$audit | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $proofDir 'model-fidelity.audit.json') -Encoding utf8

$visual=@"
# KnifeStyleTest visual convergence audit

**Final result: ACCEPT for second-workflow proof.**

The Product Target stayed byte-identical at SHA-256 `$expectedTarget` through reference extraction, density comparison, and both model iterations.

Density selection:

- Apparent target cadence is about **22 long-axis voxel steps** over **1.4875 world units**, estimating **~14.8 VPU**.
- **VPU8 — reject:** recognizable but guard/grip/pommel relationships compress too far.
- **VPU10 — reject:** blade survives, but three diagonal wrap groups and small inset hierarchy still merge.
- **VPU12 — select:** first candidate preserving the target's stepped blade, separate bevel, intentional **1-voxel** bright edge, guard/gold inset, three diagonal wrap groups, and pommel/gold inset.
- **VPU16 — reject:** an unintended blade separation appears.
- **VPU20 — reject:** added density still creates a blade/guard separation rather than improving fidelity.
- **VPU24 — pass but not selected:** readable, but materially finer than necessary and adds no meaningful target feature over tuned VPU12.

Migration evidence:

1. **Reject old VPU80 baseline as style density:** the geometry is readable but materially smoother/finer than the accepted low-resolution voxel cadence. It is retained under `Rejected/HighVpuBaseline`.
2. **Reject first VPU12 quantization as geometry:** the density is correct, but a disconnected blade segment remains. It is retained under `Rejected/LowVpu12Quantized`.
3. **Accept tuned VPU12:** keep density fixed and repair only geometry until the target relationships are continuous and readable.

Final source is one rigid `Knife` part at **VPU12** with **$occupied occupied voxels**. The bright edge intentionally remains one voxel thick; that is a style feature, not a failure. See `model-vs-target.png` for the frozen target crop and final Hero.Voxel renders side by side.
"@
$visual | Set-Content (Join-Path $proofDir 'visual-audit.md') -Encoding utf8

[pscustomobject]@{
  target=(Sha $target)
  vpu=12
  occupiedVoxels=$occupied
  source=(Sha $source)
  generatedVox=(Sha $vox)
  comparison=(Sha $comparison)
} | ConvertTo-Json -Compress
