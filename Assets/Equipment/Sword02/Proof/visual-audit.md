# Sword02 model visual audit

**PASS** against the accepted Product Target after per-unit VPU migration.

The canonical model now uses **VPU10**, selected from Sword02's own cadence evidence rather than a shared equipment density. It preserves the broad stepped blade, tapered tip, dark grip, purple/gold guard hierarchy, cyan energy channel, purple reinforced regions, and restrained blue accents. Its authored voxel orientation is normalized to the shared Unity model frame (`+X` right, `+Y` up, `-Z` forward), matching avatars without runtime corrective rotation.

VPU8 was rejected because the overall blade/guard/grip cadence compressed too far. VPU10 is the first sufficient candidate and keeps meaningful cyan/spine/guard details as intentional **one-voxel** features. VPU12+ add density without recovering a required target relationship.

Canonical source contains **54 occupied voxels** in one rigid `Sword` part, down from the previous VPU80 / 14,016-voxel model. Exact target/source/palette/generated-VOX/standalone/part-render hashes are sealed in `model-fidelity.audit.json`; the store proof audit independently binds canonical source and Hero.Voxel proof renders.
