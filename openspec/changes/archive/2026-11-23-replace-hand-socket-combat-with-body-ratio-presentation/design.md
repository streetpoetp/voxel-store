# Design

## Context

See `proposal.md` for motivation. Today the store's reusable compatibility contract is profile/socket-based: avatars expose profile-declared physical sockets, equipment manifests declare `mount.profile` plus `mount.capability`, and compatibility proof expands avatar/equipment/socket matrices. The current `SkeletonGrunt01` worktree experiment also adds articulated parts and hand-driven animation. The approved replacement is a rigid single-Body asset contract with ratio-derived equipment placement and presentation-only whole-composition motion.

Hero Studio already supplies DOTween to the Unity dependency graph and already uses DOTween for transform-based voxel presentation. The voxel-store Unity workspace is Editor-only and must remain proof tooling rather than production gameplay runtime.

## Goals / Non-Goals

**Goals:**
- Make reusable avatar authoring exactly one rigid `Body` part with canonical local bounds.
- Remove avatar sockets, mount capabilities, compatibility profiles, and pair-specific corrective transforms from ordinary avatar/equipment composition.
- Compose any number of independent equipment instances by normalized Body-bounds offset ratios.
- Keep equipment identity independent from placement index/ratio and keep one canonical equipment-local composition frame.
- Separate authoritative unit/world transforms from presentation-only transforms so whole-body motion cannot corrupt simulation state.
- Use DOTween only for presentation motion and feedback demonstrations, with explicit reset/kill behavior.
- Make Unity proof inspect the real baked Body + equipment composition with dense deterministic temporal captures.

**Non-Goals:**
- Defining combat damage, hit timing, attack selection, inventory slots, handedness, or weapon occupancy rules.
- Adding a production gameplay runtime to voxel-store.
- Preserving the hand/socket experiment as a compatibility mode.
- Creating per-avatar or per-weapon animation clips in voxel-store.
- Solving arbitrary equipment/equipment collision at runtime; the store proves representative canonical compositions and reports failures.

## Decisions

### 1. Canonical avatar composition space is Body local bounds

The authoritative placement space is the compiled `Body` part's local bounds after VPU normalization, not Unity `Renderer.bounds` and not a runtime-transformed world AABB.

For a normalized ratio `r = (rx, ry, rz)`, each component is interpreted in `[-1, +1]` and local position is:

`localPosition = bodyBounds.center + Vector3.Scale(bodyBounds.extents, r)`

This makes placement proportional to avatar dimensions and stable across presentation motion. Ratio values are consumer presentation inputs, not avatar authoring metadata.

Alternative considered: model-authored sockets/mounts. Rejected because the single rigid Body makes those transforms redundant and recreates per-avatar authoring/verification cost.

### 2. Equipment owns one canonical local frame

Equipment remains an independent rigid asset. Its assembly origin and canonical orientation define how it is placed when its root is positioned at a Body-ratio location. Placement does not mutate equipment identity and does not introduce avatar-specific correction data.

Alternative considered: store-authored per-slot rotations or mount records. Rejected because they reintroduce a mount abstraction. If a physical form cannot compose acceptably under the canonical equipment convention, the equipment/global convention must be corrected or the evidence must fail.

### 3. Multi-equipment is an ordered list of independent ratio placements

A consumer presentation request is conceptually an ordered collection of equipment instances:

`{ equipmentId, offsetRatio }[]`

Each instance is parented directly under the unified presentation composition and resolved independently from the same Body bounds. The store does not name `Primary`, `Secondary`, `Left`, `Right`, `Hand`, `Grip`, or `WeaponSlot`; those meanings remain consumer-owned.

An equipment index is only deterministic ordering for composition/capture. Adding a third or fourth item does not require avatar source changes.

Alternative considered: generate named Body mounts such as `Grip.0`/`Grip.1`. Rejected because ratio placement already supplies the only information those mounts would carry.

### 4. Runtime hierarchy separates authority from feedback

Consumer runtime should use:

```text
UnitRoot                 authoritative gameplay/world transform
└─ PresentationRoot      presentation-only offset transform
   ├─ Body
   ├─ Equipment[0]
   ├─ Equipment[1]
   └─ ...
```

`UnitRoot` is owned by simulation/navigation/network state. `PresentationRoot` starts at identity relative to `UnitRoot`; Body and equipment local composition are established below it. Presentation motion changes only `PresentationRoot`.

Alternative considered: tween `UnitRoot`. Rejected because animation recoil/lunge/shake would fight authoritative movement, collision, targeting, or synchronization.

### 5. DOTween is a presentation driver, not an asset or simulation authority

Consumer code may build reusable DOTween sequences for whole-composition anticipation, lunge, recoil, hit reaction, shake, death, and recovery. Sequences must kill/reset existing tweens before taking ownership and restore `PresentationRoot` to its neutral local transform when cancelled, reused, or destroyed.

Voxel-store may use DOTween in Editor proof scenarios because the dependency is already present, but those sequences remain disposable proof inputs. Store assets do not embed combat motion semantics and DOTween callbacks do not apply damage or determine authoritative hit timing.

