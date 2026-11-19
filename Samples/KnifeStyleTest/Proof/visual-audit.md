# KnifeStyleTest visual convergence audit

**Final result: ACCEPT for second-workflow proof.**

The Product Target stayed byte-identical at SHA-256 $expectedTarget through reference extraction, density comparison, and both model iterations.

Density selection:

- Apparent target cadence is about **22 long-axis voxel steps** over **1.4875 world units**, estimating **~14.8 VPU**.
- **VPU8 — reject:** recognizable but guard/grip/pommel relationships compress too far.
- **VPU10 — reject:** blade survives, but three diagonal wrap groups and small inset hierarchy still merge.
- **VPU12 — select:** first candidate preserving the target's stepped blade, separate bevel, intentional **1-voxel** bright edge, guard/gold inset, three diagonal wrap groups, and pommel/gold inset.
- **VPU16 — reject:** an unintended blade separation appears.
- **VPU20 — reject:** added density still creates a blade/guard separation rather than improving fidelity.
- **VPU24 — pass but not selected:** readable, but materially finer than necessary and adds no meaningful target feature over tuned VPU12.

Migration evidence:

1. **Reject old VPU80 baseline as style density:** the geometry is readable but materially smoother/finer than the accepted low-resolution voxel cadence. It is retained under Rejected/HighVpuBaseline.
2. **Reject first VPU12 quantization as geometry:** the density is correct, but a disconnected blade segment remains. It is retained under Rejected/LowVpu12Quantized.
3. **Accept tuned VPU12:** keep density fixed and repair only geometry until the target relationships are continuous and readable.

Final source is one rigid Knife part at **VPU12** with **215 occupied voxels**. The bright edge intentionally remains one voxel thick; that is a style feature, not a failure. See model-vs-target.png for the frozen target crop and final Hero.Voxel renders side by side.
