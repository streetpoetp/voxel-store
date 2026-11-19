# Sword02 density candidate visual audit

All candidates use the same normalized Sword construction and palette; only per-unit VPU quantization changes.

- **VPU 8 — REJECT:** named regions remain recognizable, but the overall blade/guard/grip cadence is over-compressed relative to the accepted Sword reference.
- **VPU 10 — PASS / SELECT:** first candidate preserving the broad stepped blade, cyan energy channel, purple spine, cyan guard accents, tip step, grip bands, and exact 1.5-unit long axis. Meaningful thin accents intentionally remain one voxel.
- **VPU 12 — PASS:** readable but adds no required construction relationship missing at VPU10.
- **VPU 16/20/24 — NOT NEEDED:** progressively finer density does not recover a target feature absent at VPU10.

Expected selection: **VPU 10**, independently derived for Sword02 from its own cadence evidence.
