# Dagger02 model visual audit

**PASS** against the accepted Product Target after per-unit VPU migration.

The canonical model now uses **VPU10**, independently selected from Dagger02's own cadence evidence. It preserves the short broad blade, stepped taper, compact purple/gold guard, dark grip, cyan channel, purple reinforced edge, gold transitions, and restrained blue accents. Its authored voxel orientation is normalized to the shared Unity model frame (`+X` right, `+Y` up, `-Z` forward), matching avatars without runtime corrective rotation.

VPU8 was rejected because the short blade/guard/grip cadence compressed too far. VPU10 is the first sufficient candidate, matches the intended 1.2-unit long axis exactly, and intentionally retains one-voxel cyan/spine/guard details. Higher candidates add density without a required target gain.

Canonical source contains **43 occupied voxels** in one rigid `Dagger` part, down from the previous VPU80 / 11,480-voxel model. Exact target/source/palette/generated-VOX/standalone/part-render hashes are sealed in `model-fidelity.audit.json`; the store proof audit independently binds canonical source and Hero.Voxel proof inputs.
