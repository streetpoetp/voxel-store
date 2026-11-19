from __future__ import annotations
import json
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "Source" / "HighResolutionStaffProof.voxel.yaml"
METRICS = ROOT / "Proof" / "metrics.generated.json"
VPU = 64

boxes: list[tuple[tuple[int,int,int], tuple[int,int,int], str]] = []

def add_box(x:int, y:int, z:int, sx:int, sy:int, sz:int, color:str) -> None:
    if min(x, y, z) < 0 or min(sx, sy, sz) <= 0:
        raise ValueError((x,y,z,sx,sy,sz,color))
    if x + sx > 256 or y + sy > 256 or z + sz > 256:
        raise ValueError("Hero.Voxel 256-axis limit exceeded")
    boxes.append(((x,y,z),(sx,sy,sz),color))

def add_centered(cx:int, cy:int, cz:int, sx:int, sy:int, sz:int, color:str) -> None:
    add_box(cx - sx//2, cy - sy//2, cz - sz//2, sx, sy, sz, color)

# Base crystal + hardware.
add_box(9,9,0,6,6,4,"cyan.dark")
add_box(10,8,1,4,2,2,"cyan.mid")
add_box(10,14,1,4,2,2,"cyan.light")
add_box(8,8,4,8,8,4,"gold.dark")
add_box(9,8,5,6,2,2,"gold.light")

# Stepped, slightly bent shaft. The successive center shifts give the staff a
# product-style hand-made silhouette instead of one straight cuboid.
shaft = [
    (10,10,8,4,4,14,"wood.dark"),
    (10,9,11,4,1,7,"wood.mid"),
    (8,8,22,8,8,4,"gold.dark"),
    (9,8,23,6,2,2,"gold.light"),
    (9,10,26,5,4,12,"wood.mid"),
    (9,9,29,5,1,7,"wood.light"),
    (7,8,38,8,8,4,"gold.mid"),
    (8,8,39,6,2,2,"gold.light"),
    (8,9,42,5,5,11,"wood.dark"),
    (7,9,52,5,5,8,"wood.mid"),
    (6,8,58,7,7,5,"gold.dark"),
    (7,8,59,5,2,3,"gold.light"),
    (6,9,63,5,5,9,"wood.dark"),
]
for b in shaft:
    add_box(*b)

# Filled stepped cyan orb. Each y/z scanline becomes one x-run, yielding a
# true sphere-like voxel volume rather than nested rectangular slabs.
cx, cy, cz, radius = 12, 12, 81, 9
for dz in range(-radius, radius + 1):
    for dy in range(-radius, radius + 1):
        rem = radius * radius - dz * dz - dy * dy
        if rem < 0:
            continue
        xr = int(math.floor(math.sqrt(rem)))
        x0 = cx - xr
        length = xr * 2 + 1
        if dz >= 4 or dy <= -5:
            color = "cyan.light"
        elif dz <= -5 or dy >= 6:
            color = "cyan.dark"
        else:
            color = "cyan.mid"
        add_box(x0, cy + dy, cz + dz, length, 1, 1, color)

# Secondary facets on the crystal.
add_box(7,4,78,8,3,5,"cyan.light")
add_box(5,11,79,3,4,5,"cyan.dark")
add_box(16,10,80,3,5,4,"cyan.light")
add_box(9,18,81,6,2,4,"cyan.dark")

# Curved wood supports behind the crystal.
for i, deg in enumerate(range(205, 326, 20)):
    a = math.radians(deg)
    px = int(round(cx + math.cos(a) * 10))
    pz = int(round(cz + math.sin(a) * 10))
    py = 16 + (i % 2)
    add_centered(px, py, pz, 4, 4, 4, "wood.dark" if i % 2 == 0 else "wood.mid")

# Two crossing cage rings. Small 3-voxel segments make the curve read as an
# intentionally stepped voxel arc at VPU64.
ring_colors = ["purple.dark", "purple.mid", "purple.light", "purple.mid"]
for i, deg in enumerate(range(0, 360, 15)):
    a = math.radians(deg)
    px = int(round(cx + math.cos(a) * 10))
    pz = int(round(cz + math.sin(a) * 10))
    add_centered(px, cy, pz, 3, 5, 3, ring_colors[i % len(ring_colors)])

for i, deg in enumerate(range(0, 360, 15)):
    a = math.radians(deg)
    py = int(round(cy + math.cos(a) * 10))
    pz = int(round(cz + math.sin(a) * 10))
    add_centered(cx, py, pz, 5, 3, 3, ring_colors[(i + 1) % len(ring_colors)])

# Gold structural nodes at cardinal and diagonal points.
for deg in (0, 90, 180, 270):
    a = math.radians(deg)
    add_centered(int(round(cx + math.cos(a)*10)), cy, int(round(cz + math.sin(a)*10)),
                 5, 6, 5, "gold.mid")
for deg in (45, 135, 225, 315):
    a = math.radians(deg)
    add_centered(cx, int(round(cy + math.cos(a)*10)), int(round(cz + math.sin(a)*10)),
                 6, 5, 5, "gold.dark")

# Highlight faces on selected gold nodes.
add_box(20,10,80,2,4,3,"gold.light")
add_box(2,10,80,2,4,3,"gold.light")
add_box(10,2,80,4,2,3,"gold.light")
add_box(10,20,80,4,2,3,"gold.light")

# Crown and lower retaining claws.
add_box(9,8,92,6,8,4,"purple.dark")
add_box(10,7,93,4,2,3,"purple.light")
add_box(10,15,93,4,2,3,"purple.mid")
add_box(4,9,70,4,6,4,"purple.dark")
add_box(16,9,70,4,6,4,"purple.mid")
add_box(8,4,71,8,4,3,"gold.dark")
add_box(8,16,71,8,4,3,"gold.mid")

# Floating accent cubes from the visual target.
add_box(0,3,85,3,3,3,"cyan.mid")
add_box(21,4,88,3,3,3,"cyan.light")
add_box(1,19,91,3,3,3,"purple.light")
add_box(20,18,70,3,3,3,"gold.mid")
add_box(11,11,94,2,2,2,"purple.light")

# Rasterize in source order so later boxes overwrite earlier colors exactly
# like VoxelSpecCompiler.
voxels: dict[tuple[int,int,int], str] = {}
for (x,y,z),(sx,sy,sz),color in boxes:
    for zz in range(z,z+sz):
        for yy in range(y,y+sy):
            for xx in range(x,x+sx):
                voxels[(xx,yy,zz)] = color

mins = [min(p[i] for p in voxels) for i in range(3)]
maxs = [max(p[i] for p in voxels) for i in range(3)]
sizes = [maxs[i] - mins[i] + 1 for i in range(3)]

# Naive exposed colored faces.
dirs = ((1,0,0),(-1,0,0),(0,1,0),(0,-1,0),(0,0,1),(0,0,-1))
naive_faces = 0
for x,y,z in voxels:
    for dx,dy,dz in dirs:
        if (x+dx,y+dy,z+dz) not in voxels:
            naive_faces += 1

# Greedy rectangle merging equivalent to Hero.Voxel's axis masks: merge only
# contiguous faces with the same color and normal on each slice.
def greedy_quads() -> int:
    dims = [maxs[i] + 1 for i in range(3)]
    total = 0
    for d in range(3):
        u = (d + 1) % 3
        v = (d + 2) % 3
        for plane in range(-1, dims[d]):
            mask: dict[tuple[int,int], tuple[str,int]] = {}
            for vv in range(dims[v]):
                for uu in range(dims[u]):
                    a = [0,0,0]
                    b = [0,0,0]
                    a[d] = plane
                    b[d] = plane + 1
                    a[u] = b[u] = uu
                    a[v] = b[v] = vv
                    ca = voxels.get(tuple(a))
                    cb = voxels.get(tuple(b))
                    if (ca is None) == (cb is None):
                        continue
                    if ca is not None:
                        mask[(uu,vv)] = (ca,1)
                    else:
                        mask[(uu,vv)] = (cb,-1)
            visited:set[tuple[int,int]] = set()
            for vv in range(dims[v]):
                uu = 0
                while uu < dims[u]:
                    key=(uu,vv)
                    if key not in mask or key in visited:
                        uu += 1
                        continue
                    value=mask[key]
                    w=1
                    while uu+w < dims[u] and (uu+w,vv) in mask and (uu+w,vv) not in visited and mask[(uu+w,vv)] == value:
                        w += 1
                    h=1
                    while vv+h < dims[v]:
                        if any((uu+k,vv+h) not in mask or (uu+k,vv+h) in visited or mask[(uu+k,vv+h)] != value for k in range(w)):
                            break
                        h += 1
                    for yy in range(vv,vv+h):
                        for xx in range(uu,uu+w):
                            visited.add((xx,yy))
                    total += 1
                    uu += w
    return total

quads = greedy_quads()

lines = [
    "name: HighResolutionStaffProof",
    f"voxelsPerUnit: {VPU}",
    "assemblyOrigin:",
    "  part: Staff",
    "parts:",
    "  - part: Staff",
    "    pivot: [12, 12, 12]",
    "    voxels:",
]
for (x,y,z),(sx,sy,sz),color in boxes:
    lines.append(f"      - box: {{ min: [{x}, {y}, {z}], size: [{sx}, {sy}, {sz}], color: {color} }}")
SOURCE.parent.mkdir(parents=True, exist_ok=True)
SOURCE.write_text("\n".join(lines) + "\n", encoding="utf-8")

metrics = {
    "version": 1,
    "voxelsPerUnit": VPU,
    "primitiveBoxes": len(boxes),
    "occupiedVoxels": len(voxels),
    "integerBounds": {"min": mins, "max": maxs, "size": sizes},
    "worldBounds": {"size": [round(s / VPU, 6) for s in sizes]},
    "naiveExposedFaces": naive_faces,
    "greedyMergedQuads": quads,
    "vertices": quads * 4,
    "triangles": quads * 2,
    "sourceVoxelBudget": 65536,
    "withinSourceVoxelBudget": len(voxels) <= 65536,
    "normalizedLength": round(sizes[2] / VPU, 6),
}
METRICS.parent.mkdir(parents=True, exist_ok=True)
METRICS.write_text(json.dumps(metrics, indent=2) + "\n", encoding="utf-8")
print(json.dumps(metrics))
