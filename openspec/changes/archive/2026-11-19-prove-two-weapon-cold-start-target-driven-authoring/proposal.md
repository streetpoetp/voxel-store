# Proposal

## Why

The current target-driven v3 workflow is proven by bounded samples, but the published catalog was largely created before that workflow existed. We need a cold-start production test that removes legacy weapon authority and proves the real maintenance planner can rebuild new publishable assets from style/coverage intent through target, density, source, proof, compatibility, and publication.

## What Changes

- **BREAKING** Retire all ten currently published equipment assets and remove their active asset/source/reference/proof directories and stale avatar/equipment compatibility evidence. Preserve every retired asset id and `UnitRef` as reserved identity.
- Keep the existing ten-form `CombatFoundation.v1` coverage policy unchanged so the experiment tests partial convergence instead of weakening the product contract.
- Start from the healthy foundation avatar plus ten equipment coverage gaps and require the deterministic planner to select `foundation.equipment.sword` first.
- Create a new Sword asset with a fresh identity (expected `foundation.equipment.sword.02`) using the complete target-driven v3 flow: style/form intent -> Product Target -> Composition -> Part Sheet(s) -> fidelity acceptance -> reference-driven density -> VoxelSpec -> deterministic VOX/proof -> compatibility -> publication.
- Re-scan after Sword publication and require the planner to select `foundation.equipment.dagger` next.
- Create a new Dagger asset with a fresh identity (expected `foundation.equipment.dagger.02`) through the same full v3/reference-driven workflow.
- Do not reuse retired weapon VoxelSpecs, generated VOX, proof renders, or construction sheets as upstream design authority. Existing shared style authority may be reused.
- Stop intentionally after two published weapons. Expected end state is two healthy published equipment assets, eight required equipment coverage gaps, and `NextJob = foundation.equipment.axe`; the store is intentionally not globally healthy yet.
- Update repository-level verification so this intentional partial-convergence state is proven without weakening the separate deterministic tests that still assert the ten-form coverage contract and healthy full-catalog behavior.

## Capabilities

### New Capabilities

None. This change is an end-to-end proof of existing published contracts.

### Modified Capabilities

None. The durable requirements for ten-form coverage, target-driven v3 authoring, reference-driven density, retirement, compatibility, and deterministic one-job maintenance remain unchanged.

## Impact

- Published equipment inventory under `Art/CombatFoundation/Units/Equipment/**`.
- Avatar compatibility evidence under `Art/CombatFoundation/Units/Avatars/FoundationGuard01/Proof/Compatibility/**`.
- Retired identity authority in `policy/retired-assets.yaml`.
- Derived catalog/health outputs.
- Store integration tests and proof fixtures that currently assume the repository baseline itself contains all ten published weapon forms.
- No change to Hero.Voxel ownership, the pinned provider contract, or the ten-form coverage policy.
