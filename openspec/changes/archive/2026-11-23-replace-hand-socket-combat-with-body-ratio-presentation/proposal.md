# Proposal

## Why

The current hand/socket experiment adds articulation, compatibility, and authoring complexity without producing proportionate visual value for the intended low-cost voxel mobile style. The approved direction is to make avatars a single rigid Body and derive equipment placement and combat feedback from normalized Body bounds plus runtime presentation transforms.

## What Changes

- **BREAKING** Replace articulated avatar construction (`Head`/`Arm`/`Hand` parts and model-authored sockets) with a single rigid `Body` part as the reusable avatar contract.
- **BREAKING** Remove compatibility-profile, mount-capability, and physical-socket requirements from ordinary avatar/equipment composition.
- **BREAKING** Replace equipment `mount.profile` / `mount.capability` declarations with a canonical equipment-local origin/orientation convention and Body-bounds-relative placement.
- Define deterministic normalized offset-ratio placement for one or more simultaneous equipment instances using canonical Body local bounds and consumer-selected slot/index placement.
- Keep gameplay meaning outside voxel-store: weapon role, attack type, damage, hit timing, and slot semantics remain consumer-owned.
- Use a separate runtime `PresentationRoot` under the authoritative unit transform; DOTween may drive presentation-only motion/feedback on this root but SHALL NOT own simulation state or hit/damage authority.
- Rework compatibility evidence around canonical Body/equipment conventions and real composition proof instead of socket/profile pair matrices.
- Extend the Unity preview proof loop to compose published Body + equipment assets using ratio placement and capture dense temporal samples from multiple deterministic views.
- Deliver an updated `SkeletonGrunt01` single-Body model plus a runnable Unity sample that demonstrates ratio-placed equipment and DOTween-driven whole-composition combat presentation.
- Supersede and remove the current `SkeletonGrunt01` hand/socket combat experiment during implementation rather than preserving it as a compatibility layer.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `asset-compatibility`: replace profile/socket/mount compatibility with single-Body canonical bounds, canonical equipment origin/orientation, normalized ratio placement, and multi-equipment composition rules.
- `asset-publication`: update avatar/equipment publication metadata and gates so publication no longer depends on hand/mount/profile declarations and instead requires the new canonical Body/equipment composition contract where applicable.
- `unity-baked-preview`: extend derived Editor proof to build real Body + equipment compositions from ratio placement and capture deterministic dense multi-view evidence without becoming runtime authority.

## Impact

- Affects avatar VoxelSpec structure, equipment manifests, compatibility profiles/validation, generated compatibility evidence, catalog/health projections, and the Unity preview sample/capture tooling.
- Removes the hand/socket animation direction currently present in the dirty `SkeletonGrunt01` experiment.
- Uses the existing DOTween dependency already supplied by Hero Studio for consumer/runtime presentation; voxel-store remains asset/proof authority and does not become a gameplay runtime.
- Requires migration of current published avatars/equipment and regeneration of affected proof/catalog outputs.
- This is a breaking compatibility-contract change and must preserve rollback boundaries until migrated assets and proof gates are green.
