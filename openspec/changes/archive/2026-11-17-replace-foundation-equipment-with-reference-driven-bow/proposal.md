# Proposal

## Why

The current CombatFoundation equipment baseline contains three published assets (right-hand blade, left-hand guard, and backpack) that no longer match the desired product direction. The supplied ranger/bow image should become the visual source for a single canonical bow, while the old equipment models and their derived style authority are removed completely rather than remaining as hidden dependencies.

## What Changes

- **BREAKING** Retire and delete `RightBlade01`, `LeftGuard01`, and `BackPack01` as active store assets, including their generated/reference/proof/compatibility evidence.
- Add one new equipment asset with a new immutable identity: `foundation.equipment.bow.left-01` / `voxel-store/equipment/bow/left-01`, mounted through `humanoid.medium.v1 / Hand.L`.
- Replace the current equipment coverage requirements for `Hand.R`, `Hand.L`, and `Back` with one required bow coverage slot on `Hand.L`; keep the avatar compatibility profile and its three socket capabilities unchanged.
- Replace equipment style authority derived from the retiring equipment models with a repository-owned copy of the supplied ranger/bow source image.
- Author Bow01 strictly reference-first: canonical Hero.Voxel Composition Sheet -> frozen `RequiredParts` -> Part Sheet(s) -> VoxelSpec -> generated proof -> animated `Hand.L` compatibility proof.
- Do not preselect bow voxel bounds in policy or source. Freeze bounds from the accepted canonical Composition Sheet before VoxelSpec authoring.
- Keep the bow as one rigid construction part unless the accepted Composition Sheet proves an independently moving/attachable part is required; visual limb/grip/wrap/string regions remain geometry subparts by default.
- Remove stale test/catalog assumptions about four assets, three equipment pairs, six compatibility rows, and blade-specific evidence; regenerate expectations around one avatar + one bow and two representative-pose compatibility rows.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-publication`: Published assets may be explicitly retired/removed from the active baseline without reusing their stable identities, and a replacement asset must receive a new id/UnitRef and pass the normal reference-first publication gates.
- `reference-style-authority`: Active style authority must not depend on generated/reference outputs owned only by retired assets; the equipment scope must resolve to independent repository-owned visual authority.
- `store-maintenance`: An intentional foundation-baseline replacement may atomically retire obsolete assets/coverage requirements and establish the new required coverage without treating removed slots as repair work.

## Impact

- Removes the three current equipment directories under `Art/CombatFoundation/Units/Equipment/` and their avatar-side compatibility evidence.
- Adds `Art/CombatFoundation/Units/Equipment/Bow01/` with a canonical Hero.Voxel reference package, bow-specific palette/source, generated outputs, proof, and `Hand.L` compatibility evidence.
- Updates `policy/coverage.yaml`, the equipment reference-style inputs in `Art/Style/low-resolution-voxel.v1/`, generated catalog/health, Tool tests, and any maintenance fixtures that hard-code the four-asset/three-equipment baseline.
- Does not change `humanoid.medium.v1` socket names/frame semantics, Hero.Voxel compiler ownership, avatar source/reference authority, or gameplay semantics.
