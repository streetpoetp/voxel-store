# Tasks

## 1. Establish the cold-start baseline

- [x] 1.1 Resolve the pinned Hero.Voxel provider, run the current Tool test suite plus `scan --write`, `verify`, and `plan`, and record the ten active equipment ids/UnitRefs; verify the pre-change baseline has one avatar, ten published equipment assets, 40 passing compatibility rows, zero coverage gaps, and no next job.
- [x] 1.2 Atomically retire all ten current equipment identities, remove every active equipment root under `Art/CombatFoundation/Units/Equipment/**`, and remove stale equipment compatibility evidence from the foundation avatar; verify every removed id/UnitRef is present exactly once in `policy/retired-assets.yaml`, no active equipment manifest remains, and `policy/coverage.yaml` is unchanged.
- [x] 1.3 Regenerate derived state for the avatar-only baseline and verify the cold-start contract: the avatar remains published, exactly ten required equipment gaps exist, `NextJob.Priority == required-coverage`, and `NextJob.Target == foundation.equipment.sword`; do not create any equipment before this checkpoint passes.

## 2. Rebuild Sword as the first target-driven job

- [x] 2.1 Claim only the planned Sword coverage job and create a fresh Sword replacement manifest using new id `foundation.equipment.sword.02`, new `UnitRef` `voxel-store/equipment/sword/02`, form `sword`, Hand/`humanoid.medium.v2` compatibility, and `density: reference-driven`; verify retired identities are not reused and the incomplete asset remains blocked at its earliest expected authoring gate.
- [x] 2.2 Resolve current `low-resolution-voxel.v1` target authority plus the pinned Hero.Voxel workflow, generate a repository-owned Sword Product Target from shared high-fidelity style authority and sword form intent, and seal target provenance only after visual acceptance; verify the target preserves the shared voxel language and is not merely a copy of the simple equipment construction-board silhouette.
- [x] 2.3 Generate the Sword v3 3:2 Composition Sheet and then exactly one required Sword Part Sheet, auditing each against both the Product Target and canonical Hero.Voxel construction rules before advancing; verify current fidelity evidence passes all required criteria and the visual package declares no exact VPU, voxel bounds, pivots, sockets, or runtime transforms.
- [x] 2.3a Expose and test a reference-only seal operation before density selection: add `seal-reference <asset-id>` backed by the existing reference-audit logic, make full `seal` reuse it, and document the command; verify v2/v3 reference resealing remains byte-identical and accepted reference evidence can advance to density without requiring missing source/proof or an expected failed `seal`.
- [x] 2.4 Author Sword density input from the accepted Part Sheet, generate/evaluate deterministic VPU candidates, and select the lowest sufficient VPU; verify every meaningful feature survives, normalized size/axis/source-budget constraints pass, and written density evidence is current and reproducible.
- [x] 2.5 Author a fresh Sword VoxelSpec/palette from the accepted visual authority, generate deterministic VOX plus isolated-part/standalone Hero.Voxel previews, iterate until the model visually preserves the Product Target, and seal proof; verify source uses the selected VPU, generated output is reproducible, source/part budgets pass, and model visual acceptance is hash-bound.
- [x] 2.6 Regenerate compatibility/catalog/health for Sword and verify it publishes against both Hand attachments across both required pose samples; then verify exactly nine required equipment gaps remain and the deterministic next job is `foundation.equipment.dagger` before any Dagger authoring starts.

## 3. Rebuild Dagger as the second target-driven job

- [x] 3.1 Claim only the planned Dagger coverage job and create a fresh Dagger replacement manifest using new id `foundation.equipment.dagger.02`, new `UnitRef` `voxel-store/equipment/dagger/02`, form `dagger`, Hand/`humanoid.medium.v2` compatibility, and `density: reference-driven`; verify no retired Dagger identity or legacy geometry becomes active authority.
- [x] 3.2 Resolve current target authority and pinned Hero.Voxel workflow, generate and visually accept a repository-owned Dagger Product Target from shared high-fidelity style authority plus dagger form intent, and seal target provenance; verify the target reaches the shared style's visual richness rather than inheriting the simple construction board as a quality ceiling.
- [x] 3.3 Generate and sequentially audit the Dagger v3 Composition and required Part Sheet(s), refusing to advance on any target-fidelity or construction failure; verify the accepted package is complete, current, visually construction-authoritative, and implementation-neutral.
- [x] 3.4 Produce deterministic Dagger density analysis/candidate evidence and select the lowest sufficient VPU; verify meaningful features, normalized size, Hero.Voxel axis limits, and source budget all pass with current evidence.
- [x] 3.5 Author fresh Dagger VoxelSpec/palette, generate deterministic VOX plus isolated-part/standalone previews, iterate against the Product Target until visual acceptance, and seal proof; verify selected VPU/source agreement, deterministic generation, budget compliance, and current hash-bound model evidence.
- [x] 3.6 Regenerate compatibility/catalog/health for Dagger and verify both new weapons publish with eight total passing equipment compatibility rows; verify exactly eight required coverage gaps remain and `NextJob.Target == foundation.equipment.axe`, then stop without authoring Axe.

## 4. Preserve verification strength for the intentional partial catalog

- [x] 4.1 Refactor repository-baseline integration assertions that currently assume ten checked-in weapons/40 compatibility rows so they assert the intentional final baseline instead: one published avatar, exactly Sword02 and Dagger02 published, eight compatibility rows, eight required coverage gaps, `Healthy == false`, and next required job Axe; verify the updated assertions fail on the wrong count/order and pass on the intended state.
- [x] 4.2 Preserve or add isolated deterministic fixtures that independently prove the unchanged ten-form coverage contract, exact physical-form matching, failure-first maintenance priority, retirement identity rejection, and full-catalog/no-gap convergence; verify no existing contract coverage was deleted merely to accommodate the two-weapon repository baseline.
- [x] 4.3 Run the full Tool tests, regenerate catalog/health, run store `verify` and `plan`, run strict OpenSpec validation and `git diff --check`, and inspect the final diff; verify the only expected store-level failure is the intentional eight uncovered required forms, the planner points to Axe, no third weapon was created, and unrelated local files such as `.idea/` remain untouched.

## Workflow follow-up

- Visually review both final Product Targets, construction packages, and model proof renders before accepting implementation.
- Archive the change only after the two-weapon cold-start evidence and implementation review are accepted.
