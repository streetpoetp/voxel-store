from __future__ import annotations
import json, sys
from pathlib import Path

HERE = Path(__file__).resolve().parent

def q(value: float, vpu: int, minimum: int = 1) -> int:
    return max(minimum, int(round(value * vpu)))

def add(boxes, x, y, z, sx, sy, sz, color):
    boxes.append(((x, y, z), (sx, sy, sz), color))

def build(vpu: int):
    boxes = []
    depth = q(0.18, vpu)
    handle_w = q(0.14, vpu)
    handle_x = q(0.78, vpu)
    handle_h = q(1.08, vpu)

    # Walnut haft and darker rear strip establish readable depth.
    add(boxes, handle_x, 0, q(0.08, vpu), handle_w, depth, handle_h, "walnut.mid")
    add(boxes, handle_x + max(0, handle_w - 1), 0, q(0.08, vpu), 1, depth, handle_h, "walnut.dark")

    # Teal grip and brass butt reinforcement are product-significant regions.
    grip_z = q(0.36, vpu)
    grip_h = q(0.24, vpu)
    add(boxes, handle_x - q(0.04, vpu), 0, grip_z, handle_w + q(0.08, vpu), depth, grip_h, "teal.mid")
    add(boxes, handle_x - q(0.05, vpu), 0, 0, handle_w + q(0.10, vpu), depth, q(0.10, vpu), "brass.mid")

    # Four stepped head bands preserve the accepted crescent silhouette.
    add(boxes, q(0.36, vpu), 0, q(1.28, vpu), q(0.72, vpu), depth, q(0.22, vpu), "steel.mid")
    add(boxes, q(0.22, vpu), 0, q(1.16, vpu), q(0.90, vpu), depth, q(0.20, vpu), "steel.mid")
    add(boxes, q(0.12, vpu), 0, q(1.02, vpu), q(0.98, vpu), depth, q(0.18, vpu), "steel.mid")
    add(boxes, q(0.25, vpu), 0, q(0.91, vpu), q(0.72, vpu), depth, q(0.14, vpu), "steel.mid")

    edge_w = q(0.10, vpu)
    add(boxes, q(0.22, vpu), 0, q(1.33, vpu), edge_w, depth, q(0.14, vpu), "edge.ivory")
    add(boxes, q(0.10, vpu), 0, q(1.18, vpu), edge_w, depth, q(0.18, vpu), "edge.ivory")
    add(boxes, q(0.03, vpu), 0, q(1.04, vpu), edge_w, depth, q(0.17, vpu), "edge.ivory")
    add(boxes, q(0.15, vpu), 0, q(0.92, vpu), edge_w, depth, q(0.14, vpu), "edge.ivory")
    add(boxes, q(0.30, vpu), 0, q(1.30, vpu), q(0.10, vpu), depth, q(0.13, vpu), "steel.light")
    add(boxes, q(0.18, vpu), 0, q(1.15, vpu), q(0.10, vpu), depth, q(0.14, vpu), "steel.light")

    # Compact rear poll is separated from the large crescent by a narrow neck.
    add(boxes, q(0.92, vpu), 0, q(1.08, vpu), q(0.24, vpu), depth, q(0.28, vpu), "steel.mid")
    add(boxes, q(1.13, vpu), 0, q(1.12, vpu), q(0.28, vpu), depth, q(0.24, vpu), "steel.dark")
    add(boxes, q(1.09, vpu), 0, q(1.15, vpu), q(0.24, vpu), depth, q(0.20, vpu), "steel.mid")
    add(boxes, q(1.09, vpu), 0, q(1.18, vpu), 1, depth, q(0.14, vpu), "steel.light")

    # Brass throat/socket bridges the head into the haft.
    add(boxes, q(0.70, vpu), 0, q(0.91, vpu), q(0.38, vpu), depth, q(0.22, vpu), "brass.dark")
    add(boxes, q(0.66, vpu), 0, q(0.95, vpu), q(0.38, vpu), depth, q(0.22, vpu), "brass.mid")
    add(boxes, q(0.68, vpu), 0, q(0.98, vpu), q(0.08, vpu), depth, q(0.16, vpu), "brass.light")
    add(boxes, q(0.81, vpu), 0, q(0.98, vpu), q(0.14, vpu), depth, q(0.15, vpu), "steel.dark")

    spans = {
        "crescent-edge-band": edge_w,
        "compact-poll-separation": q(0.08, vpu),
        "socket-thickness-transition": q(0.14, vpu),
        "teal-grip-band": grip_h,
        "butt-band-step": q(0.10, vpu),
    }
    return boxes, spans

