from __future__ import annotations
import hashlib, json
from pathlib import Path

HERE=Path(__file__).resolve().parent
REVIEW=HERE/"candidate-feature-review.json"

def sha256(path:Path)->str:
    return hashlib.sha256(path.read_bytes()).hexdigest()

review=json.loads(REVIEW.read_text(encoding="utf-8"))
for vpu_text,features in review["candidates"].items():
    vpu=int(vpu_text)
    root=HERE/"Candidates"/f"vpu-{vpu:03d}"
    metrics=json.loads((root/"metrics.json").read_text(encoding="utf-8"))
    audit={
      "version":1,
      "vpu":vpu,
      "features":{k:bool(v) for k,v in features.items()},
      "normalizedWorldSize":metrics["normalizedWorldSize"],
      "integerBounds":metrics["integerBounds"],
      "occupiedVoxels":metrics["occupiedVoxels"],
      "axisCompliant":metrics["axisCompliant"],
      "greedyMergedQuads":metrics["greedyMergedQuads"],
      "vertices":metrics["vertices"],
      "triangles":metrics["triangles"],
      "sourceSha256":sha256(root/"Source"/"candidate.voxel.yaml"),
      "previewSha256":sha256(root/"Proof"/"hero.png"),
    }
    (root/"audit.json").write_text(json.dumps(audit,indent=2)+"\n",encoding="utf-8",newline="\n")
