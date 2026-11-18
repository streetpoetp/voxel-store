from __future__ import annotations
import json, sys
from pathlib import Path

HERE = Path(__file__).resolve().parent

def q(value: float, vpu: int, minimum: int = 1) -> int:
    return max(minimum, int(round(value * vpu)))

def add_box(boxes, x, y, z, sx, sy, sz, color):
    boxes.append(((x,y,z),(sx,sy,sz),color))

def build(vpu: int):
    long_axis = q(1.2, vpu)
    cx = q(0.24, vpu)
    depth = q(0.08, vpu)
    boxes=[]

    # Pommel and grip: normalized product intent, independent of candidate density.
    add_box(boxes, cx-q(0.10,vpu), 1, 0, q(0.20,vpu), depth, q(0.10,vpu), "purple.mid")
    add_box(boxes, cx-q(0.055,vpu), 0, q(0.025,vpu), q(0.11,vpu), 1, q(0.05,vpu), "cyan.mid")
    add_box(boxes, cx-q(0.075,vpu), 1, q(0.10,vpu), q(0.15,vpu), depth, q(0.25,vpu), "grip.dark")
    add_box(boxes, cx-q(0.075,vpu), 0, q(0.18,vpu), q(0.15,vpu), 1, q(0.035,vpu), "gold.mid")
    add_box(boxes, cx-q(0.075,vpu), 0, q(0.27,vpu), q(0.15,vpu), 1, q(0.035,vpu), "purple.mid")

    # Wide guard. Cyan blocks are construction-significant accents.
    guard_z=q(0.35,vpu)
    guard_w=q(0.44,vpu)
    guard_x=cx-guard_w//2
    add_box(boxes, guard_x, 1, guard_z, guard_w, depth, q(0.09,vpu), "purple.mid")
    add_box(boxes, guard_x, 0, guard_z+q(0.02,vpu), q(0.09,vpu), 1, q(0.05,vpu), "gold.mid")
    add_box(boxes, guard_x+guard_w-q(0.09,vpu), 0, guard_z+q(0.02,vpu), q(0.09,vpu), 1, q(0.05,vpu), "gold.mid")
    cyan_block=q(0.05,vpu)
    add_box(boxes, guard_x+q(0.055,vpu), 0, guard_z+q(0.045,vpu), cyan_block, 1, cyan_block, "cyan.mid")
    add_box(boxes, guard_x+guard_w-q(0.105,vpu), 0, guard_z+q(0.045,vpu), cyan_block, 1, cyan_block, "cyan.mid")

    # Broad blade with stepped taper. It reaches exactly the normalized 1.5-unit long axis.
    blade_z=q(0.40,vpu)
    blade_w=q(0.31,vpu)
    blade_x=cx-blade_w//2
    z1=q(0.80,vpu)
    z2=q(0.97,vpu)
    z3=q(1.10,vpu)
    add_box(boxes, blade_x, 1, blade_z, blade_w, depth, z1-blade_z, "blade.mid")
    w2=q(0.25,vpu); x2=cx-w2//2
    add_box(boxes, x2, 1, z1, w2, depth, z2-z1, "blade.mid")
    w3=q(0.19,vpu); x3=cx-w3//2
    add_box(boxes, x3, 1, z2, w3, depth, z3-z2, "blade.mid")
    w4=q(0.10,vpu); x4=cx-w4//2
    add_box(boxes, x4, 1, z3, w4, depth, long_axis-z3, "blade.light")

    # Reinforced spine and gold transition blocks.
    spine_w=q(0.04,vpu)
    add_box(boxes, blade_x, 0, blade_z+q(0.03,vpu), spine_w, 1, q(0.25,vpu), "purple.mid")
    add_box(boxes, blade_x, 0, blade_z+q(0.27,vpu), q(0.075,vpu), 1, q(0.09,vpu), "gold.mid")
    add_box(boxes, x2, 0, z1+q(0.03,vpu), q(0.06,vpu), 1, q(0.13,vpu), "purple.mid")
    add_box(boxes, x3, 0, z2+q(0.03,vpu), q(0.05,vpu), 1, q(0.07,vpu), "gold.mid")

    # Cyan energy channel: at VPU64 this quantizes to 2 voxels, at VPU80 to 3.
    channel_w=q(0.035,vpu)
    channel_x=cx-channel_w//2
    add_box(boxes, channel_x, 0, blade_z+q(0.16,vpu), channel_w, 1, q(0.38,vpu), "cyan.mid")

    # Restrained lower-face accents.
    add_box(boxes, cx-q(0.09,vpu), 0, blade_z+q(0.20,vpu), q(0.055,vpu), 1, q(0.08,vpu), "accent.blue")
    add_box(boxes, cx+q(0.045,vpu), 0, blade_z+q(0.42,vpu), q(0.05,vpu), 1, q(0.06,vpu), "accent.blue")

    return boxes, (cx, depth, long_axis), channel_w, spine_w, cyan_block

