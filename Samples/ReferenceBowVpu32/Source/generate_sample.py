from __future__ import annotations
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "Source" / "ReferenceBowVpu32.voxel.yaml"
METRICS = ROOT / "Proof" / "metrics.generated.json"
VPU = 32

boxes: list[tuple[tuple[int,int,int], tuple[int,int,int], str]] = []

def add(x:int,y:int,z:int,sx:int,sy:int,sz:int,color:str) -> None:
    boxes.append(((x,y,z),(sx,sy,sz),color))

# Bow string: intentionally one voxel thick. The reference already depicts it
# as a thin line, so it must not drive the density of the whole weapon.
add(13, 1, 1, 1, 1, 30, "string.light")

# Lower recurve / tip. Coarse stepped modules follow the reference image.
add(10, 0, 0, 3, 3, 3, "wood.dark")
add(9, 0, 2, 3, 3, 3, "wood.mid")
add(8, 0, 4, 3, 3, 3, "wrap.green")
add(7, 0, 5, 3, 3, 4, "wood.mid")
add(6, 0, 8, 3, 3, 4, "wood.light")
add(5, 0, 10, 3, 3, 4, "wrap.green")
add(3, 0, 12, 4, 3, 4, "wood.mid")
add(1, 0, 14, 4, 3, 3, "wood.dark")

# Center grip / riser. Wider and darker like the isolated weapon reference.
add(0, 0, 14, 4, 4, 5, "grip.dark")
add(3, 0, 15, 4, 3, 3, "wood.mid")
add(5, 0, 15, 3, 3, 3, "wrap.green")
add(7, 0, 15, 2, 3, 3, "wood.light")

# Upper half mirrored with slight asymmetry from the product reference.
add(1, 0, 18, 4, 3, 3, "wood.dark")
add(3, 0, 19, 4, 3, 4, "wood.mid")
add(5, 0, 19, 3, 3, 4, "wrap.green")
add(6, 0, 21, 3, 3, 4, "wood.light")
add(7, 0, 23, 3, 3, 4, "wood.mid")
add(8, 0, 25, 3, 3, 3, "wrap.green")
add(9, 0, 27, 3, 3, 3, "wood.mid")
add(10, 0, 29, 3, 3, 3, "wood.dark")

# Squared nocks at the string endpoints.
add(11, 0, 0, 2, 4, 2, "wood.tip")
add(11, 0, 30, 2, 4, 2, "wood.tip")

# Small face highlights preserve the readable block hierarchy.
add(8, 3, 6, 2, 1, 2, "wood.highlight")
add(4, 3, 13, 2, 1, 2, "wood.highlight")
add(4, 3, 20, 2, 1, 2, "wood.highlight")
add(8, 3, 26, 2, 1, 2, "wood.highlight")

voxels: dict[tuple[int,int,int], str] = {}
for (x,y,z),(sx,sy,sz),color in boxes:
    for zz in range(z,z+sz):
        for yy in range(y,y+sy):
            for xx in range(x,x+sx):
                voxels[(xx,yy,zz)] = color

mins=[min(p[i] for p in voxels) for i in range(3)]
maxs=[max(p[i] for p in voxels) for i in range(3)]
sizes=[maxs[i]-mins[i]+1 for i in range(3)]

lines=[
    "name: ReferenceBowVpu32",
    f"voxelsPerUnit: {VPU}",
    "assemblyOrigin:",
    "  part: Bow",
    "parts:",
    "  - part: Bow",
    "    pivot: [2, 2, 16]",
    "    voxels:",
]
for (x,y,z),(sx,sy,sz),color in boxes:
    lines.append(f"      - box: {{ min: [{x}, {y}, {z}], size: [{sx}, {sy}, {sz}], color: {color} }}")

SOURCE.parent.mkdir(parents=True, exist_ok=True)
SOURCE.write_text("\n".join(lines)+"\n", encoding="utf-8")

metrics={
    "version":1,
    "voxelsPerUnit":VPU,
    "primitiveBoxes":len(boxes),
    "occupiedVoxels":len(voxels),
    "integerBounds":{"min":mins,"max":maxs,"size":sizes},
    "worldBounds":{"size":[round(s/VPU,6) for s in sizes]},
    "normalizedLongAxis":round(sizes[2]/VPU,6),
}
METRICS.parent.mkdir(parents=True, exist_ok=True)
METRICS.write_text(json.dumps(metrics,indent=2)+"\n",encoding="utf-8")
print(json.dumps(metrics))
