# Dagger02 density candidate visual audit

All candidates preserve the same normalized Dagger construction and palette; only VPU quantization changes.

- **VPU 64 — REJECT:** the overall close-combat silhouette remains readable, but the cyan energy channel is only 2 voxels thick and therefore fails its accepted 3-voxel structural requirement. Its quantized long axis also lands at 1.203125 rather than the declared 1.2 units.
- **VPU 80 — PASS:** every declared meaningful feature is readable, the cyan channel reaches 3 voxels, all hard bounds/budgets pass, and the normalized long axis is exactly 1.2 units.
- **VPU 96 — visual features PASS but hard normalized-size check fails:** it adds no required feature beyond VPU80 and quantizes to 1.197917 units rather than exactly 1.2.

Expected selection: **VPU 80**.