def greedy_quads(voxels, maxs):
    total=0
    dims=[maxs[i]+1 for i in range(3)]
    for d in range(3):
        u=(d+1)%3; v=(d+2)%3
        for plane in range(-1,dims[d]):
            mask={}
            for vv in range(dims[v]):
                for uu in range(dims[u]):
                    a=[0,0,0]; b=[0,0,0]
                    a[d]=plane; b[d]=plane+1
                    a[u]=b[u]=uu; a[v]=b[v]=vv
                    ca=voxels.get(tuple(a)); cb=voxels.get(tuple(b))
                    if (ca is None)==(cb is None): continue
                    mask[(uu,vv)] = (ca,1) if ca is not None else (cb,-1)
            visited=set()
            for vv in range(dims[v]):
                uu=0
                while uu<dims[u]:
                    k=(uu,vv)
                    if k not in mask or k in visited:
                        uu+=1; continue
                    val=mask[k]; w=1
                    while uu+w<dims[u] and (uu+w,vv) in mask and (uu+w,vv) not in visited and mask[(uu+w,vv)]==val:
                        w+=1
                    h=1
                    while vv+h<dims[v]:
                        if any((uu+x,vv+h) not in mask or (uu+x,vv+h) in visited or mask[(uu+x,vv+h)]!=val for x in range(w)):
                            break
                        h+=1
                    for yy in range(vv,vv+h):
                        for xx in range(uu,uu+w):
                            visited.add((xx,yy))
                    total+=1; uu+=w
    return total

def generate(vpu:int):
    boxes, (cx, depth, long_axis), channel_w, spine_w, cyan_block = build(vpu)
    out=HERE/"Candidates"/f"vpu-{vpu:03d}"
    src=out/"Source"/"candidate.voxel.yaml"
    src.parent.mkdir(parents=True,exist_ok=True)
    lines=[
        f"name: DaggerDensityCandidate{vpu:03d}",
        f"voxelsPerUnit: {vpu}",
        "assemblyOrigin:",
        "  part: Dagger",
        "parts:",
        "  - part: Dagger",
        f"    pivot: [{cx}, {max(1,depth//2)}, {q(0.22,vpu)}]",
        "    voxels:",
    ]
    for (x,y,z),(sx,sy,sz),color in boxes:
        lines.append(f"      - box: {{ min: [{x}, {y}, {z}], size: [{sx}, {sy}, {sz}], color: {color} }}")
    src.write_text("\n".join(lines)+"\n",encoding="utf-8",newline="\n")

    vox={}
    for (x,y,z),(sx,sy,sz),color in boxes:
        for zz in range(z,z+sz):
            for yy in range(y,y+sy):
                for xx in range(x,x+sx):
                    vox[(xx,yy,zz)] = color
    mins=[min(p[i] for p in vox) for i in range(3)]
    maxs=[max(p[i] for p in vox) for i in range(3)]
    sizes=[maxs[i]-mins[i]+1 for i in range(3)]
    quads=greedy_quads(vox,maxs)
    metrics={
      "vpu":vpu,
      "occupiedVoxels":len(vox),
      "integerBounds":sizes,
      "normalizedWorldSize":[round(x/vpu,6) for x in sizes],
      "greedyMergedQuads":quads,
      "vertices":quads*4,
      "triangles":quads*2,
      "axisCompliant":all(x<=256 for x in sizes),
      "featureVoxelSpans":{
        "energy-channel-thickness":channel_w,
        "purple-spine-thickness":spine_w,
        "guard-cyan-block":cyan_block,
        "broad-tip-step":q(0.10,vpu),
      },
    }
    (out/"metrics.json").write_text(json.dumps(metrics,indent=2)+"\n",encoding="utf-8",newline="\n")
    print(json.dumps(metrics))

if __name__=="__main__":
    for arg in sys.argv[1:]:
        generate(int(arg))
