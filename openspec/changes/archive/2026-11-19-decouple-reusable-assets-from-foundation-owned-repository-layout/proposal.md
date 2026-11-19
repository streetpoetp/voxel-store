# Proposal: Decouple reusable assets from foundation-owned repository layout

## Why

`voxel-store` is a reusable voxel visual asset library, but its current repository layout makes the active `CombatFoundation` baseline own the asset paths, store identity, compatibility profiles, coverage policy, and visual style selector. That reverses the intended dependency: a collection should select reusable assets; it should not contain or define their identity.

The current `unitRef` contract already expresses the reusable identity correctly (`voxel-store/avatar/...`, `voxel-store/equipment/...`). The repository structure should match that domain model.

## What Changes

- Introduce first-class roots: `Assets`, `Styles`, `Profiles`, `Collections`, `Samples`, `Store`, and `Catalog`.
- Move published avatars/equipment from `Art/CombatFoundation/Units/**` to `Assets/**` without changing published asset ids or UnitRefs.
- Move style authority from `Art/Style/**` to `Styles/**`.
- Move compatibility profiles from `policy/families/**` to `Profiles/**`.
- Convert `policy/coverage.yaml` into `Collections/combat-foundation.v1.yaml`; `CombatFoundation` becomes a selected collection rather than the store/container.
- Move global policy to `Store/**` and generated catalog/health to `Catalog/**`.
- Move experiments from `Art/Samples/**` to top-level `Samples/**`; Tool asset discovery scans only `Assets/**`.
- Make the store identity `voxel-store.v1` and set the default/shared visual family to `low-resolution-voxel.v1` instead of `combat-foundation.v1`.
- Allow asset manifests to resolve their declared style family from `Styles/**`; stop requiring all assets to equal a foundation-specific store style family.
- Preserve Hero.Voxel source/generated sibling ownership inside each asset.

## Non-goals

- Do not rename already published asset ids or UnitRefs.
- Do not alter Hero.Voxel construction semantics, reference package semantics, VPU selection semantics, or compatibility geometry.
- Do not rewrite historical archived OpenSpec documents solely to make old paths look current.
- Do not implement provider-independent planning in this change.

## Impact

- Repository layout and active policy/manifests.
- `Tool/VoxelStore.Tool` root discovery, asset/style/profile/collection/policy/catalog paths, and style validation.
- Tool tests and fixtures that encode the old layout.
- Active workflow/spec documentation.
- Existing reference/proof provenance is regenerated where repository-relative paths are part of evidence.
