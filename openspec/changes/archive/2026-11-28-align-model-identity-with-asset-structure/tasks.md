# Tasks

## 1. Establish the canonical identity convention

- [x] 1.1 Add one Tool-owned ModelName/ModelKey canonicalization helper for UpperCamel words, acronym runs, and decimal runs; verify focused tests cover `Wraith01 -> wraith-01`, `FoundationGuard01 -> foundation-guard-01`, `WarPick01 -> war-pick-01`, `Rifle02 -> rifle-02`, acronym handling, leading-zero preservation, and collision/error cases.
- [x] 1.2 Integrate canonical identity validation into active manifest discovery so catalog path, `kind`, model directory, `source.base`, `id`, and `unitRef` must agree; verify tests reject legacy namespace segments, wrong catalog/kind pairs, mismatched source model names, noncanonical numeric separators, and otherwise-unique but structurally incorrect ids/UnitRefs.
- [x] 1.3 Document the active convention in the repository authoring/workflow guidance, including the distinction between canonical creation/migration identity and later non-semantic path reorganization; verify examples use only `avatar.<model-key>` / `equipment.<model-key>` and `voxel-store/<kind>/<model-key>` forms.

## 2. Migrate the 25 active published identities atomically

- [x] 2.1 Produce the deterministic migration map from the 25 current `Assets/Avatars/*` and `Assets/Equipment/*` ModelNames, capture pre-migration source/palette/reference/animation hashes, and verify there are exactly 25 unique new ids and UnitRefs with no collision against each other or existing retired history.
- [x] 2.2 Update all 25 active `asset.yaml` files to their canonical ids/UnitRefs and append all 25 previous active `(id, UnitRef)` pairs to `Store/retired-assets.yaml`; verify Tool discovery accepts exactly 25 active canonical identities and rejects reuse of every newly retired legacy identity.
- [x] 2.3 Verify the identity migration did not rename any model directory/source base or change authored source, palette, reference, animation, socket, form, or visual-proof bytes by comparing the captured hashes and reviewing the task-scoped Git diff.

## 3. Reissue identity-bearing evidence and derived state

- [x] 3.1 Remove compatibility evidence whose filename/payload is keyed by a legacy active id and regenerate compatibility proof through the canonical Tool write path; verify every regenerated proof uses canonical avatar/equipment ids, current provider contract data, unchanged source/palette input hashes, and passing samples.
- [x] 3.2 Regenerate catalog/health and other checked derived projections from the migrated authority; verify `dotnet run --project Tool/VoxelStore.Tool -- verify` reports all 25 assets healthy/published with no compatibility failures or collection gaps introduced by the identity change.
- [x] 3.3 Run `preview-plan` after regeneration and verify all 25 returned units use canonical ids/UnitRefs while model/source paths remain the same ModelName-based paths as before the migration.

## 4. Close legacy identity references and downstream review surfaces

- [x] 4.1 Run an active-repository stale-reference scan for the 25 previous ids and UnitRefs, excluding archived OpenSpec history; verify legacy identities remain only in `Store/retired-assets.yaml` and no active manifest, compatibility proof, catalog/health output, Tool fixture, preview input, workflow text, or other active authority/evidence still uses them.
- [x] 4.2 Refresh the canonical Unity preview grid and verify all 25 models still load under unchanged ModelNames, avatar/equipment composition remains valid, and hierarchy/report identity uses the migrated canonical ids without reintroducing collection/origin namespaces.
- [x] 4.3 Run the retained `avatar-motion-gallery` capture and verify provider-attested graphical evidence still passes with unchanged visual content, confirming the identity-only migration did not alter model rendering or animation behavior.

## 5. Integration and release readiness

- [x] 5.1 Run `dotnet run --project Tool/VoxelStore.Tool.Tests`, Tool `verify`, strict OpenSpec validation, and `git diff --check`; verify all task-owned checks pass with no Hero Studio source modification and no unintended authored-content diff.
- [x] 5.2 Review the final old-to-new identity mapping as the downstream consumer migration contract and verify no compatibility alias layer was introduced; report the breaking consumer-reference impact explicitly before archive/commit.

## Workflow follow-up

- After implementation review, sync the `asset-publication` delta into the main spec and archive `align-model-identity-with-asset-structure`.
- After archive validation, commit/push the voxel-store migration as one coherent identity-convention change.
