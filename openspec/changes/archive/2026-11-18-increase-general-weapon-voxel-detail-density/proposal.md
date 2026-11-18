# Proposal

## Why

The general weapon catalog is structurally correct and readable, but most non-bow weapons are visually under-detailed compared with the existing low-resolution voxel style's available construction budget. The current models often stop at primary silhouettes with only four to six authored geometry regions, leaving weak secondary construction detail such as taper, thickness variation, grip shaping, material breaks, and stepped voxel contours.

## What Changes

- Strengthen the active low-resolution equipment style grammar so equipment preserves chunky low-resolution readability while also expressing purposeful secondary construction detail.
- Refine the ten general weapon reference packages before source changes, keeping each weapon's current semantic identity, `form`, rigid-part ownership, and hand-neutral `Hand` capability unchanged.
- Re-author weapon VoxelSpec geometry to match the accepted richer references using silhouette shaping, taper, thickness variation, stepped/bevel-like contours, grip construction, material separation, and restrained secondary accents.
- Use Bow01 as the existing detail-density benchmark rather than increasing global voxel resolution or introducing a new compatibility profile.
- Preserve `voxelsPerUnit: 8`, `humanoid.medium.v2`, `Hand.0` / `Hand.1`, current coverage identities, publication semantics, and gameplay-neutral metadata.
- Do not introduce a mechanical minimum box/voxel count; visual usefulness and reference fidelity remain the acceptance criteria.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `reference-style-authority`: strengthen the low-resolution equipment style requirement from merely restrained detail density to readable secondary construction detail while preserving chunky voxel readability and preventing realistic/noisy over-detailing.

## Impact

- Equipment style authority under `Art/Style/low-resolution-voxel.v1/`.
- Canonical `Reference/`, `Source/`, generated Hero.Voxel siblings, standalone proof, and compatibility evidence for the ten general weapons.
- Existing asset identities, forms, compatibility contracts, coverage policy, avatar rig, and consuming-game semantics remain unchanged.
- Existing reference-first publication and one-asset-at-a-time maintenance rules remain authoritative and are reused rather than redesigned.
