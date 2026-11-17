# Tasks

## 1. Establish concrete low-resolution style authority

- [x] 1.1 Import the exact approved uploaded character images 2-4 into `Art/Style/low-resolution-voxel.v1/reference-01.png` through `reference-03.png`, excluding uploaded image 1 (the fishing-boat board) from the active style set; verify the stored PNG bytes match the imported uploads and the style directory contains only the intended three canonical references plus its manifest.
- [x] 1.2 Add `Art/Style/low-resolution-voxel.v1/style.yaml` with stable id `low-resolution-voxel.v1`, ordered reference paths, and the approved low-resolution voxel visual traits; verify parsing rejects missing/duplicate/non-PNG references and accepts the three-image authority set.
- [x] 1.3 Extend store policy/contracts with a separate `referenceStyle` selector while preserving existing `styleFamily: combat-foundation.v1`; verify tests prove asset style-family validation is unchanged and missing/unresolvable reference style fails closed.
- [x] 1.4 Add deterministic style-authority resolution and hashing without adding an image-processing dependency; verify the store can resolve all three PNGs from repository-relative paths and reports their stable provenance hashes.

## 2. Replace implicit SVG reference acceptance with ReferenceContract v2

- [x] 2.1 Evolve `ReferenceContract` to require `version: 2`, explicit `composition`, `requiredParts`, and `partSheets` mapping; verify tests reject old v1/implicit SVG contracts, missing/extra Part IDs, duplicate mapped paths, composition reuse, and paths escaping the asset Reference directory.
- [x] 2.2 Implement lightweight PNG signature/IHDR validation for declared Composition and Part Sheets, requiring a valid PNG for every accepted sheet and exact 3:2 aspect ratio for Composition; verify tests cover invalid signatures, zero/invalid dimensions, non-3:2 Composition, and valid Part Sheet PNGs without introducing a raster library dependency.
- [x] 2.3 Replace all `Parts/<PartId>.svg` derivation in reference enumeration/validation with the explicit v2 mapping; verify unreferenced files do not become authority and exact mapped coverage equals `RequiredParts`.
- [x] 2.4 Extend generated reference-audit provenance with active `referenceStyle`, style-manifest hash, all three style-reference PNG hashes, and pinned Hero.Voxel workflow fingerprint; verify changing any one of those inputs invalidates a previously accepted asset at the reference gate.
- [x] 2.5 Update reference sealing/tests so PNG existence and hashes alone never substitute for the agent-owned visual audit contract; verify tests continue to distinguish deterministic structural/provenance checks from visual acceptance responsibility.

## 3. Make autonomous maintenance style-aware

- [x] 3.1 Update `.agents/skills/voxel-store-maintain/SKILL.md` so every reference design/repair job resolves and visually inspects the active style references before generation, while continuing to read the pinned Hero.Voxel workflow for sheet semantics; verify the skill contains no copied Hero.Voxel layout/checklist rules.
- [x] 3.2 Add the reusable project-specific generation clause for chunky low-resolution voxel forms, chibi proportions, simplified geometry, restrained detail density, clear color blocking, strong silhouettes, and no realistic/painterly rendering; verify it is explicitly subordinate to Hero.Voxel construction orientation/view/lighting rules.
- [x] 3.3 Harden fail-closed guidance: unresolved style authority or failed style/construction visual audit keeps ownership on the current sheet and asset; verify a simulated fresh-agent maintenance trace cannot advance to another Part Sheet or asset after such a failure.

## 4. Migrate FoundationGuard01 reference authority sequentially

- [x] 4.1 With the new v2 validator active, confirm the existing SVG-backed `foundation.avatar.humanoid.guard-01` fails the reference gate before migration; verify the failure reason identifies obsolete/missing v2 PNG reference authority rather than a later generation/proof gate.
- [x] 4.2 Generate exactly one 3:2 `FoundationGuard01/Reference/composition.png` using the three style-reference images for visual language and current healthy VoxelSpec/proof only as migration reconstruction evidence; visually audit the canonical Hero.Voxel Composition Sheet and verify its manifest remains exactly `Body, Head, Arm.L, Arm.R, BackPlate`.
- [x] 4.3 Generate and visually audit only `Parts/Body.png` as the next output; verify it represents Body alone in the approved low-resolution voxel style and passes the pinned Hero.Voxel Part Sheet contract before advancing.
- [x] 4.4 Generate and visually audit only `Parts/Head.png`; verify Head alone passes the same style and canonical construction audit before advancing.
- [x] 4.5 Generate and visually audit only `Parts/Arm.L.png`; verify neutral local geometry does not bake the avatar presentation pose into the part before advancing.
- [x] 4.6 Generate and visually audit only `Parts/Arm.R.png`; verify neutral local geometry does not bake the avatar presentation pose into the part before advancing.
- [x] 4.7 Generate and visually audit only `Parts/BackPlate.png`; verify the accepted avatar Part Sheet set now covers `RequiredParts` exactly once.
- [x] 4.8 Replace FoundationGuard01 `reference.yaml` with v2 explicit PNG mappings, remove its old schematic SVG authority, reseal reference evidence, and verify the avatar returns to healthy base publication without changing its VoxelSpec, UnitRef, sockets, or compatibility profile.

## 5. Migrate independent equipment reference authority

- [x] 5.1 Generate and audit only `RightBlade01/Reference/composition.png`, then only `Parts/Item.png` in a second image output; migrate its v2 mapping/remove SVGs/reseal and verify `foundation.equipment.blade.right-01` returns healthy without changing source geometry or mount contract.
- [x] 5.2 Generate and audit only `LeftGuard01/Reference/composition.png`, then only `Parts/Item.png` in a second image output; migrate its v2 mapping/remove SVGs/reseal and verify `foundation.equipment.guard.left-01` returns healthy without changing source geometry or mount contract.
- [x] 5.3 Generate and audit only `BackPack01/Reference/composition.png`, then only `Parts/Item.png` in a second image output; migrate its v2 mapping/remove SVGs/reseal and verify `foundation.equipment.pack.back-01` returns healthy without changing source geometry or mount contract.
- [x] 5.4 Audit `Art/CombatFoundation/**/Reference` after all migrations and verify no accepted `.svg` construction authority remains, all v2 mappings point to existing PNGs, and rejected/intermediate image generations are not retained.

## 6. Re-establish whole-store health

- [x] 6.1 Regenerate reference/proof audit state plus catalog/health from current authority and run the full Tool regression suite; verify all four V1 assets publish healthy, the three style references are present in reference provenance, compatibility remains 6/6 PASS, coverage gaps are 0, and `NextJob` is null.
- [x] 6.2 Mutate one canonical style-reference PNG in a controlled test and verify every affected previously accepted asset becomes stale at the reference gate; restore the exact bytes and verify resealed/current evidence returns healthy.
- [x] 6.3 Run maintenance planning twice on the completed migrated store while hashing authoritative style/reference/source inputs; verify both runs return no creative job and mutate no authoritative asset/style content.
- [x] 6.4 Run `openspec validate adopt-low-resolution-voxel-reference-image-authority --strict`, both Tool formatter checks, the full test suite, repository boundary/path/gameplay-semantic audits, and `git diff --check`; verify all checks pass before workflow finalization.

## Workflow follow-up

- Sync the validated deltas into `reference-style-authority`, `asset-publication`, and `store-maintenance`, then archive the change after implementation acceptance.
- Finalize Git history through the repository workflow and push normally only after post-archive verification succeeds.
