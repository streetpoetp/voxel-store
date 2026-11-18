from __future__ import annotations
import json, math, re, sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = next(p for p in [HERE, *HERE.parents] if (p / "policy" / "store.yaml").exists())
BASE = ROOT / "Art" / "Samples" / "HighResolutionStaffProof" / "Source" / "HighResolutionStaffProof.voxel.yaml"
BOX_RE = re.compile(r'^\s*- box: \{ min: \[(\d+), (\d+), (\d+)\], size: \[(\d+), (\d+), (\d+)\], color: ([^ }]+) \}\s*$')

def scale_int(value:int, factor:float) -> int:
    return int(round(value * factor))

def parse_base():
    boxes=[]
    for line in BASE.read_text(encoding="utf-8").splitlines():
        m=BOX_RE.match(line)
        if m:
            nums=list(map(int,m.groups()[:6]))
            boxes.append((tuple(nums[:3]),tuple(nums[3:]),m.group(7)))
    if not boxes:
        raise RuntimeError("no base boxes parsed")
    return boxes

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
    if vpu<=0: raise ValueError(vpu)
    factor=vpu/64.0
    scaled=[]
    for mn,sz,color in parse_base():
        smn=tuple(scale_int(v,factor) for v in mn)
        ssz=tuple(max(1,scale_int(v,factor)) for v in sz)
        scaled.append((smn,ssz,color))
    out=HERE/"Candidates"/f"vpu-{vpu:03d}"
    src=out/"Source"/"candidate.voxel.yaml"
    metrics_path=out/"metrics.json"
    src.parent.mkdir(parents=True,exist_ok=True)
    lines=[
        f"name: StaffDensityCandidate{vpu:03d}",
        f"voxelsPerUnit: {vpu}",
        "assemblyOrigin:",
        "  part: Staff",
        "parts:",
        "  - part: Staff",
        f"    pivot: [{scale_int(12,factor)}, {scale_int(12,factor)}, {scale_int(12,factor)}]",
        "    voxels:",
    ]
    for (x,y,z),(sx,sy,sz),color in scaled:
        lines.append(f"      - box: {{ min: [{x}, {y}, {z}], size: [{sx}, {sy}, {sz}], color: {color} }}")
    src.write_text("\n".join(lines)+"\n",encoding="utf-8")

    vox={}
    for (x,y,z),(sx,sy,sz),color in scaled:
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
    }
    metrics_path.write_text(json.dumps(metrics,indent=2)+"\n",encoding="utf-8")
    print(json.dumps(metrics))

if __name__=="__main__":
    for arg in sys.argv[1:]:
        generate(int(arg))
