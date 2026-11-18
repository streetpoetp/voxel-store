# Sword02 density candidate visual audit

All candidates use the same normalized construction and palette; only VPU quantization changes.

- **VPU 64 — REJECT:** overall silhouette, purple spine, guard blocks, and tip steps remain readable, but the cyan energy channel is only 2 voxels thick. The accepted density input requires 3 voxels for this secondary construction feature.
- **VPU 80 — PASS:** the cyan energy channel reaches 3 voxels while all other declared features remain readable. Bounds and occupied voxels remain within hard limits.
- **VPU 96 — PASS / completeness check:** all declared features remain readable, but it introduces no required structural feature missing at VPU 80.

Expected selection: **VPU 80**, the lowest evaluated candidate satisfying every declared feature and hard constraint.
