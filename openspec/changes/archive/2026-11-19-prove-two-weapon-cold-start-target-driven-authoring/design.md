# Design

## Context

See proposal.md for motivation. The repository currently has one healthy foundation avatar and ten healthy published equipment assets satisfying the ten-form `CombatFoundation.v1` coverage policy. The deterministic planner repairs unhealthy existing assets before filling coverage gaps; with an avatar-only baseline and unchanged coverage ordering, the first two required equipment jobs are Sword then Dagger.

The current v3 authoring contract already defines the complete authority chain:

`target style authority -> Product Target -> Composition -> Part Sheet(s) -> reference-driven density -> VoxelSpec -> generated VOX/proof -> compatibility/publication`.

The experiment must exercise that chain against the real store baseline without changing the ten-form product contract or treating legacy model output as design authority.

## Goals / Non-Goals

**Goals:**

- Produce an intentional cold-start repository state with no active legacy equipment and ten deterministic equipment coverage gaps.
- Prove the real planner selects Sword first and Dagger second after Sword becomes healthy.
- Publish exactly two fresh equipment identities using target-driven v3 plus reference-driven density end to end.
- Preserve retired asset ids and `UnitRef` values as permanently reserved historical identity.
- Preserve the ten-form coverage requirement and finish in an intentional partial-convergence state: two published equipment assets, eight required gaps, next job Axe.
- Keep verification strong by separating repository-baseline expectations from isolated tests of the full ten-form contract.

**Non-Goals:**

- Reducing the active coverage policy from ten forms to two.
- Migrating or repairing the eight remaining forms in this change.
- Reusing retired weapon VoxelSpecs, generated VOX, proof renders, or construction sheets as the design basis for the new Sword or Dagger.
- Changing Hero.Voxel, compatibility-profile semantics, publication gate ordering, density-selection behavior, or maintenance-job priority.
- Making the final repository globally healthy; eight required gaps are the expected end state.

## Decisions

### 1. Treat the reset as an atomic baseline migration, not ten independent repair jobs

Remove all active equipment directories and all equipment compatibility evidence from the foundation avatar in one bounded reset step. At the same time, append every removed published asset id and `UnitRef` to `policy/retired-assets.yaml`.

This avoids leaving half-deleted manifests that the planner would interpret as higher-priority regressed assets. After the reset, a scan should see one healthy avatar, zero active equipment assets, ten coverage gaps, and `NextJob.Target == foundation.equipment.sword`.

Alternative considered: delete equipment one at a time and let maintenance repair each missing/broken asset. Rejected because existing asset failures outrank coverage gaps, which would test repair semantics rather than the intended cold-start coverage flow.

### 2. Keep the ten-form coverage policy unchanged

`policy/coverage.yaml` remains authoritative and continues to require sword, dagger, axe, hammer, mace, spear, staff, bow, crossbow, and rifle.

The experiment ends before convergence. After two successful jobs, the store is expected to be unhealthy only because eight required forms remain uncovered.

Alternative considered: temporarily reduce coverage to Sword and Dagger so the final store is healthy. Rejected because that would bypass the planner continuation behavior and weaken the product contract being tested.

### 3. Follow planner order; do not manually choose the two weapons

The first post-reset scan must select `foundation.equipment.sword`. Only after the new Sword is fully published may the next scan select `foundation.equipment.dagger`. After Dagger publication the next job must be `foundation.equipment.axe`.

Each asset phase is a separate maintenance-owned job. A failed gate remains on the current asset until repaired; implementation must not start Dagger while Sword is unhealthy.

Alternative considered: choose Bow and Staff because existing proof samples are stronger. Rejected because manual selection would fail to test deterministic coverage-job ordering.

### 4. Use fresh semantic identities for replacement assets

The replacements use new identities, expected as:

- `foundation.equipment.sword.02` / `voxel-store/equipment/sword/02`
- `foundation.equipment.dagger.02` / `voxel-store/equipment/dagger/02`

All ten removed equipment identities remain in retirement authority and must fail if reused.

The physical `form` remains `sword` or `dagger`, so each replacement satisfies the existing coverage slot without inheriting historical semantic identity.

### 5. Both replacements must exercise target-driven v3 and reference-driven density

Each new manifest declares `density: reference-driven`. Each asset is authored in this order:

1. Resolve current target-style authority and pinned Hero.Voxel workflow.
2. Establish a repository-owned Product Target from shared high-fidelity style authority plus the equipment form intent.
3. Visually accept the Product Target.
4. Generate and audit the 3:2 Composition Sheet against the target.
5. Freeze `RequiredParts`.
6. Generate and audit exactly one uncovered Part Sheet at a time.
7. Produce current density input/candidate evidence and select the lowest sufficient VPU.
8. Author a fresh VoxelSpec and palette from the accepted visual authority.
9. Deterministically generate VOX and Hero.Voxel part/standalone previews.
10. Visually audit the model against the Product Target, seal proof, then regenerate compatibility/catalog/health.

