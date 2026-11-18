# Design

## Context

See `proposal.md` for motivation. The current general weapon catalog already satisfies publication, hand-neutral compatibility, coverage, and deterministic maintenance. All ten weapons use `voxelsPerUnit: 8`; the active `Hand` capability envelope is large enough for richer low-resolution construction, and store budgets leave substantial headroom. The visual gap is therefore reference/source quality rather than resolution, compatibility, or capacity.

The catalog also already has the required reference-first authority chain: style input -> Composition Sheet -> Part Sheet(s) -> VoxelSpec -> generated output -> standalone proof -> compatibility proof. This change must refine that chain rather than bypass it.

## Goals / Non-Goals

**Goals:**
- Raise the general weapon catalog from primary-silhouette-only construction to readable low-resolution secondary construction detail.
- Preserve each weapon's identity, form, overall physical role, one-rigid-part ownership, and hand-neutral compatibility.
- Make detail decisions visually auditable from reference authority rather than from geometry-count heuristics.
- Keep the store deterministic and convergent after every refined asset is regenerated, resealed, and reverified.

**Non-Goals:**
- Increasing `voxelsPerUnit`, creating a new compatibility profile, or changing `Hand.0` / `Hand.1`.
- Adding gameplay semantics, mechanical weapon behavior, damage, rarity, balance, or animation.
- Turning visual subregions such as blade/guard/grip/head/stock into new rigid parts unless a future reference independently proves articulation or attachment ownership.
- Enforcing minimum box, voxel, palette, or accent counts.

## Decisions

### 1. Keep the existing voxel scale and compatibility profile

All refined weapons remain at `voxelsPerUnit: 8` and continue to use `humanoid.medium.v2 / Hand`.

Rationale: Bow01 already demonstrates materially richer construction at the same scale, while the current Hand envelope and voxel budget provide ample room. Increasing resolution would expand scope, invalidate compatibility assumptions, and encourage detail density beyond the intended chunky style.

Alternative considered: introduce a higher-resolution equipment profile. Rejected because the observed problem is sparse construction, not insufficient grid resolution.

### 2. Define detail as a visual hierarchy, not a geometry count

Every refined weapon is evaluated through three visual layers:

1. **Primary silhouette** - unmistakable weapon form at a glance.
2. **Secondary construction** - taper, thickness changes, structural transitions, shaped grip/guard/head/stock/limb regions, and material separation.
3. **Tertiary accents** - restrained stepped edges, bands, caps, bevel-like voxel shaping, or small color accents that reinforce construction.

Tertiary detail must never obscure the primary silhouette or become texture-like noise.

Rationale: this gives agents and reviewers a stable quality model without incentivizing arbitrary box inflation.

Alternative considered: require a minimum number of boxes/voxels. Rejected because counts measure implementation density rather than visual quality.

### 3. Update equipment style authority before revising individual assets

The equipment-scoped `low-resolution-voxel.v1` visual authority should be revised so its examples clearly demonstrate the desired secondary construction density. Bow01 can be used as an internal comparison benchmark, but active style inputs must remain independent repository-owned style authority rather than generated proof from a published asset.

Changing equipment style authority intentionally stales equipment reference acceptance. The catalog is then repaired one asset at a time under existing maintenance rules.

Rationale: without an updated shared style target, per-weapon refinements would rely on ad hoc agent taste and drift apart.

Alternative considered: leave style authority unchanged and edit only sources. Rejected because source changes would have no stronger upstream visual authority and would violate the reference-first redesign contract.

### 4. Refine references before source for every weapon

For each weapon, the accepted Composition Sheet is revised first, then its required Part Sheet is revised and accepted, and only then may the VoxelSpec change.

The refined reference should preserve the existing semantic weapon form and rigid-part ownership while making secondary construction visually explicit. Exact pivot, socket, assembly origin, and compatibility transforms remain downstream implementation details and must not appear in the reference package.

Rationale: the existing publication spec already defines intentional visual redesign as reference-first. Reusing that rule keeps the change within established architecture.

### 5. Preserve current overall construction bounds unless detail proves a safe internal redistribution

The default is to retain each weapon's existing target bounding dimensions and grip/attachment intent. Detail should primarily come from redistributing geometry inside those bounds: tapering, stepping, narrowing, thickening, and material segmentation.

A weapon may change its internal silhouette distribution, but no refinement may require a new compatibility profile or exceed the current Hand envelope. If a proposed detail requires that, the asset stops and the design must be reconsidered rather than silently expanding compatibility scope.

Rationale: this limits visual refinement from becoming a physical-contract migration.

### 6. Treat Bow01 as the benchmark, not a mandatory rebuild

Bow01 already has substantially richer authored construction than the other weapons. It should be audited against the strengthened style authority and retained unchanged if it passes. It is refined only if the new visual audit identifies a real construction deficiency.

Rationale: idempotent maintenance should not mutate healthy assets merely to make the change appear uniform.

### 7. Use one bounded asset job at a time

After style authority is updated, assets are repaired sequentially under the existing maintenance contract. A weapon does not advance to publication until its reference, source, deterministic generation, standalone proof, and four hand-neutral compatibility rows all pass. Only then does work advance to the next weapon.

Recommended refinement order:
1. Sword01
2. Dagger01
3. Axe01
4. Hammer01
5. Mace01
6. Spear01
7. Staff01
8. Crossbow01
9. Rifle01
10. Bow01 audit/refine-if-needed

The exact order is not semantically important; the one-asset boundary is.

## Risks / Trade-offs

- **[Risk] More detail drifts toward realistic or noisy voxel art** -> Use the strengthened style authority and the primary/secondary/tertiary hierarchy; remove any detail that does not improve readable construction.
- **[Risk] Refinement changes grip placement or compatibility geometry** -> Preserve existing overall bounds and attachment intent, then require all four `Hand.0` / `Hand.1` x Idle compatibility rows before publication.
- **[Risk] Style-authority update temporarily stales all equipment** -> Treat the change as an intentional catalog-wide visual migration and repair assets sequentially; do not skip failing assets to continue elsewhere.
- **[Risk] Agents optimize for box count** -> Never use box/voxel count as an acceptance threshold; use visual reference fidelity and construction readability.
- **[Risk] Bow is needlessly churned** -> Audit first and retain it unchanged when it already satisfies the strengthened authority.
- **[Trade-off] Preserving `voxelsPerUnit: 8` limits micro-detail** -> This is intentional; the product goal is richer low-resolution construction, not realism.

## Migration Plan

1. Strengthen the equipment-scoped style authority and visually verify the new authority communicates readable secondary construction detail.
2. Scan the catalog and confirm equipment references become stale only because of the intentional style-authority change.
3. For each selected weapon, revise and accept the Composition Sheet, freeze unchanged rigid-part ownership, then revise and accept its Part Sheet.
4. Re-author the VoxelSpec to the accepted reference, regenerate deterministic siblings/proof, visually audit the result, and reseal.
5. Verify both neutral hand attachments across both required Idle samples.
6. Continue one weapon at a time until all ten are current against the strengthened style authority.
7. Regenerate catalog/health, run the full Tool suite and strict OpenSpec validation, then run the planner twice to prove the store converges to a healthy no-op state.

Rollback is repository-level: restore the prior equipment style authority and the prior accepted reference/source/proof set together. Do not roll back only generated outputs or only source geometry because those are downstream of reference/style authority.
