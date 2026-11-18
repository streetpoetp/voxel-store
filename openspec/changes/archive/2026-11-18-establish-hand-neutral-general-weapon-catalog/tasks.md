# Tasks

## 1. Version-2 contracts and deterministic semantics

- [x] 1.1 Extend manifest, compatibility-profile, and coverage contracts with version-2 shapes for `form`, logical mount `capability`, and capability-owned physical socket instances while keeping the pre-migration v1 baseline readable; verify Tool contract tests accept both valid transition states and reject incomplete/mixed v2 declarations.
- [x] 1.2 Refactor compatibility validation/evaluation so equipment declares one logical capability and proof expands across every profile socket implementing that capability; verify deterministic tests cover missing capability instances, incompatible frames, both physical hand instances, every representative pose, and rejection of per-instance/per-pair corrective data.
- [x] 1.3 Extend compatibility evidence/health records with logical capability plus concrete attachment instance and keep one deterministic evidence file per avatar/equipment pair; verify fixture output is stable across repeated scans and a failing row identifies avatar, equipment, profile, capability, attachment, pose, and evidence path.
- [x] 1.4 Add publication-stable equipment `form` handling, exact form-aware coverage matching, and form/capability fields in derived catalog/health projections; verify tests prove a different form cannot satisfy a requirement and an unpublished form mutation is detected unless performed through explicit migration.

## 2. Hand-neutral profile and avatar migration

- [x] 2.1 Add `humanoid.medium.v2` with logical `Hand` capability mapped to neutral `Hand.0` / `Hand.1` sockets plus the existing `Back` capability, deriving the shared hand envelope from the published v1 hand envelopes; verify profile validation/history tests reject duplicate instances, invalid frames/envelopes, and incompatible in-place v2 changes.
- [x] 2.2 Migrate FoundationGuard01 to active manifest/profile v2 and rename only its hand attachment sockets to `Hand.0` / `Hand.1` while retaining existing arm/body part identities; regenerate deterministic source siblings/proof and verify avatar conformance passes for both required hand instances at both Idle samples.
- [x] 2.3 Audit the active avatar/profile authority after migration and verify no non-archived policy, manifest, active avatar socket, or generated compatibility evidence still exposes `Hand.R` / `Hand.L` as equipment-mount semantics.

## 3. Bow replacement and catalog coverage activation

- [x] 3.1 Add retirement authority for `foundation.equipment.bow.left-01` / `voxel-store/equipment/bow/left-01`, establish replacement identity `foundation.equipment.bow.01` / `voxel-store/equipment/bow/01` with manifest v2, `form: bow`, and `humanoid.medium.v2 / Hand`; verify retired identity/UnitRef reuse is rejected.
- [x] 3.2 Reuse Bow01's accepted visual construction package only if its visual design remains unchanged, reseal all identity/profile/source/proof authority for the replacement, and verify compatibility produces four passing rows: two physical hand instances across two Idle samples with no corrective transform.
- [x] 3.3 Switch `policy/coverage.yaml` to version 2 with the v2 avatar requirement plus ten distinct required equipment forms on capability `Hand`; verify Bow01 satisfies only the bow row, exactly nine non-bow gaps remain, and `PlanNext` selects only the first deterministic gap.
- [x] 3.4 Audit the current equipment-scoped `low-resolution-voxel.v1` authority against the expanded catalog intent; verify it is sufficient for clear low-resolution reference authoring or establish one repository-owned general equipment style input under the same scope and prove the resulting style audit is current before non-bow asset work begins.

## 4. General catalog asset jobs

- [x] 4.1 Run one bounded maintenance job for Sword01: create and accept its canonical reference package before source, publish `foundation.equipment.sword.01` with `form: sword` and v2 `Hand` capability, and verify deterministic proof plus both attachment instances across both Idle samples pass before selecting another asset.
- [x] 4.2 Run one bounded maintenance job for Dagger01 with the same reference-first/publication sequence and verify `foundation.equipment.dagger.01` alone satisfies the dagger coverage row with four passing compatibility rows.
- [x] 4.3 Run one bounded maintenance job for Axe01 with the same reference-first/publication sequence and verify `foundation.equipment.axe.01` alone satisfies the axe coverage row with four passing compatibility rows.
- [x] 4.4 Run one bounded maintenance job for Hammer01 with the same reference-first/publication sequence and verify `foundation.equipment.hammer.01` alone satisfies the hammer coverage row with four passing compatibility rows.
- [x] 4.5 Run one bounded maintenance job for Mace01 with the same reference-first/publication sequence and verify `foundation.equipment.mace.01` alone satisfies the mace coverage row with four passing compatibility rows.
- [x] 4.6 Run one bounded maintenance job for Spear01 with the same reference-first/publication sequence and verify `foundation.equipment.spear.01` alone satisfies the spear coverage row with four passing compatibility rows.
- [x] 4.7 Run one bounded maintenance job for Staff01 with the same reference-first/publication sequence and verify `foundation.equipment.staff.01` alone satisfies the staff coverage row with four passing compatibility rows.
- [x] 4.8 Run one bounded maintenance job for Crossbow01 with the same reference-first/publication sequence and verify `foundation.equipment.crossbow.01` alone satisfies the crossbow coverage row with four passing compatibility rows.
- [x] 4.9 Run one bounded maintenance job for Rifle01 with the same reference-first/publication sequence and verify `foundation.equipment.rifle.01` alone satisfies the rifle coverage row with four passing compatibility rows.

## 5. Integration convergence and cleanup

- [x] 5.1 Remove v1 active profile/content and transitional compatibility assumptions that are no longer needed after every active asset is on v2; verify an active-tree audit finds no `Hand.R` / `Hand.L` equipment-mount semantics outside archived OpenSpec/history while avatar rig-side part names remain allowed.
- [x] 5.2 Regenerate catalog/health from authoritative inputs and verify the final snapshot contains one healthy avatar plus ten healthy equipment assets, all ten form requirements satisfied, exactly forty passing equipment compatibility rows for the current avatar, and `NextJob` is null.
- [x] 5.3 Run the full VoxelStore Tool test suite and clean deterministic regeneration, then run the maintenance planner twice with unchanged inputs; verify tests pass, generated outputs are stable, both planner runs are no-ops, and no asset identity/form is mutated.
- [x] 5.4 Run `openspec validate establish-hand-neutral-general-weapon-catalog --strict` plus repository formatting/diff/hygiene checks; verify the change is strict-valid and the final diff contains no gameplay-only metadata, pair-specific correction records, or developer-local paths.