The current simple equipment construction board is supplemental form guidance; it must not cap Product Target richness. Existing retired weapon geometry is not an upstream input.

### 6. Preserve shared style authority, but not old weapon authority

The existing `low-resolution-voxel.v1` style family and its target references remain reusable. New Product Targets may use current shared/common and equipment-scoped style inputs because those are global art-direction authority.

Old Sword01/Dagger01 reference packages, source, generated model, or proof may be inspected only for explicit migration/recovery diagnostics if necessary; they must not be copied, traced, or used to lower the new target to the legacy model.

### 7. Make partial convergence an explicit repository-baseline assertion

Repository-level integration assertions should match the intentional end state rather than claiming the checked-in repository is a complete ten-weapon catalog:

- one published foundation avatar;
- exactly two published equipment assets: the fresh Sword and Dagger;
- eight compatibility rows for equipment composition (`2 equipment x 2 Hand attachments x 2 pose samples`);
- eight required coverage gaps;
- `Healthy == false` because required coverage is incomplete;
- `NextJob.Priority == required-coverage`;
- `NextJob.Target == foundation.equipment.axe`.

Separate deterministic fixture tests must continue to prove the ten-form coverage contract, exact-form matching, failure-first job priority, and full-catalog/no-gap behavior. Do not satisfy the new baseline by deleting those contract tests.

### 8. Expose reference acceptance as its own executable phase

The store already separates reference-audit generation from proof-audit generation internally, but the public CLI exposes only `seal`, which performs both. That is incompatible with the target-driven density sequence because `density` requires a current reference audit before source/proof exists.

Expose `StoreEngine.SealReference(assetId)` and CLI `seal-reference <asset-id>`. It validates policy and the current reference package, then writes only `Reference/audit.generated.json`. Full `seal` delegates to that phase and then seals proof.

This does not change publication gates or durable behavior; it makes the existing reference -> density boundary executable without relying on a failed full-seal side effect.

Alternative considered: continue calling `seal`, allow proof sealing to fail, and keep the reference audit written as an incidental side effect. Rejected because normal workflow advancement must not depend on an expected exception.

## Risks / Trade-offs

- **[Risk] Removing ten published assets is a breaking catalog change for current consumers.** -> Preserve all historical ids/UnitRefs in retirement authority and make the migration atomic and reviewable.
- **[Risk] A half-completed reset can produce repair jobs instead of coverage jobs.** -> Remove complete equipment roots and stale compatibility evidence before the first post-reset scan; validate the exact avatar-only planner state immediately.
- **[Risk] Generated Sword/Dagger Product Targets collapse to the simple construction board.** -> Audit against the shared high-fidelity target style authority and fail the current asset if silhouette richness, material hierarchy, or meaningful detail are materially simplified.
- **[Risk] Density selection may fail to find a sufficient bounded VPU.** -> Keep density failure as the current asset's blocking gate; revise meaningful-feature evidence or visual construction only when justified by accepted target authority, never silently choose a lower VPU.
- **[Risk] Existing repository tests assume 10 published equipment assets and 40 compatibility rows.** -> Refactor repository-baseline assertions to the intended two-asset state while retaining isolated tests for the ten-form/full-convergence contracts.
- **[Risk] Retired geometry is accidentally reused because it is convenient.** -> Delete active legacy equipment roots before authoring replacements and require fresh v3 target/provenance/density/source evidence for each new identity.
- **[Risk] Work advances to Dagger while Sword has a failed visual or publication gate.** -> Require a scan showing the new Sword published and the planner selecting Dagger before any Dagger authoring begins.

## Migration Plan

1. Record the current clean published baseline and verify the pinned Hero.Voxel provider.
2. Capture the ten active equipment ids/UnitRefs for retirement.
3. Remove all active equipment roots and equipment compatibility evidence, update retirement authority, regenerate derived state, and prove the avatar-only cold-start state selects Sword.
4. Run one complete Sword maintenance job through target-driven v3, density, fresh source, model proof, compatibility, and publication. Re-scan and require Dagger as the next job.
5. Run one complete Dagger maintenance job through the same gates. Re-scan and require Axe as the next job.
6. Update repository-baseline test expectations and any synthetic fixtures necessary to preserve independent full-catalog contract coverage.
7. Run Tool tests, scan/verify/plan, strict OpenSpec validation, and diff hygiene. Expected repository health is intentionally false only because eight required coverage gaps remain.

Rollback is an atomic revert of the change, restoring the prior ten-weapon baseline together with its previous retirement and compatibility state. Do not partially roll back only the asset folders or only the retirement list, because those authorities must remain coherent.
