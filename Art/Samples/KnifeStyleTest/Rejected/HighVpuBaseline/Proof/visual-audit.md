# KnifeStyleTest visual convergence audit

**Final result: ACCEPT for second-workflow proof.**

The Product Target stayed byte-identical at SHA-256 $expectedTarget through reference extraction, density comparison, and both model iterations.

Density selection:

- **VPU64 — reject:** 2-voxel bright cutting-edge band is below the three-voxel target-feature readability threshold.
- **VPU80 — select:** 3-voxel edge band, stepped blade, gold guard/pommel insets, and diagonal wraps all remain readable.
- **VPU96 — pass but not selected:** 4-voxel edge band adds density but no new meaningful target feature.

Model iteration:

1. **Reject** — first VPU80 interpretation preserved the broad knife silhouette but the green wraps read as horizontal rings and the blade had only a dark body plus bright edge.
2. **Accept** — second interpretation changes only model construction: three stepped diagonal wrap groups plus a separate light steel bevel beneath the bright edge. The broad asymmetric blade, brown/gold guard, brown grip core, and brown/gold pommel remain aligned to the target.

Final source is one rigid Knife part at **VPU80** with **24908 occupied voxels**. See model-vs-target.png for the frozen target crop and final Hero.Voxel renders side by side.
