$ErrorActionPreference='Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$candidates = Join-Path $root 'Candidates'
Remove-Item $candidates -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $candidates | Out-Null

$palette = @'
version: 1
colors:
  wood.light: "#A8663DFF"
  wood.mid: "#7B4327FF"
  wood.dark: "#4C2B1AFF"
  wrap.light: "#9BD43BFF"
  wrap.green: "#5AA632FF"
  wrap.dark: "#2F6F2CFF"
  steel.dark: "#4D4D52FF"
  steel.mid: "#77777EFF"
  steel.light: "#B9B7BCFF"
  steel.edge: "#EEECE8FF"
  gold.mid: "#E4A62EFF"
  gold.light: "#FFD05AFF"
'@

$boxes = @(
  @{min=@(.3625,.025,0); size=@(.3000,.125,.1375); color='wood.dark'},
  @{min=@(.3875,.0125,.0125); size=@(.2500,.0125,.1000); color='wood.mid'},
  @{min=@(.4500,0,.0375); size=@(.1250,.025,.0750); color='gold.mid'},
  @{min=@(.4750,0,.0500); size=@(.0750,.0125,.0375); color='gold.light'},
  @{min=@(.4250,.0375,.1375); size=@(.1750,.0875,.5000); color='wood.dark'},
  @{min=@(.4375,.0250,.1500); size=@(.1500,.0125,.4750); color='wood.mid'},
  # Three stepped diagonal green wrap groups. Each group intentionally crosses the brown core instead of reading as a horizontal ring.
  @{min=@(.3875,.0250,.1750); size=@(.0875,.1125,.0500); color='wrap.green'},
  @{min=@(.4375,.0250,.2000); size=@(.0875,.1125,.0500); color='wrap.green'},
  @{min=@(.4875,.0250,.2250); size=@(.1125,.1125,.0500); color='wrap.light'},
  @{min=@(.4875,.0125,.2250); size=@(.0750,.0125,.0375); color='wrap.light'},

  @{min=@(.4875,.0250,.3125); size=@(.1125,.1125,.0500); color='wrap.light'},
  @{min=@(.4375,.0250,.3375); size=@(.0875,.1125,.0500); color='wrap.green'},
  @{min=@(.3875,.0250,.3625); size=@(.0875,.1125,.0500); color='wrap.green'},
  @{min=@(.4000,.0125,.3625); size=@(.0750,.0125,.0375); color='wrap.dark'},

  @{min=@(.3875,.0250,.4500); size=@(.0875,.1125,.0500); color='wrap.dark'},
  @{min=@(.4375,.0250,.4750); size=@(.0875,.1125,.0500); color='wrap.green'},
  @{min=@(.4875,.0250,.5000); size=@(.1125,.1125,.0500); color='wrap.green'},
  @{min=@(.5000,.0125,.5000); size=@(.0750,.0125,.0375); color='wrap.light'},
  @{min=@(.2500,.0250,.6375); size=@(.5625,.1250,.1250); color='wood.dark'},
  @{min=@(.2750,.0125,.6625); size=@(.5125,.0125,.0750); color='wood.mid'},
  @{min=@(.4625,0,.6750); size=@(.1250,.0250,.0750); color='gold.mid'},
  @{min=@(.5000,0,.6875); size=@(.0625,.0125,.0375); color='gold.light'},
  @{min=@(.3750,.0375,.7625); size=@(.4125,.0875,.2750); color='steel.dark'},
  @{min=@(.3875,.0375,1.0375); size=@(.3750,.0875,.2000); color='steel.mid'},
  @{min=@(.4375,.0375,1.2375); size=@(.3250,.0875,.1375); color='steel.mid'},
  @{min=@(.5000,.0375,1.3750); size=@(.2250,.0875,.1000); color='steel.light'},
  @{min=@(.3250,.0500,.8375); size=@(.0750,.0625,.1500); color='steel.dark'},
  @{min=@(.3500,.0500,1.0125); size=@(.0625,.0625,.1000); color='steel.dark'},

  # Light bevel band sits inside the bright cutting edge and preserves the target's layered steel hierarchy.
  @{min=@(.6875,.0375,.7875); size=@(.0625,.0875,.2500); color='steel.light'},
  @{min=@(.6625,.0375,1.0375); size=@(.0625,.0875,.2000); color='steel.light'},
  @{min=@(.6375,.0375,1.2375); size=@(.0625,.0875,.1375); color='steel.light'},
  @{min=@(.6000,.0375,1.3750); size=@(.0625,.0875,.0750); color='steel.light'},

  @{min=@(.7500,.0250,.7875); size=@(.0375,.1125,.2500); color='steel.edge'},
  @{min=@(.7250,.0250,1.0375); size=@(.0375,.1125,.2000); color='steel.edge'},
  @{min=@(.7000,.0250,1.2375); size=@(.0375,.1125,.1375); color='steel.edge'},
  @{min=@(.6625,.0250,1.3750); size=@(.0375,.1125,.0750); color='steel.edge'},
  @{min=@(.5625,.0250,1.4500); size=@(.1250,.1125,.0375); color='steel.edge'}
)

function Q([double]$v,[int]$vpu){ [int][Math]::Round($v*$vpu,[MidpointRounding]::AwayFromZero) }
function QS([double]$v,[int]$vpu){ [Math]::Max(1,(Q $v $vpu)) }

$summary=@()
foreach($vpu in @(8,10,12,16,20,24)){
  $dir=Join-Path $candidates ("Vpu{0}" -f $vpu)
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
  $spec=Join-Path $dir ("KnifeStyleTest.Vpu{0}.voxel.yaml" -f $vpu)
  $pal=Join-Path $dir ("KnifeStyleTest.Vpu{0}.palette.yaml" -f $vpu)
  $palette | Set-Content $pal -Encoding utf8

  $lines=[System.Collections.Generic.List[string]]::new()
  $lines.Add(("name: KnifeStyleTestVpu{0}" -f $vpu))
  $lines.Add(("voxelsPerUnit: {0}" -f $vpu))
  $lines.Add('assemblyOrigin:')
  $lines.Add('  part: Knife')
  $lines.Add('parts:')
  $lines.Add('  - part: Knife')
  $px=Q .525 $vpu; $py=Q .075 $vpu; $pz=Q .375 $vpu
  $lines.Add(("    pivot: [{0}, {1}, {2}]" -f $px,$py,$pz))
  $lines.Add('    voxels:')
  foreach($b in $boxes){
    $min=@((Q $b.min[0] $vpu),(Q $b.min[1] $vpu),(Q $b.min[2] $vpu))
    $size=@((QS $b.size[0] $vpu),(QS $b.size[1] $vpu),(QS $b.size[2] $vpu))
    $lines.Add(("      - box: {{ min: [{0}, {1}, {2}], size: [{3}, {4}, {5}], color: {6} }}" -f $min[0],$min[1],$min[2],$size[0],$size[1],$size[2],$b.color))
  }
  $lines | Set-Content $spec -Encoding utf8

  $edge=QS .0375 $vpu
  $summary += [pscustomobject]@{vpu=$vpu;edgeThicknessVoxels=$edge;spec=$spec;palette=$pal}
}
[pscustomobject]@{
  apparentVoxelCount=22
  targetWorldLongAxis=1.4875
  estimatedVpu=(22/1.4875)
  candidates=$summary
} | ConvertTo-Json -Depth 6