Alternative considered: Hero.Voxel authored animation clips on Body. Rejected for this workflow because the goal is to remove per-avatar combat animation authoring and reuse transform-based presentation across avatars.

### 6. Compatibility validation moves from socket matrices to composition conventions

`CompatibilityProfile`, `MountCapabilityContract`, avatar socket conformance, and equipment `mount.profile`/`mount.capability` become legacy migration inputs and are removed from the target contract.

Target validation checks:
- avatar source has exactly one authored `Body` part;
- canonical Body local bounds are finite and non-empty;
- equipment has a valid canonical assembly origin/orientation and positive normalized scale;
- required representative Body/equipment ratio compositions render deterministically and stay visually acceptable;
- compatibility evidence identifies avatar, equipment, and ratio when a composition fails.

The evidence set is representative rather than every possible consumer ratio. Consumer-specific ratios remain outside store publication authority.

### 7. Unity proof uses real baked composition and dense temporal sampling

The existing preview builder remains derived Editor tooling. It bakes current published assets through pinned Hero.Voxel, creates a `PresentationRoot`, resolves equipment positions from baked Body local bounds, and captures deterministic front plus weapon-side/three-quarter views.

For a presentation-motion scenario, proof samples ordered times densely enough to include intermediate trajectory states, not only design keyframes. The current experimental 9-time capture pattern is the baseline shape: ready, windup-mid, anticipation, strike-in, contact, strike-out, follow-through, recovery-mid, recovered.

Capture files and reports remain under derived preview state and are never publication/source authority by themselves.

### 8. The current hand/socket experiment is superseded, not migrated in place

The dirty `SkeletonGrunt01` hand/socket work is treated as disposable experiment evidence. Apply begins by preserving only useful capture infrastructure and discarding articulated combat-source changes before constructing the single-Body vertical slice. This avoids accidentally cementing experimental arm/socket semantics into the target migration.

## Risks / Trade-offs

- [Body-ratio placement may look less precise on unusual silhouettes] → Use representative composition proof and correct the global convention or asset geometry rather than adding per-avatar offsets.
- [One canonical equipment orientation may not suit every visual form] → Keep form-specific geometry/origin authoring authoritative; fail incompatible forms instead of adding hidden corrective transforms.
- [No articulated limbs reduces close-up combat fidelity] → Accept this intentionally for the target low-cost voxel mobile style and sell motion through whole-body DOTween, trails, hit-stop, VFX, audio, and victim reaction in consumers.
- [Ratio values can become gameplay slot metadata if stored in asset manifests] → Keep them in consumer presentation configuration and preview scenarios, not stable asset identity.
- [DOTween sequences can fight each other or leave residual offsets] → Require per-root kill/reset ownership and neutral-transform restoration on cancel/reuse/destroy.
- [Breaking manifest/profile migration can temporarily unpublish many assets] → Migrate through a vertical slice first, keep deletions late, regenerate proof/catalog state, and remove legacy structures only after the new gates are green.
- [Dense capture can pass structurally while motion is still aesthetically weak] → Treat automated capture as evidence generation; visual acceptance remains an explicit proof outcome, not inferred from test success.

## Migration Plan

1. Preserve the new autonomous Unity capture infrastructure that is useful independently of hand/socket semantics; discard the experimental `SkeletonGrunt01` articulated combat edits and pair-specific grip corrections.
2. Introduce the target validator/data model for a single `Body`, canonical Body bounds, canonical equipment frame, and ratio-placement evidence while legacy profile data still exists on disk.
3. Convert `SkeletonGrunt01` into the vertical slice: merge all geometry into `Body`, remove sockets and authored combat animation, remove mount/profile dependency, regenerate standalone proof, and compose `Sword02` by ratio in Unity.
4. Build and keep a runnable Unity sample scene/entry that uses the updated `SkeletonGrunt01`, ratio-placed equipment, `UnitRoot`/`PresentationRoot`, and a DOTween whole-composition combat demonstration; this sample is derived proof/example content, not gameplay authority.
5. Prove one- and multi-equipment ratio composition plus the sample's DOTween-driven `PresentationRoot` demonstration through dense multi-view captures.
6. Migrate remaining published avatars to one Body and equipment manifests away from `mount.profile` / `mount.capability`; regenerate compatibility/publication evidence and catalog health.
7. Remove obsolete compatibility profiles, socket validation paths, hand/socket compatibility matrices, and preview code that assumes physical sockets.
8. Run full store verification, Unity EditMode tests, deterministic regeneration checks, and workspace hygiene before finalizing.

Rollback strategy: keep legacy files and validators until the rigid-body vertical slice and migration gates pass. If the new contract cannot satisfy representative equipment composition, revert the target validator/manifest migration and restore the last healthy published baseline; do not preserve a half-migrated dual authority.

## Open Questions

None. Placement ratios, single-Body ownership, canonical equipment frame, multi-equipment ordering, `PresentationRoot` separation, and DOTween's presentation-only role are treated as approved decisions for implementation.
