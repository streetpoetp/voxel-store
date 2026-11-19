# Tasks

## 1. Provider-neutral Product Target provenance

- [x] 1.1 Extend Product Target provenance contracts with version 3 acquisition-neutral semantics, keeping v1/v2 shapes readable; verify serialization/deserialization fixtures cover `repository-authored` and `imported` v3 origins without requiring generator/provider/model fields.
- [x] 1.2 Update `StoreEngine.ValidateTargetProvenance` with explicit v3 validation for target hash, repository-authored style authority, imported source authority, and optional non-authoritative producer metadata while leaving v1/v2 behavior unchanged; verify focused tests reject stale/missing v3 authority and accept valid v3 targets without generator metadata.
- [x] 1.3 Extend Product Target provenance regression tests to prove existing v1 generated/imported and v2 image-model evidence still pass unchanged, and verify no migration of current published target provenance is required.

## 2. Product Target acquisition workflow

- [x] 2.1 Rewrite `.agents/skills/voxel-store-product-target/SKILL.md` around provider-neutral acquire -> visually accept -> freeze -> provenance semantics, removing mandatory image-model, ChatGPT/WebCodex bridge, provider/model, and generation-request requirements; verify the skill still requires repository-owned exact accepted target bytes and fails closed when no valid target can be acquired.
- [x] 2.2 Update `.agents/skills/voxel-store-create-model/SKILL.md`, `.agents/skills/voxel-store-maintain/SKILL.md`, `AGENTS.md` where needed, and `WORKFLOW.md` so the canonical production sequence is identity -> shared style grammar -> acquire/accept Product Target -> Composition/Parts -> density -> VoxelSpec -> proof -> compatibility -> publish; verify active guidance contains no Samples/model-first fallback.
- [x] 2.3 Reclassify `Tool/import-product-target.ps1` as an optional exact-byte transport helper rather than a canonical dependency, and verify no active workflow/skill requires a specific ChatGPT, public-download, or image-generation transfer path.

## 3. Publication and maintenance behavior

- [x] 3.1 Update Tool/publication gate logic and fixtures as needed so a current v3 Product Target without generator metadata can unlock target-driven Composition/source gates, while stale/missing Product Target authority still blocks them; verify publication tests exercise both passing provider-neutral authority and blocked invalid authority.
- [x] 3.2 Preserve the single production lane: when Product Target acquisition is blocked, report the Product Target/reference gate and stop before Composition, density, or VoxelSpec; verify deterministic maintenance/publication tests never satisfy collection coverage or publication through an alternate Samples/experimental path.

## 4. Integration verification

- [x] 4.1 Run the full VoxelStore Tool test suite with the pinned Hero.Voxel provider and verify all provenance, reference, density, publication, and compatibility tests pass.
- [x] 4.2 Run `dotnet run --project Tool/VoxelStore.Tool -- scan --write`, `verify`, and `plan` with the pinned provider and verify existing published assets/coverage state remain semantically unchanged apart from intended derived refreshes.
- [x] 4.3 Run `openspec validate decouple-product-target-acquisition-from-image-generation --strict` and `git diff --check`; verify the change is strict-valid and introduces no unrelated workspace modifications.
