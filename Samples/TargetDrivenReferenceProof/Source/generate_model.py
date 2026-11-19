from __future__ import annotations

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "Source" / "TargetDrivenBowProof.voxel.yaml"
METRICS = ROOT / "Proof" / "model.metrics.generated.json"
VPU = 64

boxes: list[tuple[tuple[int, int, int], tuple[int, int, int], str]] = []


def add_box(x: int, y: int, z: int, sx: int, sy: int, sz: int, color: str) -> None:
    if min(x, y, z) < 0 or min(sx, sy, sz) <= 0:
        raise ValueError((x, y, z, sx, sy, sz, color))
    if x + sx > 256 or y + sy > 256 or z + sz > 256:
        raise ValueError("Hero.Voxel 256-axis limit exceeded")
    boxes.append(((x, y, z), (sx, sy, sz), color))


def add_limb_segment(z: int, length: int, x: int, width: int, color: str) -> None:
    add_box(x, 2, z, width, 7, length, color)
    add_box(x + 1, 1, z, max(2, width - 2), 1, length, "wood.light")
    add_box(x + width - 2, 8, z, 2, 1, length, "wood.dark")


# One half of the target silhouette, mirrored around the center grip.
# The shallow x progression keeps the bow broad and chunky like target.png.
half_segments = [
    (0, 6, 22, 8, "wood.dark"),
    (6, 6, 18, 8, "wood.mid"),
    (12, 6, 15, 8, "wrap.green"),
    (18, 7, 12, 8, "wood.mid"),
    (25, 7, 9, 9, "wood.mid"),
    (32, 6, 6, 9, "wood.dark"),
    (38, 4, 4, 10, "wrap.green"),
]

for z, length, x, width, color in half_segments:
    add_limb_segment(z, length, x, width, color)
    mirror_z = 96 - (z + length)
    add_limb_segment(mirror_z, length, x, width, color)

# Green collars have their own lighter front accents.
for z in (12, 38, 54, 78):
    x = 15 if z in (12, 78) else 4
    add_box(x + 1, 0, z + 1, 5, 1, 4, "wrap.light")

# Broad dark center grip, with brown shoulder blocks just behind the collars.
add_box(2, 0, 42, 12, 10, 12, "grip.dark")
add_box(4, 1, 42, 8, 1, 12, "wood.dark")
add_box(3, 0, 42, 3, 10, 12, "grip.dark")

# Squared nocks and reinforced tips.
add_box(24, 1, 0, 6, 9, 5, "wood.dark")
add_box(24, 1, 91, 6, 9, 5, "wood.dark")
add_box(22, 0, 1, 5, 11, 3, "wood.mid")
add_box(22, 0, 92, 5, 11, 3, "wood.mid")

# Taut string. It intersects the reinforced nocks but remains clearly separated
# from the center grip, matching the target's shallow U silhouette.
add_box(28, 4, 2, 2, 2, 92, "string.light")

# Rasterize in source order, matching VoxelSpec overwrite semantics.
voxels: dict[tuple[int, int, int], str] = {}
for (x, y, z), (sx, sy, sz), color in boxes:
    for zz in range(z, z + sz):
        for yy in range(y, y + sy):
            for xx in range(x, x + sx):
                voxels[(xx, yy, zz)] = color

mins = [min(p[i] for p in voxels) for i in range(3)]
maxs = [max(p[i] for p in voxels) for i in range(3)]
sizes = [maxs[i] - mins[i] + 1 for i in range(3)]

lines = [
    "name: TargetDrivenBowProof",
    f"voxelsPerUnit: {VPU}",
    "assemblyOrigin:",
    "  part: Bow",
    "parts:",
    "  - part: Bow",
    "    pivot: [6, 5, 48]",
    "    voxels:",
]
for (x, y, z), (sx, sy, sz), color in boxes:
    lines.append(
        f"      - box: {{ min: [{x}, {y}, {z}], size: [{sx}, {sy}, {sz}], color: {color} }}"
    )

SOURCE.parent.mkdir(parents=True, exist_ok=True)
SOURCE.write_text("\n".join(lines) + "\n", encoding="utf-8")

metrics = {
    "version": 1,
    "voxelsPerUnit": VPU,
    "primitiveBoxes": len(boxes),
    "occupiedVoxels": len(voxels),
    "integerBounds": {"min": mins, "max": maxs, "size": sizes},
    "worldBounds": {"size": [round(s / VPU, 6) for s in sizes]},
    "sourceVoxelBudget": 65536,
    "withinSourceVoxelBudget": len(voxels) <= 65536,
    "normalizedLongAxis": round(max(sizes) / VPU, 6),
}
METRICS.parent.mkdir(parents=True, exist_ok=True)
METRICS.write_text(json.dumps(metrics, indent=2) + "\n", encoding="utf-8")
print(json.dumps(metrics))
