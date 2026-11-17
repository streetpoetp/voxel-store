# Proposal

## Why

Equipment already follows the canonical Hero.Voxel Composition/Part Sheet structure, but its visual authority is still inherited from a character-only style reference set and its current reference packages were reconstructed from existing model proof. This weakens equipment-specific art direction and blurs the intended direction from visual intent -> reference authority -> VoxelSpec -> compatibility proof.

## What Changes

- Extend `low-resolution-voxel.v1` from one flat style-reference list into scoped style authority with `common`, `avatar`, and `equipment` reference sets.
- Make style provenance asset-kind aware: avatars bind to `common + avatar`; equipment binds to `common + equipment`.
- Add approved equipment visual references that define equipment-specific low-resolution voxel language such as silhouette, thickness, proportions, grip/attachment-region readability, and restrained detail density.
- Keep the canonical Hero.Voxel Composition/Part Sheet contract unchanged for equipment; do not introduce weapon-, shield-, backpack-, or equipment-specific sheet formats.
- Tighten equipment reference acceptance so equipment intent/reference authority is upstream of VoxelSpec/source authoring rather than being derived from existing model proof except during explicit migration/recovery.
- Keep compatibility/mount implementation separate from visual reference authority: equipment references may show visual attachment intent but do not own exact pivots, assembly origin, sockets, corrective transforms, or runtime compatibility semantics.
- Re-author the current RightBlade01, LeftGuard01, and BackPack01 reference packages style-first under scoped equipment authority, then verify their existing VoxelSpecs/proofs against the accepted references and repair source only when visual evidence requires it.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `reference-style-authority`: Style families gain asset-kind-scoped reference sets and provenance so equipment uses equipment-relevant visual authority without unnecessarily depending on avatar-only references.
- `asset-publication`: Equipment publication requires reference authority to precede source/model authority and keeps visual attachment intent separate from implementation compatibility semantics.
- `store-maintenance`: Equipment design/repair jobs resolve the applicable scoped style references and follow a style-first reference -> source -> proof sequence.

## Impact

- Affects `Art/Style/low-resolution-voxel.v1/style.yaml`, equipment style-reference images, style resolution/provenance in the store Tool, tests, and `.agents/skills/voxel-store-maintain/SKILL.md`.
- Replaces the current flat style-reference dependency with scoped style provenance while preserving the same `low-resolution-voxel.v1` family identity.
- Re-authoring the three existing equipment reference packages may invalidate their current reference audit evidence and, if accepted visual references disagree with current geometry, may require bounded VoxelSpec repair followed by regenerated proof.
- Does not change Hero.Voxel's reference-sheet format, stable asset IDs/UnitRefs, equipment mount profiles/sockets, gameplay semantics, or runtime compatibility architecture.
