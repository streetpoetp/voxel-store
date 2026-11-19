# Tasks

## 1. Establish image-model provenance contracts

- [x] 1.1 Add generated Product Target provenance v2 contracts for image-model generator identity and hash-bound repository request evidence while preserving existing v1 generated/imported compatibility.
- [x] 1.2 Extend Product Target provenance validation so v2 generated targets require `kind=image-model`, nonempty provider/model, an existing contained request artifact, and a current request hash; reject procedural/missing/stale generator evidence.
- [x] 1.3 Add deterministic tests proving valid v2 generated provenance, missing generator rejection, non-image generator rejection, stale request rejection, and unchanged v1/imported compatibility.

## 2. Make image-model target creation an explicit agent stage

- [x] 2.1 Add a dedicated `voxel-store-product-target` skill that owns image-model candidate generation, visual iteration, exact-byte project delivery, target request capture, provenance v2, and fail-closed behavior when generation or transfer is unavailable.
- [x] 2.2 Update `voxel-store-maintain` so new/generated v3 target work invokes the Product Target skill before Composition and never uses procedural renderers as fallback target authority.
- [x] 2.3 Update `WORKFLOW.md` to distinguish creative image-model Product Target generation from deterministic construction-sheet rendering and document exact-byte delivery plus model-to-target convergence.
- [x] 2.4 Add and prove a same-desktop exact-byte bridge with `Tool/import-product-target.ps1`, and integrate it into the Product Target skill/workflow; verify a downloaded accepted PNG imports with identical SHA-256 rather than screenshot/redraw substitution.

## 3. Remove the misleading procedural-target pattern

- [x] 3.1 Delete Sword02 and Dagger02 procedural `Reference/generate-target.ps1` helpers while leaving their accepted target PNGs, v1 provenance, construction evidence, source, and proof unchanged.
- [x] 3.2 Verify the checked-in two-weapon baseline retains the same publication/gap/plan state after those non-authoritative helper removals.
- [x] 3.3 Convert `Art/Samples/AxeStyleTest` into an intentional end-to-end proof: exact image-model target -> target-derived reference evidence -> fresh VPU80 VoxelSpec -> Hero.Voxel renders -> iterative model-to-target audit; verify rejected model interpretations repair the model while the target hash remains unchanged.
- [x] 3.4 Prove the same workflow on an independent `KnifeStyleTest`: exact downloaded image-model target -> target-derived construction evidence -> normalized VPU64/80/96 candidates -> lowest-sufficient density selection -> Hero.Voxel comparison/iteration, while keeping the target hash frozen.


## 4. Validate the whole change

- [x] 4.1 Run the full Tool test suite with the pinned Hero.Voxel provider and confirm provenance-v2 coverage plus existing publication contracts pass.
- [x] 4.2 Run `scan --write`, `verify`, and `plan`; confirm the only store-level incompleteness remains the intentional eight coverage gaps and Axe remains the next job.
- [x] 4.3 Run strict OpenSpec validation and `git diff --check`; inspect the implementation diff and keep unrelated `.idea/` outside the change.
- [x] 4.4 Re-run the full Tool suite, exact-byte Axe import/reference regeneration, Axe Hero.Voxel compile/preview proof, store scan/verify/plan, strict OpenSpec validation, and final diff hygiene after the bridge/proof additions.
