# Tasks

## 1. Secure independent bow visual authority

- [x] 1.1 Import the exact supplied ranger/bow PNG into `Art/Style/low-resolution-voxel.v1/equipment-bow-source.png` without re-encoding; record its SHA-256 and verify the repository copy is byte-identical to the supplied source before any destructive asset/coverage change.
- [x] 1.2 If exact-byte import is unavailable, stop the apply at this gate and verify no existing equipment/style/coverage authority has been deleted or rewritten; do not substitute a generated or re-encoded image.
- [x] 1.3 Update focused style-authority tests/fixtures to support the new independent equipment source input and verify equipment style provenance can resolve from the imported image without depending on any current equipment asset.

## 2. Establish explicit retirement authority

- [x] 2.1 Add a repository-owned retirement/tombstone contract for published asset id + UnitRef pairs and register `foundation.equipment.blade.right-01`, `foundation.equipment.guard.left-01`, and `foundation.equipment.pack.back-01`; verify the retirement file is portable and contains no gameplay semantics.
- [x] 2.2 Extend store validation so active manifests cannot reuse a retired asset id or retired UnitRef; add tests proving each reuse path fails while a fresh replacement identity remains valid.
- [x] 2.3 Add a regression proving intentional deletion of a retired manifest does not itself count as an in-place identity mutation; verify historical identity protection for still-active manifests remains unchanged.

## 3. Replace active equipment baseline authority

- [x] 3.1 Change `policy/coverage.yaml` from Hand.R + Hand.L + Back equipment coverage to one required equipment slot on `humanoid.medium.v1 / Hand.L` for the bow while preserving the avatar requirement; verify no retired equipment coverage id remains active.
- [x] 3.2 Replace the `low-resolution-voxel.v1` equipment style scope with only `equipment-bow-source.png`, then remove the three old model-derived `equipment-reference-*.png` boards; verify avatar style scope/hashes are unchanged and equipment authority has no retired-asset dependency.
- [x] 3.3 Delete `RightBlade01`, `LeftGuard01`, and `BackPack01` active asset directories plus their avatar-side compatibility JSON evidence; verify no active path, manifest, reference, proof, or compatibility evidence for those three assets remains outside retirement/history artifacts.
- [x] 3.4 Run a non-writing store scan after the baseline switch and verify it reports the avatar healthy, exactly one required bow coverage gap, and no repair/coverage target for the retired blade/guard/backpack slots.

## 4. Author Bow01 canonical reference package

- [x] 4.1 Create Bow01 manifest scaffolding with fresh identity `foundation.equipment.bow.left-01` / `voxel-store/equipment/bow/left-01`, kind `equipment`, profile `humanoid.medium.v1`, socket `Hand.L`, and a Bow01-local palette path; verify no retired identity or shared avatar palette is reused.
- [x] 4.2 Generate and visually audit only Bow01's canonical 3:2 Composition Sheet from the imported source image using the pinned Hero.Voxel workflow: fixed Hero/Manifest top regions, full-width Exploded Composition middle region, full-width Assembly/Pivots/Reference Pose bottom region, flat/unlit construction authority, and no implementation transforms; freeze target bounds and `RequiredParts` only after this audit.
- [x] 4.3 Default the Composition Sheet to one rigid `Bow` part unless visual construction evidence requires an independently moving/attachable part; verify limb/grip/wrap/tip/string features remain visual geometry subparts rather than extra rigid parts when no independent movement exists.
- [x] 4.4 Generate and visually audit exactly one Part Sheet per frozen Part ID using a local Hero View plus Front/Back/Left/Right/Top/Bottom at the same scale/orientation and a visual geometry-subpart breakdown; verify every construction view is flat/unlit and the sheet contains no pivot/socket/mount implementation metadata.
- [x] 4.5 Write Bow01 `reference.yaml` from the accepted Composition/Part Sheet set and seal reference evidence against the exact equipment source/style authority + pinned Hero.Voxel workflow; verify reference coverage is exact and reference acceptance precedes any VoxelSpec source.

## 5. Author Bow01 source and deterministic proof

- [x] 5.1 Derive a Bow01-local palette from the accepted bow source/reference colors (wood tones, green wraps, dark grip/accent, pale string as visually justified); verify the shared `Art/CombatFoundation/Palette.yaml` remains byte-unchanged.
- [x] 5.2 Author `Bow01.voxel.yaml` only from the accepted reference package, matching frozen bounds/parts and using one rigid Bow assembly unless the Composition Sheet froze more; verify no gameplay fields, pair-specific corrections, or precomposed avatar geometry are introduced.
- [x] 5.3 Author the downstream Bow01 assembly origin/mount frame for `Hand.L` without encoding that implementation into reference sheets; run Hero.Voxel generation and verify generated siblings remain beside the source and are deterministic.
- [x] 5.4 Generate standalone and isolated-part Hero.Voxel proof, visually compare it to the accepted reference package/source image, repair Bow01 source rather than weakening the reference when mismatched, then seal proof only after visual acceptance.
- [x] 5.5 Run Bow01 publication/budget gates and verify `foundation.equipment.bow.left-01` is base-published before compatibility is considered.

## 6. Verify Hand.L compatibility and new baseline

- [x] 6.1 Generate compatibility evidence for FoundationGuard01 + Bow01 at `Hand.L` across both existing representative Idle samples; verify exactly two compatibility rows pass and no pair-specific corrective transform is required.
- [x] 6.2 Regenerate catalog/health and verify the active store contains exactly two published assets (FoundationGuard01 + Bow01), one equipment asset, zero coverage gaps, two passing compatibility rows, and `NextJob = null`.
- [x] 6.3 Update baseline regression tests that currently assert four assets / three equipment / six compatibility rows / blade-specific evidence to assert two assets / one bow / two compatibility rows / bow evidence; verify the full Tool regression suite passes.
- [x] 6.4 Replace blade-specific stale-generation/maintenance fixtures with Bow01 equivalents and verify failure-first maintenance still prioritizes stale reproducible output ahead of unrelated coverage work.
- [x] 6.5 Add a no-ghost audit proving retired ids, UnitRefs, directories, old style-board names, and old compatibility evidence names occur only in retirement tombstones or archived/history material; verify active catalog/style/coverage/evidence contain none of them.

## 7. Final integration and idempotence

- [x] 7.1 Run maintenance planning twice while hashing authoritative style/reference/source/retirement/coverage inputs; verify both runs return no creative job and mutate no authoritative content.
- [x] 7.2 Delete generated catalog/health outputs and regenerate them; verify the regenerated files are byte-deterministic for the two-asset bow baseline.
- [x] 7.3 Run `openspec validate replace-foundation-equipment-with-reference-driven-bow --strict`, both Tool formatter checks, the full regression suite, store verify, repository path/gameplay/boundary audits, and `git diff --check`; verify all checks pass before workflow finalization.

## Workflow follow-up

- Sync the validated deltas into `asset-publication`, `reference-style-authority`, and `store-maintenance`, then archive the change after implementation acceptance.
- Finalize Git history through the repository workflow and push normally only after post-archive and post-rebase verification succeed.
