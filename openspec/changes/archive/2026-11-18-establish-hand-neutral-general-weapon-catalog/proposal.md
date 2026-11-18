# Proposal

## Why

The current compatibility contract makes handedness part of weapon identity and mounting (`Hand.R` / `Hand.L`), which prevents one reusable weapon asset from composing with either physical hand. The foundation also has only one active weapon asset, so it does not yet provide a representative general-purpose weapon catalog for consumers.

## What Changes

- **BREAKING** Replace side-specific weapon mount contracts with a hand-neutral `Hand` mount capability: weapon assets declare `Hand`, while a compatible avatar may expose multiple physical hand attachment instances that all satisfy that capability.
- **BREAKING** Introduce a new compatibility-profile version instead of redefining published `humanoid.medium.v1` in place; the new profile verifies hand-neutral equipment against every physical hand instance and every required representative pose.
- **BREAKING** Retire the side-specific bow identity `foundation.equipment.bow.left-01` / `voxel-store/equipment/bow/left-01` and replace it with a new side-neutral Bow01 identity that passes the normal reference-first publication gates.
- Extend equipment and coverage authority with a gameplay-neutral physical `form` classification so distinct weapon-form requirements cannot all be satisfied by the same published `kind + profile + mount` asset.
- Establish ten general hand-mountable weapon forms in `CombatFoundation.v1`: sword, dagger, axe, hammer, mace, spear, staff, bow, crossbow, and rifle. The set contains ten total weapons, including the replacement Bow01.
- Keep weapon metadata gameplay-neutral: no damage, rarity, attack behavior, two-hand occupancy rule, combat class, progression, or balance semantics enter voxel-store authority.
- Preserve reference-first authoring for every new weapon: scoped equipment style authority -> Composition Sheet -> frozen `RequiredParts` -> Part Sheet(s) -> VoxelSpec -> deterministic proof -> compatibility proof -> publication.
- Keep one maintenance invocation bounded to one asset-scoped creative/repair job; the catalog is filled incrementally rather than by bypassing the existing maintenance contract.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-compatibility`: Replace side-specific equipment socket ownership with a logical hand-neutral mount capability that can resolve to multiple physical avatar attachment instances and requires compatibility proof across all matching instances.
- `asset-publication`: Add gameplay-neutral physical-form classification to authored equipment identity/metadata and require replacement Bow01 plus every new weapon to pass normal reference-first publication gates without handed identity.
- `store-maintenance`: Make coverage distinguish required physical forms, migrate active foundation coverage to the new hand-neutral profile, and drive the ten-weapon catalog as separate bounded asset jobs.

## Impact

- Changes compatibility contracts in `Tool/VoxelStore.Tool/Contracts.cs`, validation/composition/coverage logic in `Tool/VoxelStore.Tool/StoreEngine.cs`, and their deterministic tests.
- Adds a new versioned humanoid compatibility profile and updates active `policy/coverage.yaml` to require ten distinct hand-neutral weapon forms.
- Replaces the current side-specific Bow01 manifest/compatibility evidence with a new side-neutral asset identity, preserving the retired identity as unavailable for reuse.
- Adds nine additional weapon asset packages beside Bow01, each with canonical Hero.Voxel reference/source/proof evidence and family compatibility evidence.
- Regenerates catalog/health projections and removes active assumptions that a weapon owns exactly one side-specific hand socket.