def greedy_quads(voxels, mins, maxs):
    shifted = {(x-mins[0], y-mins[1], z-mins[2]): c for (x,y,z),c in voxels.items()}
    dims = [maxs[i]-mins[i]+1 for i in range(3)]
    total = 0
    for d in range(3):
        u, v = (d+1)%3, (d+2)%3
        for plane in range(-1, dims[d]):
            mask = {}
            for vv in range(dims[v]):
                for uu in range(dims[u]):
                    a=[0,0,0]; b=[0,0,0]
                    a[d]=plane; b[d]=plane+1; a[u]=b[u]=uu; a[v]=b[v]=vv
                    ca=shifted.get(tuple(a)); cb=shifted.get(tuple(b))
                    if (ca is None) == (cb is None):
                        continue
                    mask[(uu,vv)] = (ca,1) if ca is not None else (cb,-1)
            visited=set()
            for vv in range(dims[v]):
                uu=0
                while uu < dims[u]:
                    key=(uu,vv)
                    if key not in mask or key in visited:
                        uu += 1; continue
                    val=mask[key]; w=1
                    while uu+w < dims[u] and (uu+w,vv) in mask and (uu+w,vv) not in visited and mask[(uu+w,vv)] == val:
                        w += 1
                    h=1
                    while vv+h < dims[v] and all((uu+x,vv+h) in mask and (uu+x,vv+h) not in visited and mask[(uu+x,vv+h)] == val for x in range(w)):
                        h += 1
                    for yy in range(vv,vv+h):
                        for xx in range(uu,uu+w): visited.add((xx,yy))
                    total += 1; uu += w
    return total

def generate(vpu: int):
    boxes, spans = build(vpu)
    root = HERE / "Candidates" / f"vpu-{vpu:03d}"
    src = root / "Source" / "candidate.voxel.yaml"
    src.parent.mkdir(parents=True, exist_ok=True)
    lines=[f"name: Axe03DensityCandidate{vpu:03d}", f"voxelsPerUnit: {vpu}", "assemblyOrigin:", "  part: Axe", "parts:", "  - part: Axe", f"    pivot: [{q(0.84,vpu)}, {max(1,q(0.09,vpu))}, {q(0.48,vpu)}]", "    voxels:"]
    for (x,y,z),(sx,sy,sz),color in boxes:
        lines.append(f"      - box: {{ min: [{x}, {y}, {z}], size: [{sx}, {sy}, {sz}], color: {color} }}")
    src.write_text("\n".join(lines)+"\n", encoding="utf-8", newline="\n")
    vox={}
    for (x,y,z),(sx,sy,sz),color in boxes:
        for zz in range(z,z+sz):
            for yy in range(y,y+sy):
                for xx in range(x,x+sx): vox[(xx,yy,zz)] = color
    mins=[min(p[i] for p in vox) for i in range(3)]; maxs=[max(p[i] for p in vox) for i in range(3)]
    sizes=[maxs[i]-mins[i]+1 for i in range(3)]
    quads=greedy_quads(vox, mins, maxs)
    metrics={"vpu":vpu,"occupiedVoxels":len(vox),"integerBounds":sizes,"normalizedWorldSize":[round(x/vpu,6) for x in sizes],"greedyMergedQuads":quads,"vertices":quads*4,"triangles":quads*2,"axisCompliant":all(x<=256 for x in sizes),"featureVoxelSpans":spans}
    (root/"metrics.json").write_text(json.dumps(metrics,indent=2)+"\n",encoding="utf-8",newline="\n")
    print(json.dumps(metrics))

if __name__ == "__main__":
    for arg in sys.argv[1:]: generate(int(arg))
