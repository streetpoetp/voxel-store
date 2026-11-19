# Dagger02 density candidate visual audit

All candidates use the same normalized Dagger construction and palette; only per-unit VPU quantization changes.

- **VPU 8 — REJECT:** recognizable, but the short blade/guard/grip cadence is too compressed and the intended close-combat construction hierarchy becomes blocky.
- **VPU 10 — PASS / SELECT:** first candidate preserving the broad stepped blade, cyan channel, purple spine, cyan guard accents, broad tip step, grip bands, and exact 1.2-unit long axis. Thin accents intentionally remain one voxel.
- **VPU 12 — PASS:** remains readable, but its quantized long axis is 1.166667 units and it adds no required target feature over VPU10.
- **VPU 16/20/24 — NOT NEEDED:** added density does not justify a higher selected VPU.

Expected selection: **VPU 10**, independently derived for Dagger02 from its own cadence evidence.
